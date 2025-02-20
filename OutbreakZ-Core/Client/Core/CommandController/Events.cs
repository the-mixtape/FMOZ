using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.CommandController
{
    public partial class CommandController
    {
        
        [EventHandler("onClientResourceStart")]
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            BeginPlay();
        }
    }
}