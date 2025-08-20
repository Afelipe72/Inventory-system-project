using InventorySystemProject.Data;
using InventorySystemProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<InventorySystemProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<InventorySystemProjectContext>();
    

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Identity needs authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// access the services that we have created
// role creation
using(var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // create the roles
    var roles = new[] { "Admin", "Manager", "Member" };
    // create the roles if they don't exist when the application starts
    foreach(var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}





// assign roles
// create the admin account
using (var scope = app.Services.CreateScope())
{
    // take the user manager
    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string email = "admin@admin.com";
    string password = "Test123#";

    // if we dont find the account registered in the database
    if(await userManager.FindByEmailAsync(email) == null)
    {
        // create a new user
        var user = new ApplicationUser();
        user.UserName = email;
        user.Email = email;

        // register the new user
        await userManager.CreateAsync(user, password);
        // add that user to a specific role
        await userManager.AddToRoleAsync(user, "Admin");
    }
}






// Also map Razor Pages (for Identity UI)
app.MapRazorPages();

app.Run();
