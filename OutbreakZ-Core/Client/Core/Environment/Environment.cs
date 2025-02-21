using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public class Environment : BaseScript
    {
        private const string DefaultAudioScene = "CHARACTER_CHANGE_IN_SKY_SCENE";
        private const string DefaultScenario = "LSA_Plane";

        public Environment()
        {
            SetArtificialLightsState(true);
            SetScenarioGroupEnabled(DefaultScenario, false);
            StartAudioScene(DefaultAudioScene);
            SetDistantCarsEnabled(false);
            SetMaxWantedLevel(0);
        }

        [Tick]
        private async Task EnablePvp()
        {
            NetworkSetFriendlyFireOption(true);
            await Task.FromResult(0);
        }
    }
}