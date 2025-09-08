using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class OturumKalemFiyatlari
    {
        [Key]
        public int FiyatId { get; set; }

        [Required]
        public int OturumId { get; set; }

        [Required]
        public int KalemId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BirimFiyati { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? KalemToplamFiyati { get; set; }

        public DateTime GirisTarihi { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? GirenKullanici { get; set; }

        // Navigation Properties
        [ForeignKey("OturumId")]
        public virtual EksiltmeOturumlari EksiltmeOturumlari { get; set; }

        [ForeignKey("KalemId")]
        public virtual IhaleKalemleri IhaleKalemleri { get; set; }
    }
}