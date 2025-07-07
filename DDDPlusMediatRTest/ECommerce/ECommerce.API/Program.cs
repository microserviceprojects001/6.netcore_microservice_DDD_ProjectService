

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// var serviceProvider = builder.Services.BuildServiceProvider();
// var config = serviceProvider.GetRequiredService<IConfiguration>();
// var connectionString = config.GetConnectionString("Default");
// Console.WriteLine($"Connection String: {connectionString}");

builder.Services.AddApplication();      // 封装在 Application 层扩展方法中
builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure 扩展

builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
