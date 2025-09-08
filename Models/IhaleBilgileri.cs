using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class IhaleBilgileri
    {
        [Key]
        public int IhaleID { get; set; }
        public string IhaleKN { get; set; } = string.Empty;
        public string IdareAdi { get; set; } = string.Empty;
        public string IhaleAdi { get; set; } = string.Empty;
        public string KisimAdi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public decimal? AsgariFark { get; set; }
        public int Durum { get; set; } // enum ile uyumlu
    }
}