using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public partial class Player : BaseScript
    {
        public delegate void PlayerEventHandler();
        public static event PlayerEventHandler DeathEvent;
        public static event PlayerEventHandler RequestRespawnEvent;
        
        private static bool Dead { get; set; } = false;
        private static int OnPressed { get; set; } = 0;
        private static bool InRespawnProcess { get; set; } = false;
        
        private const Control RespawnAction = Control.Pickup;
        
        private void BeginPlay()
        {
            InitializePlayerGroup();
            InitializeCrouchCrawl();

            Tick += DeathDetection;
            Tick += DeathMessage;
        }

        private void InitializePlayerGroup()
        {
            var playerPedId = PlayerPedId();
            Groups.AddPedInHumanGroup(playerPedId);   
        }

        private async Task DeathDetection()
        {
            bool prevDead = Dead;
            if (NetworkIsPlayerActive(PlayerId()))
            {
                if (IsPedFatallyInjured(PlayerPedId()) && !Dead)
                {
                    Dead = true;
                    int killer = GetPedSourceOfDeath(PlayerPedId());
                    int weaponHash = GetPedCauseOfDeath(PlayerPedId());
                    int killerId = NetworkGetPlayerIndexFromPed(killer);
                    
                    if (killer != PlayerPedId() && NetworkIsPlayerActive(killerId))
                    {
                        PlayerDeathByPlayer(killerId, weaponHash);
                    }
                    else
                    {
                        PlayerDeath(weaponHash);
                    }
                }
                else if(!IsPedFatallyInjured(PlayerPedId()))
                {
                    Dead = false;
                }
            }

            if (Dead)
            {
                if (prevDead != Dead) // do once
                {
                    OnDeath();
                }
                
                if (Game.IsControlPressed(0, RespawnAction))
                {
                    OnPressed += 1;
                    if (OnPressed > 30 && !InRespawnProcess)
                    {
                        InRespawnProcess = true;
                        RequestRespawnEvent?.Invoke();
                    }
                }
                else
                {
                    OnPressed = 0;
                }
            }
            
            await Delay(0);
        }

        private async Task DeathMessage()
        {
            if (Dead)
            {
                int scaleform = RequestScaleformMovie("instructional_buttons");
                while (!HasScaleformMovieLoaded(scaleform))
                {
                    await Delay(10);
                }

                int landId = GetCurrentLanguageId();
                var lang = Localization.GtaLangToLanguages(landId);
                
                PushScaleformMovieFunction(scaleform, "CLEAR_ALL");
                PopScaleformMovieFunctionVoid();
                
                PushScaleformMovieFunction(scaleform, "SET_DATA_SLOT");
                PushScaleformMovieFunctionParameterInt(0);
                PushScaleformMovieFunctionParameterString(GetControlInstructionalButton(0, (int)RespawnAction, 1));
                PushScaleformMovieFunctionParameterString(Localization._(lang, Localization.Keys.RespawnButton));
                PopScaleformMovieFunctionVoid();

                PushScaleformMovieFunction(scaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
                PopScaleformMovieFunctionVoid();
                
                DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);

                var youAreDeadTitle = Localization._(lang, Localization.Keys.YouAreDead);
                Utils.Game.DrawText2D(youAreDeadTitle, 0.5f, 0.5f, 1f, 4, 0, 255, 255, 255, 255);
            }
            
            await Task.FromResult(100);
        }

        private void PlayerDeathByPlayer(int killer, int weapon)
        {
        }

        private void PlayerDeath(int weapon)
        {
        }

        private static void OnDeath()
        {
            DeathEvent?.Invoke();
        }
        
        public static async Task OnRespawnProcIn()
        {
            DoScreenFadeOut(1000);
            await Delay(2000);
        }


        public static async Task OnRespawnProcOut()
        {
            InRespawnProcess = false;
            ClearPedBloodDamage(PlayerPedId());
            StopScreenEffect("DeathFailOut");
            DoScreenFadeIn(1000);
            PlaySoundFrontend(-1, "Hit", "RESPAWN_ONLINE_SOUNDSET", true);
            await Task.FromResult(0);
        }
    }
}