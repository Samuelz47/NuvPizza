namespace NuvPizza.Application.DTOs;

public class UltimoEnderecoDTO
{
    public string NomeCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string? EmailCliente { get; set; }
    public string? Logradouro { get; set; }
    public int? Numero { get; set; }
    public string? BairroNome { get; set; }
    public string? PontoReferencia { get; set; }
    public string? Cep { get; set; }
}
