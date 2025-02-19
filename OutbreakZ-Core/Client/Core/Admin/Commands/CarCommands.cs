using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Admin.Commands
{
    public class CarCommands : BaseScript
    {
        private const string DefaultVehicle = "bmx";
        private readonly List<int> _spawnedVehicles = new List<int>();

        public async Task OnSpawn(int source, List<object> args, string rawCommand)
        {
            string vehicleName = DefaultVehicle;
            if (args.Count > 0)
            {
                vehicleName = args[0].ToString();   
            }
                
            uint vehicleHash = (uint)GetHashKey(vehicleName);

            if (!IsModelInCdimage(vehicleHash) || !IsModelAVehicle(vehicleHash))
            {
                Debug.WriteLine($"[ERROR] Vehicle model {vehicleName} is invalid!");
                return;
            }

            RequestModel(vehicleHash);
            while (!HasModelLoaded(vehicleHash))
            {
                await Delay(100);
            }

            var playerPed = GetPlayerPed(-1);
            Vector3 position = GetEntityCoords(playerPed, true);
            int vehicle = CreateVehicle(vehicleHash, position.X, position.Y, position.Z, GetEntityHeading(playerPed), true, true);

            if (vehicle == 0)
            {
                Debug.WriteLine($"[ERROR] Vehicle not spawned!");
                return;
            }
            
            _spawnedVehicles.Add(vehicle);
            
            int netId = NetworkGetNetworkIdFromEntity(vehicle);
            SetNetworkIdCanMigrate(netId, true); // Позволяет передавать объект другим игрокам
            SetEntityAsMissionEntity(vehicle, true, true);
            SetVehicleNumberPlateText(vehicle, "O1488Z");

            SetPedIntoVehicle(playerPed, vehicle, -1);
            SetModelAsNoLongerNeeded(vehicleHash);
        }


        public Task OnClear(int source, List<object> args, string rawCommand)
        {
            foreach (int vehicle in _spawnedVehicles)
            {
                int vehicleHandle = vehicle;
                if (DoesEntityExist(vehicle))
                {
                    SetEntityAsMissionEntity(vehicleHandle, true, true);
                    DeleteVehicle(ref vehicleHandle);
                }
            }
            
            _spawnedVehicles.Clear();
            Debug.WriteLine("[INFO] All spawned vehicles have been deleted.");
            return Task.FromResult(true);
        }
    }
}