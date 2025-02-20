using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class ZombieCommands : BaseScript
    {
        public async Task OnSpawnZombie(int source, List<object> args, string rawCommand)
        {
            var spawnPosition = Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 20);
            ZombieSpawnManager.FindSafeSpawnLocation(spawnPosition, 0, 0);
            bool success = await ZombieSpawnManager.SpawnZombie(spawnPosition);
            string result = success ? "" : "not ";
            UI.ShowNotification($"Zombie was {result}spawned.");
        }

        public async Task OnEnableSpawner(int source, List<object> args, string rawCommand)
        {
            bool success = ZombieSpawnManager.Enable();
            if (success) UI.ShowNotification("Zombie Spawn Manager was enabled.");
            await Task.FromResult(0);
        }

        public async Task OnDisableSpawner(int source, List<object> args, string rawCommand)
        {
            bool success = ZombieSpawnManager.Disable();
            if (success) UI.ShowNotification("Zombie Spawn Manager was disabled.");
            await Task.FromResult(0);
        }

        public async Task OnShowZombieStats(int source, List<object> args, string rawCommand)
        {
            var statsMessage = ZombieSpawnManager.GetZombieStats();
            UI.ShowNotification(statsMessage);
            await Task.FromResult(0);
        }

        public async Task OnToggleZombieDebug(int source, List<object> args, string rawCommand)
        {
            ZombieSpawnManager.Variables.ToggleZombieDebug(); 
            UI.ShowNotification("Zombie Debug was toggled.");
            await Task.FromResult(0);
        }
    }
}