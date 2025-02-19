using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Core.Admin.Commands;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin
{
    public partial class Admin : BaseScript
    {
        private readonly CarCommands _carCommands = new CarCommands();
        private readonly HealthCommands _healthCommands = new HealthCommands();
        private readonly WeaponCommands _weaponCommands = new WeaponCommands();
        private readonly SkinCommands _skinCommands = new SkinCommands();
        private readonly DevCommands _devCommands = new DevCommands();
        

        private void BeginPlay()
        {
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            RegisterBaseCommand("car", _carCommands.OnSpawn);
            RegisterBaseCommand("clearcars", _carCommands.OnClear);
            RegisterBaseCommand("revive", _healthCommands.OnRevive);
            RegisterBaseCommand("heal", _healthCommands.OnHeal);
            RegisterBaseCommand("weapon", _weaponCommands.OnTakeWeapon);
            RegisterBaseCommand("weapons", _weaponCommands.OnTakeWeapons);
            RegisterBaseCommand("setskin", _skinCommands.OnSetSkin);
            RegisterBaseCommand("randskin", _skinCommands.OnRandomSkin);
            RegisterBaseCommand("zombie", _devCommands.OnSpawnZombie);
        }

        private void RegisterBaseCommand(string commandName, Func<int, List<object>, string, Task> command)
        {
            RegisterCommand(commandName, new Func<int, List<object>, string, Task>(command), false);
        }
    }
}