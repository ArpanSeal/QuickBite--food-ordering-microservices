using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ProjectSolution.Services.OrderAPI.Extensions
{
    public static class WebApplicationBuilderExtension
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            // Add authentication services here
            var secret = builder.Configuration.GetValue<string>("AuthSettings:Secret") ?? string.Empty;
            var issuer = builder.Configuration.GetValue<string>("AuthSettings:Issuer");
            var audience = builder.Configuration.GetValue<string>("AuthSettings:Audience");

            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;  //optional
                options.SaveToken = true;  // important if you want to access the token later to propagate it to other services, e.g., in a DelegatingHandler for outgoing HTTP requests to other APIs. API -> API calls. If you don't set SaveToken = true, the token won't be saved in the authentication properties, and you won't be able to retrieve it later using GetToken
                /*
                 * Without SaveTokens = true: 👉 GetTokenAsync("access_token") returns null.
                 */
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = issuer,
                    ValidAudience = audience
                };
            });
            return builder;
        }
    }
}
