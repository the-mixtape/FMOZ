using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using OutbreakZCore.Client.Config;
using OutbreakZCore.Client.Core.Zombie;
using OutbreakZCore.Shared;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.GameMode
{
    public partial class GameMode : BaseScript
    {
        private SpawnPosition _spawnPosition = null;
        private bool _respawnCheckerFinished = false;
        private int _startCam = 20;

        private const int FromStartCameraToPlayerDelay = 1500; 

        public GameMode()
        {
        }

        private async Task BeginPlay()
        {
            Weather.RequestSync();
            // BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Uniq);

            Player.DeathEvent += OnPlayerDeath;
            Player.RequestRespawnEvent += OnRequestRespawn;

            // EnableStartCam();

            await Delay(ClientConfig.GameModeFirstSpawnDelayMs);
            Exports["spawnmanager"].setAutoSpawn(false);

            TriggerServerEvent("GameMode.InitPlayer");
        }

        private void EnableStartCam()
        {
            DoScreenFadeIn(10);
            SetTimecycleModifier("hud_def_blur");
            SetTimecycleModifierStrength(1);
            
            FreezeEntityPosition(GetPlayerPed(-1), true);
            
            _startCam = Utils.Game.CreateCamera(_startCam, 320, 0, 90);
            SetCamCoord(_startCam, -358.56f, -981.96f, 286.25f);
            // _startCam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", , , , false, 0);
            // SetCamActive(_startCam, true);
            // RenderScriptCams(true, false, 1, true, true);
        }

        private void DisableStartCam()
        {
            ClearTimecycleModifier();
            RenderScriptCams(false, true, FromStartCameraToPlayerDelay, true, true);
            FreezeEntityPosition(GetPlayerPed(-1), false);
            Utils.Game.DeleteCamera(_startCam);
            DestroyCam(_startCam, false);
        }

        private void OnPlayerDeath()
        {
            ZombieSpawnManager.Disable();
        }

        private void OnRequestRespawn()
        {
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Uniq);
            TriggerServerEvent("GameMode.RespawnPlayer");
        }

        private async Task StartRespawn(SpawnPosition position)
        {
            _spawnPosition = position;
            await Player.OnRespawnProcIn();

            _respawnCheckerFinished = false;
            Tick += RespawnChecker;
        }

        private async Task FinishRespawn()
        {
            await Player.OnRespawnProcOut();
            DisableStartCam();
            
            await Delay(FromStartCameraToPlayerDelay);
            
            BucketsController.SetLocalPlayerRoutingBucket(RoutingBuckets.Default);
            ZombieSpawnManager.Enable();
        }

        private async Task RespawnChecker()
        {
            if (_respawnCheckerFinished) await Delay(0);

            Vector3 targetPosition = _spawnPosition.Location;
            Vector3 playerPosition = GetEntityCoords(Game.Player.Character.Handle, true);
            float distance = Vector3.Distance(playerPosition, targetPosition);
            Debug.WriteLine($"RespawnChecker - Target: {targetPosition} Current {playerPosition} Distance: {distance}");
            if (distance < ClientConfig.GameModeSpawnThreshold)
            {
                _respawnCheckerFinished = true;
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