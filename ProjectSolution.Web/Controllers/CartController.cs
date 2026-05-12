using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectSolution.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            string? userIdFromClaims = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (userIdFromClaims != null)
            {
                ResponseDto? responseDto = await _cartService.GetAllCartsByUserIdAsync(userIdFromClaims);
                if (responseDto != null && responseDto.IsSuccess)
                {
                    CartDto? cartDto = JsonConvert.DeserializeObject<CartDto>(responseDto.Result!.ToString()!);
                    return cartDto ?? new CartDto();
                }
            }
            TempData["ErrorMessage"] = "Cart cannot be loaded!";
            return new CartDto();
        }

        public async Task<IActionResult> RemoveFromCart(int cartDetailsId)
        {
            ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Cart has been updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cart item cannot be removed!";
            }
            return RedirectToAction(nameof(CartIndex));
        }

        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            if (cartDto.CartHeaderDto != null)
            {
                cartDto.CartHeaderDto.CouponCode = cartDto.CartHeaderDto.CouponCode.ToUpper();
            }
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Coupon has been applied successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Coupon cannot be applied";
            }
            return RedirectToAction(nameof(CartIndex));
        }

        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            if (cartDto.CartHeaderDto != null)
                cartDto.CartHeaderDto.CouponCode = "";
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Coupon has been removed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Coupon cannot be removed";
            }
            return RedirectToAction(nameof(CartIndex));
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            CartDto cartDto = await LoadCartDtoBasedOnLoggedInUser();

            if (cartDto.CartHeaderDto!.CartTotal < 50)
            {
                TempData["ErrorMessage"] = "Minimum order amount is ₹50.";
                return RedirectToAction("CartIndex");
            }

            return View(cartDto);
        }

        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> CheckoutPost(CartDto cartDto)
        {
            CartDto cartFromAPI = await LoadCartDtoBasedOnLoggedInUser();
            if (cartFromAPI.CartHeaderDto != null && cartDto.CartHeaderDto != null)
            {
                cartFromAPI.CartHeaderDto.Name = cartDto.CartHeaderDto.Name;
                cartFromAPI.CartHeaderDto.Email = cartDto.CartHeaderDto.Email;
                cartFromAPI.CartHeaderDto.PhoneNumber = cartDto.CartHeaderDto.PhoneNumber;
            }

            ResponseDto? response = await _orderService.CreateOrderAsync(cartFromAPI);
            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(response!.Result!.ToString()!)!;

            if (response != null && response.IsSuccess)
            {
                //get stripe session id and redirect to stripe checkout page to place the order

                var domain = $"{Request.Scheme}://{Request.Host.Value}/";

                StripeRequestDto stripeRequestDto = new()
                {
                    ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "cart/Checkout",
                    OrderHeaderDto = orderHeaderDto
                };
                ResponseDto stripeResponse = (await _orderService.CreateStripeSessionAsync(stripeRequestDto))!;

                if (stripeResponse.Result != null && stripeResponse.IsSuccess)
                {
                    StripeRequestDto stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>(stripeResponse.Result.ToString()!)!;
                    Response.Headers.Append("Location", stripeResponseResult.StripeSessionUrl!);
                    return new StatusCodeResult(303);
                }

                else
                {
                    TempData["ErrorMessage"] = stripeResponse?.Message ?? "Checkout cannot be processed";
                    return View(cartDto);
                }


                //return Redirect(stripeResponseResult.StripeSessionUrl!);
                /*
                 If asked:
                “What’s the difference between setting Location header and using Response.Redirect?”

                Answer:
                “Response.Redirect sets both the HTTP status code 302 and the Location header. Adding only the Location header does not trigger a browser redirect because redirect behavior depends on the status code.”
                 */
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Checkout cannot be processed";
                return View(cartDto);
            }
        }

        [HttpGet] /*So MVC uses default conventional routing:
        {controller=Home}/{action=Index}/{id?}

        Notice:
        👉 It expects id, not orderId
        Since your parameter name is orderId, MVC cannot map it to {id}, so it becomes a query string. Only id is reserved for route segment. Anything else becomes query string.
        
        Define Explicit Route:
        [HttpGet("cart/Confirmation/{orderId:int}")]
        */
        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDto? response = await _orderService.ValidateStripeSessionAsync(orderId);
            if (response != null && response.IsSuccess)
            {
                OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(response.Result!.ToString()!)!;
                if (orderHeaderDto.Status == SD.StatusApproved)
                {
                    TempData["SuccessMessage"] = "Order has been placed successfully!";
                    return View(orderId);
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment not approved. Order cannot be placed.";
                    return RedirectToAction(nameof(PaymentFailed), new { orderId = orderId });
                }
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Order cannot be placed";
            }
            return RedirectToAction(nameof(PaymentFailed), new { orderId });
        }

        public IActionResult PaymentFailed(int orderId)
        {
            return View(orderId);
        }
    }
}
