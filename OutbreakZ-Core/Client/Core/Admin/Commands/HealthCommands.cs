using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class HealthCommands : BaseScript
    {

        public async Task OnRevive(int source, List<object> args, string rawCommand)
        {
            int playerPed = PlayerPedId();

            if (IsPedDeadOrDying(playerPed, true))
            {
                ResurrectPed(playerPed);
                int maxHealth = GetEntityMaxHealth(playerPed);
                SetEntityHealth(playerPed, maxHealth);
                ClearPedTasksImmediately(playerPed);
            }
            
            await Task.FromResult(0);
        }

        public async Task OnHeal(int source, List<object> args, string rawCommand)
        {
            int playerPed = PlayerPedId();

            int maxHealth = GetEntityMaxHealth(playerPed);
            SetEntityHealth(playerPed, maxHealth);
            
            UI.ShowNotification($"Player {playerPed} has healed.");
            await Task.FromResult(0);
        }
    }
}