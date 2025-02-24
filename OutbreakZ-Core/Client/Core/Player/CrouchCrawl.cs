using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Mono.CSharp;
using MsgPack.Serialization;
using OutbreakZCore.Shared.Utils;
using Math = System.Math;

namespace OutbreakZCore.Client.Core
{
    public partial class Player
    {
        private const bool CrouchEnabled = true;
        private const bool CrouchKeyBindEnabled = true;
        private const string CrouchKeyBind = "LCONTROL";
        private const string CrouchKeyBindDescription = "Crouch";
        private const string CrouchChatSuggestion = "Crouch";
        
        private const bool CrawlEnabled = true;
        private const bool CrawlKeyBindEnabled = true;
        private const string CrawlKeyBind = "X";
        private const string CrawlKeyBindDescription = "Crawl";
        private const string CrawlChatSuggestion = "Crawl";
        
        private const bool CrawlFlipKeyBindEnabled = true;
        private const string CrawlFlipKeyBind = "Z";
        private const string CrawlFlipKeyBindDescription = "Crawl flip";
        private const string CrawlFlipChatSuggestion = "Crawl flip";
        
        private const bool CrouchOverrideStealthMode = true;
        private const int CrouchKeypressTimer = 1000;

        private const string PedCrouchedMoveClipSet = "move_ped_crouched";
        private const string PedCrouchedStrafeClipSet = "move_ped_crouched_strafing";
        private const string DefaultAction = "DEFAULT_ACTION";

        private const string MoveCrawlAnimDict = "move_crawl";
        private const string MoveCrawlProneAnimDict = "move_crawlprone2crawlfront";

        private const string TransitionProneToKneesAnimDict = "get_up@directional@transition@prone_to_knees@crawl";
        private const string TransitionProneToKneesAnim = "front";

        private const string MovementFromKneesAnimDict = "get_up@directional@movement@from_knees@standard";
        private const string MovementFromKneesAnim = "getup_l_0";

        private const string TransitionProneToSeatedAnimDict = "get_up@directional@transition@prone_to_seated@crawl";
        private const string TransitionProneToSeatedAnim = "back";

        private const string MovementFromSeatedAnimDict = "get_up@directional@movement@from_seated@standard";
        private const string MovementFromSeatedAnim = "get_up_l_0";

        private const string CrawlFlipToBackAnimDict = "get_up@directional_sweep@combat@pistol@front";
        private const string CrawlFlipToBackAnim = "front_to_prone";

        private const string CrawlFlipToFrontAnimDict = "move_crawlprone2crawlfront";
        private const string CrawlFlipToFrontAnim = "back";

        private const string CrawlFrontLeftMoveAnimDict = "move_crawlprone2crawlfront";
        private const string CrawlFrontLeftMoveAnim = "left";

        private const string CrawlBackLeftMoveAnimDict = "get_up@directional_sweep@combat@pistol@left";
        private const string CrawlBackLeftMoveAnim = "left_to_prone";

        private const string CrawlFrontRightMoveAnimDict = "move_crawlprone2crawlfront";
        private const string CrawlFrontRightMoveAnim = "right";

        private const string CrawlBackRightMoveAnimDict = "get_up@directional_sweep@combat@pistol@right";
        private const string CrawlBackRightMoveAnim = "right_to_prone";

        private const string DiveToCrawlAnimDict = "explosions";
        private const string DiveToCrawlAnim = "react_blown_forwards";
        
        private const string CrouchToProneAnimDict = "amb@world_human_sunbathe@male@front@enter";
        private const string CrouchToProneAnim = "enter";

        private const string FrontProneType = "onfront";
        private const string BackProneType = "onback";

        private const string DirectionFwd = "fwd";
        private const string DirectionBwd = "bwd";

        private const int CrawlFrontFwdDelay = 820;
        private const int CrawlFrontBwdDelay = 990;
        private const int CrawlBackFwdDelay = 1200;
        private const int CrawlBackBwdDelay = 1200;

        private static bool _isProne = false;
        private static bool _isCrouched = false;
        private static bool _isCrawling = false;
        private static bool _inAction = false;
        private static string _proneType = FrontProneType;
        private static int _lastKeyPressed = 0;
        private static string _walkstyle = null;
        private static bool _forceEndProne = false;

        private static Timer _crawlTimer = null;

        private void InitializeCrouchCrawl()
        {
            RegisterCommandsAndKeyMappings();
        }

        #region CommandsAndKeyMapping

        private void RegisterCommandsAndKeyMappings()
        {
            if (CrouchEnabled)
            {
                if (CrouchKeyBindEnabled)
                {
                    API.RegisterKeyMapping("+crouch", CrouchKeyBindDescription, "keyboard", CrouchKeyBind);
                    API.RegisterCommand("+crouch", new Func<Task>(CrouchKeyPressed), false);
                    API.RegisterCommand("-crouch", new Action(() => { }), false);
                }

                API.RegisterCommand("crouch", new Func<Task>(async () =>
                {
                    if (_isCrouched)
                    {
                        _isCrouched = false;
                        return;
                    }

                    await AttemptCrouch(API.PlayerPedId());
                }), false);
                TriggerEvent("chat:addSuggestion", "/crouch", CrouchChatSuggestion);
            }
            
            
            if (CrawlEnabled)
            {
                if (CrawlKeyBindEnabled)
                {
                    API.RegisterKeyMapping("+crawl", CrawlKeyBindDescription, "keyboard", CrawlKeyBind);
                    API.RegisterCommand("+crawl", new Func<Task>(CrawlKeyPressed), false);
                    API.RegisterCommand("-crawl", new Action(() => { }), false);
                }
                API.RegisterCommand("crawl", new Func<Task>(CrawlKeyPressed), false);
                TriggerEvent("chat:addSuggestion", "/crawl", CrawlChatSuggestion);
                
                if (CrawlFlipKeyBindEnabled)
                {
                    API.RegisterKeyMapping("+crawlflip", CrawlFlipKeyBindDescription, "keyboard", CrawlFlipKeyBind);
                    API.RegisterCommand("+crawlflip", new Func<Task>(CrawlFlipKeyPressed), false);
                    API.RegisterCommand("-crawlflip", new Action(() => { }), false);
                }
                API.RegisterCommand("crawlflip", new Func<Task>(CrawlFlipKeyPressed), false);
                TriggerEvent("chat:addSuggestion", "/crawl", CrawlFlipChatSuggestion);
            }
        }

        #endregion

        #region Crouch

        private async Task OnCrouchTick()
        {
            if (!_isCrouched)
            {
                await ResetCrouch();
                DisableCrouchTick();
                return;
            }

            var playerPed = API.PlayerPedId();
            if (!CanPlayerCrouchCrawl(playerPed))
            {
                _isCrouched = false;
                return;
            }

            if (IsPedAiming(playerPed))
            {
                API.SetPedMaxMoveBlendRatio(playerPed, 0.15f);
            }

            API.SetPedCanPlayAmbientAnims(playerPed, false);

            DisableDuckInputThisFrame();
            if (API.IsPedUsingActionMode(playerPed))
            {
                API.SetPedUsingActionMode(playerPed, false, -1, DefaultAction);
            }

            API.DisableFirstPersonCamThisFrame();
            
            var jumpPressed = API.IsControlPressed(0, 22); // INPUT_JUMP
            if (jumpPressed)
            {
                _isCrouched = false;
            }
        }

        private async Task StartCrouch()
        {
            _isCrouched = true;
            await LoadClipSet(PedCrouchedMoveClipSet);
            var playerPed = API.PlayerPedId();

            if (API.GetPedStealthMovement(playerPed))
            {
                API.SetPedStealthMovement(playerPed, false, DefaultAction);
                await Delay(100);
            }

            // force leave first person
            if (API.GetFollowPedCamViewMode() == 4)
            {
                API.SetFollowPedCamViewMode(0); // third person
            }

            var walkStyle = GetPedWalkStyle(playerPed);
            if (walkStyle != null)
            {
                _walkstyle = walkStyle;
            }

            API.SetPedMovementClipset(playerPed, PedCrouchedMoveClipSet, 0.6f);
            API.SetPedStrafeClipset(playerPed, PedCrouchedStrafeClipSet);
            EnableCrouchTick();
        }

        private async Task<bool> AttemptCrouch(int playerPed)
        {
            if (!CanPlayerCrouchCrawl(playerPed) || !API.IsPedHuman(playerPed)) return true;
            await StartCrouch();
            return true;
        }

        private async Task DisableControlUntilReleased(int pad, int control)
        {
            while (API.IsDisabledControlPressed(pad, control))
            {
                API.DisableControlAction(pad, control, true);
                await Delay(0);
            }
        }

        private async Task CrouchKeyPressed()
        {
            if (_inAction)
            {
                return;
            }

            if (API.IsPauseMenuActive() || API.IsNuiFocused())
            {
                return;
            }

            var crouchKey = GetCrouchControl();
            var lookBehindKey = GetLookBehindControl();

            if (_isCrouched)
            {
                _isCrouched = false;

                if (crouchKey == lookBehindKey)
                {
                    await DisableControlUntilReleased(0, 26);
                }

                return;
            }

            var playerPed = API.PlayerPedId();
            if (!CanPlayerCrouchCrawl(playerPed) || !API.IsPedHuman(playerPed))
            {
                return;
            }


            if (crouchKey == lookBehindKey)
            {
                await DisableControlUntilReleased(0, 26);
            }

            var duckKey = GetDuckControl();
            if (crouchKey == duckKey)
            {
                if (CrouchOverrideStealthMode)
                {
                    DisableDuckInputThisFrame();
                }
                else if (!_isProne)
                {
                    var timer = API.GetGameTimer();
                    if (API.GetPedStealthMovement(playerPed) && timer - _lastKeyPressed < CrouchKeypressTimer)
                    {
                        DisableDuckInputThisFrame();
                        _lastKeyPressed = 0;
                    }
                    else
                    {
                        _lastKeyPressed = timer;
                        return;
                    }
                }
            }

            await StartCrouch();

            if (_isProne)
            {
                _inAction = true;
                _isProne = false;
                await PlayAnimOnce(playerPed, "get_up@directional@transition@prone_to_knees@crawl", "front",
                    duration: 780);
                await Delay(780);
                _inAction = false;
            }
        }

        private void EnableCrouchTick()
        {
            Tick += OnCrouchTick;
        }

        private void DisableCrouchTick()
        {
            Tick -= OnCrouchTick;
        }

        private async Task ResetCrouch()
        {
            var playerPed = API.PlayerPedId();

            API.ResetPedStrafeClipset(playerPed);
            API.ResetPedWeaponMovementClipset(playerPed);
            API.SetPedMaxMoveBlendRatio(playerPed, 1.0f);
            API.SetPedCanPlayAmbientAnims(playerPed, true);

            if (_walkstyle != null)
            {
                await SetPlayerClipSet(_walkstyle);
            }
            else
            {
                API.ResetPedMovementClipset(playerPed, 0.5f);
            }

            API.RemoveAnimSet(PedCrouchedMoveClipSet);
        }

        #endregion

        #region Crawling

        private static bool ShouldPlayerDiveToCrawl(int playerPed)
        {
            return API.IsPedRunning(playerPed) || API.IsPedSprinting(playerPed);
        }

        private static void StopPlayerProne(bool force)
        {
            _isProne = false;
            _forceEndProne = force;
        }

        private static void PlayIdleCrawlAnim(int playerPed, float heading, bool useCurrentHeading = false,
            float blendIn = 2.0f)
        {
            if (useCurrentHeading)
            {
                heading = API.GetEntityHeading(playerPed);
            }

            var playerCoords = API.GetEntityCoords(playerPed, true);
            var animName = _proneType + "_fwd";
            API.TaskPlayAnimAdvanced(playerPed, MoveCrawlAnimDict, animName, playerCoords.X, playerCoords.Y,
                playerCoords.Z, 0.0f, 0.0f, heading, blendIn, 2.0f, -1, 2, 1.0f, 0, 0);
        }

        private static async Task PlayExitCrawlAnims(bool forceEnd)
        {
            if (forceEnd) return;
            _inAction = true;
            var playerPed = API.PlayerPedId();

            if (_proneType == FrontProneType)
            {
                await PlayAnimOnce(playerPed, TransitionProneToKneesAnimDict, TransitionProneToKneesAnim,
                    duration: 780);

                if (!_isCrouched)
                {
                    await Delay(780);
                    await PlayAnimOnce(playerPed, MovementFromKneesAnimDict, MovementFromKneesAnim, duration: 1300);
                }
            }
            else
            {
                await PlayAnimOnce(playerPed, TransitionProneToSeatedAnimDict, TransitionProneToSeatedAnim,
                    blendIn: 16.0f, duration: 950);

                if (!_isCrouched)
                {
                    await Delay(950);
                    await PlayAnimOnce(playerPed, MovementFromSeatedAnimDict, MovementFromSeatedAnim, duration: 1300);
                }
            }
        }

        private static void Crawl(int playerPed, string type, string direction)
        {
            _isCrawling = true;
            string animName = type + "_" + direction;
            API.TaskPlayAnim(playerPed, MoveCrawlAnimDict, animName, 8.0f, -8.0f, -1, 2, 0.0f, false, false, false);

            var timeout = GetTimeoutToCrawl(type, direction);
            SetCrawlTimeout(() => { _isCrawling = false; }, timeout);
        }

        static void SetCrawlTimeout(Action action, int delayMs)
        {
            _crawlTimer?.Dispose();
            
            _crawlTimer = new Timer(_ =>
            {
                action();
                _crawlTimer?.Dispose();
                _crawlTimer = null;
            }, null, delayMs, Timeout.Infinite);
        }

        private static async Task CrawlFlip(int playerPed)
        {
            _inAction = true;
            var heading = API.GetEntityHeading(playerPed);

            if (_proneType == FrontProneType)
            {
                _proneType = BackProneType;
                await PlayAnimOnce(playerPed, CrawlFlipToBackAnimDict, CrawlFlipToBackAnim, 2.0f);
                await ChangeHeadingSmooth(playerPed, -18.0f, 3600);
            }
            else
            {
                _proneType = FrontProneType;
                await PlayAnimOnce(playerPed, CrawlFlipToFrontAnimDict, CrawlFlipToFrontAnim, 2.0f);
                await ChangeHeadingSmooth(playerPed, 12.0f, 1700);
            }

            PlayIdleCrawlAnim(playerPed, heading + 180.0f);
            await Delay(400);
            _inAction = false;
        }


        private async Task OnCrawlTick()
        {
            var playerPed = API.PlayerPedId();
            if (!_isProne)
            {
                DisableCrawlTick();
                await PlayExitCrawlAnims(_forceEndProne);

                _isCrawling = false;
                _inAction = false;
                _forceEndProne = false;
                _proneType = FrontProneType;
                API.SetPedConfigFlag(playerPed, 48, false); // CPED_CONFIG_FLAG_BlockWeaponSwitching

                API.RemoveAnimDict(MoveCrawlAnimDict);
                API.RemoveAnimDict(CrawlFlipToFrontAnimDict);
                return;
            }

            if (!CanPlayerCrouchCrawl(playerPed) || API.IsEntityInWater(playerPed))
            {
                API.ClearPedTasks(playerPed);
                StopPlayerProne(true);
                _isProne = false;
                return;
            }

            var forward = API.IsControlPressed(0, 32); // INPUT_MOVE_UP_ONLY
            var backward = API.IsControlPressed(0, 33); // INPUT_MOVE_DOWN_ONLY

            if (!_isCrawling)
            {
                if (forward)
                {
                    Crawl(playerPed, _proneType, DirectionFwd);
                }
                else if (backward)
                {
                    Crawl(playerPed, _proneType, DirectionBwd);
                }
            }

            var left = API.IsControlPressed(0, 34); // INPUT_MOVE_LEFT_ONLY
            var right = API.IsControlPressed(0, 35); // INPUT_MOVE_RIGHT_ONLY
            if (left)
            {
                if (_isCrawling)
                {
                    var headingDiff = forward ? 1 : -1;
                    var heading = API.GetEntityHeading(playerPed);
                    API.SetEntityHeading(playerPed, heading + headingDiff);
                    Debug.WriteLine($"{heading} + {headingDiff} | forward: {forward}");
                }
                else
                {
                    _inAction = true;
                    if (_proneType == FrontProneType)
                    {
                        var playerCoords = API.GetEntityCoords(playerPed, true);
                        var heading = API.GetEntityHeading(playerPed);
                        API.TaskPlayAnimAdvanced(playerPed, CrawlFrontLeftMoveAnimDict, CrawlFrontLeftMoveAnim,
                            playerCoords.X,
                            playerCoords.Y, playerCoords.Z, 0.0f, 0.0f, heading, 2.0f, 2.0f, -1, 2, 0.1f, 0, 0);
                        await ChangeHeadingSmooth(playerPed, -10.0f, 300);
                        await Delay(700);
                    }
                    else
                    {
                        await PlayAnimOnce(playerPed, CrawlBackLeftMoveAnimDict, CrawlBackLeftMoveAnim);
                        await ChangeHeadingSmooth(playerPed, 25.0f, 400);
                        PlayIdleCrawlAnim(playerPed, 0, true);
                        await Delay(600);
                    }

                    _inAction = false;
                }
            }
            else if (right)
            {
                if (_isCrawling)
                {
                    var headingDiff = backward ? 1 : -1;
                    var heading = API.GetEntityHeading(playerPed);
                    API.SetEntityHeading(playerPed, heading + headingDiff);
                    Debug.WriteLine($"{heading} + {headingDiff} | backward: {backward}");
                }
                else
                {
                    _inAction = true;
                    if (_proneType == FrontProneType)
                    {
                        var playerCoords = API.GetEntityCoords(playerPed, true);
                        var heading = API.GetEntityHeading(playerPed);
                        API.TaskPlayAnimAdvanced(playerPed, CrawlFrontRightMoveAnimDict, CrawlFrontRightMoveAnim,
                            playerCoords.X,
                            playerCoords.Y, playerCoords.Z, 0, 0, heading, 2, 2, -1, 2, 0.1f, 0, 0);
                        await ChangeHeadingSmooth(playerPed, 10.0f, 300);
                        await Delay(700);
                    }
                    else
                    {
                        await PlayAnimOnce(playerPed, CrawlBackRightMoveAnimDict, CrawlBackRightMoveAnim);
                        await ChangeHeadingSmooth(playerPed, -25.0f, 400);
                        PlayIdleCrawlAnim(playerPed, 0, true);
                        await Delay(600);
                    }

                    _inAction = false;
                }
            }

            var jumpPressed = API.IsControlPressed(0, 22); // INPUT_JUMP
            if (jumpPressed)
            {
                _isProne = false;
            }
            await Delay(0);
        }

        private void EnableCrawlTick()
        {
            Tick += OnCrawlTick;
        }

        private void DisableCrawlTick()
        {
            Tick -= OnCrawlTick;
        }

        private async Task CrawlKeyPressed()
        {
            if (_inAction)
            {
                return;
            }

            if (API.IsPauseMenuActive() || API.IsNuiFocused())
            {
                return;
            }

            if (_isProne)
            {
                _isProne = false;
                return;
            }

            var wasCrouched = false;
            if (_isCrouched)
            {
                _isCrouched = false;
                wasCrouched = true;
            }

            var playerPed = API.PlayerPedId();
            if (!CanPlayerCrouchCrawl(playerPed) || API.IsEntityInWater(playerPed) || !API.IsPedHuman(playerPed))
            {
                return;
            }

            _inAction = true;

            _isProne = true;
            API.SetPedConfigFlag(playerPed, 48, true); // CPED_CONFIG_FLAG_BlockWeaponSwitching

            if (API.GetPedStealthMovement(playerPed))
            {
                API.SetPedStealthMovement(playerPed, false, DefaultAction);
                await Delay(100);
            }

            await LoadAnimDict(MoveCrawlAnimDict);
            await LoadAnimDict(MoveCrawlProneAnimDict);

            if (ShouldPlayerDiveToCrawl(playerPed))
            {
                await PlayAnimOnce(playerPed, DiveToCrawlAnimDict, DiveToCrawlAnim, blendOut: 3.0f);
                await Delay(1100);
            }
            else if (wasCrouched)
            {
                await PlayAnimOnce(playerPed, CrouchToProneAnimDict, CrouchToProneAnim, duration: -1, startTime: 0.3f);
                await Delay(1500);
            }
            else
            {
                await PlayAnimOnce(playerPed, CrouchToProneAnimDict, CrouchToProneAnim);
                await Delay(3000);
            }

            if (CanPlayerCrouchCrawl(playerPed) && !API.IsEntityInWater(playerPed))
            {
                PlayIdleCrawlAnim(playerPed, 0, true, 3.0f);
            }
            
            _inAction = false;
            await Delay(400);
            EnableCrawlTick();
        }


        private async Task CrawlFlipKeyPressed()
        {
            if (_isProne && !_inAction && !_isCrawling)
            {
                var playerPed = API.PlayerPedId();
                await CrawlFlip(playerPed);
            }
        }

        private static int GetTimeoutToCrawl(string type, string direction)
        {
            if (type == FrontProneType)
            {
                if (direction == DirectionFwd)
                {
                    return CrawlFrontFwdDelay;
                }

                if (direction == DirectionBwd)
                {
                    return CrawlFrontBwdDelay;
                }
            }

            if (type == BackProneType)
            {
                if (direction == DirectionFwd)
                {
                    return CrawlBackFwdDelay;
                }

                if (direction == DirectionBwd)
                {
                    return CrawlBackBwdDelay;
                }
            }

            return 0;
        }

        #endregion

        #region Utils

        private static string GetCrouchControl()
        {
            var crouchKey = API.GetHashKey("+crouch") | unchecked((int)0x80000000);
            return API.GetControlInstructionalButton(0, crouchKey, 0);
        }

        private static string GetLookBehindControl()
        {
            return API.GetControlInstructionalButton(0, 26, 0);
        }

        private static string GetDuckControl()
        {
            return API.GetControlInstructionalButton(0, 36, 0);
        }


        private static bool CanPlayerCrouchCrawl(int playerPed)
        {
            return API.IsPedOnFoot(playerPed) && !API.IsPedJumping(playerPed) && !API.IsPedFalling(playerPed) &&
                   !API.IsPedInjured(playerPed) && !API.IsPedInMeleeCombat(playerPed) && !API.IsPedRagdoll(playerPed);
        }

        private static async Task LoadAnimDict(string dict)
        {
            API.RequestAnimDict(dict);
            while (!API.HasAnimDictLoaded(dict))
            {
                await Delay(0);
            }
        }

        private static async Task LoadClipSet(string clipset)
        {
            API.RequestClipSet(clipset);
            while (!API.HasClipSetLoaded(clipset))
            {
                await Delay(0);
            }
        }

        private static async Task SetPlayerClipSet(string clipset)
        {
            await LoadClipSet(clipset);
            API.SetPedMovementClipset(API.PlayerPedId(), clipset, 0.5f);
            API.RemoveClipSet(clipset);
        }

        private static bool IsPedAiming(int ped)
        {
            return API.GetPedConfigFlag(ped, 78, true);
        }

        private static async Task PlayAnimOnce(int ped, string animDict, string animName, float blendIn = 2,
            float blendOut = 2,
            int duration = -1, float startTime = 0)
        {
            await LoadAnimDict(animDict);
            API.TaskPlayAnim(ped, animDict, animName, blendIn, blendOut, duration, 0, startTime, false, false, false);
            API.RemoveAnimDict(animDict);
        }

        private static async Task ChangeHeadingSmooth(int ped, float amount, int time)
        {
            var times = Math.Abs(amount);
            var test = amount / times;
            var wait = (int)(time / times);

            for (int i = 0; i < times; i++)
            {
                await Delay(wait);
                var currentHeading = API.GetEntityHeading(ped);
                API.SetEntityHeading(ped, currentHeading + test);
            }
        }

        private void DisableDuckInputThisFrame()
        {
            API.DisableControlAction(0, 36, true);
        }

        #endregion

        #region WalkStyles

        private Dictionary<int, string> _walkStyles = new Dictionary<int, string>()
        {
            { -2146642687, "move_m@alien" },
            { 1450392727, "anim_group_move_ballistic" },
            { 1646588077, "move_f@arrogant@a" },
            { -1273245730, "move_m@hurry_butch@a" },
            { -1654611352, "move_m@hurry_butch@b" },
            { 1135734536, "move_m@hurry_butch@c" },
            { -1768281232, "move_m@brave" },
            { 1160259160, "move_m@casual@a" },
            { 1249489219, "move_m@casual@b" },
            { 1022236204, "move_m@casual@c" },
            { 1730505370, "move_m@casual@d" },
            { 1500565297, "move_m@casual@e" },
            { -742407223, "move_m@casual@f" },
            { -2125795638, "move_f@chichi" },
            { 1130158996, "move_m@confident" },
            { 1607161685, "move_m@business@a" },
            { 1845818312, "move_m@business@b" },
            { -59928421, "move_m@business@c" },
            { -2055591238, "move_chubby" },
            { -108537538, "move_f@chubby@a" },
            { -1401903942, "move_f@multiplayer" },
            { 1113513977, "move_m@multiplayer" },
            { -1287120285, "move_m@depressed@a" },
            { -502630425, "move_m@depressed@b" },
            { 685317947, "move_f@depressed@a" },
            { -859042698, "move_m@drunk@a" },
            { 2037534323, "move_m@buzzed" },
            { -1925018459, "move_m@drunk@moderatedrunk" },
            { -1201085968, "move_m@drunk@moderatedrunk_head_up" },
            { 875753685, "move_m@drunk@slightlydrunk" },
            { -297078218, "move_m@drunk@verydrunk" },
            { 1524082234, "move_m@fat@a" },
            { 522820593, "move_f@fat@a" },
            { -1732630094, "move_m@fat@bulky" },
            { -669438934, "move_f@femme@" },
            { -1857789306, "move_characters@franklin@fire" },
            { -433101684, "move_characters@michael@fire" },
            { 989819896, "move_m@fire" },
            { 2077811903, "move_f@flee@a" },
            { 864310395, "move_f@flee@c" },
            { -1960902366, "move_m@flee@a" },
            { 1287652361, "move_m@flee@b" },
            { -796046076, "move_p_m_one" },
            { -1810566716, "move_m@gangster@generic" },
            { -2114609648, "move_m@gangster@ng" },
            { -875359244, "move_m@gangster@var_e" },
            { 1203637196, "move_m@gangster@var_f" },
            { -1796495834, "move_m@gangster@var_i" },
            { 132330440, "move_m@generic" },
            { 642383383, "move_f@generic" },
            { 696702737, "anim@move_m@grooving@" },
            { -705606766, "anim@move_f@grooving@" },
            { 1013381506, "move_m@prison_gaurd" },
            { 1500055922, "move_m@prisoner_cuffed" },
            { 101970339, "move_f@heels@c" },
            { -1100881352, "move_f@heels@d" },
            { 1712688432, "move_m@hiking" },
            { -1806913316, "move_f@hiking" },
            { -1261021058, "move_m@hipster@a" },
            { -1027640375, "move_m@hobo@a" },
            { -725870658, "move_m@hobo@b" },
            { -1694147212, "move_m@hurry@a" },
            { 1605790647, "move_f@hurry@a" },
            { -32565260, "move_f@injured" },
            { 868295932, "move_m@intimidation@1h" },
            { -749057629, "move_m@intimidation@cop@unarmed" },
            { 584873396, "move_m@intimidation@unarmed" },
            { 92422612, "move_p_m_zero_janitor" },
            { 1864844954, "move_p_m_zero_slow" },
            { 1103953188, "move_m@jog@" },
            { -708603839, "move_characters@jimmy@nervous@" },
            { 1909742916, "anim_group_move_lemar_alley" },
            { 1690913150, "move_heist_lester" },
            { 549262148, "move_lester_caneup" },
            { 186601483, "move_f@maneater" },
            { -578327514, "move_ped_bucket" },
            { -1269633907, "move_m@money" },
            { -207491758, "move_m@muscle@a" },
            { -1543095923, "move_m@posh@" },
            { -1868494245, "move_f@posh@" },
            { 1023544707, "move_m@quick" },
            { 636261340, "female_fast_runner" },
            { -1599479573, "move_m@sad@a" },
            { -1847704748, "move_m@sad@b" },
            { -2077448207, "move_m@sad@c" },
            { -566100771, "move_f@sad@a" },
            { -930295437, "move_f@sad@b" },
            { 1207987305, "move_m@sassy" },
            { 1235276737, "move_f@sassy" },
            { -1472832709, "move_f@scared" },
            { -1990894342, "move_f@sexy@a" },
            { -1818270454, "move_m@shadyped@a" },
            { -510722362, "move_characters@jimmy@slow@" },
            { -409852351, "move_m@swagger" },
            { 1802187645, "move_m@tough_guy@" },
            { -1568317798, "move_f@tough_guy@" },
            { -500831769, "move_m@tool_belt@a" },
            { -976584416, "move_f@tool_belt@a" },
            { 1844458253, "clipset@move@trash_fast_turn" },
            { -435990891, "missfbi4prepp1_garbageman" },
            { -895219889, "move_p_m_two" },
            { 1258529727, "move_m@bag" },
            { -650503762, "move_m@injured" },
            { -1104677118, "move_injured_generic" },
            { -2129845123, "MOVE_M@BAIL_BOND_NOT_TAZERED" },
            { -70818445, "MOVE_M@BAIL_BOND_TAZERED" },
            { -618380859, "MOVE_P_M_ONE_BRIEFCASE" },
            { 666904976, "move_ped_mop" },
            { -1312865774, "move_m@femme@" },
            { 735579764, "move_f@gangster@ng" },
            { -1168427927, "move_characters@orleans@core@" },
            { -1164222247, "move_m@coward" },
            { 279703740, "move_characters@dave_n" },
            { 1539166312, "move_characters@jimmy" },
            { 1899314058, "move_characters@patricia" },
            { 1583990743, "move_characters@ron" },
            { 1528838481, "move_m@swagger@b" },
            { 148072839, "move_m@leaf_blower" },
            { -2018280977, "move_m@flee@c" },
            { -1960115337, "move_characters@amanda@bag" },
            { 1701187980, "move_f@film_reel" },
            { -1163090857, "move_f@flee@generic" },
            { 922192683, "move_f@handbag" },
            { -905417764, "move_m@flee@generic" },
            { -871949441, "move_m@shocked@a" },
            { 1728327052, "move_characters@floyd" },
            { 756811395, "move_f@hurry@b" },
            { -975292135, "move_characters@lamar" },
            { 70692426, "move_characters@tracey" },
            { -582520880, "move_m@brave@a" },
            { -388968941, "move_m@gangster@var_a" },
            { -1874148793, "move_f@stripper@a" },
            { -2076638015, "move_m@gangster@var_b" },
            { -1366140557, "move_m@gangster@var_c" },
            { -535479176, "move_m@gangster@var_d" },
            { 2038230857, "move_m@gangster@var_g" },
            { 1664205491, "move_m@gangster@var_h" },
            { 445985183, "move_m@gangster@var_j" },
            { -288695797, "move_m@gangster@var_k" },
            { 862223719, "move_m@clipboard" },
            { -409207550, "move_cop@action" },
            { 1259887674, "move_gangster" },
            { -795792088, "move_casey" },
            { -1938021834, "move_dreyfuss" },
            { 202679515, "move_paramedic" },
            { -1345269979, "move_f@fat@a_no_add" },
            { -1267550608, "move_f@depressed@c" },
            { 1720274816, "anim@move_f@grooving@slow@" },
            { 148615797, "anim@move_m@grooving@slow@" },
            { 422291091, "AMBIENT_WALK_VARIATION_F_ARROGANT_A" },
            { 1510605100, "AMBIENT_WALK_VARIATION_M_SAD_B" },
            { -746382641, "AMBIENT_WALK_VARIATION_M_BUSINESS_B" },
            { 1799136145, "AMBIENT_WALK_VARIATION_M_SAD_A" },
            { 11564329, "AMBIENT_WALK_VARIATION_M_BUSINESS_C" },
            { 471477248, "AMBIENT_WALK_VARIATION_M_BUSINESS_A" },
            { -1749517176, "AMBIENT_WALK_VARIATION_M_SAD_C" },
            { -1561136569, "AMBIENT_WALK_VARIATION_F_SAD_A" },
            { 381019249, "HUSKY@MOVE" },
            { -289665739, "RETRIEVER@MOVE" },
            { -1914955993, "move_p_m_zero" }
        };

        private string GetPedWalkStyle(int ped)
        {
            var clipset = API.GetPedMovementClipset(ped);
            _walkStyles.TryGetValue(clipset, out var result);
            return result;
        }

        #endregion
    }
}