using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Mappings;
using NuvPizza.Application.Services;
using NuvPizza.Application.Validator;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;
using NuvPizza.Infrastructure.Repositories;
using NuvPizza.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Isso faz o Enum virar Texto (String) no JSON de retorno
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ViaCepService>();
builder.Services.AddAutoMapper(typeof(ProdutoMappingProfile).Assembly);
builder.Services.AddFluentValidationAutoValidation(); // Ativa a validação automática
builder.Services.AddFluentValidationClientsideAdapters(); // (Opcional) Ajuda frontends MVC
builder.Services.AddValidatorsFromAssemblyContaining<PedidoValidator>(); // Registra TODOS os validadores daquele projeto

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IItemPedidoRepository, ItemPedidoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IWhatsappService, WhatsappService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();

