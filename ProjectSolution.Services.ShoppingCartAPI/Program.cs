using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProjectSolution.Services.ShoppingCartAPI.Data;
using ProjectSolution.Services.ShoppingCartAPI.Extensions;
using ProjectSolution.Services.ShoppingCartAPI.Services;
using ProjectSolution.Services.ShoppingCartAPI.Services.IService;
using ProjectSolution.Services.ShoppingCartAPI.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("ProductAPI").AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
// alternative approach to add HttpClient -
builder.Services.AddHttpClient("CouponAPI", u => u.BaseAddress = new Uri(builder.Configuration["Services:CouponAPI"]!)).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();

builder.Services.AddHttpContextAccessor(); // important for DelegatingHandler to access the current HTTP context and retrieve the JWT token for outgoing API calls
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();

// Enable Swagger to use JWT Authentication
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Correct usage for Swashbuckle 10.0+
    option.AddSecurityRequirement(document =>
    {
        OpenApiSecuritySchemeReference? schemeRef = new(JwtBearerDefaults.AuthenticationScheme, document);

        OpenApiSecurityRequirement? requirement = new()
        {
            [schemeRef] = []
        };
        return requirement;
    });
});


builder.AddAppAuthentication();
builder.Services.AddAuthorization();

//builder.Services.AddScoped<IMessageBus, MessageBus>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}