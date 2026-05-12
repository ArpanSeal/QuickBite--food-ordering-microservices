using Microsoft.AspNetCore.Mvc;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectSolution.Web.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        /* To use multiple view components, each one requires its own corresponding folder in the Views/Shared/Components directory, and each folder should contain a Default.cshtml file.You cannot use multiple view components if they all try to use the same single Default.cshtml file in a generic Views/Shared/Components/CartCount/ folder without specific C# classes. 
         */

        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartCountViewComponent(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            string? userIdFromClaims = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdFromClaims != null)
            {
                ResponseDto? responseDto = await _cartService.GetCartCountByUserIdAsync(userIdFromClaims);
                if (responseDto != null && responseDto.IsSuccess)
                {
                    int cartCount = Convert.ToInt32(responseDto.Result); // use Convert.ToInt32 to convert the result to an integer, in case the result is not already an integer
                    return View(cartCount);
                }
            }
            return View(0); // Return 0 if user is not authenticated or if there was an error
        }
    }
}
