using FluentValidation;
using Shared.DTOs;

namespace APIs.Gateway.Validators
{
    public class PedidoRequestDTOValidator : AbstractValidator<PedidoRequestDTO>
    {
        public PedidoRequestDTOValidator()
        {
            RuleFor(x => x.Itens)
                .NotEmpty()
                .WithMessage("O pedido deve conter ao menos um item.");

            RuleForEach(x => x.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.ProdutoId)
                    .GreaterThan(0)
                    .WithMessage("ProdutoId inválido.");

                item.RuleFor(i => i.Quantidade)
                    .InclusiveBetween(1, 1000)
                    .WithMessage("A quantidade deve estar entre 1 e 1000.");
            });
        }
    }
}