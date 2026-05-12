using ProjectSolution.Services.AuthAPI.Models.Dtos;

namespace ProjectSolution.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<bool> AssignRole(RoleRequestDto roleRequestDto);
    }
}
