using FluentValidation;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Validator;

public class PedidoValidator : AbstractValidator<PedidoForRegistrationDTO>
{
    public PedidoValidator()
    {
        RuleFor(x => x.NomeCliente)
            .NotEmpty().WithMessage("O Nome do cliente é Obrigatório")
            .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.");
        
        RuleFor(x => x.EmailCliente)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("O formato do email é inválido.");
        
        RuleFor(x => x.TelefoneCliente)
            .NotEmpty().WithMessage("O telefone é obrigatório.")
            .Matches(@"^[1-9]{2}9?[0-9]{8}$")
            .WithMessage("O telefone deve estar no formato DDD + Número (ex: 11999998888), apenas números.");
        
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("O pedido não pode estar vazio.")
            .Must(itens => itens.Count > 0).WithMessage("Adicione pelo menos um item ao pedido.");

        // Valida cada item individualmente dentro da lista
        RuleForEach(x => x.Itens).SetValidator(new ItemPedidoValidator());
    }
}

public class ItemPedidoValidator : AbstractValidator<ItemPedidoForRegistrationDTO>
{
    public ItemPedidoValidator()
    {
        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
            
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0).WithMessage("Produto inválido.");
    }
}