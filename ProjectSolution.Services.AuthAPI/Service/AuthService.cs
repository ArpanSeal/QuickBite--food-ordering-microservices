using Microsoft.AspNetCore.Identity;
using ProjectSolution.Services.AuthAPI.Data;
using ProjectSolution.Services.AuthAPI.Models;
using ProjectSolution.Services.AuthAPI.Models.Dtos;
using ProjectSolution.Services.AuthAPI.Service.IService;

namespace ProjectSolution.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext appDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                Email = registrationRequestDto.Email,
                UserName = registrationRequestDto.UserName,
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber,
            };
            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    ApplicationUser _dbUser = _appDbContext.ApplicationUsers.First(u => u.NormalizedEmail == registrationRequestDto.Email.ToUpper());
                    UserDto userDto = new()
                    {
                        Id = _dbUser.Id,
                        Email = _dbUser.Email!,
                        UserName = _dbUser.UserName!,
                        Name = _dbUser.Name,
                        PhoneNumber = _dbUser.PhoneNumber
                    };
                    return "";
                }
                return result.Errors.FirstOrDefault()!.Description;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            ApplicationUser? dbUser = _appDbContext.ApplicationUsers.FirstOrDefault(u => u.Email == loginRequestDto.Email);

            if (dbUser == null)
            {
                return new() { User = null, Token = "" };
            }

            bool isValid = await _userManager.CheckPasswordAsync(dbUser, loginRequestDto.Password);

            if (!isValid)
            {
                return new() { User = null, Token = "" };
            }
            // generate jwt
            IEnumerable<string> roles = await _userManager.GetRolesAsync(dbUser);
            string jwtToken = _jwtTokenGenerator.GenerateToken(dbUser, roles);

            UserDto user = new()
            {
                Id = dbUser.Id,
                Email = dbUser.Email!,
                UserName = dbUser.UserName!,
                Name = dbUser.Name,
                PhoneNumber = dbUser.PhoneNumber,
            };
            return new() { User = user, Token = jwtToken };
        }

        public async Task<bool> AssignRole(RoleRequestDto roleRequestDto)
        {
            ApplicationUser? dbUser = _appDbContext.ApplicationUsers.FirstOrDefault(u => u.Email!.ToLower() == roleRequestDto.Email.ToLower());
            if (dbUser != null)
            {
                if (!_roleManager.RoleExistsAsync(roleRequestDto.Role).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleRequestDto.Role.ToUpper()));
                }
                await _userManager.AddToRoleAsync(dbUser, roleRequestDto.Role);
                return true;
            }
            return false;
        }
    }
}
