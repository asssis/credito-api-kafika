using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditApi.Models
{
    public class Credito
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("numero_credito")]
        public string NumeroCredito { get; set; } = null!;

        [Required]
        [Column("numero_nfse")]
        public string NumeroNfse { get; set; } = null!;

        [Required]
        [Column("data_constituicao")]
        public DateTime DataConstituicao { get; set; }

        [Required]
        [Column("valor_issqn")]
        public decimal ValorIssqn { get; set; }

        [Required]
        [Column("tipo_credito")]
        public string TipoCredito { get; set; } = null!;

        [Required]
        [Column("simples_nacional")]
        public bool SimplesNacional { get; set; }

        [Required]
        [Column("aliquota")]
        public decimal Aliquota { get; set; }

        [Required]
        [Column("valor_faturado")]
        public decimal ValorFaturado { get; set; }

        [Required]
        [Column("valor_deducao")]
        public decimal ValorDeducao { get; set; }

        [Required]
        [Column("base_calculo")]
        public decimal BaseCalculo { get; set; }
    }
}
