using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UMonitorWebAPI.Models;
using UMonitorWebAPI.Profiles;
using UMonitorWebAPI.Hubs;
using UMonitorWebAPI.Utility;
using System.Diagnostics;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//啟用環境變數替換功能
builder.Configuration.AddEnvironmentVariables();

// 新增 CORS 設定
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.AllowAnyHeader()                       // 允許任意 Header
              .AllowAnyMethod()                       // 允許任意 HTTP 方法
              .AllowCredentials();                    // 允許傳送憑證 (例如 cookies)
    });
});

// Add services to the container.
// 資料庫物件的 DI 注入，使用 PostgreSQL
builder.Services.AddDbContext<UneoWebContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("UneoWebDatabase")));
Console.WriteLine("資料庫物件的 DI 注入，使用 PostgreSQL：" + builder.Configuration.GetConnectionString("UneoWebDatabase"));

//AutoMapper
builder.Services.AddAutoMapper(typeof(UMonitorProfile));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = Config.Title,
        Version = Config.Version,
    });
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddSignalR();

//// authentication token
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateLifetime = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:EmailKey"]))
//    }
//});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Config.Title} V{Config.Version}");
});



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// 使用 CORS
app.UseCors("AllowSpecificOrigins");

app.MapHub<NotificationHub>("/notifyHub");

app.Run();


//Scaffold-DbContext "Host=192.9.120.29;Port=5432;Database=uneo_web;Username=postgres;Password=uccc07568009" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -Force
//Scaffold-DbContext "Host=localhost;Port=5432;Database=uneo_web;Username=postgres;Password=54156486" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -Force

