namespace CreditApi.DTOs
{
    public class CreditoDto
    {
        public string NumeroCredito { get; set; } = null!;
        public string NumeroNfse { get; set; } = null!;
        public DateTime DataConstituicao { get; set; }
        public decimal ValorIssqn { get; set; }
        public string TipoCredito { get; set; } = null!;
        public string SimplesNacional { get; set; } = null!;
        public decimal Aliquota { get; set; }
        public decimal ValorFaturado { get; set; }
        public decimal ValorDeducao { get; set; }
        public decimal BaseCalculo { get; set; }
    }
}
