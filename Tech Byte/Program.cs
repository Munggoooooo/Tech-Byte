using Microsoft.AspNetCore.Authentication.Cookies;
using Tech_Byte.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<MongoDBService>();

builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<CartService>();
builder.Services.AddSingleton<CartService>();

builder.Services.AddSession();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
