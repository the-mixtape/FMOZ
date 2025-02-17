using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Utils
{
    public class Helper
    {
        public static int GetPedModelHashFromPlayer()
        {
            int playerPed = PlayerPedId();
            return GetEntityModel(playerPed);
        }
    }
}