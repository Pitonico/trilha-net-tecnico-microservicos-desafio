namespace Shared.DTOs
{
    public class ProdutoRequestDTO
    {
        public required string Nome { get; set; }
        public string Descricao { get; set; } = default!;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }

        public override string ToString()
        {
            return $"Nome='{Nome}', Preço={Preco:C}, Estoque={QuantidadeEstoque}";
        }
    }
}