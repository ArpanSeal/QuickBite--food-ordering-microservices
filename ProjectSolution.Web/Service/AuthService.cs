using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using ProjectSolution.Web.Utility;
using static ProjectSolution.Web.Utility.SD;

namespace ProjectSolution.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseService)
        {
            this._baseService = baseService;
        }
        public async Task<ResponseDto?> AssignRoleAsync(RoleRequestDto roleRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.AuthAPIBase + "/api/auth/AssignRole",
                Data = roleRequestDto
            });
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.AuthAPIBase + "/api/auth/login",
                Data = loginRequestDto
            }, withBearerToken: false); // We are using colon (:) after withBearerToken and not equals (=) because by using = it would be interpreted as an assignment rather than specifying the parameter name explicitly.
            //In C#, the colon (:) is used for `named arguments` in a function call to avoid ambiguity with the assignment operator (=) and other language features. 
        }

        public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = ApiType.POST,
                Url = SD.AuthAPIBase + "/api/auth/register",
                Data = registrationRequestDto
            }, withBearerToken: false); // We are using colon (:) after withBearerToken to specify the parameter name explicitly.
        }
    }
}
