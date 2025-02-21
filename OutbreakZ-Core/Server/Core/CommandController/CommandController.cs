using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using OutbreakZCore.Server.Core.CommandController.Commands;

namespace OutbreakZCore.Server.Core.CommandController
{
    public class CommandController : BaseScript
    {
        private readonly WeatherCommand _weatherCommands = new WeatherCommand();
        
        public CommandController()
        {
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            RegisterBaseCommand("time", _weatherCommands.OnTime);
            RegisterBaseCommand("settime", _weatherCommands.OnSetTime);
        }
        
        
        private void RegisterBaseCommand(string commandName, Func<int, List<object>, string, Task> command)
        {
            API.RegisterCommand(commandName, new Func<int, List<object>, string, Task>(command), false);
        }
    }
}