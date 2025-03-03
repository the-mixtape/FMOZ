using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Shared;
using OutbreakZCore.Shared.Utils;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.GameMode
{
    public partial class GameMode
    {
        [EventHandler("onClientResourceStart")]
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            _ = BeginPlay();
        }

        // [EventHandler("GameMode:SetPlayerPosition")]
        // public void OnSetPlayerPosition(string data)
        // {
        //     var spawnPosition = Converter.FromJson<SpawnPosition>(data);
        //     _ = SetPlayerPosition(spawnPosition);
        // }

        [EventHandler("GameMode.RespawnPlayer")]
        public void OnRespawnPlayer(string data)
        {
            var spawnPosition = Converter.FromJson<SpawnPosition>(data);
            _ = StartRespawn(spawnPosition);
        }
    }
}