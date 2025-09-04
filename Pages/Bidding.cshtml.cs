using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

public class BiddingModel : PageModel
{
    [BindProperty]
    public decimal BidAmount { get; set; }

    public List<RankingEntry> Rankings { get; set; } = new()
    {
        new RankingEntry { Rank = 1, Firm = "Firm A", Offer = 100000 },
        new RankingEntry { Rank = 2, Firm = "Firm B", Offer = 105000 }
    };

    public void OnGet()
    {
        // Load initial data if needed
    }

    public IActionResult OnPost()
    {
        // Handle bid submission logic here
        // Example: Add new bid, update rankings, etc.
        return Page();
    }

    public class RankingEntry
    {
        public int Rank { get; set; }
        public string Firm { get; set; }
        public decimal Offer { get; set; }
    }
}