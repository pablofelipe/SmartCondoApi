using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SmartCondoApi.Controllers;
using SmartCondoApi.GraphQL;
using SmartCondoApi.GraphQL.Mutations;
using SmartCondoApi.GraphQL.Queries;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Auth;
using SmartCondoApi.Services.Condominium;
using SmartCondoApi.Services.Crypto;
using SmartCondoApi.Services.Email;
using SmartCondoApi.Services.ForgotPassword;
using SmartCondoApi.Services.LinkGenerator;
using SmartCondoApi.Services.Message;
using SmartCondoApi.Services.Permissions;
using SmartCondoApi.Services.User;
using SmartCondoApi.Services.Vehicle;

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

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
//Console.WriteLine($"jwtKey: {jwtKey}");

if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = builder.Configuration["Jwt:Key"];

    if (string.IsNullOrEmpty(jwtKey)) { 
        throw new InvalidOperationException(
            "Configure variável JWT_KEY no arquivo .env or appsettings");
    }
}

var key = Convert.FromBase64String(jwtKey);
if (key.Length < 32)
{
    throw new InvalidOperationException(
        "Configure uma JWT Key válida com pelo menos 32 caracteres no arquivo .env");
}

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

builder.Services.AddScoped<IEmailService>(email =>
{
    var configuration = email.GetRequiredService<IConfiguration>();
    return new EmailService(configuration);
});

builder.Services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();

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
        policy.WithOrigins("https://smartcondoapp.com", "http://localhost:3000")
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

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<VehicleQueries>()
    .AddTypeExtension<VehicleMutations>()
    .AddVehicleTypes()
    .AddProjections()
    .ModifyRequestOptions(options =>
    {
        options.IncludeExceptionDetails = true;
    })
    .AddErrorFilter(error =>
    {
        Console.WriteLine($"GraphQL Error: {error.Exception}");
        return error;
    });

var app = builder.Build();

if (args.Contains("migrate"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SmartCondoContext>();
    Console.WriteLine("Applying migrations...");
    context.Database.Migrate();
    Console.WriteLine("Migrations applied successfully!");
    return;
}

if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine("Seeding database...");

    // Migration applied
    var context = scope.ServiceProvider.GetRequiredService<SmartCondoContext>();
    context.Database.Migrate();

    // Agora executar os seeds
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
    await SmartCondoContext.SeedPermissionsAsync(roleManager);

    // Seed do admin
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var adminEmail = builder.Configuration["ADMIN_EMAIL"];
    var adminPassword = builder.Configuration["ADMIN_PASSWORD"];

    if (!context.Users.Any(u => u.Email == adminEmail))
    {
        var adminProfile = new UserProfile
        {
            Name = "Administrador do Sistema",
            UserTypeId = 1, // SystemAdministrator
            Address = "Celso Garcia Avenue, 1907",
            Phone1 = "5511985356026",
            RegistrationNumber = "ADM001"
        };

        context.UserProfiles.Add(adminProfile);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            Id = adminProfile.Id,
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Enabled = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "SystemAdministrator");
        }
        else
        {
            // Rollback em caso de erro
            context.UserProfiles.Remove(adminProfile);
            await context.SaveChangesAsync();
            throw new Exception($"Falha ao criar usuário: {string.Join(", ", result.Errors)}");
        }
    }

    Console.WriteLine("Database seeded successfully!");
    return;
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Fatal($"Unhandled exception: {ex}");
        throw;
    }
});

app.MapGraphQL();

app.UseHttpsRedirection();

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

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".json"] = "application/json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name == "manifest.json")
        {
            ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Type");
        }
    }
});

app.MapControllers();

app.Run();