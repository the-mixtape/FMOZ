using System;
using System.Collections.Generic;
using System.Linq;

namespace OutbreakZCore.Shared
{
    public class WeatherTypes
    {
        public enum Types
        {
            None,
            Clear,
            Extrasunny,
            Clouds,
            Overcast,
            Rain,
            Clearing,
            Thunder,
            Smog,
            Foggy,
            Xmas,
            Snow,
            SnowLight,
            Blizzard,
            Halloween,
            Neutral,
        }

        public static bool HasWeatherType(string weather)
        {
            return WeatherTypesDict.ContainsValue(weather);
        }

        public static string GetWeatherString(Types weather)
        {
            return WeatherTypesDict.TryGetValue(weather, out var value) ? value : "";
        }

        public static Types GetWeatherType(string weather)
        {
            foreach (var pair in WeatherTypesDict)
            {
                if (pair.Value == weather)
                {
                    return pair.Key;
                }
            }

            return Types.None;
        }

        private static readonly Dictionary<Types, string> WeatherTypesDict = new Dictionary<Types, string>
        {
            { Types.None, "" },
            { Types.Clear, "CLEAR" },
            { Types.Extrasunny,"EXTRASUNNY" },
            { Types.Clouds,"CLOUDS" },
            { Types.Overcast,"OVERCAST" },
            { Types.Rain,"RAIN" },
            { Types.Clearing,"CLEARING" },
            { Types.Thunder,"THUNDER" },
            { Types.Smog,"SMOG" },
            { Types.Foggy,"FOGGY" },
            { Types.Xmas,"XMAS" },
            { Types.Snow,"SNOW" },
            { Types.SnowLight,"SNOWLIGHT" },
            { Types.Blizzard,"BLIZZARD" },
            { Types.Halloween,"HALLOWEEN" },
            { Types.Neutral,"NEUTRAL" },
        };
    }
}