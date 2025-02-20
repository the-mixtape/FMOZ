using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Core.Admin.Commands;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.CommandController
{
    public partial class CommandController : BaseScript
    {
        private readonly CarCommands _carCommands = new CarCommands();
        private readonly HealthCommands _healthCommands = new HealthCommands();
        private readonly WeaponCommands _weaponCommands = new WeaponCommands();
        private readonly SkinCommands _skinCommands = new SkinCommands();
        private readonly PositionCommands _tpCommands = new PositionCommands();
        private readonly ZombieCommands _zombieCommands = new ZombieCommands();
        

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
            RegisterBaseCommand("kill", _healthCommands.OnKill);
            
            RegisterBaseCommand("weapon", _weaponCommands.OnTakeWeapon);
            RegisterBaseCommand("weapons", _weaponCommands.OnTakeWeapons);
            
            RegisterBaseCommand("setskin", _skinCommands.OnSetSkin);
            RegisterBaseCommand("randskin", _skinCommands.OnRandomSkin);
            
            RegisterBaseCommand("zombie", _zombieCommands.OnSpawnZombie);
            RegisterBaseCommand("zombiestats", _zombieCommands.OnShowZombieStats);
            RegisterBaseCommand("zombieon", _zombieCommands.OnEnableSpawner);
            RegisterBaseCommand("zombieoff", _zombieCommands.OnDisableSpawner);
            RegisterBaseCommand("zombiedebug", _zombieCommands.OnToggleZombieDebug);
            
            RegisterBaseCommand("tp", _tpCommands.OnBlipTeleport);
            RegisterBaseCommand("pos", _tpCommands.OnShowPosition);
        }

        private void RegisterBaseCommand(string commandName, Func<int, List<object>, string, Task> command)
        {
            RegisterCommand(commandName, new Func<int, List<object>, string, Task>(command), false);
        }
    }
}