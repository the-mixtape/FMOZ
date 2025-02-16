using System.IO;
using CitizenFX.Core;
using OutbreakZCore.Server.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server
{
    public class OutbreakZServer : BaseScript
    {   
        public static Config ServerConfig { get; private set; } 
        
        public OutbreakZServer()
        {
            Debug.WriteLine("OutbreakZServer initialized");
            ReadConfig();
        }

        private void ReadConfig()
        {
            var filePath = Path.Combine(GetResourcePath(GetCurrentResourceName()), "server.yml");
            if (string.IsNullOrEmpty(filePath))
            {
                throw new FileNotFoundException($"The server configuration file could not be found. Config: {filePath}.");
                return;
            }
            
            var yaml = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            ServerConfig = deserializer.Deserialize<Config>(yaml);
            Debug.WriteLine("Server Config loaded");
        }
    }
}