using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjectSolution.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            ResponseDto? responseDto = await _authService.LoginAsync(loginRequestDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["SuccessMessage"] = "Login Successful.";
                LoginResponseDto? loginResponseDto = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponseDto>(responseDto.Result!.ToString()!);
                if (loginResponseDto != null)
                {
                    await SignInUser(loginResponseDto);
                    _tokenProvider.SetToken(loginResponseDto.Token);
                }
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("CustomError", responseDto != null ? responseDto.Message : "Error while login.");
            return View(loginRequestDto);
        }

        //[HttpGet]
        public IActionResult Register()
        {
            List<SelectListItem> roleList = new List<SelectListItem>
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer, Value=SD.RoleCustomer}
            }; // asp-core-in-action page 486

            ViewBag.RoleList = roleList;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto? responseResult = await _authService.RegisterAsync(registrationRequestDto);
            ResponseDto? roleAssigned;
            if (responseResult != null && responseResult.IsSuccess)
            {
                if (string.IsNullOrEmpty(registrationRequestDto.Role))
                {
                    registrationRequestDto.Role = SD.RoleCustomer;
                }
                RoleRequestDto roleRequestDto = new RoleRequestDto
                {
                    Email = registrationRequestDto.Email,
                    Role = registrationRequestDto.Role
                };
                roleAssigned = await _authService.AssignRoleAsync(roleRequestDto);
                if (roleAssigned != null && roleAssigned.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Registration Successful.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError("CustomError", roleAssigned != null ? roleAssigned.Message : "Error while assigning role."); //asp-net-in-action page 492
                    //TempData["ErrorMessage"] = roleAssigned != null ? roleAssigned.Message : "Error while assigning role.";
                }
            }
            ModelState.AddModelError("CustomError", responseResult != null ? responseResult.Message : "Error while registration.");
            //TempData["ErrorMessage"] = responseResult != null ? responseResult.Message : "Error while registration.";

            //lifetime of ViewBag is limited only to the current HTTP request
            List<SelectListItem> roleList = new List<SelectListItem>
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer, Value=SD.RoleCustomer}
            }; // asp-core-in-action page 486

            ViewBag.RoleList = roleList;

            return View(registrationRequestDto);
        }

        // [HttpGet] is always optional
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        public async Task SignInUser(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(loginResponseDto.Token);


            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                token.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email)!.Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                token.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)!.Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                token.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name)!.Value));


            identity.AddClaim(new Claim(ClaimTypes.Name,
                token.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name)!.Value));
            identity.AddClaim(new Claim(ClaimTypes.Role,
               token.Claims.FirstOrDefault(u => u.Type == "role")!.Value));



            var principal = new ClaimsPrincipal(identity);
            // HttpContext can be accessed in Controller directly
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
