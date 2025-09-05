using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAuctionSettingsService
    {
        AuctionSettings Get();
        void Save(AuctionSettings settings);
    }

    public class AuctionSettingsService : IAuctionSettingsService
    {
        private readonly string _settingsFilePath;

        public AuctionSettingsService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _settingsFilePath = Path.Combine(dataDir, "auction_settings.json");
        }

        public AuctionSettings Get()
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaults = new AuctionSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AuctionSettings>(json);
            return settings ?? new AuctionSettings();
        }

        public void Save(AuctionSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}


