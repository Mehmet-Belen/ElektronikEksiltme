using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

public class ElektronikEksiltmeOzetModel : PageModel
{
    private readonly WebApplication1.Data.AppDbContext _db;

    public ElektronikEksiltmeOzetModel(WebApplication1.Data.AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true, Name = "ikn")]
    public string IKN { get; set; } = string.Empty;

    public ElektronikEksiltmeCanliModel.IhaleSimple Ihale { get; set; } = new();
    public decimal LastGrandTotal { get; set; }
    public decimal LastDecrement { get; set; }
    public int FinalRank { get; set; }

    public void OnGet()
    {
        var entity = _db.IhaleBilgileri.FirstOrDefault(x => x.IhaleKN == (string.IsNullOrWhiteSpace(IKN) ? x.IhaleKN : IKN));
        if (entity != null)
        {
            Ihale = new ElektronikEksiltmeCanliModel.IhaleSimple
            {
                IKN = entity.IhaleKN,
                AuthorityName = entity.IdareAdi,
                TenderName = entity.IhaleAdi,
                PartName = entity.KisimAdi
            };
        }

        var json = HttpContext.Session.GetString($"EE:{IKN}:Submitted");
        if (!string.IsNullOrEmpty(json))
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<ElektronikEksiltmeCanliModel.SubmittedBid>>(json) ?? new();
            if (list.Count > 0)
            {
                var last = list.OrderByDescending(x => x.Round).First();
                LastGrandTotal = last.GrandTotal;
                LastDecrement = last.Decrement;
                FinalRank = last.Rank;
            }
        }

        // Clear session after reading summary
        HttpContext.Session.Remove($"EE:{IKN}:Items");
        HttpContext.Session.Remove($"EE:{IKN}:Submitted");
    }
}


