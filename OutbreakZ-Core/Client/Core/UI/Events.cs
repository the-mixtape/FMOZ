using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace OutbreakZCore.Client.Core
{
    public abstract partial class UI
    {
        /// <summary>
        /// Displays a notification on the screen.
        /// </summary>
        /// <param name="message">The notification text to display.</param>
        /// <remarks>
        /// This method is triggered by the <see cref="EventHandler"/> event "UI:ShowNotification".
        /// </remarks>
        [EventHandler("UI:ShowNotification")]
        public static void ShowNotification(string message)
        {
            Screen.ShowNotification(message);
        }

        /// <summary>
        /// Displays subtitles on the screen.
        /// </summary>
        /// <param name="message">The subtitle text to display.</param>
        /// <param name="duration">The duration in milliseconds for which the subtitles will be displayed. Default is 2500 ms.</param>
        /// <remarks>
        /// This method is triggered by the <see cref="EventHandler"/> event "UI:ShowSubtitle".
        /// </remarks>
        [EventHandler("UI:ShowSubtitle")]
        public static void ShowSubtitle(string message, int duration = 2500)
        {
            Screen.ShowSubtitle(message, duration);
        }
    }
}