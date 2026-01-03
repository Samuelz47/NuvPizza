namespace NuvPizza.Application.DTOs;

public class PagamentoRequestDTO
{
    public int PedidoId { get; set; }
    public decimal TransactionAmount { get; set; }
    public string Token { get; set; } // Token do cartão (vem do Brick)
    public string Description { get; set; }
    public string PaymentMethodId { get; set; } // "pix", "visa", "master"...
    public int Installments { get; set; }
    public PayerDTO Payer { get; set; } // Dados do pagador
    public string? IssuerId { get; set; } // Banco emissor (obrigatório p/ cartão)
}

public class PayerDTO
{
    public string? Email { get; set; }
    public string Phone { get; set; }
    public string FirstName { get; set; }
    public IdentificationDTO Identification { get; set; }
}

public class IdentificationDTO
{
    public string Type { get; set; } // CPF, CNPJ
    public string Number { get; set; }
}

public class PagamentoResponseDTO
{
    public long PaymentId { get; set; }
    public string Status { get; set; } // approved, pending, rejected
    public string StatusDetail { get; set; }
    public string? QrCodeBase64 { get; set; } // Só preenchido se for Pix
    public string? QrCodeCopiaCola { get; set; } // Só preenchido se for Pix
}