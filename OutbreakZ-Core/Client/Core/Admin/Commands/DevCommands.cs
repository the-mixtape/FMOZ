using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class DevCommands : BaseScript
    {
        public async Task OnSpawnZombie(int source, List<object> args, string rawCommand)
        {
            var spawnPosition = Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 20);
            await ZombieSpawnManager.SpawnZombie(spawnPosition);
        }
    }
}