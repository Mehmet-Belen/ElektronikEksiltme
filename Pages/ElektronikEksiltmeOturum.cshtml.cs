using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;

public class ElektronikEksiltmeOturumModel : PageModel
{
    private readonly AppDbContext _db;

    public ElektronikEksiltmeOturumModel(AppDbContext db)
    {
        _db = db;
    }
    [BindProperty(SupportsGet = true, Name = "ikn")]
    public string IKN { get; set; } = string.Empty;

    public IhaleDetail Ihale { get; set; } = new();
    public List<RoundInfo> Rounds { get; set; } = new();

    public void OnGet()
    {
        var entity = _db.IhaleBilgileri.FirstOrDefault(x => x.IhaleKN == (string.IsNullOrWhiteSpace(IKN) ? x.IhaleKN : IKN));
        if (entity == null)
        {
            Ihale = new IhaleDetail();
            Rounds = new List<RoundInfo>();
            return;
        }

        var start = entity.BaslangicTarihi;
        var end = entity.BitisTarihi ?? start.AddHours(1);
        Ihale = new IhaleDetail
        {
            IKN = entity.IhaleKN,
            AuthorityName = entity.IdareAdi,
            TenderName = entity.IhaleAdi,
            PartName = entity.KisimAdi,
            MinimumDecrement = (int)(entity.AsgariFark ?? 0),
            SessionStart = start,
            SessionEnd = end
        };

        Rounds = BuildRounds(Ihale.SessionStart, Ihale.SessionEnd, 3);
    }

    // Removed sample detail method; now using database

    private List<RoundInfo> BuildRounds(DateTime start, DateTime end, int roundCount)
    {
        var rounds = new List<RoundInfo>();
        if (roundCount <= 0) return rounds;
        var total = end - start;
        var perRound = TimeSpan.FromTicks(total.Ticks / roundCount);
        for (int i = 0; i < roundCount; i++)
        {
            var rStart = start.AddTicks(perRound.Ticks * i);
            var rEnd = i == roundCount - 1 ? end : rStart.Add(perRound);
            rounds.Add(new RoundInfo { Name = $"{i + 1}. Tur", Start = rStart, End = rEnd });
        }
        return rounds;
    }

    public class IhaleDetail
    {
        public string IKN { get; set; } = string.Empty;
        public string AuthorityName { get; set; } = string.Empty;
        public string TenderName { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int MinimumDecrement { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
    }

    public class RoundInfo
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}


