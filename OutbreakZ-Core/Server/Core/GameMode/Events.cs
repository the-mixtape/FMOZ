using CitizenFX.Core;
using OutbreakZCore.Shared.Utils;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server.Core
{
    public partial class GameMode
    {
        [EventHandler("GameMode:InitPlayer")]
        private void OnInitPlayer([FromSource] CitizenFX.Core.Player source)
        {
            var license = source.Identifiers["license"];
            
            var spawnPosition = GetRandomSpawnPosition();
            Debug.WriteLine($"{source.Name}#{license} spawn in position: {spawnPosition}");
            TriggerClientEvent(source, "GameMode:SetPlayerPosition", Converter.ToJson(spawnPosition));
        }
    }
}