using KesarjotShop.Api.Repositories;
using kithub.api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using kithub.api.Areas.Identity.Data;

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

builder.Services.AddDefaultIdentity<KithubUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<KithubIdentityDbContext>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

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
