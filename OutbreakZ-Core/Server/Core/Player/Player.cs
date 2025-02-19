using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server.Core.Player
{
    public class Player : BaseScript
    {
        [EventHandler("ServerPlayer:DealDamageToPlayer")]
        public void DealDamageToPlayer([FromSource] CitizenFX.Core.Player source, int targetPlayerId, int damage)
        {
            var targetPlayer = Players[targetPlayerId];
            if (targetPlayer == null)
            {
                return;
            }
            TriggerClientEvent(targetPlayer, "Player:DealDamageToPlayer", damage);
            
        }
    }
}