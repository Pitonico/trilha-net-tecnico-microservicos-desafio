using System.Globalization;

namespace APIs.Estoque.Application.DTOs
{
    public class ProdutoResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Descricao { get; set; } = default!;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
    }
}