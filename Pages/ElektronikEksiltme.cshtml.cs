using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;

public class ElektronikEksiltmeModel : PageModel
{
    private readonly AppDbContext _db;

    public ElektronikEksiltmeModel(AppDbContext db)
    {
        _db = db;
    }
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
        AllItems = _db.IhaleBilgileri
            .Select(x => new EksiltmeItem
            {
                IKN = x.IhaleKN,
                PartName = x.KisimAdi,
                SessionDateTime = x.BaslangicTarihi,
                Status = MapDurumToStatus(x.Durum)
            })
            .ToList();

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
            if (FilterStarted) selectedStatuses.Add(EksiltmeStatus.Baslamadi);
            if (FilterEnded) selectedStatuses.Add(EksiltmeStatus.Basladi);
            if (FilterCompleted) selectedStatuses.Add(EksiltmeStatus.Tamamlandi);
            query = query.Where(x => selectedStatuses.Contains(x.Status));
        }

        return query
            .OrderByDescending(x => x.SessionDateTime)
            .ToList();
    }

    private static EksiltmeStatus MapDurumToStatus(int durum)
    {
        return durum switch
        {
            0 => EksiltmeStatus.Baslamadi,
            1 => EksiltmeStatus.Basladi,
            2 => EksiltmeStatus.Tamamlandi,
            _ => EksiltmeStatus.Baslamadi
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
            EksiltmeStatus.Baslamadi => "Başlamadı",
            EksiltmeStatus.Basladi => "Başladı",
            EksiltmeStatus.Tamamlandi => "Tamamlandı",
            _ => string.Empty
        };
    }

    public enum EksiltmeStatus
    {
        Baslamadi,
        Basladi,
        Tamamlandi
    }
}


