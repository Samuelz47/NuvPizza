using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _uow;

    public ClienteService(IClienteRepository clienteRepository, IUnitOfWork uow)
    {
        _clienteRepository = clienteRepository;
        _uow = uow;
    }

    public async Task<Guid> EnsureClienteExistsAsync(string telefone, string nome, string? email)
    {
        var cliente = await _clienteRepository.GetByTelefoneAsync(telefone);

        if (cliente == null)
        {
            cliente = new Cliente
            {
                Nome = nome,
                Telefone = telefone,
                Email = email,
                QuantidadePedidos = 0,
                ValorTotalGasto = 0,
                DataPrimeiroPedido = DateTime.UtcNow.AddHours(-3),
                DataUltimoPedido = DateTime.UtcNow.AddHours(-3)
            };
            _clienteRepository.Create(cliente);
        }
        else
        {
            cliente.Nome = nome; // Atualiza o nome caso tenha mudado
            if (!string.IsNullOrEmpty(email)) cliente.Email = email;
            _clienteRepository.Update(cliente);
        }

        await _uow.CommitAsync();
        return cliente.Id;
    }

    public async Task AddPedidoToRankingAsync(Guid clienteId, decimal valorPedido)
    {
        var cliente = await _clienteRepository.GetAsync(c => c.Id == clienteId);
        if (cliente != null)
        {
            cliente.QuantidadePedidos++;
            cliente.ValorTotalGasto += valorPedido;
            cliente.DataUltimoPedido = DateTime.UtcNow.AddHours(-3);
            _clienteRepository.Update(cliente);
            await _uow.CommitAsync();
        }
    }

    public async Task<PagedResult<ClienteDTO>> GetRankingAsync(ClienteRankingParameters parameters)
    {
        var result = await _clienteRepository.GetRankingAsync(parameters);

        var dtos = result.Items.Select(c => new ClienteDTO
        {
            Id = c.Id,
            Nome = c.Nome,
            Telefone = c.Telefone,
            Email = c.Email,
            QuantidadePedidos = c.QuantidadePedidos,
            ValorTotalGasto = c.ValorTotalGasto,
            DataPrimeiroPedido = c.DataPrimeiroPedido,
            DataUltimoPedido = c.DataUltimoPedido
        });

        return new PagedResult<ClienteDTO>(dtos, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
