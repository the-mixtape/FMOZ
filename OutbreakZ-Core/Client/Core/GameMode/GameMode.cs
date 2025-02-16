using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Client.Core.Utils;

namespace OutbreakZCore.Client.Core.GameMode
{
    public partial class GameMode : BaseScript
    {
        private readonly Model _defaultModel = PedHash.Armymech01SMY; //PedHash.FreemodeMale01 
        private const int FirstSpawnDelayMs = 1000;
        
        private async Task BeginPlay()
        {
            await Delay(FirstSpawnDelayMs);
            
            Exports["spawnmanager"].setAutoSpawn(false);
            await Spawner.SpawnPlayer(_defaultModel, new Vector3(-3000.0f, -3000.0f, 0.0f), 0.0f);
            
            TriggerServerEvent("GameMode:InitPlayer");
        }
    }
}