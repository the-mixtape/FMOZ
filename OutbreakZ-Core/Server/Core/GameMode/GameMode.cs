using System;
using System.Collections.Generic;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server.Core
{
    public partial class GameMode : BaseScript
    {
        private readonly Random _random = new Random();
        private int _lastSpawnIndex = 0;
        
        private SpawnPosition GetRandomSpawnPosition()
        {
            var randomIndex = _random.Next(OutbreakZServer.ServerConfig.SpawnPositions.Count);
            return OutbreakZServer.ServerConfig.SpawnPositions[randomIndex];
        }

        private SpawnPosition GetNextSpawnPosition()
        {
            _lastSpawnIndex = (_lastSpawnIndex + 1) % OutbreakZServer.ServerConfig.SpawnPositions.Count;
            return OutbreakZServer.ServerConfig.SpawnPositions[_lastSpawnIndex];
        }
    }
}