using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class IhaleKalemleri
    {
        [Key]
        public int KalemId { get; set; }

        [Required]
        public int IhaleId { get; set; }

        [Required]
        [StringLength(200)]
        public string KalemAdi { get; set; }

        [StringLength(100)]
        public string? Mensei { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Miktar { get; set; }

        [StringLength(50)]
        public string? Birimi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OncekiBirimFiyati { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OncekiKalemToplamFiyati { get; set; }

        // Navigation Properties
        [ForeignKey("IhaleId")]
        public virtual IhaleBilgileri IhaleBilgileri { get; set; }

        public virtual ICollection<OturumKalemFiyatlari> OturumKalemFiyatlari { get; set; } = new List<OturumKalemFiyatlari>();
    }
}
