using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuvPizza.API.Hubs;
using NuvPizza.API.Middlewares;
using NuvPizza.API.Services;
using NuvPizza.API.Workers;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Mappings;
using NuvPizza.Application.Services;
using NuvPizza.Application.Validator;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;
using NuvPizza.Infrastructure.Repositories;
using NuvPizza.Infrastructure.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nuvpizza-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog();
    
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddIdentity<Usuario, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddSignalR();
    builder.Services.AddHostedService<LojaWorkers>();

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

    builder.Services.AddScoped<IProdutoService, ProdutoService>();
    builder.Services.AddScoped<IPedidoService, PedidoService>();
    builder.Services.AddScoped<IWhatsappService, WhatsappService>();
    builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
    builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
    builder.Services.AddScoped<IPagamentoService, PagamentoDummyService>();
    builder.Services.AddScoped<IEmailService, EmailDummyService>();
    builder.Services.AddScoped<IViaCepService, ViaCepService>();
    builder.Services.AddScoped<TokenService>();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
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

    app.UseHttpsRedirection();
    
    app.UseStaticFiles();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<NotificacaoHub>("/notificacao");
    
    app.MapControllers();

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