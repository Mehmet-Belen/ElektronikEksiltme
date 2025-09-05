using System;

namespace WebApplication1.Models
{
    public class AuctionSettings
    {
        public bool ExcludeTopBidderInNextRound { get; set; } = true;

        public bool AllowUnlimitedRounds { get; set; } = true;
    }
}


