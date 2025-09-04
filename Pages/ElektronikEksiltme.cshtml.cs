using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

public class ElektronikEksiltmeModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Query { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public bool FilterStarted { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool FilterEnded { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool FilterCompleted { get; set; }

    public List<EksiltmeItem> AllItems { get; set; } = new();
    public List<EksiltmeItem> FilteredItems { get; set; } = new();

    public void OnGet()
    {
        AllItems = GetSampleData();
        FilteredItems = ApplyFilters(AllItems);
    }

    private List<EksiltmeItem> ApplyFilters(List<EksiltmeItem> items)
    {
        IEnumerable<EksiltmeItem> query = items;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            var q = Query.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.IKN.ToLowerInvariant().Contains(q) ||
                x.PartName.ToLowerInvariant().Contains(q));
        }

        var selectedAnyStatus = FilterStarted || FilterEnded || FilterCompleted;
        if (selectedAnyStatus)
        {
            var selectedStatuses = new List<EksiltmeStatus>();
            if (FilterStarted) selectedStatuses.Add(EksiltmeStatus.Basledi);
            if (FilterEnded) selectedStatuses.Add(EksiltmeStatus.Bitti);
            if (FilterCompleted) selectedStatuses.Add(EksiltmeStatus.Tamamlandi);
            query = query.Where(x => selectedStatuses.Contains(x.Status));
        }

        return query
            .OrderByDescending(x => x.SessionDateTime)
            .ToList();
    }

    private List<EksiltmeItem> GetSampleData()
    {
        return new List<EksiltmeItem>
        {
            new EksiltmeItem{ IKN = "2024/123456", PartName = "Tıbbi Sarf Malzeme - Kısım 1", SessionDateTime = DateTime.Now.AddHours(2), Status = EksiltmeStatus.Basledi },
            new EksiltmeItem{ IKN = "2024/654321", PartName = "Bilgisayar Donanım - Kısım 2", SessionDateTime = DateTime.Now.AddDays(-1).AddHours(1), Status = EksiltmeStatus.Bitti },
            new EksiltmeItem{ IKN = "2025/111222", PartName = "Temizlik Hizmeti - Kısım 3", SessionDateTime = DateTime.Now.AddDays(-7), Status = EksiltmeStatus.Tamamlandi },
            new EksiltmeItem{ IKN = "2025/333444", PartName = "İnşaat Onarım - Kısım 4", SessionDateTime = DateTime.Now.AddHours(5), Status = EksiltmeStatus.Basledi }
        };
    }

    public class EksiltmeItem
    {
        public string IKN { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public DateTime SessionDateTime { get; set; }
        public EksiltmeStatus Status { get; set; }
        public string StatusText => Status switch
        {
            EksiltmeStatus.Basledi => "Başladı",
            EksiltmeStatus.Bitti => "Bitti",
            EksiltmeStatus.Tamamlandi => "Tamamlandı",
            _ => string.Empty
        };
    }

    public enum EksiltmeStatus
    {
        Basledi,
        Bitti,
        Tamamlandi
    }
}


