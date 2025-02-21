using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Shared;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core
{
    public class Weather : BaseScript
    {
        private string _currentWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Extrasunny);
        private string _lastWeather = WeatherTypes.GetWeatherString(WeatherTypes.Types.Extrasunny);
        private double _baseTime = 0;
        private double _timeOffset = 0;
        private int _timer = 0;
        private bool _freezeTime;
        
        private const float WeatherTransitionTime = 15.0f;

        public Weather()
        {
            EventHandlers["Weather:UpdateWeather"] += new Action<string>(UpdateWeather);
            EventHandlers["Weather:UpdateTime"] += new Action<double, double, bool>(UpdateTime);
            Tick += UpdateWeatherTick;
            Tick += UpdateTimeTick;
        }
        
        public static void RequestSync()
        {
            TriggerServerEvent("Weather:RequestSync");
        }

        private void UpdateWeather(string newWeather)
        {
            _currentWeather = newWeather;
        }

        private void UpdateTime(double baseTime, double offset, bool freeze)
        {
            _freezeTime = freeze;
            _timeOffset = offset;
            _baseTime = baseTime;
        }

        private async Task UpdateWeatherTick()
        {
            if (_lastWeather != _currentWeather)
            {
                _lastWeather = _currentWeather;
                SetWeatherTypeOverTime(_currentWeather, WeatherTransitionTime);
                await Delay((int)WeatherTransitionTime * 1000);
            }
            
            // SetBlackout(_blackout);
            // ClearOverrideWeather();
            // ClearWeatherTypePersist();
            SetWeatherTypePersist(_lastWeather);
            SetWeatherTypeNow(_lastWeather);
            SetWeatherTypeNowPersist(_lastWeather);
            
            var isXmas = WeatherTypes.GetWeatherType(_lastWeather) == WeatherTypes.Types.Xmas;
            SetForceVehicleTrails(isXmas);
            SetForcePedFootstepsTracks(isXmas);
            await Delay(100);
        }


        private async Task UpdateTimeTick()
        {
            var newBaseTime = _baseTime;
            if (GetGameTimer() - 500 > _timer)
            {
                newBaseTime += 0.25f;
                _timer = GetGameTimer();
            }
            
            if (_freezeTime)
            {
                var deltaTime = _baseTime - newBaseTime;
                _timeOffset += deltaTime;
            }

            _baseTime = newBaseTime;
            var hour = (int)((_baseTime + _timeOffset) / 60) % 24;
            var minute = (int)((_baseTime + _timeOffset) % 60);
            NetworkOverrideClockTime(hour, minute, 0);
            
            await Delay(0);
        }

    }
}