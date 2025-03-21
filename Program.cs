using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Auth;
using SmartCondoApi.Services.Email;
using SmartCondoApi.Services.User;
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

builder.Services.AddScoped<IEmailService>(email => {
    var configuration = email.GetRequiredService<IConfiguration>();
    return new EmailService(configuration);
});

builder.Services.AddScoped<IUserProfileServiceDependencies, UserProfileServiceDependencies>();
builder.Services.AddScoped(provider =>
{
    var userProfileDependencies = provider.GetRequiredService<IUserProfileServiceDependencies>();

    return new UserProfileService(userProfileDependencies);
});

builder.Services.AddScoped<IUserProfileServiceDependencies, UserProfileServiceDependencies>();
builder.Services.AddScoped(provider =>
{
    var userProfileDependencies = provider.GetRequiredService<IUserProfileServiceDependencies>();

    return new UserProfileService(userProfileDependencies);
});

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

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();