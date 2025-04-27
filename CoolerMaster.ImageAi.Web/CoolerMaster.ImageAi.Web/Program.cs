using CoolerMaster.ImageAi.Shared;
using CoolerMaster.ImageAi.Shared.Configurations;
using CoolerMaster.ImageAi.Shared.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var awsS3Config = config.GetSection("AwsS3Config").Get<AwsS3Config>() ?? throw new ArgumentOutOfRangeException("AwsS3Config is missing.");
var awsBedrockConfig = config.GetSection("AwsBedrockConfig").Get<AwsBedrockConfig>() ?? throw new ArgumentOutOfRangeException("AwsBedrockConfig is missing.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IAwsS3Client>(_ => new AwsS3Client(awsS3Config));
builder.Services.AddSingleton<IAwsBedrockClient>(_ => new AwsBedrockClient(awsBedrockConfig));

// Add ProductDbContext initialization
builder.Services.AddDbContext<ProductDbContext>();

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
