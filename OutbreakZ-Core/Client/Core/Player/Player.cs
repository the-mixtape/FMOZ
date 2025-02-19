using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public partial class Player : BaseScript
    {

        private void BeginPlay()
        {
            var playerPedId = PlayerPedId();
            Groups.AddPedInHumanGroup(playerPedId);
        }
    }
}