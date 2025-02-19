
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public partial class Player
    {
        
        [EventHandler("onClientResourceStart")]
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            BeginPlay();
        }


        [EventHandler("Player:DealDamageToPlayer")]
        private void OnDealDamageToPlayer(int damage)
        {
            var playerPed = GetPlayerPed(-1);
            ApplyDamageToPed(playerPed, damage, false);
        }
    }
}