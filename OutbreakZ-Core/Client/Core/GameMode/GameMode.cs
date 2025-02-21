using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Config;
using OutbreakZCore.Shared;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.GameMode
{
    public partial class GameMode : BaseScript
    {
        private SpawnPosition _spawnPosition = new SpawnPosition();
        private bool respawnCheckerFinished = false;
        
        public GameMode()
        {
        }

        private async Task BeginPlay()
        {   
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Uniq);
            
            Player.DeathEvent += OnPlayerDeath;
            Player.RequestRespawnEvent += OnRequestRespawn;
            
            await Delay(ClientConfig.GameModeFirstSpawnDelayMs);
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
        
        private async Task StartRespawn(SpawnPosition position)
        {
            Debug.WriteLine($"StartRespawn called");
            _spawnPosition = position;
            await Player.OnRespawnProcIn();
            
            respawnCheckerFinished = false;
            Tick += RespawnChecker;
        }

        private async Task FinishRespawn()
        {
            Debug.WriteLine($"FinishRespawn called");
            await Player.OnRespawnProcOut();
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Default);
            ZombieSpawnManager.Enable();
        }

        private async Task RespawnChecker()
        {
            if (respawnCheckerFinished) await Delay(0);
            
            Vector3 targetPosition = _spawnPosition.Location;
            Vector3 playerPosition = GetEntityCoords(Game.Player.Character.Handle, true);
            float distance = Vector3.Distance(playerPosition, targetPosition);
            Debug.WriteLine($"RespawnChecker - Target: {targetPosition} Current {playerPosition} Distance: {distance}");
            if (distance < ClientConfig.GameModeSpawnThreshold)
            {
                respawnCheckerFinished = true;
                Tick -= RespawnChecker;
                await FinishRespawn();
                return;
            }
            
            ResurrectLocalPlayerInPosition(_spawnPosition);
            await Delay(ClientConfig.GameModeRespawnCheckerTickDelay);
        }


        private void ResurrectLocalPlayerInPosition(SpawnPosition position)
        {
            NetworkResurrectLocalPlayer(
                position.Location.X, position.Location.Y, position.Location.Z, 
                position.Heading, true, false);
        }
    }
}