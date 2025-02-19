using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Core.Utils;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.GameMode
{
    public partial class GameMode : BaseScript
    {
        // private readonly Model _defaultModel = PedHash.Armymech01SMY; //PedHash.FreemodeMale01 
        private const int FirstSpawnDelayMs = 3000;

        public GameMode()
        {
        }

        private async Task BeginPlay()
        {
            var playerPedId = PlayerPedId();
            NetworkConcealPlayer(playerPedId, true,false);
            
            await Delay(FirstSpawnDelayMs);
            Exports["spawnmanager"].setAutoSpawn(false);
            
            // await Spawner.SpawnPlayer(_defaultModel, new Vector3(-3000.0f, -3000.0f, 0.0f), 0.0f);

            TriggerServerEvent("GameMode:InitPlayer");
        }
    }
}