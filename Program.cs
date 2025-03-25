using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SmartCondoApi.Controllers;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Auth;
using SmartCondoApi.Services.Condominium;
using SmartCondoApi.Services.Email;
using SmartCondoApi.Services.LinkGenerator;
using SmartCondoApi.Services.Message;
using SmartCondoApi.Services.Permission;
using SmartCondoApi.Services.User;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartCondoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.File("logs/SmartCondoApi.log", rollingInterval: RollingInterval.Day)
);

builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<SmartCondoContext>()
.AddDefaultTokenProviders();

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddScoped<IAuthDependencies, AuthDependencies>();
builder.Services.AddScoped<IAuthService>(provider =>
{
    var userDependencies = provider.GetRequiredService<IAuthDependencies>();

    return new AuthService(userDependencies);
});

builder.Services.AddScoped<IUserProfileServiceDependencies, UserProfileServiceDependencies>();
builder.Services.AddScoped<IUserProfileService>(provider =>
{
    var userProfileDependencies = provider.GetRequiredService<IUserProfileServiceDependencies>();

    return new UserProfileService(userProfileDependencies);
});

builder.Services.AddScoped<ILinkGeneratorService, LinkGeneratorService>();

builder.Services.AddScoped<IEmailService>(email => {
    var configuration = email.GetRequiredService<IConfiguration>();
    return new EmailService(configuration);
});

builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddRouting();

builder.Services.AddScoped<IUserProfileControllerDependencies>(provider =>
{
    var userProfileService = provider.GetRequiredService<IUserProfileService>();
    var linkGeneratorService = provider.GetRequiredService<ILinkGeneratorService>();
    var emailService = provider.GetRequiredService<IEmailService>();
    var emailConfService = provider.GetRequiredService<IEmailConfirmationService>();

    return new UserProfileControllerDependencies(userProfileService, linkGeneratorService, emailService, emailConfService);
});

builder.Services.AddScoped<ICondominiumService, CondominiumService>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new OpenApiInfo { Title = "API com JWT", Version = "v1" });

options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "Insira o token JWT no formato: Bearer {token}",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
});

options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });
    options.AddPolicy("ProductionCorsPolicy", policy =>
    {
        // Permitir origens específicas
        policy.WithOrigins("https://www.smartcondocli.com", "https://smartcondohub.com", "https://smartcondoapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cookies ou autenticação
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limite de tamanho em MB
});

builder.Services.AddScoped<PermissionService>();

builder.Services.AddScoped<IMessageService, MessageService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
    await SmartCondoContext.SeedPermissionsAsync(roleManager);
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAllOrigins");

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProductionCorsPolicy");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();