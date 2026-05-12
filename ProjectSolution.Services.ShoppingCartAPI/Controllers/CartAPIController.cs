using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSolution.Services.ShoppingCartAPI.Data;
using ProjectSolution.Services.ShoppingCartAPI.Extensions;
using ProjectSolution.Services.ShoppingCartAPI.Models.Dto;
using ProjectSolution.Services.ShoppingCartAPI.Models.Dtos;
using ProjectSolution.Services.ShoppingCartAPI.Services.IService;

namespace ProjectSolution.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : ControllerBase
    {
        AppDbContext _dbContext;
        ResponseDto _response;
        IProductService _productService;
        ICouponService _couponService;
        public CartAPIController(AppDbContext dbContext, IProductService productService, ICouponService couponService)
        {
            _dbContext = dbContext;
            _response = new ResponseDto();
            _productService = productService;
            _couponService = couponService;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert([FromBody] CartDto cartDto)
        {
            try
            {
                // chec whether any cart header is present for the user
                var cartHeaderFromDb = await _dbContext.CartsHeaders
                    .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeaderDto!.UserId);
                if (cartHeaderFromDb == null)
                {
                    // create new cart header and details
                    var cartHeader = cartDto.CartHeaderDto!.ToCartHeader();
                    _dbContext.CartsHeaders.Add(cartHeader);
                    await _dbContext.SaveChangesAsync();

                    cartDto.CartDetailsListDto!.First().CartHeaderId = cartHeader.CartHeaderId; // cartHeaderFromDb is null that's why to get the HeaderId we need to call SaveChangesAsync()

                    /* We never set CartHeaderId, so where did it come from?
                     * CartHeaderId is populated by the database (via EF Core) when you call SaveChangesAsync(). EF Core automatically fills it back into your cartHeader object.
                     */

                    var cartDetails = cartDto.CartDetailsListDto!.First().ToCartDetails();
                    _dbContext.CartsDetails.Add(cartDetails);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // update the cart: add new details or update existing ones
                    // check if the product is already in the cart details and has the same cart header which indicates for the same user
                    var cartDetailsFromDb = await _dbContext.CartsDetails
                        .FirstOrDefaultAsync(
                            u => u.ProductId == cartDto.CartDetailsListDto!.First().ProductId &&
                            u.CartHeaderId == cartHeaderFromDb.CartHeaderId); // using First() because user can add only one product at a time (quantity can be more than 1), ussing FirstOrDefaultAsync() could lead to null reference exception, hence using First()
                    if (cartDetailsFromDb == null)
                    {
                        cartDto.CartDetailsListDto!.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _dbContext.CartsDetails.Add(cartDto.CartDetailsListDto!.First().ToCartDetails());
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // update the count of existing product in cart details
                        cartDetailsFromDb.Count += cartDto.CartDetailsListDto!.First().Count;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                var cartDetailsFromDb = await _dbContext.CartsDetails.FirstOrDefaultAsync(u => u.CartDetailsId == cartDetailsId);
                if (cartDetailsFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "There is no item with that id.";
                }
                else
                {
                    //_dbContext.CartsDetails.Remove(cartDetailsFromDb); // count will not be 0 until SaveChangesAsync is applied

                    int cartDetailsCountWithSameHeader = _dbContext.CartsDetails.Where(u => u.CartHeaderId == cartDetailsFromDb.CartHeaderId).Count();

                    _dbContext.CartsDetails.Remove(cartDetailsFromDb); // count will not be 0 until SaveChangesAsync is applied


                    if (cartDetailsCountWithSameHeader == 1)
                    {
                        var cartHeaderFromDb = await _dbContext.CartsHeaders.FirstAsync(u => u.CartHeaderId == cartDetailsFromDb.CartHeaderId);
                        _dbContext.CartsHeaders.Remove(cartHeaderFromDb);
                    }
                    await _dbContext.SaveChangesAsync();
                    _response.Result = cartDetailsFromDb.ToCartDetailsDto();
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpGet("GetCart/{userId}")] // must not mention :string
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                var cartHeaderFromDb = await _dbContext.CartsHeaders.FirstAsync(u => u.UserId == userId);
                var cartDetailsFromDbWithSameHeaderId = await _dbContext.CartsDetails.Where(u => u.CartHeaderId == cartHeaderFromDb.CartHeaderId).ToListAsync();
                IEnumerable<ProductDto> products = await _productService.GetProductDtos();

                CartDto cartDtoResponse = new()
                {
                    CartHeaderDto = cartHeaderFromDb.ToCartHeaderDto(),
                    CartDetailsListDto = cartDetailsFromDbWithSameHeaderId.Select(u => u.ToCartDetailsDto()).ToList() // must use ToList() to convert IEnumerable<CartDetailsDto> to List<CartDetailsDto> because CartDetailsListDto is of type List<CartDetailsDto>
                };

                foreach (var cartDetails in cartDtoResponse.CartDetailsListDto)
                {
                    cartDetails.ProductDto = products.FirstOrDefault(p => p.ProductId == cartDetails.ProductId);

                    /*
                     * The Problem products is IEnumerable<ProductDto>: This means the data has already been retrieved from the database and is loaded into the application's memory.
                    FirstAsync requires IQueryable<T>: The FirstAsync method is an extension method provided by Entity Framework Core (Microsoft.EntityFrameworkCore) designed to work on DbSet or IQueryable to perform asynchronous database operations.
                    The Mismatch: You cannot call FirstAsync or FirstOrDefaultAsync on an in-memory IEnumerable because it is not an asynchronous database query. 
                    The Solution
                    Use the synchronous LINQ method First() or FirstOrDefault() from System.Linq. Since the data is already in memory, there is no need for an asynchronous await. 
                     * 
                     */
                    cartDtoResponse.CartHeaderDto.CartTotal += cartDetails.Count * cartDetails.ProductDto!.Price;
                }

                string couponCode = cartDtoResponse.CartHeaderDto.CouponCode;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    CouponDto couponByCode = await _couponService.GetCouponByName(couponCode);
                    if (cartDtoResponse.CartHeaderDto.CartTotal >= couponByCode.MinAmount)
                    {
                        cartDtoResponse.CartHeaderDto.Discount = couponByCode.DiscountAmount;
                        cartDtoResponse.CartHeaderDto.CartTotal -= couponByCode.DiscountAmount;
                    }
                }

                _response.Result = cartDtoResponse;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var coupon = await _couponService.GetCouponByName(cartDto.CartHeaderDto!.CouponCode);
                bool couponExists = coupon.CouponCode != "";
                if (cartDto.CartHeaderDto!.CouponCode != "" && !couponExists)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Invalid coupon code.";
                    return _response;
                }
                //storing the coupon code in the database
                var cartHeaderFromDb = await _dbContext.CartsHeaders.FirstAsync(u => u.UserId == cartDto.CartHeaderDto!.UserId);
                cartHeaderFromDb.CouponCode = cartDto.CartHeaderDto!.CouponCode;
                await _dbContext.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpGet("GetCartCount/{userId}")]
        public async Task<ResponseDto> GetCartCount(string userId)
        {
            try
            {
                var cartHeaderFromDb = await _dbContext.CartsHeaders.FirstOrDefaultAsync(u => u.UserId == userId);
                int cartCount = 0;
                if (cartHeaderFromDb != null)
                {
                    cartCount = await _dbContext.CartsDetails.Where(u => u.CartHeaderId == cartHeaderFromDb.CartHeaderId).SumAsync(u => u.Count);
                }
                _response.Result = cartCount;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }
    }
}