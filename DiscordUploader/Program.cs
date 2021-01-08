using System;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using Discord.Webhook;

namespace DiscordUploader
{
    public class ConfigurationHandler
    {
        private Configuration configuration;

        public string filepath;
        public string webhook;

        public ConfigurationHandler()
        {
            if (!File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                Console.WriteLine("Config does not exist. Creating.");
                string[] lines =
                {
                    "<?xml version=\"1.0\" encoding=\"utf-8\" ?>", "<configuration>", "<appSettings>",
                    "<add key=\"filepath\" value=\"\" />", "<add key=\"webhook\" value=\"\" />",
                    "</appSettings>", "</configuration>"
                };
                System.IO.File.WriteAllLines(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, lines);
            }
            
            this.configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var setting = this.configuration.AppSettings.Settings["filepath"];
            if (setting == null)
            {
                this.configuration.AppSettings.Settings.Add("filepath", "");
                this.filepath = null;
            }
            else
                this.filepath = setting.Value;

            setting = this.configuration.AppSettings.Settings["webhook"];
            if (setting == null)
            {
                this.configuration.AppSettings.Settings.Add("webhook", "");
                this.webhook = null;
            }
            else
                this.webhook = setting.Value;
        }

        ~ConfigurationHandler()
        {
            this.configuration.AppSettings.Settings["filepath"].Value = this.filepath;
            this.configuration.AppSettings.Settings["webhook"].Value = this.webhook;
            this.configuration.Save(ConfigurationSaveMode.Modified);
        }
    }

    class Program
    {

        public async Task Upload(string comment, string filepath, string url)
        {
            using (var client = new DiscordWebhookClient(url))
            {
                await client.SendFileAsync(text: comment, filePath: filepath);
            }
        }

        static void Main(string[] args)
        {
            ConfigurationHandler configuration = new ConfigurationHandler();

            if (String.IsNullOrEmpty(configuration.filepath) || !File.Exists(configuration.filepath))
            {
                Console.WriteLine("Provide path to file you want to upload:");
                configuration.filepath = Console.ReadLine();
            }

            if (String.IsNullOrEmpty(configuration.webhook))
            {
                Console.WriteLine("Provide webhook url:");
                configuration.webhook = Console.ReadLine();
            }

            Console.WriteLine("Type in Comment:");
            string comment = Console.ReadLine();
            Console.WriteLine("Uploading...");
            new Program().Upload(comment, configuration.filepath, configuration.webhook).GetAwaiter().GetResult();
            Console.WriteLine("Complete!");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
