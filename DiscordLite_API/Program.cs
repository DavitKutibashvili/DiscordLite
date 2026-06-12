using DiscordLite_API.Data;
using DiscordLite_API.Hubs;
using DiscordLite_API.Model;
using DiscordLite_API.Services;
using DiscordLite_API.Services.IServices;
using DiscordLite_API.Validators;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(
    builder.Configuration.GetSection("JwtSettings")["SecretKey"]!
);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Username: case-insensitive (Identity normalizes by default), min 5 chars
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.";
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7059")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // required for SignalR
    });
});
//Services
builder.Services.AddScoped<IUserValidator<User>, UsernameValidator<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IDMChatService, DMChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IPresenceService, PresenceService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "DiscordLite API",
            Version = "v1",
            Description = "DiscordLite REST API",
            Contact = new OpenApiContact
            {
                Name = "Daviti Kutibashvili",
                Email = "kutibashvili.daviti.13@gmail.com"
            }
        };
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token"
            }
        };
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
        ];
        return Task.CompletedTask;
    });
});
builder.Services.AddAutoMapper(u =>
{
    u.CreateMap<User, UserDTO>().ReverseMap();
    u.CreateMap<DirectMessageChat, DMChatDTO>()
    .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.User1UserName, opt => opt.MapFrom(src => src.User1.UserName))
    .ForMember(dest => dest.User1DisplayName, opt => opt.MapFrom(src => src.User1.DisplayName))
    .ForMember(dest => dest.User2UserName, opt => opt.MapFrom(src => src.User2.UserName))
    .ForMember(dest => dest.User2DisplayName, opt => opt.MapFrom(src => src.User2.DisplayName));
    u.CreateMap<Message, MessageDTO>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId))
    .ForMember(dest => dest.SenderUserName, opt => opt.MapFrom(src => src.Sender.UserName))
    .ForMember(dest => dest.SenderDisplayName, opt => opt.MapFrom(src => src.Sender.DisplayName))
    .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "DiscordLite API";
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = new List<string> { "Bearer" }
        };
    });
}
app.UseCors("AllowMVC");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DMChatHub>("/hubs/dmchat");
app.MapHub<PresenceHub>("/hubs/presence");

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.Run();