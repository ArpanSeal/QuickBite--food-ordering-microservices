using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectSolution.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto>? products = new();
            ResponseDto apiResponse = await _productService.GetAllProductAsync();

            if (apiResponse.IsSuccess && apiResponse.Result != null)
            {
                products = JsonConvert.DeserializeObject<List<ProductDto>>(apiResponse.Result.ToString()!);
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message;
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //[HttpGet("{productId:int}")] //why not required? beacuse here we are using conventional routing not attribute routing
        /*conventional routing: 
         * app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        Here, the default route pattern includes an optional "id" parameter. and in the action method, we can directly use "productId" as a parameter without needing to specify it in the route attribute. name, pattern, url, defaults all are used for conventional routing. name is just a unique identifier for the route, pattern defines the URL structure, url is an older term used in previous versions of ASP.NET MVC for defining the route template, and defaults specify default values for route parameters.
         */

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto? product = new();
            ResponseDto apiResponse = await _productService.GetProductByIdAsync(productId);
            if (apiResponse.IsSuccess && apiResponse.Result != null)
            {
                product = JsonConvert.DeserializeObject<ProductDto>(apiResponse.Result.ToString()!);
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message;
            }

            return View(product);
        }

        [Authorize]
        [ActionName(nameof(ProductDetails))] // not needed, just to use
        [HttpPost]
        public async Task<IActionResult> ProductDetails(ProductDto productDto)
        {
            CartDto cartDto = new()
            {
                CartHeaderDto = new CartHeaderDto
                {
                    UserId = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value!,

                },
                CartDetailsListDto = new List<CartDetailsDto>
                {
                    new CartDetailsDto
                    {
                        ProductId = productDto.ProductId,
                        Count = productDto.Quantity
                    }
                }
            };

            ResponseDto? apiResponse = await _cartService.UpsertCartAsync(cartDto);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                TempData["SuccessMessage"] = "Product has been added to the shopping cart successfully!";
                return RedirectToAction(nameof(Index), "Home");
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message ?? "Something went wrong while adding the product to the shopping cart.";
            }

            return View(productDto);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
