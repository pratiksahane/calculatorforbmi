using bmiCalculator.Services;
using GenerativeAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

builder.Services.AddScoped<IGeminiService, GeminiService>();

var app = builder.Build();
//builder.Services.AddSingleton<GeminiService>(provider =>
//{
//    var config = provider.GetRequiredService<IConfiguration>();
//    return new GeminiService(config);
//});

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
