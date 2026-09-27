namespace VaughanBar.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Login { get; set; } = "";
        public string? Senha { get; set; } // nunca é devolvido nas respostas de API
        public string Cargo { get; set; } = ""; // "Garcom" ou "Gerente"
        public bool Ativo { get; set; } = true;
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Telefone { get; set; }
    }

    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }
        public string? Categoria { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int EstoqueMinimo { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class Mesa
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidade { get; set; }
        public string Status { get; set; } = "Livre"; // Livre | Ocupada
    }

    public class Pedido
    {
        public int Id { get; set; }
        public int? ClienteId { get; set; }
        public string? ClienteNome { get; set; }
        public int? MesaId { get; set; }
        public int? MesaNumero { get; set; }
        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public DateTime DataPedido { get; set; }
        public string Status { get; set; } = "Aberto"; // Aberto | Fechado | Cancelado
        public decimal Total { get; set; }
        public List<ItemPedido> Itens { get; set; } = new();
    }

    public class ItemPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public string? ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => Quantidade * PrecoUnitario;
    }

    public class MovimentoEstoque
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string? ProdutoNome { get; set; }
        public string TipoMovimento { get; set; } = "Entrada"; // Entrada | Saida
        public int Quantidade { get; set; }
        public DateTime DataMovimento { get; set; }
        public string? Observacao { get; set; }
    }
}
