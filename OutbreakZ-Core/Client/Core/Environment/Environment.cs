using CitizenFX.Core;
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
            SetDistantCarsEnabled(true);
            SetMaxWantedLevel(0);
        }
    }
}