using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.OrderAPI.Data;
using ProjectSolution.Services.OrderAPI.Extensions;
using ProjectSolution.Services.OrderAPI.Models.Dto;
using ProjectSolution.Services.OrderAPI.Services.IService;
using ProjectSolution.Services.OrderAPI.Utility;
using Stripe;
using Stripe.Checkout;

namespace ProjectSolution.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        protected ResponseDto _responseDto;
        private IProductService _productService;
        public OrderAPIController(AppDbContext db, IProductService productService)
        {
            _db = db;
            _productService = productService;
            this._responseDto = new ResponseDto();
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = cartDto.CartHeaderDto!.ToOrderHeaderDtoFromCartHeaderDto();
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.StatusPending;
                orderHeaderDto.OrderDetailsDtoList = cartDto.CartDetailsListDto!.Select(x => x.ToOrderDetailsDtoFromCartDetailsDto()).ToList();

                OrderHeader orderHeader = orderHeaderDto.ToOrderHeaderFromOrderHeaderDto();
                OrderHeader savedOrderHeader = (await _db.OrderHeaders.AddAsync(orderHeader)).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = savedOrderHeader.OrderHeaderId;
                _responseDto.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };

                var DiscountsObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions()
                    {
                        Coupon = stripeRequestDto.OrderHeaderDto!.CouponCode
                    }
                };

                foreach (var item in stripeRequestDto.OrderHeaderDto!.OrderDetailsDtoList!)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.ProductPrice * 100), // Convert to paise
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName,
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                if (stripeRequestDto.OrderHeaderDto.Discount > 0)
                {
                    options.Discounts = DiscountsObj;
                }

                var service = new SessionService();
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;
                OrderHeader orderHeader = await _db.OrderHeaders.FirstAsync(u => u.OrderHeaderId == stripeRequestDto.OrderHeaderDto!.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();
                _responseDto.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = await _db.OrderHeaders.FirstAsync(u => u.OrderHeaderId == orderHeaderId);

                var service = new SessionService();
                Session session = service.Get(orderHeader.StripeSessionId!);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent.Status == "succeeded")
                {
                    // payment successful, update order status and payment intent id
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = SD.StatusApproved;
                    await _db.SaveChangesAsync();
                    _responseDto.Result = orderHeader.ToOrderHeaderDtoFromOrderHeader();
                }
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpGet("GetOrders/{userId?}")]
        public async Task<ResponseDto> GetOrders(string? userId = "")
        {
            try
            {
                List<OrderHeader> orderHeaders;
                if (User.IsInRole(SD.RoleAdmin))
                {
                    orderHeaders = await _db.OrderHeaders.Include(u => u.OrderDetailsList).OrderByDescending(x => x.OrderHeaderId).ToListAsync();
                }
                else
                {
                    orderHeaders = await _db.OrderHeaders.Include(u => u.OrderDetailsList).Where(x => x.UserId == userId).OrderByDescending(y => y.OrderHeaderId).ToListAsync();
                }
                List<OrderHeaderDto> orderHeaderDtos = orderHeaders.Select(x => x.ToOrderHeaderDtoFromOrderHeader()).ToList();
                _responseDto.Result = orderHeaderDtos;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpGet("GetOrderById/{orderId:int}")]
        public async Task<ResponseDto> GetOrderById(int orderId)
        {
            try
            {
                OrderHeader orderHeader = await _db.OrderHeaders.Include(u => u.OrderDetailsList).FirstAsync(x => x.OrderHeaderId == orderId); // Include helps to fetch the related OrderDetailsList (separate table) along with the OrderHeader, otherwise we would get null for OrderDetailsList in the returned OrderHeader object, or we would have to make a separate query to fetch the OrderDetailsList for the given OrderHeaderId, or run outer join, which is inefficient. So, Include is used to fetch the related data in a single query.
                OrderHeaderDto orderHeaderDto = orderHeader.ToOrderHeaderDtoFromOrderHeader();
                _responseDto.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            try
            {
                OrderHeader? orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.OrderHeaderId == orderId);
                if (orderHeader != null)
                {
                    if (newStatus == SD.StatusCancelled)
                    {
                        // we will give refund
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId
                        };
                        var refundService = new RefundService();
                        Refund refund = await refundService.CreateAsync(options);
                    }
                    orderHeader.Status = newStatus;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }
    }
}
