
using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public partial class Player
    {

        private void Events()
        {
            // Exports["spawnmanager"].spawnPlayer(SpawnPosition());

            EventHandlers["Player.DealDamageToPlayer"] += new Action<int>(OnDealDamageToPlayer);

        }
        
        [EventHandler("onClientResourceStart")]
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            BeginPlay();
        }

        private void OnDealDamageToPlayer(int damage)
        {
            var playerPed = GetPlayerPed(-1);
            ApplyDamageToPed(playerPed, damage, false);
        }
    }
}