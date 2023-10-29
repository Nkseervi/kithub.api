using KesarjotShop.Api.Repositories;
using kithub.api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using kithub.api.Areas.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<KithubDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContext") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

builder.Services.AddDbContextPool<KithubIdentityDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContext") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

builder.Services.AddDefaultIdentity<KithubUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<KithubIdentityDbContext>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Secrets:SecurityKey"))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)

                };
            });

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

app.MapRazorPages();

app.Run();
