using System.Collections.Generic;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server.Core
{
    public class Config
    {
        public List<SpawnPosition> SpawnPositions { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseMigrationsPath { get; set; }
    }
}