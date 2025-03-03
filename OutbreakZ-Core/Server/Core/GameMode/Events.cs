using CitizenFX.Core;
using OutbreakZCore.Shared.Utils;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Server.Core
{
    public partial class GameMode
    {
        [EventHandler("GameMode.InitPlayer")]
        private void OnInitPlayer([FromSource] CitizenFX.Core.Player source)
        {
            var license = source.Identifiers["license"];
            
            var spawnPosition = GetNextSpawnPosition();
            Debug.WriteLine($"{source.Name}#{license} spawn in position: {spawnPosition}");
            TriggerClientEvent(source, "GameMode.RespawnPlayer", Converter.ToJson(spawnPosition));
        }

        [EventHandler("GameMode.RespawnPlayer")]
        private void OnRespawnPlayer([FromSource] CitizenFX.Core.Player source)
        {
            var spawnPosition = GetNextSpawnPosition();
            Debug.WriteLine($"{source.Name} respawned in position: {spawnPosition}");
            TriggerClientEvent(source, "GameMode.RespawnPlayer", Converter.ToJson(spawnPosition));
        }
    }
}