using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace OutbreakZCore.Client.Core.Defaults
{
    public class HardCap : BaseScript
    {
        public HardCap()
        {
            Delay(1000);
            Debug.WriteLine("HARDCAP Resource constructor called");
            Tick += PlayerActivatedCheck;
        }

        public void RegisterEventHandler(string trigger, Delegate callback)
        {
            EventHandlers[trigger] += callback;
        }

        internal async Task PlayerActivatedCheck()
        {
            if (API.NetworkIsSessionStarted())
            {
                TriggerServerEvent("HardCap.PlayerActivated");
                Tick -= PlayerActivatedCheck;
            }
            await Task.FromResult(0);
        }
    }
}