using KesarjotShop.Api.Repositories;
using kithub.api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using kithub.api.Areas.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#if DEBUG
builder.Services.AddDbContextPool<KithubDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContextDev") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

builder.Services.AddDbContextPool<KithubIdentityDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContextDev") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

#else
builder.Services.AddDbContextPool<KithubDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContext") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

builder.Services.AddDbContextPool<KithubIdentityDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KithubAPIContext") ?? throw new InvalidOperationException("Connection string 'KesarjotAPIContext' not found.")));

#endif
builder.Services.AddDefaultIdentity<KithubUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<KithubIdentityDbContext>();


builder.Services.AddScoped(sp => new HttpClient());


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

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
	app.UseCors(policy =>
    policy.SetIsOriginAllowed( origin => true)
	//policy.WithOrigins("https://localhost:7161")
	.AllowAnyMethod()
	.AllowAnyHeader()
    .AllowCredentials()
);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.Run();
