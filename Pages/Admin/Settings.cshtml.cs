using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly IAuctionSettingsService _settingsService;
        private readonly IConfiguration _configuration;

        public SettingsModel(IAuctionSettingsService settingsService, IConfiguration configuration)
        {
            _settingsService = settingsService;
            _configuration = configuration;
        }

        public string SuccessMessage { get; set; } = string.Empty;

        [BindProperty]
        public SettingsForm Form { get; set; } = new SettingsForm();

        public IActionResult OnGet()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var settings = _settingsService.Get();
            Form = new SettingsForm
            {
                ExcludeTopBidderInNextRound = settings.ExcludeTopBidderInNextRound,
                AllowUnlimitedRounds = settings.AllowUnlimitedRounds
            };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var toSave = new AuctionSettings
            {
                ExcludeTopBidderInNextRound = Form.ExcludeTopBidderInNextRound,
                AllowUnlimitedRounds = Form.AllowUnlimitedRounds
            };
            _settingsService.Save(toSave);
            SuccessMessage = "Ayarlar kaydedildi.";
            return Page();
        }

        private bool IsAdmin()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return false;

            var admins = _configuration.GetSection("AdminUsers").Get<string[]>() ?? Array.Empty<string>();
            return admins.Contains(username, StringComparer.OrdinalIgnoreCase);
        }

        public class SettingsForm
        {
            [Display(Name = "En yüksek teklifi veren bir sonraki tura katılamasın")]
            public bool ExcludeTopBidderInNextRound { get; set; }

            [Display(Name = "Bir oturum içinde sınırsız sayıda tur yapılabilsin")]
            public bool AllowUnlimitedRounds { get; set; }
        }
    }
}


