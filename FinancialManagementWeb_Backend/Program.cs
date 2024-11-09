using EntityFramework;
using Microsoft.EntityFrameworkCore;
using ProjectModel.AuthModel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using TeamManagementProject_Backend.Controllers.Hubs;
using EntityFramework.DbEntities.ReceiptComponents;
using EntityFramework.DbEntities.Chats;
using EntityFramework.DbEntities.Groups;
using EntityFramework.Repository.Chats;
using EntityFramework.Repository.Groups;
using Microsoft.Extensions.FileProviders;
using EntityFramework.Repository.Pictures;
using TeamManagementProject_Backend.Global;
using EntityFramework.DbEntities;
using TeamManagementProject_Backend.Helpers;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProjectDbContext>(opts => opts.UseSqlServer(configuration.GetConnectionString("FinancialManagement")));
builder.Services.AddTransient<IChatRepository, ChatRepository>();
builder.Services.AddTransient<IGroupRepository, GroupRepository>();
builder.Services.AddTransient<IPicturesRepository, PictureRepository>();

builder.Services.AddSignalR(e => {
    e.MaximumReceiveMessageSize = 102400000;
});

builder.Services.AddIdentity<CustomUser, IdentityRole>()
    .AddEntityFrameworkStores<ProjectDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };

    options.Events = new JwtBearerEvents 
    {
        OnMessageReceived = context => 
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && 
                path.StartsWithSegments("/api/chat")) {
                
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://192.168.1.9:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

AppFolders.Init(builder.Environment);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/api/chat");

app.UseMiddleware<EncryptionMiddleware>();

app.Run();
