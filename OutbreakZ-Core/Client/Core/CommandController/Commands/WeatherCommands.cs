using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class WeatherCommands : BaseScript
    {
        public async Task OnShowTime(int source, List<object> args, string rawCommand)
        {
            var timeString = Weather.GetTimeString();
            UI.ShowNotification($"Current Time: {timeString}");
            await Task.FromResult(0);
        }
        
        public async Task OnSetTime(int source, List<object> args, string rawCommand)
        {
            if (args.Count != 2)
            {
                Debug.WriteLine($"settime [hour] [minute]");
                return;
            }
                
            if (!int.TryParse(args[0].ToString(), out var hour))
            {
                Debug.WriteLine($"First argument is invalid");
                return;
            }
                    
            if (!int.TryParse(args[1].ToString(), out var minute))
            {
                Debug.WriteLine($"Second argument is invalid");
                return;
            }
                    
            hour = Shared.Utils.Math.Clamp(hour, 0, 24);
            minute = Shared.Utils.Math.Clamp(minute, 0, 60);
                    
            Weather.SetTime(hour, minute);
            await Task.FromResult(0);
        }


        public async Task OnSetWeather(int source, List<object> args, string rawCommand)
        {
            if (args.Count != 1)
            {
                Debug.WriteLine($"setweather [weathertype]");
                return;
            }

            string weather = args[0].ToString();
            if (!WeatherTypes.IsValidWeather(weather))
            {
                string avaiableWeather = WeatherTypes.GetAvailableWeatherTypes();
                UI.ShowNotification($"Invalid weather type, valid weather types are: {avaiableWeather}");
                return;
            }
            
            Weather.SetWeather(weather);
            await Task.FromResult(0);
        }

        public async Task OnFreezeTime(int source, List<object> args, string rawCommand)
        {
            Weather.ToggleFreezeTime();
            // UI.ShowNotification($"Freeze time has been toggled");
            await Task.FromResult(0);
        }
        
        public async Task OnFreezeWeather(int source, List<object> args, string rawCommand)
        {
            Weather.ToggleFreezeWeather();
            // UI.ShowNotification($"Freeze weather has been toggled");
            await Task.FromResult(0);
        }
    }
}