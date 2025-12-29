using System.Net.Http.Json;

namespace NuvPizza.Infrastructure.Services;

public class ViaCepResponse
{
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public int Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public bool Erro { get; set; }
}

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _httpClient;
    
    public ViaCepService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ViaCepResponse?> CheckAsync(string cep)
    {
        try
        {
            var cleanCep = cep.Replace("-", "").Replace(".", "").Trim();

            if (cleanCep.Length != 8) return null;

            var response = 
                await _httpClient.GetFromJsonAsync<ViaCepResponse>($"https://viacep.com.br/ws/{cleanCep}/json/");
            
            if (response is null || response.Erro) return null;
            
            return response;
        }
        catch
        {
            return null;
        }
    }
}