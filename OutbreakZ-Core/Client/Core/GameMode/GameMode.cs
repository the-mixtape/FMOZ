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
        {   
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Uniq);
            Player.DeathEvent += OnPlayerDeath;
            Player.RequestRespawnEvent += OnRequestRespawn;
            
            await Delay(FirstSpawnDelayMs);
            Exports["spawnmanager"].setAutoSpawn(false);

            TriggerServerEvent("GameMode:InitPlayer");
        }

        private void OnPlayerDeath()
        {
            ZombieSpawnManager.Disable();
        }

        private void OnRequestRespawn()
        {
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Uniq);
            TriggerServerEvent("GameMode:RespawnPlayer");
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
            
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Default);
            ZombieSpawnManager.Enable();
        }
    }
}