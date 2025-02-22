using CitizenFX.Core;

namespace OutbreakZCore.Client.Core.Zombie
{
    public class ZombieContextData
    {
        public Ped Zombie { get; private set; }
        public ZombieSettings ZombieSettings { get; private set; }
        public CitizenFX.Core.Player Target { get; set; }

        public ZombieContextData(Ped ped, ZombieSettings zombieSettings)
        {
            Zombie = ped;
            ZombieSettings = zombieSettings;
        }
    }
}