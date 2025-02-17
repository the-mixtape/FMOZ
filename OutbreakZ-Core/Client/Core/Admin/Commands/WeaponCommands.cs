using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class WeaponCommands : BaseScript
    {
        public async Task OnTakeWeapon(int source, List<object> args, string rawCommand)
        {
            if (args.Count < 2)
            {
                UI.ShowNotification("/weapon [Ped ID (local -1)] [Weapon Name] [Ammo (Optional)]");
                return;
            }

            int pedId = Convert.ToInt32(args[0]);
            string weaponName = args[1].ToString();

            if (pedId == -1)
            {
                pedId = PlayerPedId();
            }

            int ammoCount = 999;
            if (args.Count >= 3)
            {
                ammoCount = Convert.ToInt32(args[2]);
            }
                
            if (!DoesEntityExist(pedId))
            {
                UI.ShowNotification($"Ped with ID {pedId} does not exist.");
                return;
            }

            var weaponHash = (uint)GetHashKey(weaponName);
            GiveWeaponToPed(pedId, weaponHash, ammoCount, false, true);
            UI.ShowNotification($"Weapon {weaponName} was given to ped {pedId}");
            
        }
    }
}