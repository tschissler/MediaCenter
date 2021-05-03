using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MediaCenter.Models;

namespace MediaCenter.Managers
{
    public class ConfigurationSettingsManager
    {
        private const string configFile = "settings.json";

        public async static Task WriteCurrentConfiguration(ConfigurationSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            Task.Run(() => File.WriteAllText(configFile, json));
        }

        public static ConfigurationSettings ReadLastSettings()
        {
            string json = "";
            ConfigurationSettings result=null;

            if (!File.Exists(configFile))
            {
                return null;
            }
            json = File.ReadAllText(configFile);
            try
            {
                result = JsonSerializer.Deserialize<ConfigurationSettings>(json);
            }
            catch
            {
                File.Delete(configFile);
            }

            return result;
        }
    }
}
