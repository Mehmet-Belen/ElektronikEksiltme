using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

public class ElektronikEksiltmeOturumModel : PageModel
{
    [BindProperty(SupportsGet = true, Name = "ikn")]
    public string IKN { get; set; } = string.Empty;

    public IhaleDetail Ihale { get; set; } = new();
    public List<RoundInfo> Rounds { get; set; } = new();

    public void OnGet()
    {
        // In real app, fetch by IKN. For now, mock data based on IKN or default.
        Ihale = GetSampleDetail(IKN);
        Rounds = BuildRounds(Ihale.SessionStart, Ihale.SessionEnd, 3);
    }

    private IhaleDetail GetSampleDetail(string ikn)
    {
        var start = DateTime.Now.AddMinutes(15);
        var end = start.AddHours(1);
        return new IhaleDetail
        {
            IKN = string.IsNullOrWhiteSpace(ikn) ? "2024/123456" : ikn,
            AuthorityName = "Örnek İdare Başkanlığı",
            TenderName = "Malzeme Alımı İşi",
            PartName = "Kısım 1",
            MinimumDecrement = 1000,
            SessionStart = start,
            SessionEnd = end
        };
    }

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


