using CitizenFX.Core;
using OutbreakZCore.Shared.Helpers;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server.Core
{
    public partial class GameMode
    {
        [EventHandler("GameMode:InitPlayer")]
        private void OnInitPlayer([FromSource] CitizenFX.Core.Player source)
        {
            var fiveMId = int.Parse(source.Identifiers["fivem"]);
            
            var spawnPosition = GetRandomSpawnPosition();
            Debug.WriteLine($"{source.Name}#{fiveMId} spawn in position: {spawnPosition}");
            TriggerClientEvent("GameMode:SetPlayerPosition", Converter.ToJson(spawnPosition));
        }
    }
}