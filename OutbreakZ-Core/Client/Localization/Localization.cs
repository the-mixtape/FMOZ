using System.Collections.Generic;
using CitizenFX.Core;

namespace OutbreakZCore.Client
{
    public static class Localization
    {
        public enum Languages
        {
            English = 0,
            Russian = 1
        }

        public static class Keys
        {
            public const string RespawnButton = "Buttons:Respawn_Button";

            public const string YouAreDead = "Title:You_Are_Deed";
        }
        
        private const Languages DefaultLanguage = Languages.English;

        private static readonly Dictionary<string, string> EnDictionary;
        private static readonly Dictionary<string, string> RuDictionary;
        
        static Localization()
        {
            EnDictionary = new Dictionary<string, string>()
            {
                [Keys.RespawnButton] = "Respawn",
                [Keys.YouAreDead] = "You are dead",
            };
            
            RuDictionary = new Dictionary<string, string>()
            {
                [Keys.RespawnButton] = "Возродится",
                [Keys.YouAreDead] = "Вы мертвы",
            };
        }

        public static Languages GtaLangToLanguages(int langId)
        {
            if (langId == 7)
            {
                return Languages.Russian;
            }
            
            return DefaultLanguage;
        }

        public static string _(Languages lang, string key)
        {
            var localError = $"Locale {lang.ToString()} does not exist.";
            var translationError = $"Translation {key} does not exist.";

            var landDictionary = GetLandDictionary(lang);
            if (landDictionary == null)
            {
                return localError;
            }

            return landDictionary.TryGetValue(key, out var value) ? value : translationError;
        }

        private static Dictionary<string, string> GetLandDictionary(Languages languages)
        {
            switch (languages)
            {
                case Languages.English:
                    return EnDictionary;
                case Languages.Russian:
                    return RuDictionary;
                default:
                    return null;
            }
        }
    }
}