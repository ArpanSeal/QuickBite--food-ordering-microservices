using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectSolution.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<OrderHeaderDto> orders = new List<OrderHeaderDto>();
            bool authenticatedUser = false;
            string userId = "";

            if (User.IsInRole(SD.RoleAdmin))
            {
                userId = "";
                authenticatedUser = true;
            }
            else if (!User.IsInRole(SD.RoleAdmin))
            {
                userId = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "";
                if (!string.IsNullOrEmpty(userId))
                {
                    authenticatedUser = true;
                }
            }

            if (authenticatedUser)
            {
                ResponseDto? response = await _orderService.GetAllOrdersAsync(userId);
                if (response != null && response.IsSuccess)
                {
                    orders = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result)!)!;
                }
            }
            return Json(new { data = orders }); // Return the orders as JSON for DataTables
        }

        public async Task<IActionResult> OrderDetail(int orderId)
        {
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "";

            ResponseDto? response = await _orderService.GetOrderByIdAsync(orderId);
            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result)!)!;
            }

            // if the user is not an admin and the order does not belong to the user, return NotFound
            if (!User.IsInRole(SD.RoleAdmin) && orderHeaderDto.UserId != userId)
            {
                return NotFound();
            }

            return View(orderHeaderDto);
        }

        [HttpPost("OrderReadyForPickup")] // No need to add route parameter /{orderId:int} in the route value just like we do for API action methods.
        public async Task<IActionResult> OrderReadyForPickup(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatusAsync(orderId, SD.StatusReadyForPickup);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Status Updated Successfully!";
                return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
            }
            return View();
        }

        [HttpPost("OrderCompleted")]
        public async Task<IActionResult> OrderCompleted(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatusAsync(orderId, SD.StatusCompleted);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Status Updated Successfully!";
                return RedirectToAction(nameof(OrderDetail), new { orderId });
            }
            return View();
        }

        [HttpPost("OrderCanceled")]
        public async Task<IActionResult> OrderCanceled(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatusAsync(orderId, SD.StatusCancelled);
            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Status Updated Successfully!";
                return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
            }
            return View();
        }
    }
}
