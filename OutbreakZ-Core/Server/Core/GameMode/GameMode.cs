using System;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server.Core
{
    public partial class GameMode : BaseScript
    {
        private SpawnPosition GetRandomSpawnPosition()
        {
            var random = new Random();
            var randomIndex = random.Next(OutbreakZServer.ServerConfig.SpawnPositions.Count);
            return OutbreakZServer.ServerConfig.SpawnPositions[randomIndex];
        }
    }
}