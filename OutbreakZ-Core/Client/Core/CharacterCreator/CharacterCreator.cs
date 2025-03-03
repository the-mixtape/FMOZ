using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;

namespace OutbreakZCore.Client.Core.CharacterCreator
{
    public partial class CharacterCreator : BaseScript
    {
        private const string MaleGender = "Male";
        private const string FemaleGender = "Female";

        private readonly List<dynamic> _genders = new List<dynamic>()
        {
            MaleGender,
            FemaleGender
        };

        private bool _enable = false;
        
        private readonly Vector3 _editPoint = new Vector3(402.89f, -996.87f, -99.0f); 
        private readonly float _editPointHeading = 173.97f;

        private KeyValuePair<string, string> _hairDecorDefault =
            new KeyValuePair<string, string>("mpbeach_overlays", "FM_hair_fuzz");

        private readonly Dictionary<int, KeyValuePair<string, string>> _maleHairDecor =
            new Dictionary<int, KeyValuePair<string, string>>()
            {
                [0] = new KeyValuePair<string, string>("", ""),
                [1] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a"),
                [2] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_002"),
                [3] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a"),
                [4] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_004"),
                [5] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a"),
                [6] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_006_a"),
                // 7
                [8] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a"),
                [9] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_009"),
                [10] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_013"),
                [11] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_002"),
                [12] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_011"),
                [13] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_012"),
                [14] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_014"),
                [15] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_015"),
                [16] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBea_M_Hair_000"),
                [17] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBea_M_Hair_001"),
                [18] = new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a"),
                [19] = new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a"),
                [20] = new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a"),
                [21] = new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a"),
                [22] = new KeyValuePair<string, string>("multiplayer_overlays", "NGInd_M_Hair_000"),
                // 23
                [24] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_M_Hair_000"),
                [25] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_M_Hair_001"),
                [26] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_M_Hair_002"),
                [27] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_M_Hair_003"),
                [28] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_M_Hair_004"),
                [29] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_M_Hair_005"),
                [30] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_M_Hair_006"),
                [31] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_000_M"),
                [32] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_001_M"),
                [33] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_002_M"),
                [34] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_003_M"),
                [35] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_004_M"),
                [36] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_005_M"),

                [72] = new KeyValuePair<string, string>("mpgunrunning_overlays", "MP_Gunrunning_Hair_M_000_M"),
                [73] = new KeyValuePair<string, string>("mpgunrunning_overlays", "MP_Gunrunning_Hair_M_001_M"),
                [74] = new KeyValuePair<string, string>("mpvinewood_overlays", "MP_Vinewood_Hair_M_000_M"),
                [75] = new KeyValuePair<string, string>("mptuner_overlays", "MP_Tuner_Hair_001_M"),
                [76] = new KeyValuePair<string, string>("mpsecurity_overlays", "MP_Security_Hair_001_M"),
            };

        private readonly Dictionary<int, KeyValuePair<string, string>> _femaleHairDecor =
            new Dictionary<int, KeyValuePair<string, string>>()
            {
                [0] = new KeyValuePair<string, string>("", ""),
                [1] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_001"),
                [2] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_002"),
                [3] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_F_Hair_003_a"),
                [4] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_004"),
                [5] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_F_Hair_005_a"),
                [6] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_F_Hair_006_a"),
                [7] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_007"),
                [8] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_008"),
                [9] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_009"),
                [10] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_010"),
                [11] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_011"),
                [12] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_012"),
                [13] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_F_Hair_013_a"),
                [14] = new KeyValuePair<string, string>("multiplayer_overlays", "FM_F_Hair_014_a"),
                [15] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_M_Hair_015"),
                [16] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBea_F_Hair_000"),
                [17] = new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_F_Hair_a"),
                [18] = new KeyValuePair<string, string>("multiplayer_overlays", "NG_F_Hair_007"),
                [19] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBus_F_Hair_000"),
                [20] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBus_F_Hair_001"),
                [21] = new KeyValuePair<string, string>("multiplayer_overlays", "NGBea_F_Hair_001"),
                [22] = new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_F_Hair_000_a"),
                [23] = new KeyValuePair<string, string>("multiplayer_overlays", "NGInd_F_Hair_000"),
                // 24
                [25] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_F_Hair_000"),
                [26] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_F_Hair_001"),
                [27] = new KeyValuePair<string, string>("mplowrider_overlays", "LR_F_Hair_002"),
                [29] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_F_Hair_003"),
                [30] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_F_Hair_004"),
                [31] = new KeyValuePair<string, string>("mplowrider2_overlays", "LR_F_Hair_006"),
                [32] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_000_F"),
                [33] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_001_F"),
                [34] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_002_F"),
                [35] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_003_F"),
                [38] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_004_F"),
                [36] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_005_F"),
                [37] = new KeyValuePair<string, string>("mpbiker_overlays", "MP_Biker_Hair_005_F"),

                [76] = new KeyValuePair<string, string>("mpgunrunning_overlays", "MP_Gunrunning_Hair_F_000_F"),
                [77] = new KeyValuePair<string, string>("mpgunrunning_overlays", "MP_Gunrunning_Hair_F_001_F"),
                [78] = new KeyValuePair<string, string>("mpvinewood_overlays", "MP_Vinewood_Hair_F_000_F"),
                [79] = new KeyValuePair<string, string>("mptuner_overlays", "MP_Tuner_Hair_000_F"),
                [80] = new KeyValuePair<string, string>("mpsecurity_overlays", "MP_Security_Hair_000_F"),
            };


        private readonly List<dynamic> _maleHairNames = new List<dynamic>()
        {
            "Close Shave", "Buzzcut", "Faux Hawk", "Hipster", "Side Parting", "Shorter Cut", "Biker", "Ponytail",
            "Cornrows", "Slicked", "Short Brushed", "Spikey", "Caesar", "Chopped", "Dreads", "Long Hair",
            "Shaggy Curls", "Surfer Dude", "Short Side Part", "High Slicked Sides", "Long Slicked", "Hipster Youth",
            "Mullet"
        };

        private readonly List<dynamic> _femaleHairNames = new List<dynamic>()
        {
            "Close Shave", "Short", "Layered Bob", "Pigtails", "Ponytail", "Braided Mohawk", "Braids", "Bob",
            "Faux Hawk", "French Twist", "Long Bob", "Loose Tied", "Pixie", "Shaved Bangs", "Top Knot", "Wavy Bob",
            "Pin Up Girl", "Messy Bun", "Unknown", "Tight Bun", "Twisted Bob", "Big Bangs", "Braided Top Knot"
        };

        private readonly List<dynamic> _eyebrowsNames = new List<dynamic>()
        {
            "Balanced", "Fashion", "Cleopatra", "Quizzical", "Femme", "Seductive", "Pinched", "Chola", "Triomphe",
            "Carefree", "Curvaceous", "Rodent", "Double Tram", "Thin", "Penciled", "Mother Plucker",
            "Straight and Narrow", "Natural", "Fuzzy", "Unkempt", "Caterpillar", "Regular", "Mediterranean", "Groomed",
            "Bushels", "Feathered", "Prickly", "Monobrow", "Winged", "Triple Tram", "Arched Tram", "Cutouts",
            "Fade Away", "Solo Tram"
        };

        private readonly List<dynamic> _beardNames = new List<dynamic>()
        {
            "Clean Shaven", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz",
            "Pencil Chin Strap", "Scruffy", "Musketeer", "Mustache",
            "Trimmed Beard", "Stubble", "Thin Circle Beard", "Horseshoe", "Pencil and Chops", "Chin Strap Beard",
            "Balbo and Sideburns", "Mutton Chops", "Scruffy Beard", "Curly",
            "Curly and Deep Stranger", "Handlebar", "Faustic", "Otto and Patch", "Otto and Full Stranger",
            "Light Franz", "The Hampstead", "The Ambrose", "Lincoln Curtain"
        };

        private readonly List<dynamic> _blemishes = new List<dynamic>()
        {
            "None", "Measles", "Pimples", "Spots", "Break Out", "Blackheads", "Build Up", "Pustules", "Zits",
            "Full Acne", "Acne", "Cheek Rash", "Face Rash",
            "Picker", "Puberty", "Eyesore", "Chin Rash", "Two Face", "T Zone", "Greasy", "Marked", "Acne Scarring",
            "Full Acne Scarring", "Cold Sores", "Impetigo"
        };

        private readonly List<dynamic> _agingNames = new List<dynamic>()
        {
            "None", "Crow's Feet", "First Signs", "Middle Aged", "Worry Lines", "Depression", "Distinguished", "Aged",
            "Weathered", "Wrinkled", "Sagging", "Tough Life",
            "Vintage", "Retired", "Junkie", "Geriatric"
        };

        private readonly List<dynamic> _complexion = new List<dynamic>()
        {
            "None", "Rosy Cheeks", "Stubble Rash", "Hot Flush", "Sunburn", "Bruised", "Alchoholic", "Patchy", "Totem",
            "Blood Vessels", "Damaged", "Pale", "Ghostly"
        };

        private readonly List<dynamic> _molefreckle = new List<dynamic>()
        {
            "None", "Cherub", "All Over", "Irregular", "Dot Dash", "Over the Bridge", "Baby Doll", "Pixie",
            "Sun Kissed", "Beauty Marks", "Line Up", "Modelesque",
            "Occasional", "Speckled", "Rain Drops", "Double Dip", "One Sided", "Pairs", "Growth"
        };

        private readonly List<dynamic> _sunDamage = new List<dynamic>()
        {
            "None", "Uneven", "Sandpaper", "Patchy", "Rough", "Leathery", "Textured", "Coarse", "Rugged", "Creased",
            "Cracked", "Gritty"
        };

        private readonly List<dynamic> _eyeColorNames = new List<dynamic>()
        {
            "Green", "Emerald", "Light Blue", "Ocean Blue", "Light Brown", "Dark Brown", "Hazel", "Dark Gray",
            "Light Gray", "Pink", "Yellow", "Purple", "Blackout",
            "Shades of Gray", "Tequila Sunrise", "Atomic", "Warp", "ECola", "Space Ranger", "Ying Yang", "Bullseye",
            "Lizard", "Dragon", "Extra Terrestrial", "Goat", "Smiley", "Possessed",
            "Demon", "Infected", "Alien", "Undead", "Zombie"
        };

        private readonly List<dynamic> _makeups = new List<dynamic>()
        {
            "None", "Smoky Black", "Bronze", "Soft Gray", "Retro Glam", "Natural Look", "Cat Eyes", "Chola", "Vamp",
            "Vinewood Glamour", "Bubblegum", "Aqua Dream", "Pin up", "Purple Passion", "Smoky Cat Eye",
            "Smoldering Ruby", "Pop Princess"
        };

        private readonly List<dynamic> _lipsticks = new List<dynamic>()
        {
            "None", "Color Matte", "Color Gloss", "Lined Matte", "Lined Gloss", "Heavy Lined Matte",
            "Heavy Lined Gloss", "Lined Nude Matte", "Liner Nude Gloss", "Smudged", "Geisha"
        };

        private readonly List<dynamic> _blushes = new List<dynamic>()
        {
            "None", "Full", "Angled", "Round", "Horizontal", "High", "Sweetheart", "Eighties"
        };

        private readonly Dictionary<int, KeyValuePair<int, int>> _maleDefaultOutfits =
            new Dictionary<int, KeyValuePair<int, int>>()
            {
                [3] = new KeyValuePair<int, int>(15, 0),
                [4] = new KeyValuePair<int, int>(61, 0),
                [6] = new KeyValuePair<int, int>(34, 0),
                [8] = new KeyValuePair<int, int>(15, 0),
                [11] = new KeyValuePair<int, int>(15, 0)
            };
        
        private readonly Dictionary<int, KeyValuePair<int, int>> _femaleDefaultOutfits =
            new Dictionary<int, KeyValuePair<int, int>>()
            {
                [3] = new KeyValuePair<int, int>(15, 0),
                [4] = new KeyValuePair<int, int>(15, 0),
                [6] = new KeyValuePair<int, int>(35, 0),
                [8] = new KeyValuePair<int, int>(15, 0),
                [11] = new KeyValuePair<int, int>(15, 0)
            };

        private readonly CharacterData _character = new CharacterData();
        private const string DefaultCameraName = "DEFAULT_SCRIPTED_CAMERA";

        private readonly Vector3 _faceCameraPos = new Vector3(402.74f, -1000.72f, -98.45f);
        private readonly float _faceCameraFov = 10.00f;

        private readonly Vector3 _bodyCameraPos = new Vector3(402.92f, -1000.72f, -99.01f);
        private readonly float _bodyCameraFov = 30.00f;

        private const PedHash MaleModelHash = PedHash.FreemodeMale01;
        private const PedHash FemaleModelHash = PedHash.FreemodeFemale01;

        private uint _modelHash;
        private int _bodyCamera, _faceCamera;
        private Vector3 _initPos;


        public CharacterCreator()
        {
            _bodyCamera = API.CreateCamWithParams(DefaultCameraName,
                _bodyCameraPos.X, _bodyCameraPos.Y, _bodyCameraPos.Z,
                0, 0, 0, _bodyCameraFov, false, 0);

            _faceCamera = API.CreateCamWithParams(DefaultCameraName,
                _faceCameraPos.X, _faceCameraPos.Y, _faceCameraPos.Z,
                0, 0, 0, _faceCameraFov, false, 0);

            InitializeMainMenu();
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            API.RegisterCommand("charedit", new Func<Task>(async () => { await StartCharacterEditor(); }), false);
        }

        private async Task StartCharacterEditor()
        {
            var playerPed = API.PlayerPedId();
            _initPos = API.GetEntityCoords(playerPed, true);

            API.DisplayRadar(false);
            API.DoScreenFadeOut(1000);
            await Delay(1000);

            API.RequestCollisionAtCoord(_editPoint.X, _editPoint.Y, _editPoint.Z);
            await Delay(5000);

            await RefreshModel();
            ChangeComponents();

            API.SetEntityCoordsNoOffset(playerPed, _editPoint.X, _editPoint.Y, _editPoint.Z, false, false, false);
            API.NetworkResurrectLocalPlayer(_editPoint.X, _editPoint.Y, _editPoint.Z, _editPointHeading, true, true);
            API.ClearPedTasksImmediately(playerPed);
            
            await Delay(1000);
            API.FreezeEntityPosition(playerPed, true);
            
            await AnimCam();
            
            EnableMenu();
            EnableCharacterCreator();
        }

        private async Task EndCharacterCreator()
        {
            DisableMenu();
            
            API.DoScreenFadeOut(1000);
            await Delay(1000);

            var playerPed = API.PlayerPedId();
            API.SetEntityCoords(playerPed, _initPos.X, _initPos.Y, _initPos.Z, true, false, false, true);
            API.SetCamActive(_bodyCamera, false);
            API.SetCamActive(_faceCamera, false);
            API.RenderScriptCams(false, false, 0, true, true);
            DisableCharacterCreator();
            API.DisplayRadar(true);

            ChangeComponents();
            API.EnableAllControlActions(0);
            API.FreezeEntityPosition(playerPed, false);
            
            API.DoScreenFadeIn(1000);
            await Delay(1000);
            
            API.ClearFocus();
        }

        private void SwitchCamera(int camTo, int camFrom)
        {
            API.SetCamActiveWithInterp(camTo, camFrom, 250, 1, 1);
        }

        private async Task AnimCam()
        {
            API.SetCamActive(_bodyCamera, true);
            API.RenderScriptCams(true, false, 2000, true, true);

            var enterCamera =
                API.CreateCamWithParams(DefaultCameraName, 402.99f, -998.02f, -99.00f, 0, 0, 0, 50.00f, false, 0);
            API.PointCamAtCoord(enterCamera, 402.99f, -998.02f, -99.00f);
            API.SetCamActiveWithInterp(_bodyCamera, enterCamera, 5000, 1, 1);
            
            API.DoScreenFadeIn(2000);
            await Delay(5000);
        }

        private void EnableCharacterCreator()
        {
            _enable = true;
            Tick += CharacterCreatorTick;
        }

        private void DisableCharacterCreator()
        {
            _enable = false;
            Tick -= CharacterCreatorTick;
        }

        private async Task CharacterCreatorTick()
        {
            if (!_enable)
            {
                API.DisableAllControlActions(0);
                Collision();
            }

            await Delay(0);
        }

        private void Collision()
        {
            for (int i = 1; i < 256; i++)
            {
                if (API.NetworkIsPlayerActive(i))
                {
                    API.SetEntityVisible(API.GetPlayerPed(i), false, false);
                    API.SetEntityVisible(API.PlayerPedId(), true, true);
                    API.SetEntityNoCollisionEntity(API.GetPlayerPed(i), API.PlayerPedId(), false);
                }
            }
        }

        private async Task RefreshModel()
        {
            var playerPed = API.PlayerPedId();

            RefreshModelHash();
            if ((uint)API.GetEntityModel(playerPed) == _modelHash) return;

            while (!API.HasModelLoaded(_modelHash))
            {
                API.RequestModel(_modelHash);
                await Delay(0);
            }

            API.SetPlayerModel(API.PlayerId(), _modelHash);
            RefreshPedHead();
            ChangeComponents();
        }

        private void RefreshPedHead()
        {
            API.SetPedHeadBlendData(API.PlayerPedId(), _character.Mom, _character.Dad, 0,
                _character.Mom, _character.Dad, 0,
                _character.Resemblance, _character.Skintone, 0, true);
        }

        private void RefreshPedHair()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedComponentVariation(playerPed, 2, _character.Hair, 0, 2);
            API.SetPedHairColor(playerPed, _character.HairColor1, 0);

            API.ClearPedDecorations(playerPed);

            Dictionary<int, KeyValuePair<string, string>> hairDecor = null;
            switch (_character.Gender)
            {
                case FemaleGender:
                    hairDecor = _femaleHairDecor;
                    break;
                case MaleGender:
                    hairDecor = _maleHairDecor;
                    break;
            }

            if (hairDecor != null)
            {
                var drawableHairVariation = API.GetPedDrawableVariation(playerPed, 2); // 2 - Hair
                var pair = hairDecor[drawableHairVariation];
                var collection = (uint)API.GetHashKey(pair.Key);
                var overlay = (uint)API.GetHashKey(pair.Value);
                API.AddPedDecorationFromHashes(playerPed, collection, overlay);
            }
            else
            {
                var collection = (uint)API.GetHashKey(_hairDecorDefault.Key);
                var overlay = (uint)API.GetHashKey(_hairDecorDefault.Value);
                API.AddPedDecorationFromHashes(playerPed, collection, overlay);
            }
        }

        private void RefreshLipstick()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 8, _character.Lipstick1, _character.Lipstick2);
            API.SetPedHeadOverlayColor(playerPed, 8, 1, _character.Lipstick3, 0);
        }

        private void RefreshPedEyebrows()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 2, _character.Eyebrows, _character.Eyebrows2);
            API.SetPedHeadOverlayColor(playerPed, 2, 1, _character.Eyebrows3, 0);
        }

        private void RefreshAging()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 3, _character.Age1, _character.Age2);
        }

        private void RefreshBlemishes()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 11, _character.Bodyb1, _character.Bodyb2);
        }

        private void RefreshSunDamage()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 7, _character.Sun1, _character.Sun2);
        }

        private void RefreshComplexion()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 6, _character.Complexion1, _character.Complexion2);
        }

        private void RefreshMoleFreckle()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 9, _character.Moles1, _character.Moles2);
        }

        private void RefreshEyeColor()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedEyeColor(playerPed, _character.EyeColor);
        }

        private void RefreshMakeup()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 4, _character.Makeup1, _character.Makeup2);
            API.SetPedHeadOverlayColor(playerPed, 4, 1, _character.Makeup3, 0);
        }

        private void RefreshPedBeard()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 1, _character.Beard, _character.Beard2);
            API.SetPedHeadOverlayColor(playerPed, 1, 1, _character.Beard3, 0);
        }

        private void RefreshBlush()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedHeadOverlay(playerPed, 5, _character.Blush1, _character.Blush2);
            API.SetPedHeadOverlayColor(playerPed, 5, 2, _character.Blush3, 0);
        }

        private void RefreshModelHash()
        {
            switch (_character.Gender)
            {
                case MaleGender:
                    _modelHash = (uint)MaleModelHash;
                    break;
                case FemaleGender:
                    _modelHash = (uint)FemaleModelHash;
                    break;
            }
        }

        private void ChangeComponents()
        {
            var playerPed = API.PlayerPedId();
            API.SetPedDefaultComponentVariation(playerPed);

            RefreshPedHead();
            RefreshPedHair();
            RefreshLipstick();
            RefreshPedEyebrows();
            RefreshAging();
            RefreshBlemishes();
            RefreshSunDamage();
            RefreshComplexion();
            RefreshMoleFreckle();
            RefreshEyeColor();
            RefreshMakeup();

            API.SetPedFaceFeature(playerPed, 19, _character.NeckThick);
            API.SetPedFaceFeature(playerPed, 18, _character.ChinHole);
            API.SetPedFaceFeature(playerPed, 17, _character.ChinWidth);
            API.SetPedFaceFeature(playerPed, 16, _character.ChinLength);
            API.SetPedFaceFeature(playerPed, 15, _character.ChinHeight);
            API.SetPedFaceFeature(playerPed, 14, _character.Jaw2);
            API.SetPedFaceFeature(playerPed, 13, _character.Jaw1);
            API.SetPedFaceFeature(playerPed, 12, _character.LipsThick);
            API.SetPedFaceFeature(playerPed, 11, _character.EyeOpen);
            API.SetPedFaceFeature(playerPed, 10, _character.Cheeks3);
            API.SetPedFaceFeature(playerPed, 9, _character.Cheeks2);
            API.SetPedFaceFeature(playerPed, 8, _character.Cheeks1);
            API.SetPedFaceFeature(playerPed, 6, _character.Eyebrows6);
            API.SetPedFaceFeature(playerPed, 7, _character.Eyebrows5);
            API.SetPedFaceFeature(playerPed, 5, _character.Nose6);
            API.SetPedFaceFeature(playerPed, 4, _character.Nose5);
            API.SetPedFaceFeature(playerPed, 3, _character.Nose4);
            API.SetPedFaceFeature(playerPed, 2, _character.Nose3);
            API.SetPedFaceFeature(playerPed, 1, _character.Nose2);
            API.SetPedFaceFeature(playerPed, 0, _character.Nose1);

            if (_character.Gender == MaleGender)
            {
                // var outfitIndex = _character.Outfit;
                // var outfit = _maleOutfits[outfitIndex];
                SetPedOutfit(playerPed, _maleDefaultOutfits);
                RefreshPedBeard();
            }
            else if (_character.Gender == FemaleGender)
            {
                // var outfitIndex = _character.Outfit;
                // var outfit = _femaleOutfits[outfitIndex];
                SetPedOutfit(playerPed, _femaleDefaultOutfits);
            }
            
            RefreshBlush();

            if (_character.Glasses == 0)
            {
                if (_character.Gender == MaleGender)
                {
                    API.SetPedPropIndex(playerPed, 1, 11, 0, false);
                }
                else
                {
                    API.SetPedPropIndex(playerPed, 1, 5, 0, false);
                }
            }
            else
            {
                if (_character.Gender == MaleGender)
                {
                    API.SetPedPropIndex(playerPed, 1, 5, 0, false);
                }
                else
                {
                    API.SetPedPropIndex(playerPed, 1, 11, 0, false);
                }
            }
        }

        private void SetPedOutfit(int playerPed, Dictionary<int, KeyValuePair<int, int>> outfit)
        {
            foreach (var pair in outfit)
            {
                var componentId = pair.Key;
                var drawableId = pair.Value.Key;
                var textureId = pair.Value.Value;
                API.SetPedComponentVariation(playerPed, componentId, drawableId, textureId, 2);
            }
        }

        private async Task SetGender(string gender)
        {
            _character.Gender = gender;
            _character.Resemblance = 1.0f - _character.Resemblance;
            _character.Skintone = 1.0f - _character.Skintone;
            await RefreshModel();
        }
    }
}