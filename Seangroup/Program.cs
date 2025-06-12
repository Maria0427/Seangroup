using Microsoft.EntityFrameworkCore;
using Seangroup.Data;
using Microsoft.AspNetCore.Identity;
using Seangroup.Areas.Identity.Data;
using Yandex.Checkout.V3;
using Seangroup.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление служб
builder.Services.AddControllersWithViews();
// Регистрируем настройки YooKassaOptions
builder.Services.Configure<YooKassaOptions>(builder.Configuration.GetSection("YooKassa"));
builder.Services.AddScoped<IOrderService, OrderService>();
var client = new Yandex.Checkout.V3.Client(
      shopId: "1086547",
      secretKey: "test_bpbhDbHUboGO2RcdW_CgCX01Wi7TTslZTpkMwMwbTVQ");
// Регистрируем сервис возвратов с HttpClient
builder.Services.AddHttpClient<YooKassaService>();

// Настройка подключения к БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddDbContext<SeangroupDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
var yooConfig = builder.Configuration.GetSection("YooKassa");

// 2. Регистрация Client
builder.Services.AddSingleton(sp =>
    new Yandex.Checkout.V3.Client(
        shopId: yooConfig["ShopId"]!,
        secretKey: yooConfig["SecretKey"]!
    )
);

// 3. (Опционально) регистрация AsyncClient
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Yandex.Checkout.V3.Client>().MakeAsync()
);

// Добавление Identity с поддержкой ролей
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Поддержка ролей
    .AddEntityFrameworkStores<SeangroupDbContext>();

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});

var app = builder.Build();

// Создание ролей и администратора
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Создание администратора
    string adminEmail = "admin@seangroup.ru";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Админ",     
            LastName = "Системы",
            PhoneNumber = "+71234567890"
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Для входа/регистрации
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
