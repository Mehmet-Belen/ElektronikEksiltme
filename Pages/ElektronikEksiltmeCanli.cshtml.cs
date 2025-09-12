using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.Models;
using System.Text.Json;

public class ElektronikEksiltmeCanliModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuctionSettingsService _settingsService;

    public ElektronikEksiltmeCanliModel(AppDbContext db, IAuctionSettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }
    [BindProperty(SupportsGet = true, Name = "ikn")]
    public string IKN { get; set; } = string.Empty;

    public IhaleSimple Ihale { get; set; } = new();
    public DateTime RoundEnd { get; set; }
    public DateTime TenderEnd { get; set; }
    public int CurrentRank { get; set; }

    [BindProperty]
    public List<OfferItem> Items { get; set; } = new();

    public int TotalRounds { get; set; }

    [BindProperty]
    public int CurrentRound { get; set; }

    [BindProperty]
    public string? ActionName { get; set; }

    public decimal GrandTotal => Items.Sum(i => i.LineTotal);
    public decimal DecrementTotal => Items.Sum(i => (i.PreviousUnitPrice - i.ReOfferUnitPrice) * (decimal)i.Quantity);

    public decimal MinDecrementStep { get; set; }

    private string GetSessionKey() => $"EE:{IKN}:Items";
    private string GetSubmittedKey() => $"EE:{IKN}:Submitted";

    public List<SubmittedBid> SubmittedBids { get; set; } = new();


    public void OnGet()
    {
        var entity = _db.IhaleBilgileri.FirstOrDefault(x => x.IhaleKN == (string.IsNullOrWhiteSpace(IKN) ? x.IhaleKN : IKN));
        if (entity == null)
        {
            Ihale = new IhaleSimple();
            Items = new List<OfferItem>();
            return;
        }

        Ihale = new IhaleSimple
        {
            IKN = entity.IhaleKN,
            AuthorityName = entity.IdareAdi,
            TenderName = entity.IhaleAdi,
            PartName = entity.KisimAdi
        };

        TenderEnd = (entity.BitisTarihi ?? entity.BaslangicTarihi.AddHours(1));
        var settings = _settingsService.Get();
        TotalRounds = Math.Max(1, settings.RoundCount);
        CurrentRound = 1;
        var total = TenderEnd - entity.BaslangicTarihi;
        var perRound = TimeSpan.FromTicks(total.Ticks / TotalRounds);
        RoundEnd = entity.BaslangicTarihi.AddTicks(perRound.Ticks * CurrentRound);
        CurrentRank = 3;
        Items = LoadItemsFromDatabase(entity.IhaleID);

        // Load persisted items from session if any (continue live session)
        var sessionJson = HttpContext.Session.GetString(GetSessionKey());
        if (!string.IsNullOrEmpty(sessionJson))
        {
            var persisted = JsonSerializer.Deserialize<List<OfferItem>>(sessionJson) ?? new List<OfferItem>();
            if (persisted.Count == Items.Count)
            {
                Items = persisted;
            }
        }

        // Load submitted bids panel
        var subJson = HttpContext.Session.GetString(GetSubmittedKey());
        if (!string.IsNullOrEmpty(subJson))
        {
            SubmittedBids = JsonSerializer.Deserialize<List<SubmittedBid>>(subJson) ?? new List<SubmittedBid>();
        }
    }

    public IActionResult OnPost()
    {
        var entity = _db.IhaleBilgileri.FirstOrDefault(x => x.IhaleKN == (string.IsNullOrWhiteSpace(IKN) ? x.IhaleKN : IKN));
        if (entity != null)
        {
            Ihale = new IhaleSimple
            {
                IKN = entity.IhaleKN,
                AuthorityName = entity.IdareAdi,
                TenderName = entity.IhaleAdi,
                PartName = entity.KisimAdi
            };

            TenderEnd = (entity.BitisTarihi ?? entity.BaslangicTarihi.AddHours(1));
        }
        else
        {
            Ihale = new IhaleSimple();
        }

        var settings = _settingsService.Get();
        TotalRounds = Math.Max(1, settings.RoundCount);
        if (CurrentRound <= 0) CurrentRound = 1;

        // Ensure previously submitted bids are loaded so they remain visible after Next
        var submittedJsonExisting = HttpContext.Session.GetString(GetSubmittedKey());
        if (!string.IsNullOrEmpty(submittedJsonExisting))
        {
            SubmittedBids = JsonSerializer.Deserialize<List<SubmittedBid>>(submittedJsonExisting) ?? new List<SubmittedBid>();
        }

        // Defer advancing round until after items are merged and validations run
        bool requestNext = string.Equals(ActionName, "next", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(ActionName, "finish", StringComparison.OrdinalIgnoreCase))
        {
            // Redirect to summary page; keep session so summary can read it and then clear
            return RedirectToPage("/ElektronikEksiltmeOzet", new { ikn = IKN });
        }
        else if (string.Equals(ActionName, "submit", StringComparison.OrdinalIgnoreCase))
        {
            // Record submitted bid for current round
            var subJson = HttpContext.Session.GetString(GetSubmittedKey());
            if (!string.IsNullOrEmpty(subJson))
            {
                SubmittedBids = JsonSerializer.Deserialize<List<SubmittedBid>>(subJson) ?? new List<SubmittedBid>();
            }
            SubmittedBids.RemoveAll(x => x.Round == CurrentRound);
            SubmittedBids.Add(new SubmittedBid
            {
                Round = CurrentRound,
                GrandTotal = GrandTotal,
                Decrement = DecrementTotal,
                Rank = CurrentRank
            });
            HttpContext.Session.SetString(GetSubmittedKey(), JsonSerializer.Serialize(SubmittedBids));
        }

        // Recompute per-round end based on current round
        if (entity != null)
        {
            var total = TenderEnd - entity.BaslangicTarihi;
            var perRound = TimeSpan.FromTicks(total.Ticks / TotalRounds);
            RoundEnd = entity.BaslangicTarihi.AddTicks(perRound.Ticks * CurrentRound);
        }

        CurrentRank = 3;

        // Reload base items from database and merge posted re-offer prices
        if (entity != null)
        {
            var baseItems = LoadItemsFromDatabase(entity.IhaleID);
            if (Items != null && Items.Count == baseItems.Count)
            {
                for (int i = 0; i < baseItems.Count; i++)
                {
                    decimal posted = Items[i]?.ReOfferUnitPrice ?? 0m;
                    // Try to parse culture-agnostic using raw form value first
                    var raw = Request.Form[$"Items[{i}].ReOfferUnitPrice"].ToString();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        // Detect decimal separator by last occurrence; remove thousands
                        var s = raw.Trim();
                        int lastDot = s.LastIndexOf('.');
                        int lastComma = s.LastIndexOf(',');
                        if (lastDot >= 0 && lastComma >= 0)
                        {
                            bool commaIsDecimal = lastComma > lastDot;
                            char thousand = commaIsDecimal ? '.' : ',';
                            char dec = commaIsDecimal ? ',' : '.';
                            s = s.Replace(thousand.ToString(), string.Empty).Replace(dec, '.');
                        }
                        else if (lastComma >= 0)
                        {
                            s = s.Replace('.', ' ').Replace(" ", string.Empty).Replace(',', '.');
                        }
                        // else: only dot or digits
                        if (decimal.TryParse(s, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                        {
                            posted = parsed;
                        }
                        else
                        {
                            // Try Turkish culture as a fallback
                            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), out var trParsed))
                            {
                                posted = trParsed;
                            }
                        }
                    }
                    // Treat non-positive or empty as "no change" -> use previous unit price
                    if (posted <= 0m)
                    {
                        baseItems[i].ReOfferUnitPrice = baseItems[i].PreviousUnitPrice;
                    }
                    else
                    {
                        baseItems[i].ReOfferUnitPrice = posted;
                    }
                }
            }
            Items = baseItems;

            // Determine min decrement step from ihale or settings
            MinDecrementStep = entity.AsgariFark ?? settings.MinDecrementStep;

            // Validate minimum decrement on submit
            if (string.Equals(ActionName, "submit", StringComparison.OrdinalIgnoreCase))
            {
                var decrementNow = DecrementTotal;
                var minStep = Math.Max(0m, MinDecrementStep);
                if (minStep > 0m && decrementNow < minStep)
                {
                    ModelState.AddModelError(string.Empty, $"Yapılan eksiltme asgari fark aralığından (en az {minStep:N2} ₺) daha düşük olamaz.");
                    return Page();
                }
            }

            // If requested to advance, validate min decrement and then advance
            if (requestNext)
            {
                var minStep = Math.Max(0m, MinDecrementStep);
                if (minStep > 0m && DecrementTotal < minStep)
                {
                    ModelState.AddModelError(string.Empty, $"Yapılan eksiltme asgari fark aralığından (en az {minStep:N2} ₺) daha düşük olamaz.");
                    return Page();
                }

                if (CurrentRound < TotalRounds)
                {
                    CurrentRound++;
                }

                for (int i = 0; i < Items.Count; i++)
                {
                    var newPrev = Items[i].ReOfferUnitPrice;
                    Items[i].PreviousUnitPrice = newPrev;
                    Items[i].ReOfferUnitPrice = newPrev;
                }
            }

            // If this was a submit action, store the computed totals for this round now
            if (string.Equals(ActionName, "submit", StringComparison.OrdinalIgnoreCase))
            {
                var subJson2 = HttpContext.Session.GetString(GetSubmittedKey());
                if (!string.IsNullOrEmpty(subJson2))
                {
                    SubmittedBids = JsonSerializer.Deserialize<List<SubmittedBid>>(subJson2) ?? new List<SubmittedBid>();
                }
                SubmittedBids.RemoveAll(x => x.Round == CurrentRound);
                SubmittedBids.Add(new SubmittedBid
                {
                    Round = CurrentRound,
                    GrandTotal = GrandTotal,
                    Decrement = DecrementTotal,
                    Rank = CurrentRank
                });
                HttpContext.Session.SetString(GetSubmittedKey(), JsonSerializer.Serialize(SubmittedBids));
            }

            // Persist current items for the live session
            HttpContext.Session.SetString(GetSessionKey(), JsonSerializer.Serialize(Items));
        }
        return Page();
    }

    // Removed GetIhale; now pulling from database

    private List<OfferItem> LoadItemsFromDatabase(int ihaleId)
    {
        var kalemler = _db.IhaleKalemleri
            .Where(k => k.IhaleId == ihaleId)
            .OrderBy(k => k.KalemId)
            .ToList();

        var items = new List<OfferItem>();
        foreach (var k in kalemler)
        {
            items.Add(new OfferItem
            {
                ItemName = k.KalemAdi ?? string.Empty,
                Origin = k.Mensei ?? string.Empty,
                Quantity = (double)k.Miktar,
                Unit = k.Birimi ?? string.Empty,
                PreviousUnitPrice = k.OncekiBirimFiyati ?? 0m,
                ReOfferUnitPrice = k.OncekiBirimFiyati ?? 0m
            });
        }
        return items;
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

    public class SubmittedBid
    {
        public int Round { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Decrement { get; set; }
        public int Rank { get; set; }
    }
}


