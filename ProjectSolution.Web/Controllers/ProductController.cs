using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;

namespace ProjectSolution.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductIndex()
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

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // why is this required? - To prevent Cross-Site Request Forgery (CSRF) attacks by ensuring that the form submission is coming from the same site. Here same site means the site that served the form.
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                if (productDto.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than zero.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Product Name is required.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "Category Name is required.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.Description))
                {
                    ModelState.AddModelError("Description", "Description is required.");
                    return View(productDto);
                }
                ResponseDto apiResponse = await _productService.CreateProductAsync(productDto);
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Product created successfully.";
                    return RedirectToAction(actionName: nameof(ProductIndex), controllerName: "Product");
                }
                else
                {
                    TempData["ErrorMessage"] = apiResponse?.Message;
                }
            }
            return View(productDto); // After property binding and going through the data annotations, if there are validation errors, it will mark ModelState as invalid and return the same view with validation messages.
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int productId)
        {
            ProductDto? productDto = new();
            ResponseDto apiResponse = await _productService.GetProductByIdAsync(productId);

            if (apiResponse.IsSuccess && apiResponse.Result != null)
            {
                productDto = JsonConvert.DeserializeObject<ProductDto>(apiResponse.Result.ToString()!);
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message;
            }

            return View(productDto);
        }

        [HttpPost] //HTML form supports onlt GET and POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                if (productDto.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than zero.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Product Name is required.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "Category Name is required.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.ImageUrl))
                {
                    ModelState.AddModelError("ImageUrl", "Image URL is required.");
                    return View(productDto);
                }
                if (string.IsNullOrWhiteSpace(productDto.Description))
                {
                    ModelState.AddModelError("Description", "Description is required.");
                    return View(productDto);
                }
                ResponseDto apiResponse = await _productService.UpdateProductAsync(productDto);
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Product updated successfully.";
                    return RedirectToAction(actionName: nameof(ProductIndex), controllerName: "Product");
                }
                else
                {
                    TempData["ErrorMessage"] = apiResponse?.Message;
                }
                ViewBag.ErrorMessage = apiResponse!.Message;
            }
            return View(productDto);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            ProductDto? productDto = new();
            ResponseDto responseDto = await _productService.GetProductByIdAsync(productId);
            if (responseDto.Result != null && responseDto.IsSuccess)
            {
                productDto = JsonConvert.DeserializeObject<ProductDto>(responseDto.Result.ToString()!);
                return View(productDto);
            }
            else
            {
                TempData["ErrorMessage"] = responseDto?.Message;
            }

            return View(productDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(ProductDto productDto)
        {
            /* Only ProductId is sent because disabled form fields are NOT included in form submission. ProductId is sent because it’s in a hidden input, which is submitted.
            1️⃣ What actually gets sent in a form POST?

            When a form is submitted, the browser sends only these fields:

            ✅ Inputs that:

            Have a name attribute
            Are enabled
            Are inside <form>

            ❌ Inputs that are disabled are completely ignored.

            ❌ Disabled = browser skips them during POST
            ❌ MVC never receives them
            ❌ Model binder never sees them
            */
            ResponseDto apiResponse = await _productService.DeleteProductAsync(productDto.ProductId);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
                return RedirectToAction(actionName: nameof(ProductIndex), controllerName: "Product");
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message;
            }
            ViewBag.ErrorMessage = apiResponse!.Message;
            return View(productDto);
        }
    }
}


//In ASP.NET MVC, the front-end or client-side validation is primarily handled by the jQuery Validation Plugin and its unobtrusive validation library, which automatically interpret the data annotations in your models.

//When a form is submitted, the MVC framework automatically binds the input to your model and populates a ModelState object with any validation failures based on your data annotations. In your controller action, you must check the ModelState.IsValid property. If it is false, it means validation errors occurred and you should not process the data.