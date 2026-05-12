using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;

namespace ProjectSolution.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }
        public async Task<IActionResult> CouponIndex()
        {
            List<CouponDto>? list = null;
            ResponseDto? apiResponse = await _couponService.GetAllCouponsAsync();
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(apiResponse.Result)!);
            }
            else
            {
                TempData["ErrorMessage"] = apiResponse?.Message;
            }
            return View(list);
        }
        //In classic ASP.NET MVC(up to MVC 5), an action method without an explicit HTTP verb attribute (like[HttpPost] or [HttpPut]) is accessible by all HTTP verbs, including GET and POST.It does not exclusively default to[HttpGet]. 

        //However, the[HttpGet] attribute is the implicit default when you have overloaded methods with the same name, where one has[HttpPost] applied and the other has no attribute. In such a pairing, the one without an attribute effectively acts as the[HttpGet] method to avoid an ambiguous match exception. 

        //ASP.NET Core MVC vs. Classic MVC Web API

        //The behavior differs in ASP.NET Core MVC and ASP.NET Web API, especially when using attribute routing: 

        //ASP.NET Core MVC: If no HTTP verb attribute is applied, the public method is treated as an action method, but the framework's routing and action selection logic will generally try to match the HTTP method of the request. For the default routing, it often behaves as the [HttpGet] method.

        //ASP.NET Web API(classic) : In Web API, if no attribute is used, the framework uses a naming convention.If the method name starts with "Get", "Post", "Put", "Delete", etc., it supports that verb. If the name doesn't follow the convention and no attribute is present, it supports POST by default in some configurations or all verbs in others. 

        //In summary, while in classic ASP.NET MVC an action method without attributes is technically available via all verbs, it's best practice to use explicit HTTP verb attributes to clearly define which request types an action method handles.

        public IActionResult CreateCoupon()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon(CouponDto coupon)
        {
            if (ModelState.IsValid)
            {
                if (coupon.CouponCode.Length > 10)
                {
                    // Optionally, add a ModelState error or return a validation error view/message.
                    ModelState.AddModelError(nameof(coupon.CouponCode), "Coupon code must be 10 characters or less.");
                    return View(coupon);
                }
                if ((coupon.DiscountAmount).ToString().Length > 9)
                {
                    // Optionally, add a ModelState error or return a validation error view/message.
                    ModelState.AddModelError(nameof(coupon.DiscountAmount), "Discount Amount must be 9 characters or less.");
                    return View(coupon);
                }
                if ((coupon.MinAmount).ToString().Length > 6)
                {
                    // Optionally, add a ModelState error or return a validation error view/message.
                    ModelState.AddModelError(nameof(coupon.MinAmount), "Minimum Amount must be 6 characters or less.");
                    return View(coupon);
                }
                ResponseDto? response = await _couponService.CreateCoupon(coupon);
                if (response != null && response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Coupon Created Successfully!";
                    return RedirectToAction(nameof(CouponIndex), "Coupon");
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message;
                }
            }
            return View(coupon);
        }

        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            ResponseDto? response = await _couponService.GetCouponById(couponId);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Coupon Retrieved Successfully!";
                return View(JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result)!));
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCoupon(CouponDto couponDto)
        {
            ResponseDto? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Coupon Deleted Successfully!";
                return RedirectToAction(nameof(CouponIndex), "Coupon");
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message;
            }
            ViewBag.ErrorMessage = response!.Message; // Set the error message in ViewBag
            return View(couponDto);
        }
    }
}
