using Microsoft.AspNetCore.Mvc;
using ProjectSolution.Services.AuthAPI.Models.Dtos;
using ProjectSolution.Services.AuthAPI.Service.IService;

namespace ProjectSolution.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ResponseDto _responseDto;

        public AuthAPIController(IAuthService authService)
        {
            _authService = authService;
            _responseDto = new();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequestDto)
        {
            string message = await _authService.Register(registrationRequestDto);
            if (!string.IsNullOrEmpty(message))
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = message;
                return BadRequest(_responseDto);
            }
            return Ok(_responseDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var loginResponse = await _authService.Login(loginRequestDto);
            if (loginResponse.User == null)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "Email or Password is incorrect.";
                return BadRequest(_responseDto);
            }
            _responseDto.Result = loginResponse;
            return Ok(_responseDto);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RoleRequestDto roleRequestDto)
        {
            bool roleAssigned = await _authService.AssignRole(roleRequestDto);
            if (!roleAssigned)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "User doesn't exist.";
                return BadRequest(_responseDto);
            }
            return Ok(_responseDto);
        }
    }
}
