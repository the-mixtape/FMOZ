using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace OutbreakZCore.Server.Core.CommandController.Commands
{
    public class WeatherCommand : BaseScript
    {
        public async Task OnTime(int source, List<object> args, string rawCommand)
        {
            if (source == 0)
            {
                var timeString = Weather.GetTimeString();
                Debug.WriteLine($"Current Time: {timeString}");
            }
            
            await Task.FromResult(0);
        }


        public async Task OnSetTime(int source, List<object> args, string rawCommand)
        {
            if (source == 0)
            {
                if (args.Count != 2)
                {
                    Debug.WriteLine($"settime [hour] [minute]");
                }
                
                if (!int.TryParse(args[0].ToString(), out var hour))
                {
                    Debug.WriteLine($"First argument is invalid");
                }
                    
                if (!int.TryParse(args[1].ToString(), out var minute))
                {
                    Debug.WriteLine($"Second argument is invalid");
                }
                    
                hour = Shared.Utils.Math.Clamp(hour, 0, 24);
                minute = Shared.Utils.Math.Clamp(minute, 0, 60);
                    
                Weather.SetTime(hour, minute);
                Debug.WriteLine($"Time has changed to {hour:00}:{minute:00}");
            }
            await Task.FromResult(0);
        }
    }
}