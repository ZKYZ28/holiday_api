using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Holiday.Api.Contract;
using Holiday.Api.Core.Configurations;
using Holiday.Api.Core.Hubs;
using Holiday.Api.Core.Injections;
using Holiday.Api.Core.Swagger;
using Holiday.Api.Repository;
using Holiday.Api.Repository.Models;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Holiday.Api.Core.Helpers;

/**
 * Fonctions utilitaires, généralement de type static, qui va nous permettre de configurer les objets comme le WebBuilder
 * avec des paramètres passés en paramètres
 */
public static class BuilderHelper
{
    public static WebApplicationBuilder ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Get config
        var globalConf = builder.Configuration.GetSection(MainConfiguration.Section);
        var mainConfiguration = globalConf.Get<MainConfiguration>();

        
        // DB Context
        builder.Services.AddDbContext<HolidayDbContext>(options =>
        {
            options.UseSqlServer(mainConfiguration.DataBase.ConnectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly("Holiday.Api.Persistance");
                optionsBuilder.MigrationsHistoryTable("_migrations_history");
            });
        });
        
        builder.Services.AddIdentity<Participant, IdentityRole>(options =>
            {
                // Email
                options.SignIn.RequireConfirmedAccount = false;
                // Password
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

            })
            .AddEntityFrameworkStores<HolidayDbContext>()
            .AddDefaultTokenProviders(); 
        
        // JWT        
        var jwtConf = builder.Configuration.GetSection(JwtOptionsSetup.SectionName);
        var jwtConfiguration = jwtConf.Get<JwtConfiguration>();
        // Pour injecter en tant que dépendance
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        
        // Google
        var externalServiceSecton = builder.Configuration.GetSection(ExternalServicesConfiguration.Section);
        var googleSection = externalServiceSecton.GetSection(ExternalServicesConfiguration.GoogleSubSection);
        var googleConfiguration = googleSection.Get<GoogleConfiguration>();

        builder.Services.ConfigureOptions<GoogleOptionsSetup>();
        
        
        // Authentification
        builder.Services
                // Lorsqu'on mentionnera [Autorize] au dessus des contrôleurs, il s'attendra à recevoir un JWT token
                // car c'est devenu le mécanisme de connexion par défaut
            .AddAuthentication((options) =>
            {
                // Modifier les options d'authentification de base
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle((options) =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = googleConfiguration.ClientId;
                options.ClientSecret = googleConfiguration.ClientSecret;
            })
            .AddJwtBearer((options) =>
                {
                    //options.RequireHttpsMetadata = false;
                    // permet de stocker le jeton dans l'en-tête AUTHENTIFICATION
                    options.SaveToken = true;
                    // va nous permettre de définir les options pour valider que le jeton
                    // reçu est l'un que notre API a généré et non un aléatoire
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtConfiguration.Issuer,
                        ValidAudience = jwtConfiguration.Audience,
                        // Fournir la clé qui doit correspondre à ce que notre jeton a envoyé
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        // valider la durée de vie du jeton
                        ValidateLifetime = true,
                        // Pour chaque jeton que l'on reçoit, nous vérifions les informations d'identification
                        // via la clé qui nous permet de signer ou chiffrer
                        ValidateIssuerSigningKey = true
                        
                    };
                }
            );

        builder.Services.AddAuthorization();

        // Configure service
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
    
        builder.Services.AddContractServices();
        builder.Services.AddPersistanceServices();
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        //AJOUT DE SIGNALR
        builder.Services.AddSignalR();
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Holiday API",
                Description = "API portant sur la gestion de séjours dans le cadre du cours d'architectures logicielles",
                Version = "v1"
            });
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Holiday.Api.Core.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Holiday.Api.Contract.xml"));
        });
        
        
        // On dit à Swagger de prendre en considération les règles de validation Fluent
        builder.Services.AddFluentValidationRulesToSwagger();

        builder.Services.AddCors(cors =>
        {
            cors.AddDefaultPolicy(corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .WithOrigins("http://localhost:5173", "http://localhost:49430", "https://panoramix.cg.helmo.be")
                    //.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return builder;
    }
}