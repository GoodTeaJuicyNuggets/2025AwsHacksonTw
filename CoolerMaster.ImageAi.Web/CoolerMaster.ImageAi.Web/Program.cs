using CoolerMaster.ImageAi.Shared;
using CoolerMaster.ImageAi.Shared.Configurations;
using CoolerMaster.ImageAi.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var awsS3Config = config.GetSection("AwsS3Config").Get<AwsS3Config>() ?? throw new ArgumentOutOfRangeException("AwsS3Config is missing.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IAwsS3Client>(_ => new AwsS3Client(awsS3Config));

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


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
