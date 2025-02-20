using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class PositionCommands : BaseScript
    {

        public async Task OnBlipTeleport(int source, List<object> args, string rawCommand)
        {
            int waypointBlip = GetFirstBlipInfoId(8);
            if (!DoesBlipExist(waypointBlip))
            {
                UI.ShowNotification("Waypoint Blip not exist.");
                return;
            }
            
            Vector3 waypointCoords = GetBlipCoords(waypointBlip);
            var playerPedId = PlayerPedId();
            SetEntityCoords(
                playerPedId,
                waypointCoords.X, waypointCoords.Y, waypointCoords.Z,
                false, false, false, true
            );
            
            UI.ShowNotification($"Player teleported to {waypointCoords.X}, {waypointCoords.Y}, {waypointCoords.Z}.");
            await Task.FromResult(0);
        }

        public async Task OnShowPosition(int source, List<object> args, string rawCommand)
        {
            var playerPedId = PlayerPedId();
            var position = GetEntityCoords(PlayerPedId(), true);
            var heading = GetEntityHeading(playerPedId);
            var showMsg = $"Position: {position.X}, {position.Y}, {position.Z} | {heading}.";
            Debug.WriteLine(showMsg);
            UI.ShowNotification(showMsg);
        }
    }
}