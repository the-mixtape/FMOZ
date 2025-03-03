using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using NativeUI;
using Math = OutbreakZCore.Shared.Utils.Math;

namespace OutbreakZCore.Client.Core.CharacterCreator
{
    public partial class CharacterCreator
    {
        private NativeUI.MenuPool _menuPool;
        private NativeUI.UIMenu _mainMenu;

        private void InitializeMainMenu()
        {
            _menuPool = new NativeUI.MenuPool();
            _mainMenu = new NativeUI.UIMenu("Character Creator", "~HUD_COLOUR_FREEMODE~EDIT CHARACTER");

            _menuPool.Add(_mainMenu);
            // _mainMenu.Closed += OnMenuClosed;

            AddGenderMenu();
            AddHeritageMenu();
            AddFaceShapeMenu();
            AddAppearanceMenu();
            _menuPool.RefreshIndex();
        }

        private void EnableMenu()
        {
            _mainMenu.Visible = true;
            Tick += MenuPoolTick;
        }

        private void DisableMenu()
        {
            _mainMenu.Visible = false;
            Tick -= MenuPoolTick;
        }

        private async Task MenuPoolTick()
        {
            _menuPool.ProcessMenus();
            if (!_menuPool.IsAnyMenuOpen())
            {
                await OnMenuClosed();
            }

            await Task.FromResult(0);
        }

        private async Task OnMenuClosed()
        {
            await EndCharacterCreator();
        }

        private void AddGenderMenu()
        {
            int genderIndex = 0;
            if (_character.Gender == FemaleGender)
            {
                genderIndex = 1;
            }

            var genderItem =
                new NativeUI.UIMenuListItem("Sex", _genders, genderIndex, "Select the gender of your Character.");
            _mainMenu.AddItem(genderItem);

            genderItem.OnListChanged += (sender, index) =>
            {
                var selectedGender = _genders[index];
                _ = SetGender(selectedGender);
            };
        }

        private void AddHeritageMenu()
        {
            var momIndex = 0;
            var dadIndex = 0;

            var heritageMenu = _menuPool.AddSubMenu(_mainMenu, "Heritage", "Select to choose your parents.");
            var heritageWindow = new UIMenuHeritageWindow(momIndex, dadIndex);
            heritageMenu.AddWindow(heritageWindow);

            var momFaces = Parents.Moms.Select(mom => mom.Name).Cast<dynamic>().ToList();
            var dadFaces = Parents.Dads.Select(dad => dad.Name).Cast<dynamic>().ToList();

            var momList = new UIMenuListItem("Mom", momFaces, momIndex, "Select your Mom.");
            var dadList = new UIMenuListItem("Dad", dadFaces, dadIndex, "Select your Dad.");

            var resemblanceItem = new UIMenuSliderHeritageItem("Resemblance",
                "Select if your features are influenced more by your Mother or Father.", true)
            {
                Value = (int)(_character.Resemblance * 100)
            };
            var skintoneItem = new UIMenuSliderHeritageItem("Skin Tone",
                "Select if your skin tone is influenced more by your Mother or Father.", true)
            {
                Value = (int)(_character.Skintone * 100)
            };

            heritageMenu.AddItem(momList);
            heritageMenu.AddItem(dadList);
            heritageMenu.AddItem(resemblanceItem);
            heritageMenu.AddItem(skintoneItem);

            heritageMenu.OnListChange += (sender, listItem, newIndex) =>
            {
                if (listItem == momList)
                {
                    momIndex = newIndex;
                    var mom = Parents.Moms[momIndex];
                    _character.Mom = mom.Index;
                    heritageWindow.Index(momIndex, dadIndex);
                    RefreshPedHead();
                }
                else if (listItem == dadList)
                {
                    dadIndex = newIndex;
                    var dad = Parents.Dads[dadIndex];
                    _character.Dad = dad.Index;
                    heritageWindow.Index(momIndex, dadIndex);
                    RefreshPedHead();
                }
            };

            heritageMenu.OnSliderChange += (sender, item, newIndex) =>
            {
                if (item == resemblanceItem)
                {
                    _character.Resemblance = resemblanceItem.Value / 100.0f;
                    RefreshPedHead();
                }
                else if (item == skintoneItem)
                {
                    _character.Skintone = skintoneItem.Value / 100.0f;
                    RefreshPedHead();
                }
            };

            heritageMenu.OnMenuStateChanged += (oldMenu, newMenu, state) =>
            {
                if (state == MenuState.ChangeForward)
                {
                    SwitchCamera(_faceCamera, _bodyCamera);
                }
                else if (state == MenuState.ChangeBackward)
                {
                    SwitchCamera(_bodyCamera, _faceCamera);
                }
            };
        }

        private void AddFaceShapeMenu()
        {
            UIMenu subMenu = _menuPool.AddSubMenu(_mainMenu, "Face Shapes", "Select to alter your facial Features.");

            AddFaceShapeSlider(subMenu, "Nose Width", "Make changes to your physical Features.", _character.Nose1,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose1, 0); });
            AddFaceShapeSlider(subMenu, "Nose Peak Height", "Make changes to your physical Features.", _character.Nose2,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose2, 1); });
            AddFaceShapeSlider(subMenu, "Nose Peak Length", "Make changes to your physical Features.", _character.Nose3,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose3, 2); });
            AddFaceShapeSlider(subMenu, "Nose Bone Height", "Make changes to your physical Features.", _character.Nose4,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose4, 3); });
            AddFaceShapeSlider(subMenu, "Nose Peak Lowering", "Make changes to your physical Features.",
                _character.Nose5,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose5, 4); });
            AddFaceShapeSlider(subMenu, "Nose Bone Twist", "Make changes to your physical Features.", _character.Nose6,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Nose6, 5); });

            AddFaceShapeSlider(subMenu, "Eyebrow Depth", "Make changes to your physical Features.",
                _character.Eyebrows5,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Eyebrows5, 7); });
            AddFaceShapeSlider(subMenu, "Eyebrow Height", "Make changes to your physical Features.",
                _character.Eyebrows6,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Eyebrows6, 6); });

            AddFaceShapeSlider(subMenu, "Cheekbones Height", "Make changes to your physical Features.",
                _character.Cheeks1,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Cheeks1, 8); });
            AddFaceShapeSlider(subMenu, "Cheekbones Width", "Make changes to your physical Features.",
                _character.Cheeks2,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Cheeks2, 9); });

            AddFaceShapeSlider(subMenu, "Cheeks Width", "Make changes to your physical Features.", _character.Cheeks3,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Cheeks3, 10); });

            AddFaceShapeSlider(subMenu, "Eye Opening", "Make changes to your physical Features.", _character.EyeOpen,
                (_, value) => { ChangeFaceShapeValue(value, out _character.EyeOpen, 11); });

            AddFaceShapeSlider(subMenu, "Lips Thickness", "Make changes to your physical Features.",
                _character.LipsThick,
                (_, value) => { ChangeFaceShapeValue(value, out _character.LipsThick, 12); });

            AddFaceShapeSlider(subMenu, "Jaw Bone Width", "Make changes to your physical Features.", _character.Jaw1,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Jaw1, 13); });
            AddFaceShapeSlider(subMenu, "Jaw Bone Depth", "Make changes to your physical Features.", _character.Jaw2,
                (_, value) => { ChangeFaceShapeValue(value, out _character.Jaw2, 14); });

            AddFaceShapeSlider(subMenu, "Chin Height", "Make changes to your physical Features.", _character.ChinHeight,
                (_, value) => { ChangeFaceShapeValue(value, out _character.ChinHeight, 15); });
            AddFaceShapeSlider(subMenu, "Chin Depth", "Make changes to your physical Features.", _character.ChinLength,
                (_, value) => { ChangeFaceShapeValue(value, out _character.ChinLength, 16); });
            AddFaceShapeSlider(subMenu, "Chin Width", "Make changes to your physical Features.", _character.ChinWidth,
                (_, value) => { ChangeFaceShapeValue(value, out _character.ChinWidth, 17); });
            AddFaceShapeSlider(subMenu, "Chin Hole Size", "Make changes to your physical Features.",
                _character.ChinHole,
                (_, value) => { ChangeFaceShapeValue(value, out _character.ChinHole, 18); });

            AddFaceShapeSlider(subMenu, "Neck Thickness", "Make changes to your physical Features.",
                _character.NeckThick,
                (_, value) => { ChangeFaceShapeValue(value, out _character.NeckThick, 19); });

            subMenu.OnMenuStateChanged += (oldMenu, newMenu, state) =>
            {
                if (state == MenuState.ChangeForward)
                {
                    SwitchCamera(_faceCamera, _bodyCamera);
                }
                else if (state == MenuState.ChangeBackward)
                {
                    SwitchCamera(_bodyCamera, _faceCamera);
                }
            };
        }

        private void AddAppearanceMenu()
        {
            UIMenu subMenu = _menuPool.AddSubMenu(_mainMenu, "Appearance", "Select to change your Appearance.");

            // === Hair ===
            var hairItem = new UIMenuListItem("Hair", _maleHairNames, _character.Hair,
                "Make changes to your Appearance.");
            var hairColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Hair);
            hairItem.OnListChanged += (sender, index) =>
            {
                var colorPanel = sender.Panels.First() as UIMenuColorPanel;
                var hairColor = colorPanel?.CurrentSelection ?? 1;
                _character.Hair = index;
                _character.HairColor1 = hairColor;
                RefreshPedHair();
            };
            hairItem.AddPanel(hairColorPanel);
            subMenu.AddItem(hairItem);

            // === Eyebrows ===
            var eyebrowItem = new UIMenuListItem("Hair", _eyebrowsNames, _character.Eyebrows,
                "Make changes to your Appearance.");
            var browColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Hair);
            var browPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
            {
                Percentage = _character.Eyebrows2
            };
            eyebrowItem.OnListChanged += (sender, index) =>
            {
                var colorPanel = sender.Panels[0] as UIMenuColorPanel;
                var percentagePanel = sender.Panels[1] as UIMenuPercentagePanel;
                var percentage = percentagePanel?.Percentage ?? 1.0f;
                var color = colorPanel?.CurrentSelection ?? 1;
                _character.Eyebrows = sender.Index;
                _character.Eyebrows2 = percentage;
                _character.Eyebrows3 = color;
                RefreshPedEyebrows();
            };
            eyebrowItem.AddPanel(browColorPanel);
            eyebrowItem.AddPanel(browPercentageItem);
            subMenu.AddItem(eyebrowItem);

            // === Beard ===
            var beardItem = new UIMenuListItem("Facial Hair", _beardNames, _character.Beard,
                "Make changes to your Appearance.");
            beardItem.OnListChanged += (sender, index) =>
            {
                if (_character.Gender == FemaleGender)
                {
                    _character.Beard = 0;
                    beardItem.Index = _character.Beard;
                    UI.ShowNotification("Facial Hair unavailable for Female characters.");
                }
                else
                {
                    _character.Beard = sender.Index;
                    if (index == 0)
                    {
                        beardItem.RemovePanelAt(0);
                        beardItem.RemovePanelAt(0);
                        _character.Beard2 = 0;
                        _character.Beard3 = 0;
                    }
                    else
                    {
                        if (sender.Panels.Count == 0)
                        {
                            var beardColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Hair);
                            var beardPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                            {
                                Percentage = _character.Eyebrows2
                            };
                            beardItem.AddPanel(beardColorPanel);
                            beardItem.AddPanel(beardPercentageItem);
                        }

                        var colorPanel = sender.Panels[0] as UIMenuColorPanel;
                        var percentagePanel = sender.Panels[1] as UIMenuPercentagePanel;
                        var percentage = percentagePanel?.Percentage ?? 1.0f;
                        var color = colorPanel?.CurrentSelection ?? 1;
                        _character.Beard2 = percentage;
                        _character.Beard3 = color;
                    }

                    RefreshPedBeard();
                }
            };
            subMenu.AddItem(beardItem);

            // === Blemishes ===
            var blemishesItem = new UIMenuListItem("Skin Blemishes", _blemishes, _character.Bodyb1,
                "Make changes to your Appearance.");
            blemishesItem.OnListChanged += (sender, index) =>
            {
                _character.Bodyb1 = index;
                if (index == 0)
                {
                    blemishesItem.RemovePanelAt(0);
                    _character.Bodyb2 = 0;
                }
                else
                {
                    if (blemishesItem.Panels.Count == 0)
                    {
                        var percentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Bodyb2
                        };
                        blemishesItem.AddPanel(percentageItem);
                    }

                    var percentagePanel = sender.Panels[0] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    _character.Bodyb2 = percentage;
                }

                RefreshBlemishes();
            };
            subMenu.AddItem(blemishesItem);

            // === Aging ===
            var agingItem = new UIMenuListItem("Skin Aging", _agingNames, _character.Age1,
                "Make changes to your Appearance.");
            agingItem.OnListChanged += (sender, index) =>
            {
                _character.Age1 = index;
                if (index == 0)
                {
                    agingItem.RemovePanelAt(0);
                    _character.Age2 = 0;
                }
                else
                {
                    if (agingItem.Panels.Count == 0)
                    {
                        var agingPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Age2
                        };
                        agingItem.AddPanel(agingPercentageItem);
                    }

                    var percentagePanel = sender.Panels[0] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    _character.Age2 = percentage;
                }

                RefreshAging();
            };
            subMenu.AddItem(agingItem);


            // === Complexion ===
            var complexionItem = new UIMenuListItem("Skin Complexion", _complexion, _character.Complexion1,
                "Make changes to your Appearance.");
            complexionItem.OnListChanged += (sender, index) =>
            {
                _character.Complexion1 = index;
                if (index == 0)
                {
                    complexionItem.RemovePanelAt(0);
                    _character.Complexion2 = 0;
                }
                else
                {
                    if (complexionItem.Panels.Count == 0)
                    {
                        var percentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Age2
                        };
                        complexionItem.AddPanel(percentageItem);
                    }

                    var percentagePanel = sender.Panels[0] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    _character.Complexion2 = percentage;
                }

                RefreshComplexion();
            };
            subMenu.AddItem(complexionItem);


            // === Molefreckle ===
            var molefreckleItem = new UIMenuListItem("Moles & Freckles", _molefreckle, _character.Moles1,
                "Make changes to your Appearance.");
            molefreckleItem.OnListChanged += (sender, index) =>
            {
                _character.Moles1 = index;
                if (index == 0)
                {
                    molefreckleItem.RemovePanelAt(0);
                    _character.Moles2 = 0;
                }
                else
                {
                    if (molefreckleItem.Panels.Count == 0)
                    {
                        var percentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Age2
                        };
                        molefreckleItem.AddPanel(percentageItem);
                    }

                    var percentagePanel = sender.Panels[0] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    _character.Moles2 = percentage;
                }

                RefreshMoleFreckle();
            };
            subMenu.AddItem(molefreckleItem);


            // === SunDamage ===
            var sunDamageItem = new UIMenuListItem("Skin Damage", _sunDamage, _character.Sun1,
                "Make changes to your Appearance.");
            sunDamageItem.OnListChanged += (sender, index) =>
            {
                _character.Sun1 = index;
                if (index == 0)
                {
                    sunDamageItem.RemovePanelAt(0);
                    _character.Sun2 = 0;
                }
                else
                {
                    if (sunDamageItem.Panels.Count == 0)
                    {
                        var percentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Sun2
                        };
                        sunDamageItem.AddPanel(percentageItem);
                    }

                    var percentagePanel = sender.Panels[0] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    _character.Sun2 = percentage;
                }

                RefreshSunDamage();
            };
            subMenu.AddItem(sunDamageItem);

            // === EyeColor ===
            var eyeColorItem = new UIMenuListItem("Eye Color", _eyeColorNames, _character.EyeColor,
                "Make changes to your Appearance.");
            eyeColorItem.OnListChanged += (sender, index) =>
            {
                _character.EyeColor = index;
                RefreshEyeColor();
            };
            subMenu.AddItem(eyeColorItem);


            // === Makeup ===
            var makeupItem =
                new UIMenuListItem("Makeup", _makeups, _character.Makeup1, "Make changes to your Appearance.");
            makeupItem.OnListChanged += (sender, index) =>
            {
                _character.Makeup1 = sender.Index;
                if (index == 0)
                {
                    makeupItem.RemovePanelAt(0);
                    makeupItem.RemovePanelAt(0);
                    _character.Makeup2 = 0;
                    _character.Makeup3 = 0;
                }
                else
                {
                    if (sender.Panels.Count == 0)
                    {
                        var makeupColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
                        var makeupPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Makeup2
                        };
                        makeupItem.AddPanel(makeupColorPanel);
                        makeupItem.AddPanel(makeupPercentageItem);
                    }

                    var colorPanel = sender.Panels[0] as UIMenuColorPanel;
                    var percentagePanel = sender.Panels[1] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    var color = colorPanel?.CurrentSelection ?? 1;
                    _character.Makeup2 = percentage;
                    _character.Makeup3 = color;
                }

                RefreshMakeup();
            };
            subMenu.AddItem(makeupItem);


            // === Blush ===
            var blushItem =
                new UIMenuListItem("Blusher", _blushes, _character.Blush1, "Make changes to your Appearance.");
            blushItem.OnListChanged += (sender, index) =>
            {
                _character.Blush1 = sender.Index;
                if (index == 0)
                {
                    blushItem.RemovePanelAt(0);
                    blushItem.RemovePanelAt(0);
                    _character.Blush2 = 0;
                    _character.Blush3 = 0;
                }
                else
                {
                    if (sender.Panels.Count == 0)
                    {
                        var blushColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
                        var blushPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Blush2
                        };
                        blushItem.AddPanel(blushColorPanel);
                        blushItem.AddPanel(blushPercentageItem);
                    }

                    var colorPanel = sender.Panels[0] as UIMenuColorPanel;
                    var percentagePanel = sender.Panels[1] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    var color = colorPanel?.CurrentSelection ?? 1;
                    _character.Blush2 = percentage;
                    _character.Blush3 = color;
                }

                RefreshBlush();
            };
            subMenu.AddItem(blushItem);


            // === Lipstick ===
            var lipstickItem =
                new UIMenuListItem("Lipstick", _lipsticks, _character.Lipstick1, "Make changes to your Appearance.");
            lipstickItem.OnListChanged += (sender, index) =>
            {
                _character.Lipstick1 = sender.Index;
                if (index == 0)
                {
                    lipstickItem.RemovePanelAt(0);
                    lipstickItem.RemovePanelAt(0);
                    _character.Lipstick2 = 0;
                    _character.Lipstick3 = 0;
                }
                else
                {
                    if (sender.Panels.Count == 0)
                    {
                        var makeupColorPanel = new UIMenuColorPanel("Color", UIMenuColorPanel.ColorPanelType.Makeup);
                        var makeupPercentageItem = new UIMenuPercentagePanel("Opacity", "0%", "100%")
                        {
                            Percentage = _character.Eyebrows2
                        };
                        lipstickItem.AddPanel(makeupColorPanel);
                        lipstickItem.AddPanel(makeupPercentageItem);
                    }

                    var colorPanel = sender.Panels[0] as UIMenuColorPanel;
                    var percentagePanel = sender.Panels[1] as UIMenuPercentagePanel;
                    var percentage = percentagePanel?.Percentage ?? 1.0f;
                    var color = colorPanel?.CurrentSelection ?? 1;
                    _character.Lipstick2 = percentage;
                    _character.Lipstick3 = color;
                }

                RefreshLipstick();
            };
            subMenu.AddItem(lipstickItem);


            subMenu.OnMenuStateChanged += (oldMenu, newMenu, state) =>
            {
                if (state == MenuState.ChangeForward)
                {
                    SwitchCamera(_faceCamera, _bodyCamera);
                    if (_character.Gender == MaleGender)
                    {
                        hairItem.Items = _maleHairNames;
                        hairItem.Index = _character.Hair;
                        beardItem.Enabled = true;
                    }
                    else if (_character.Gender == FemaleGender)
                    {
                        hairItem.Items = _femaleHairNames;
                        hairItem.Index = _character.Hair;
                        _character.Beard = 0;
                        _character.Beard2 = 0;
                        _character.Beard3 = 0;
                        beardItem.Index = _character.Beard;
                        beardItem.RemovePanelAt(0);
                        beardItem.RemovePanelAt(0);
                        beardItem.Enabled = false;
                    }
                }
                else if (state == MenuState.ChangeBackward)
                {
                    SwitchCamera(_bodyCamera, _faceCamera);
                }
            };
        }

        private void AddFaceShapeSlider(UIMenu subMenu, string text, string description, float value,
            ItemSliderEvent onSliderChanged)
        {
            var slider = new UIMenuSliderItem(text, description, true)
            {
                Value = (int)Math.MapRange(value, -1, 1, 0, 100)
            };
            subMenu.AddItem(slider);
            slider.OnSliderChanged += onSliderChanged;
        }

        private void ChangeFaceShapeValue(float value, out float charValue, int faceFeatureIndex)
        {
            var newValue = Math.MapRange(value, 0, 100, -1, 1);
            charValue = newValue;
            API.SetPedFaceFeature(API.PlayerPedId(), faceFeatureIndex, charValue);
        }
    }
}