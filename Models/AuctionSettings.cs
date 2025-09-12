using System;

namespace WebApplication1.Models
{
    public class AuctionSettings
    {
        public bool ExcludeTopBidderInNextRound { get; set; } = true;

        public bool AllowUnlimitedRounds { get; set; } = true;

        public int RoundCount { get; set; } = 1;

        // Asgari fark aralığı (en az eksiltme tutarı) - toplam teklif bazında
        public decimal MinDecrementStep { get; set; } = 0m;
    }
}


