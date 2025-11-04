namespace APIs.Estoque.Domain.Entities
{
    public class Produto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string Descricao { get; set; } = default!;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }

        public override string ToString()
        {
            return $"Produto(Id={Id}, Nome='{Nome}', Preço={Preco:C}, Estoque={QuantidadeEstoque})";
        }
    }
}