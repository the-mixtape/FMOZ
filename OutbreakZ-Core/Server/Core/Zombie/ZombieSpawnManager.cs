using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server.Core.Zombie
{
    public class ZombieSpawnManager : BaseScript
    {
        [EventHandler("Server:ZombieSpawnManager:SpawnZombie")]
        public void OnSpawnZombie([FromSource] CitizenFX.Core.Player source, string skinName, float x, float y, float z)
        {
            
            var skinHash = (uint)GetHashKey(skinName);
            var pedHandle = CreatePed(1, skinHash, x, y, z, 0.0f, true, true);
            if (pedHandle == 0)
            {
                Debug.WriteLine($"Failed to spawn zombie: {pedHandle} ({x}, {y}, {z})");
                return;
            }
            
            Debug.WriteLine($"Zombie was spawned ({x}, {y}, {z})");
            
            // var ped = Entity.FromHandle(pedHanler);;
            // ped.State.Set("isZombie", true, true);
            
            
            // Debug.WriteLine($"Spawning zombie at {x}, {y}, {z}");
            
            // int networkId = NetworkGetNetworkIdFromEntity(ped.Handle);
            // TriggerClientEvent(source, "Client:ZombieSpawnManager:InitZombie", networkId);
        }
    }
}