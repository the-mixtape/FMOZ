using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public class Groups : BaseScript
    {
        private const string HumanGroup = "PLAYER_GROUP";
        private const string ZombieGroup = "ZOMBIE_GROUP";
        
        private static uint _zombieGroupHash;
        private static uint _humanGroupHash;

        public Groups()
        {
            AddRelationshipGroup(HumanGroup, ref _humanGroupHash);
            AddRelationshipGroup(ZombieGroup, ref _zombieGroupHash);
            SetRelationshipBetweenGroups(5, (uint)GetHashKey(ZombieGroup), (uint)GetHashKey(HumanGroup));
            SetRelationshipBetweenGroups(5, (uint)GetHashKey(HumanGroup), (uint)GetHashKey(ZombieGroup));
            
            Debug.WriteLine("Relationships initialized");
        }

        public static void AddPedInZombieGroup(int ped)
        {
            SetPedRelationshipGroupHash(ped, _zombieGroupHash);
        }

        public static bool IsPedInZombieGroup(int ped)
        {
            Debug.WriteLine($"Zombie hash group {_zombieGroupHash}");
            
            var groupHash = GetPedRelationshipGroupHash(ped);
            return groupHash == _zombieGroupHash;
        }
        
        public static void AddPedInHumanGroup(int ped)
        {
            SetPedRelationshipGroupHash(ped, _humanGroupHash);
        }
        
        public static bool IsPedInHumanGroup(int ped)
        {
            return IsPedInOZGroup(ped, _humanGroupHash);
        }

        private static bool IsPedInOZGroup(int ped, uint ozGroup)
        {
            if (!IsPedInGroup(ped))
            {
                return false;
            }
            
            var groupHash = GetPedRelationshipGroupHash(ped);
            return groupHash == ozGroup;
        }
    }
}