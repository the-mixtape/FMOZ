using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Shared;
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
        {   // NetworkConcealPlayer(playerPedId, true,false);

            Player.DeathEvent += OnPlayerDeath;
            
            await Delay(FirstSpawnDelayMs);
            Exports["spawnmanager"].setAutoSpawn(false);
            
            // await Spawner.SpawnPlayer(_defaultModel, new Vector3(-3000.0f, -3000.0f, 0.0f), 0.0f);

            TriggerServerEvent("GameMode:InitPlayer");
        }

        private void OnPlayerDeath()
        {
            ZombieSpawnManager.Disable();
        }

        // private async Task SetPlayerPosition(SpawnPosition position)
        // {
        //     var playerPedId = CitizenFX.Core.Game.PlayerPed.Handle;
        //     while (!DoesEntityExist(playerPedId))
        //     {
        //         await Delay(100);
        //         playerPedId = CitizenFX.Core.Game.PlayerPed.Handle;
        //     }
        //     
        //     SetEntityHeading(playerPedId, position.Heading);
        //     SetEntityCoords(
        //         playerPedId,
        //         position.Location.X, position.Location.Y, position.Location.Z,
        //         true, false, false, true
        //     );
        //     
        //     // NetworkConcealPlayer(playerPedId, true,false);
        //     UI.ShowNotification($"Player has spawned at {CitizenFX.Core.Game.PlayerPed.Position}.");
        //
        //     ZombieSpawnManager.Enable();
        // }
        
        private async Task RespawnPlayer(SpawnPosition position)
        {
            await Player.OnRespawnProcIn();
            
            NetworkResurrectLocalPlayer(
                position.Location.X, position.Location.Y, position.Location.Z, 
                position.Heading, true, false);
            
            await Player.OnRespawnProcOut();
            
            ZombieSpawnManager.Enable();
        }
    }
}