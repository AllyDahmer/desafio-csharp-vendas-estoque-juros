using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DesafioApp
{
    // ---------------- MODELOS ----------------

    public class Venda
    {
        public string Vendedor { get; set; } = "";
        public decimal Valor { get; set; }
    }

    public class VendasRoot
    {
        public List<Venda> Vendas { get; set; } = new();
    }

    public class ProdutoEstoque
    {
        public int CodigoProduto { get; set; }
        public string DescricaoProduto { get; set; } = "";
        public int Estoque { get; set; }
    }

    public class EstoqueRoot
    {
        public List<ProdutoEstoque> Estoque { get; set; } = new();
    }

    public class MovimentoEstoque
    {
        public Guid Id { get; set; }
        public int CodigoProduto { get; set; }
        public string DescricaoMovimentacao { get; set; } = "";
        public int Quantidade { get; set; }
        public int EstoqueFinal { get; set; }
        public DateTime DataMovimentacao { get; set; }
    }

    public class ResultadoJuros
    {
        public int DiasAtraso { get; set; }
        public decimal Juros { get; set; }
        public decimal Total { get; set; }
    }

    // ---------------- SERVIÇOS ----------------

    public static class SalesCommissionService
    {
        /// <summary>
        /// Regras:
        /// - valor < 100: 0%
        /// - 100 <= valor < 500: 1%
        /// - valor >= 500: 5%
        /// </summary>
        public static Dictionary<string, decimal> CalcularComissoes(List<Venda> vendas)
        {
            var resultado = new Dictionary<string, decimal>();

            foreach (var venda in vendas)
            {
                decimal comissao;

                if (venda.Valor < 100m)
                {
                    comissao = 0m;
                }
                else if (venda.Valor < 500m)
                {
                    comissao = venda.Valor * 0.01m;
                }
                else
                {
                    comissao = venda.Valor * 0.05m;
                }

                if (!resultado.ContainsKey(venda.Vendedor))
                {
                    resultado[venda.Vendedor] = 0m;
                }

                resultado[venda.Vendedor] += comissao;
            }

            return resultado;
        }
    }

    public class InventoryService
    {
        private readonly Dictionary<int, ProdutoEstoque> _itens;

        public InventoryService(List<ProdutoEstoque> itens)
        {
            _itens = new Dictionary<int, ProdutoEstoque>();
            foreach (var item in itens)
            {
                _itens[item.CodigoProduto] = item;
            }
        }

        /// <summary>
        /// quantidade > 0 -> entrada
        /// quantidade < 0 -> saída
        /// </summary>
        public MovimentoEstoque Movimentar(int codigoProduto, int quantidade, string descricao)
        {
            if (!_itens.ContainsKey(codigoProduto))
                throw new ArgumentException("Produto não encontrado.");

            var produto = _itens[codigoProduto];
            var estoqueAtual = produto.Estoque;
            var estoqueFinal = estoqueAtual + quantidade;

            if (estoqueFinal < 0)
                throw new InvalidOperationException("Estoque não pode ficar negativo.");

            produto.Estoque = estoqueFinal;

            return new MovimentoEstoque
            {
                Id = Guid.NewGuid(),
                CodigoProduto = codigoProduto,
                DescricaoMovimentacao = descricao,
                Quantidade = quantidade,
                EstoqueFinal = estoqueFinal,
                DataMovimentacao = DateTime.Now
            };
        }
    }

    public static class InterestService
    {
        /// <summary>
        /// Calcula juros de 2,5% ao dia sobre o valor em atraso.
        /// dataVencimento no formato "yyyy-MM-dd"
        /// </summary>
        public static ResultadoJuros CalcularJuros(decimal valor, string dataVencimento, DateTime? hoje = null)
        {
            var dataRef = hoje ?? DateTime.Today;
            var vencimento = DateTime.Parse(dataVencimento);
            var diasAtraso = (dataRef.Date - vencimento.Date).Days;

            if (diasAtraso <= 0)
            {
                return new ResultadoJuros
                {
                    DiasAtraso = 0,
                    Juros = 0m,
                    Total = valor
                };
            }

            var juros = valor * 0.025m * diasAtraso;
            var total = valor + juros;

            return new ResultadoJuros
            {
                DiasAtraso = diasAtraso,
                Juros = Math.Round(juros, 2),
                Total = Math.Round(total, 2)
            };
        }
    }

    // ---------------- PROGRAMA PRINCIPAL ----------------

    public class Program
    {
        private const string VENDAS_JSON = """
        {
          "vendas": [
            { "vendedor": "João Silva", "valor": 1200.50 },
            { "vendedor": "João Silva", "valor": 950.75 },
            { "vendedor": "João Silva", "valor": 1800.00 },
            { "vendedor": "João Silva", "valor": 1400.30 },
            { "vendedor": "João Silva", "valor": 1100.90 },
            { "vendedor": "João Silva", "valor": 1550.00 },
            { "vendedor": "João Silva", "valor": 1700.80 },
            { "vendedor": "João Silva", "valor": 250.30 },
            { "vendedor": "João Silva", "valor": 480.75 },
            { "vendedor": "João Silva", "valor": 320.40 },

            { "vendedor": "Maria Souza", "valor": 2100.40 },
            { "vendedor": "Maria Souza", "valor": 1350.60 },
            { "vendedor": "Maria Souza", "valor": 950.20 },
            { "vendedor": "Maria Souza", "valor": 1600.75 },
            { "vendedor": "Maria Souza", "valor": 1750.00 },
            { "vendedor": "Maria Souza", "valor": 1450.90 },
            { "vendedor": "Maria Souza", "valor": 400.50 },
            { "vendedor": "Maria Souza", "valor": 180.20 },
            { "vendedor": "Maria Souza", "valor": 90.75 },

            { "vendedor": "Carlos Oliveira", "valor": 800.50 },
            { "vendedor": "Carlos Oliveira", "valor": 1200.00 },
            { "vendedor": "Carlos Oliveira", "valor": 1950.30 },
            { "vendedor": "Carlos Oliveira", "valor": 1750.80 },
            { "vendedor": "Carlos Oliveira", "valor": 1300.60 },
            { "vendedor": "Carlos Oliveira", "valor": 300.40 },
            { "vendedor": "Carlos Oliveira", "valor": 500.00 },
            { "vendedor": "Carlos Oliveira", "valor": 125.75 },

            { "vendedor": "Ana Lima", "valor": 1000.00 },
            { "vendedor": "Ana Lima", "valor": 1100.50 },
            { "vendedor": "Ana Lima", "valor": 1250.75 },
            { "vendedor": "Ana Lima", "valor": 1400.20 },
            { "vendedor": "Ana Lima", "valor": 1550.90 },
            { "vendedor": "Ana Lima", "valor": 1650.00 },
            { "vendedor": "Ana Lima", "valor": 75.30 },
            { "vendedor": "Ana Lima", "valor": 420.90 },
            { "vendedor": "Ana Lima", "valor": 315.40 }
          ]
        }
        """;

        private const string ESTOQUE_JSON = """
        {
          "estoque":
          [
            {
              "codigoProduto": 101,
              "descricaoProduto": "Caneta Azul",
              "estoque": 150
            },
            {
              "codigoProduto": 102,
              "descricaoProduto": "Caderno Universitário",
              "estoque": 75
            },
            {
              "codigoProduto": 103,
              "descricaoProduto": "Borracha Branca",
              "estoque": 200
            },
            {
              "codigoProduto": 104,
              "descricaoProduto": "Lápis Preto HB",
              "estoque": 320
            },
            {
              "codigoProduto": 105,
              "descricaoProduto": "Marcador de Texto Amarelo",
              "estoque": 90
            }
          ]
        }
        """;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1) Cálculo de comissões
            var vendasRoot = JsonSerializer.Deserialize<VendasRoot>(
                VENDAS_JSON,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var comissoes = SalesCommissionService.CalcularComissoes(vendasRoot!.Vendas);

            Console.WriteLine("=== COMISSÕES POR VENDEDOR ===");
            foreach (var kvp in comissoes)
            {
                Console.WriteLine($"- {kvp.Key}: R$ {kvp.Value:F2}");
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 40));
            Console.WriteLine();

            // 2) Movimentações de estoque
            var estoqueRoot = JsonSerializer.Deserialize<EstoqueRoot>(
                ESTOQUE_JSON,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var inventoryService = new InventoryService(estoqueRoot!.Estoque);

            var mov1 = inventoryService.Movimentar(101, -10, "Saída - venda de canetas");
            var mov2 = inventoryService.Movimentar(104, 50, "Entrada - reposição de lápis");

            Console.WriteLine("=== MOVIMENTAÇÕES DE ESTOQUE ===");
            ImprimirMovimento(mov1);
            ImprimirMovimento(mov2);

            Console.WriteLine();
            Console.WriteLine(new string('-', 40));
            Console.WriteLine();

            // 3) Cálculo de juros
            var resultadoJuros = InterestService.CalcularJuros(1000m, "2025-11-10");

            Console.WriteLine("=== CÁLCULO DE JUROS ===");
            Console.WriteLine($"Dias de atraso: {resultadoJuros.DiasAtraso}");
            Console.WriteLine($"Juros: R$ {resultadoJuros.Juros:F2}");
            Console.WriteLine($"Total atualizado: R$ {resultadoJuros.Total:F2}");
        }

        private static void ImprimirMovimento(MovimentoEstoque mov)
        {
            Console.WriteLine($"ID: {mov.Id}");
            Console.WriteLine($"Produto: {mov.CodigoProduto}");
            Console.WriteLine($"Descrição: {mov.DescricaoMovimentacao}");
            Console.WriteLine($"Quantidade: {mov.Quantidade}");
            Console.WriteLine($"Estoque final: {mov.EstoqueFinal}");
            Console.WriteLine($"Data: {mov.DataMovimentacao}");
            Console.WriteLine();
        }
    }
}
