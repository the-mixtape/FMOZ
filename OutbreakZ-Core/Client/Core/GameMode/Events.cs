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

        [EventHandler("GameMode:SetPlayerPosition")]
        public void OnSetPlayerPosition(string data)
        {
            var playerPedId = PlayerPedId();
            
            var spawnPosition = Converter.FromJson<SpawnPosition>(data);
            SetEntityHeading(playerPedId, spawnPosition.Heading);
            SetEntityCoords(
                playerPedId,
                spawnPosition.Location.X, spawnPosition.Location.Y, spawnPosition.Location.Z,
                false, false, false, true
            );
            
            NetworkConcealPlayer(playerPedId, true,false);
            
            UI.ShowNotification($"Player has spawned at {Game.PlayerPed.Position}.");
        }
    }
}