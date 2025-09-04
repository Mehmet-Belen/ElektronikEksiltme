using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

public class ElektronikEksiltmeCanliModel : PageModel
{
    [BindProperty(SupportsGet = true, Name = "ikn")]
    public string IKN { get; set; } = string.Empty;

    public IhaleSimple Ihale { get; set; } = new();
    public DateTime RoundEnd { get; set; }
    public DateTime TenderEnd { get; set; }
    public int CurrentRank { get; set; }

    [BindProperty]
    public List<OfferItem> Items { get; set; } = new();

    public decimal GrandTotal => Items.Sum(i => i.LineTotal);
    public decimal DecrementTotal => Items.Sum(i => (i.PreviousUnitPrice - i.ReOfferUnitPrice) * (decimal)i.Quantity);

    public void OnGet()
    {
        Ihale = GetIhale();
        RoundEnd = DateTime.Now.AddMinutes(10);
        TenderEnd = DateTime.Now.AddMinutes(45);
        CurrentRank = 3; // sample
        Items = GetSampleItems();
    }

    public IActionResult OnPost()
    {
        Ihale = GetIhale();
        RoundEnd = DateTime.Now.AddMinutes(10);
        TenderEnd = DateTime.Now.AddMinutes(45);
        CurrentRank = 3;
        // Items bound from form; ensure derived values consistent
        foreach (var item in Items)
        {
            if (item.ReOfferUnitPrice < 0) item.ReOfferUnitPrice = 0;
        }
        return Page();
    }

    private IhaleSimple GetIhale()
    {
        return new IhaleSimple
        {
            IKN = string.IsNullOrWhiteSpace(IKN) ? "2024/123456" : IKN,
            AuthorityName = "Örnek İdare Başkanlığı",
            TenderName = "Malzeme Alımı İşi",
            PartName = "Kısım 1",
        };
    }

    private List<OfferItem> GetSampleItems()
    {
        return new List<OfferItem>
        {
            new OfferItem{ ItemName = "Kalem 1", Origin = "TR", Quantity = 100, Unit = "Adet", PreviousUnitPrice = 150m, ReOfferUnitPrice = 145m },
            new OfferItem{ ItemName = "Kalem 2", Origin = "DE", Quantity = 50, Unit = "Adet", PreviousUnitPrice = 200m, ReOfferUnitPrice = 190m },
            new OfferItem{ ItemName = "Kalem 3", Origin = "CN", Quantity = 20, Unit = "Koli", PreviousUnitPrice = 1200m, ReOfferUnitPrice = 1150m },
        };
    }

    public class IhaleSimple
    {
        public string IKN { get; set; } = string.Empty;
        public string AuthorityName { get; set; } = string.Empty;
        public string TenderName { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
    }

    public class OfferItem
    {
        public string ItemName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal PreviousUnitPrice { get; set; }
        public decimal ReOfferUnitPrice { get; set; }
        public decimal LineTotal => ReOfferUnitPrice * (decimal)Quantity;
    }
}


