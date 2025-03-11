using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HomeHubContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Pascual"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;


    options.SignIn.RequireConfirmedEmail = false;
}
).AddEntityFrameworkStores<HomeHubContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>

{ 
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration =true;

}
);

builder.Services.AddTransient<EmailSenderService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
  //pattern: "{controller=Provider}/{action=ProductsServices}/{id?}");
  //   pattern: "{controller=Customer}/{action=Index}/{id?}");
  pattern: "{controller=Account}/{action=SignIn}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var roles = new[] { "Customer", "Provider", "Admin" };

    foreach (var role in roles)
    {

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    }



}

app.Run();
