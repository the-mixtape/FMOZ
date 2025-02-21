using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server.Core
{
    public class Weather : BaseScript
    {
        private static string _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Extrasunny);
        private static double _baseTime = 0;
        private static double _timeOffset = 0;
        private static bool _freezeTime = false;
        private static int _newWeatherTimer = 10;

        private const bool DynamicWeather = true;
        private static readonly Random Rand = new Random();


        public Weather()
        {
        }

        [EventHandler("Weather:RequestSync")]
        private void RequestSync([FromSource] CitizenFX.Core.Player source)
        {
            TriggerClientEvent(source, "Weather:UpdateWeather", _currentWeather);
            TriggerClientEvent(source, "Weather:UpdateTime", _baseTime, _timeOffset, _freezeTime);
            Tick += UpdateTimeTick;
            Tick += UpdateWeatherTick;
            Tick += AutoUpdateTime;
            Tick += AutoUpdateWeather;
        }

        private static async Task UpdateWeatherTick()
        {
            _newWeatherTimer--;
            await Delay(60000);
            if (_newWeatherTimer == 0)
            {
                if (DynamicWeather)
                {
                    NextWeatherStage();
                }

                _newWeatherTimer = 10;
            }
        }

        private static void NextWeatherStage()
        {
            var currentWeather = WeatherTypes.GetWeatherType(_currentWeather);

            if (currentWeather == WeatherTypes.Types.Clear ||
                currentWeather == WeatherTypes.Types.Clouds ||
                currentWeather == WeatherTypes.Types.Extrasunny)
            {
                var isClearing = Rand.Next(0, 2) == 0;
                if (isClearing)
                {
                    _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Clearing);
                }
                else
                {
                    _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Overcast);
                }
            }
            else if (currentWeather == WeatherTypes.Types.Clearing ||
                     currentWeather == WeatherTypes.Types.Overcast)
            {
                int newWeather = Rand.Next(1, 6);
                switch (newWeather)
                {
                    case 1:
                        if (currentWeather == WeatherTypes.Types.Clearing)
                        {
                            _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Foggy);
                        }
                        else
                        {
                            _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Rain);
                        }

                        break;
                    case 2:
                        _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Clouds);
                        break;
                    case 3:
                        _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Clear);
                        break;
                    case 4:
                        _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Extrasunny);
                        break;
                    case 5:
                        _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Smog);
                        break;
                    default:
                        _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Foggy);
                        break;
                }
            }
            else if (currentWeather == WeatherTypes.Types.Thunder ||
                     currentWeather == WeatherTypes.Types.Rain)
            {
                _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Clearing);
            }
            else if (currentWeather == WeatherTypes.Types.Smog ||
                     currentWeather == WeatherTypes.Types.Foggy)
            {
                _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Clearing);
            }

            TriggerClientEvent("Weather:UpdateWeather", _currentWeather);
            Debug.WriteLine($"[WEATHER] New random weather type has been generated: {_currentWeather}");
        }

        private async Task AutoUpdateWeather()
        {
            await Delay(300000);
            TriggerClientEvent("Weather:UpdateWeather", _currentWeather);
        }

        private async Task AutoUpdateTime()
        {
            await Delay(5000);
            TriggerClientEvent("Weather:UpdateTime", _baseTime, _timeOffset, _freezeTime);
        }

        private async Task UpdateTimeTick()
        {
            var newBaseTime = CalculateBaseTime();
            if (_freezeTime)
            {
                var deltaTime = _baseTime - newBaseTime;
                _timeOffset += deltaTime;
            }

            _baseTime = newBaseTime;
            await Delay(0);
        }

        private double CalculateBaseTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            var unixTimestamp = new DateTimeOffset(utcNow).ToUnixTimeSeconds();
            var result = (unixTimestamp / 2) + 360;
            return result;
        }

        public static string GetTimeString()
        {
            var hour = (int)((_baseTime + _timeOffset) / 60) % 24;
            var minute = (int)((_baseTime + _timeOffset) % 60);
            return $"{hour:00}:{minute:00}";
        }

        public static void SetTime(int hour, int minute)
        {
            hour = Shared.Utils.Math.Clamp(hour, 0, 24);
            minute = Shared.Utils.Math.Clamp(minute, 0, 60);
            ShiftToHour(hour);
            ShiftToMinute(minute);
            TriggerClientEvent("Weather:UpdateTime", _baseTime, _timeOffset, _freezeTime);
        }

        private static void ShiftToHour(int hour)
        {
            _timeOffset -= ((_baseTime + _timeOffset) / 60 % 24 - hour) * 60;
        }

        private static void ShiftToMinute(int minute)
        {
            _timeOffset -= ((_baseTime + _timeOffset) % 60 - minute);
        }
    }
}