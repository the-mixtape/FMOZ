using System.Collections.Generic;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server
{
    public static class Config
    {
        public static readonly List<SpawnPosition> SpawnsPoints = new List<SpawnPosition>
        {
            new SpawnPosition // Police Dep
            {
                Location = new Vector3(430f, -980f, 30f),
                Heading = 90.0f
            }
        };
    }
}