using FluentValidation;
using Shared.DTOs;

namespace APIs.Gateway.Validators
{
    public class ProdutoRequestDTOValidator : AbstractValidator<ProdutoRequestDTO>
    {
        public ProdutoRequestDTOValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(250).WithMessage("O nome deve ter no máximo 250 caracteres.");

            RuleFor(x => x.Preco)
                .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

            RuleFor(x => x.QuantidadeEstoque)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque não pode ser negativa.");
        }
    }
}