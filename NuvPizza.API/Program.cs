using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NuvPizza.API.HealthChecks;
using NuvPizza.API.Hubs;
using NuvPizza.API.Middlewares;
using NuvPizza.API.Services;
using NuvPizza.API.Workers;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Application.Mappings;
using NuvPizza.Application.Services;
using NuvPizza.Application.Validator;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;
using NuvPizza.Infrastructure.Repositories;
using NuvPizza.Infrastructure.Services;
using Serilog;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nuvpizza-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Garante que o parsing de decimais SEMPRE use ponto como separador,
    // independente do idioma/cultura do servidor (Linux, Mac, Windows)
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // Redis Cache Configuration
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
    
    // Configura o IDistributedCache para usar Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "NuvPizza_";
    });

    // Registra o ConnectionMultiplexer como Singleton para podermos rodar comandos raw do Redis (ex: apagar por prefixo)
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
        ConnectionMultiplexer.Connect(redisConnectionString));

    builder.Services.AddIdentity<Usuario, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    
    builder.Services.AddSignalR();
    builder.Services.AddHostedService<LojaWorkers>();

    builder.Services.AddHealthChecks()
        .AddRedis(name: "Redis", redisConnectionString: redisConnectionString)
        .AddSqlite(name: "Sql", connectionString: connectionString)
        .AddCheck<ViaCepHealthCheck>("ViaCep")
        .AddCheck<MercadoPagoHealthCheck>("MercadoPago");
    
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

    builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Services.AddControllers()
        .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("X-Pagination"); // Permite que o JS leia o header de paginação
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "NuvPizza.API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT desta maneira: Bearer {seu token aqui}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                new string[] { }
            }
        });
    });
    builder.Services.AddRateLimiter(rateLimitOptions =>
    {
        rateLimitOptions.AddPolicy("CheckoutLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        rateLimitOptions.AddPolicy("LoginLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(5),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
        
        rateLimitOptions.AddPolicy("PublicApiLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
        
        rateLimitOptions.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var caminhoRequisicao = context.HttpContext.Request.Path.Value?.ToLower() ?? "";

            if (caminhoRequisicao.Contains("/login") || caminhoRequisicao.Contains("auth"))
            {
                await context.HttpContext.Response.WriteAsync("Muitas tentativas de login. Aguarde 5 minutos.");
            }
            else if (caminhoRequisicao.Contains("/pedido"))
            {
                await context.HttpContext.Response.WriteAsync("Muitos pedidos foram feitos. Tente novamente em 1 minuto.");
            }
            else
            {
                await context.HttpContext.Response.WriteAsync("Muitas requisições. Tente novamente mais tarde.");
            }
        };
    });

builder.Services.AddHttpClient<ViaCepService>(client => { client.Timeout = TimeSpan.FromSeconds(5); });

    builder.Services.AddAutoMapper(typeof(ProdutoMappingProfile).Assembly);
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<PedidoValidator>();

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IItemPedidoRepository, ItemPedidoRepository>();
    builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
    builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
    builder.Services.AddScoped<IBairroRepository, BairroRepository>();
    builder.Services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
    builder.Services.AddScoped<ICupomRepository, CupomRepository>();

    builder.Services.AddScoped<IProdutoService, ProdutoService>();
    builder.Services.AddScoped<IPedidoService, PedidoService>();
    builder.Services.AddScoped<IWhatsappService, WhatsappService>();
    builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
    builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
    builder.Services.AddScoped<IPagamentoService, MercadoPagoService>();
    builder.Services.AddScoped<IEmailService, GmailService>();
    builder.Services.AddScoped<IViaCepService, ViaCepService>();
    builder.Services.AddScoped<IFaturamentoService, FaturamentoService>();
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddScoped<ICupomService, CupomService>();
    builder.Services.AddScoped<TokenService>();

    var app = builder.Build();

    // Security Headers Middleware (adicionado antes de qualquer roteamento ou CORS)
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // HSTS somente em Produção (HTTPs)
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        await next();
    });

    // CORS deve ser o PRIMEIRO middleware de controle de fluxo para garantir que TODAS as respostas
    // (incluindo erros 401, 500, etc.) tenham os headers CORS válidos.
    app.UseCors("DevelopmentCors");

    app.UseRouting();
    app.UseRateLimiter();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            // Aplica as migrações automaticamente no banco de dados (Hostinger)
            context.Database.Migrate();

            var userManager = services.GetRequiredService<UserManager<Usuario>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            DbInitializer.SeedUsers(userManager, configuration).Wait();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocorreu um erro ao criar o utilizador Admin.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "NuvPizza.API v1"); });
    }

    //app.UseHttpsRedirection();
    
    app.UseStaticFiles();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<NotificacaoHub>("/notificacao");
    
    app.MapControllers();
    app.UseHealthChecks("/health/details", new HealthCheckOptions
    {
        ResponseWriter = async (httpContext, report) =>
        {
            httpContext.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration,
                components = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    duration = entry.Value.Duration,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message
                })
            });
            await httpContext.Response.WriteAsync(json);
        }
    });
    app.MapFallbackToFile("index.html");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}