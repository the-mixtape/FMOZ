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
            if (args.Count < 1)
            {
                UI.ShowNotification("/weapon [Weapon Name] [Ammo (Optional)]");
                return;
            }

            string weaponName = args[0].ToString();

            int ammoCount = 999;
            if (args.Count >= 2)
            {
                ammoCount = Convert.ToInt32(args[1]);
            }

            if (!DoesEntityExist(PlayerPedId()))
            {
                UI.ShowNotification($"Ped with ID {PlayerPedId()} does not exist.");
                return;
            }

            TakeWeapon(PlayerPedId(), weaponName, ammoCount);
            await Task.FromResult(0);
        }

        public async Task OnTakeWeapons(int source, List<object> args, string rawCommand)
        {
            TakeWeapon(PlayerPedId(),"WEAPON_BAT");
            TakeWeapon(PlayerPedId(),"WEAPON_GRENADE");
            TakeWeapon(PlayerPedId(),"WEAPON_HEAVYRIFLE");
            TakeWeapon(PlayerPedId(),"WEAPON_SNIPERRIFLE");
            TakeWeapon(PlayerPedId(),"WEAPON_PISTOL");
            await Task.FromResult(0);
        }

        private void TakeWeapon(int pedId, string weaponName, int ammoCount = 999)
        {
            var weaponHash = (uint)GetHashKey(weaponName);
            GiveWeaponToPed(pedId, weaponHash, ammoCount, false, true);
            UI.ShowNotification($"Weapon {weaponName} was given to ped {pedId}");
        }
    }
}