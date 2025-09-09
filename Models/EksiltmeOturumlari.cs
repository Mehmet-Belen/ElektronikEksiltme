using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class EksiltmeOturumlari
    {
        [Key]
        public int OturumId { get; set; }

        [Required]
        public int IhaleId { get; set; }

        [Required]
        public int OturumNo { get; set; }

        [Required]
        public DateTime OturumTarihi { get; set; }

        [Required]
        public TimeSpan OturumSaati { get; set; }

        [Required]
        public int Durum { get; set; } // 0: Hazırlanıyor, 1: Devam Ediyor, 2: Tamamlandı

        [StringLength(500)]
        public string? Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("IhaleId")]
        public virtual IhaleBilgileri IhaleBilgileri { get; set; }

        public virtual ICollection<OturumKalemFiyatlari> OturumKalemFiyatlari { get; set; } = new List<OturumKalemFiyatlari>();

        // Helper Properties
        [NotMapped]
        public string DurumText => Durum switch
        {
            0 => "Hazırlanıyor",
            1 => "Devam Ediyor",
            2 => "Tamamlandı",
            _ => "Bilinmiyor"
        };
    }
}