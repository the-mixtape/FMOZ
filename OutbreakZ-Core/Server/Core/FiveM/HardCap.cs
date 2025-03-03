using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace OutbreakZCore.Server.Core.FiveM
{
    public class HardCap : BaseScript
    {
        private readonly Dictionary<int, DateTime> _activePlayers = new Dictionary<int, DateTime>();
        private DateTime _serverStartTime = DateTime.UtcNow;
        private readonly int _maxClients;

        public HardCap()
        {
            RegisterEventHandler("HardCap.PlayerActivated", new Action<CitizenFX.Core.Player>(PlayerActivated));
            RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(PlayerDropped));
            RegisterEventHandler("playerConnecting", new Action<CitizenFX.Core.Player, string, CallbackDelegate>(PlayerConnecting));
            RegisterEventHandler("HardCap.RequestPlayerTimestamps", new Action<CitizenFX.Core.Player>(HandleRequest));

            _maxClients = API.GetConvarInt("sv_maxclients", 32);
            Debug.WriteLine("HardCap initialized");
        }

        private void HandleRequest([FromSource] CitizenFX.Core.Player source)
        {
            // try
            // {
            //     // source.TriggerEvent("Scoreboard.ReceivePlayerTimestamps", (activePlayers), (serverStartTime));
            // }
            // catch (Exception ex)
            // {
            //     Debug.WriteLine($"HardCap HandleRequest Error: {ex.Message}");
            // }
        }

        private async void PlayerActivated([FromSource] CitizenFX.Core.Player source)
        {
            var sessionId = Int32.Parse(source.Handle);
            if (_activePlayers.ContainsKey(sessionId)) return;
            _activePlayers.Add(sessionId, DateTime.UtcNow);

            var license = source.Identifiers["license"];
            Debug.WriteLine($"New session Id: {sessionId}, player: {source.Name}");
            
            if (!string.IsNullOrEmpty(license))
            {
                var (userId, wasCreated) = await Database.Database.GetOrCreateUser(license);
                if (userId != null)
                {
                    string joinMessage = wasCreated ? "Joined for first time to server!" : "Joined to server!"; 
                    Debug.WriteLine($"{source.Name}#{userId:0000} - {joinMessage}");
                    return;
                }
            }
            
            API.DropPlayer(source.Handle, "Unknown error!!! Please try again later.");
        }

        private void PlayerDropped([FromSource] CitizenFX.Core.Player source, string reason)
        {
            try
            { 
                int sessionId = Int32.Parse(source.Handle);
                if (_activePlayers.ContainsKey(sessionId))
                {
                    _activePlayers.Remove(sessionId);
                    
                    var license = source.Identifiers["license"];
                    Debug.WriteLine($"New session Id: {sessionId}, License: {license}");
                }
                TriggerClientEvent("playerDropped", source.Handle, reason);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"HardCap PlayerDropped Error: {ex.Message}");
            }
        }

        private void PlayerConnecting([FromSource] CitizenFX.Core.Player source, string playerName, CallbackDelegate denyWithReason)
        {
            try
            {
                Debug.WriteLine($"Connecting: '{source.Name}' (" +
                                $"steam: {source.Identifiers.Where(i => i.Contains("steam")).FirstOrDefault().ToString()} " +
                                $"ip: {source.Identifiers.Where(i => i.Contains("ip")).FirstOrDefault().ToString()}" +
                                $") | Player count {_activePlayers.Count}/{_maxClients}");

                if (_activePlayers.Count >= _maxClients)
                {
                    denyWithReason?.Invoke($"The server is full with {_activePlayers.Count}/{_maxClients} players on.");
                    API.CancelEvent();
                }
                TriggerClientEvent("playerConnecting", source.Handle, playerName);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"HardCap PlayerConnecting Error: {ex.Message}");
            }
        }

        private void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}