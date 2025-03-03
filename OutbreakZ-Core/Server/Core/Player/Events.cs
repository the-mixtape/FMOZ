using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace OutbreakZCore.Server.Core.Player
{
    public partial class Player
    {
        private void Events()
        {
            EventHandlers["Player.DealDamageToPlayer"] += new Action<CitizenFX.Core.Player, int, int>(DealDamageToPlayer);
        }

        private void DealDamageToPlayer([FromSource] CitizenFX.Core.Player source, int targetPlayerId, int damage)
        {
            var targetPlayer = Players[targetPlayerId];
            if (targetPlayer == null)
            {
                return;
            }
            TriggerClientEvent(targetPlayer, "Player.DealDamageToPlayer", damage);
        }
    }
}