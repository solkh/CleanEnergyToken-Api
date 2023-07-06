using CleanEnergyToken_Api.Data;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Google;
using System.Drawing;
using CleanEnergyToken_Api.Helper;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using Serilog;
try
{
    var builder = WebApplication.CreateBuilder(args);

    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Configuration)
                        .Enrich.FromLogContext()
                        .CreateLogger();
    builder.Host.UseSerilog(logger);


    // Add services to the container.

    builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new BigIntegerConverter()));
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(o =>
    {
        o.UseSqlServer(cs);
        o.UseOpenIddict();
    });

    builder.Services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();


    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;

        // Default Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    });

    builder.Services.AddOpenIddict()
        .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<AppDbContext>())
        // Register the OpenIddict server handler.
        .AddServer(o =>
        {
            o.SetTokenEndpointUris("connect/token")
             .SetUserinfoEndpointUris("connect/userinfo")
             .AllowPasswordFlow()
             .AllowRefreshTokenFlow()
             .AcceptAnonymousClients()
             .RegisterScopes(OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, OpenIddictConstants.Scopes.OfflineAccess)
             .UseDataProtection();
            o.AddEphemeralEncryptionKey()
             .AddEphemeralSigningKey()
             .UseAspNetCore()
             .EnableTokenEndpointPassthrough()
             .EnableUserinfoEndpointPassthrough()
             .EnableStatusCodePagesIntegration();
        })
        .AddValidation(options =>
        {
            //options.AddAudiences("faid_client");
            options.UseSystemNetHttp();
            options.UseLocalServer();
            options.UseAspNetCore();
            options.UseDataProtection();
        });
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("keys"));
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

    builder.Services.AddScoped(typeof(UserManager<AppUser>));
    builder.Services.AddScoped(typeof(SignInManager<AppUser>));
    builder.Services.AddScoped(typeof(RoleManager<IdentityRole>));
    builder.Services.AddScoped(typeof(RoleManager<IdentityRole>));

    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IForcastedDataService, ForcastedDataService>();
    builder.Services.AddScoped<IPowerProductionService, PowerProductionService>();
    builder.Services.AddScoped<IPowerConsumptionService, PowerConsumptionService>();
    builder.Services.AddScoped<IIncentiveService, IncentiveService>();
    builder.Services.AddScoped<IBlockChainService, BlockChainService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<SeedRoles>();
    builder.Services.AddScoped<SeedUsers>();

    builder.Services.AddHostedService<NotifierHostedService>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    app.UseSwagger();
    app.UseSwaggerUI();
    //}
    using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        serviceScope.ServiceProvider.GetService<AppDbContext>().Database.Migrate();
        serviceScope.ServiceProvider.EnsureSeedData().Wait();
    }
    app.UseHttpsRedirection();
    app.MapControllers();
    app.UseAuthentication();
    app.UseAuthorization();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}