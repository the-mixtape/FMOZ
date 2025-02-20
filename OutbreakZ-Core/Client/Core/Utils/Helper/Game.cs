
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Utils
{
    public class Game
    {
        public static void DrawText3D(string text, float x, float y, float z)
        {
            var screenX = (Screen.Resolution.Width / 2f);
            var screenY = (Screen.Resolution.Height / 2f);
            var onScreen = GetScreenCoordFromWorldCoord(x, y, z, ref screenX, ref screenY);
            var playerCam = GetFinalRenderedCamCoord();
            var distance = GetDistanceBetweenCoords(playerCam.X, playerCam.Y, playerCam.Z, x, y, z, true);
            var scale = ((1 / distance) * 2);
            var fov = ((1 / GetGameplayCamFov()) * 100);
            scale *= fov;
            if (onScreen)
            {
                SetTextScale((0.0f + scale), (0.35f + scale));
                SetTextFont(0);
                SetTextProportional(true);
                SetTextColour(255, 255, 255, 255);
                SetTextDropshadow(0, 0, 0, 0, 255);
                SetTextDropShadow();
                SetTextOutline();
                SetTextEntry("STRING");
                SetTextCentre(true);
                AddTextComponentString(text);
                DrawText(screenX, screenY);
            }
        }
        
        public static void DrawText2D(string text, float x, float y, float scale, int font, int justification, int red, int green, int blue, int alpha)
        {
            SetTextScale(scale, scale);
            SetTextFont(font);
            SetTextProportional(true);
            SetTextColour(red, green, blue, alpha);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            SetTextJustification(justification);
            AddTextComponentString(text);
            DrawText(x, y);
        }
    }
}