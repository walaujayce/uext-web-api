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

//�ҥ������ܼƴ����\��
builder.Configuration.AddEnvironmentVariables();

// �s�W CORS �]�w
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.AllowAnyHeader()                       // ���\���N Header
              .AllowAnyMethod()                       // ���\���N HTTP ��k
              .AllowCredentials();                    // ���\�ǰe���� (�Ҧp cookies)
    });
});

// Add services to the container.
// ��Ʈw���� DI �`�J�A�ϥ� PostgreSQL
builder.Services.AddDbContext<UneoWebContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("UneoWebDatabase")));
Console.WriteLine("��Ʈw���� DI �`�J�A�ϥ� PostgreSQL�G" + builder.Configuration.GetConnectionString("UneoWebDatabase"));

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

// �ϥ� CORS
app.UseCors("AllowSpecificOrigins");

app.MapHub<NotificationHub>("/notifyHub");

app.Run();


//Scaffold-DbContext "Host=192.9.120.29;Port=5432;Database=uneo_web;Username=postgres;Password=uccc07568009" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -Force
//Scaffold-DbContext "Host=localhost;Port=5432;Database=uneo_web;Username=postgres;Password=54156486" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -Force

