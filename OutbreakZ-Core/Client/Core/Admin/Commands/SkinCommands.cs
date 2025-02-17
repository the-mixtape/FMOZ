using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class SkinCommands : BaseScript
    {
        private static readonly Array PedHashes = Enum.GetValues(typeof(PedHash));
        
        public async Task OnSetSkin(int source, List<object> args, string rawCommand)
        {
            if (args.Count == 0)
            {
                UI.ShowNotification("/setskin [model]");
                return;
            }
            
            string modelName = args[0].ToString();
            uint modelHash = (uint)Game.GenerateHash(modelName);
            await ChangeSkin(modelName, modelHash);
        }
        
        
        public async Task OnRandomSkin(int source, List<object> args, string rawCommand)
        {   
            Random rnd = new Random();
            PedHash randomSkin = (PedHash)PedHashes.GetValue(rnd.Next(PedHashes.Length));
            await ChangeSkin(randomSkin.ToString(), (uint)randomSkin);
        }

        private async Task ChangeSkin(string modelName, uint modelHash)
        {
            if (!IsModelInCdimage(modelHash) || !IsModelValid(modelHash))
            {
                UI.ShowNotification($"Invalid model name. {modelName}");
                return;
            }

            await LoadModel(modelHash);
            SetPlayerModel(Game.Player.Handle, modelHash);
            SetModelAsNoLongerNeeded(modelHash);
            UI.ShowNotification($"Skin changed {modelName}.");
        }

        private async Task LoadModel(uint modelHash)
        {
            RequestModel(modelHash);
            while (!HasModelLoaded(modelHash))
            {
                await Delay(100);
            }
        }
    }
}