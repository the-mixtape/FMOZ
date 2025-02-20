using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace OutbreakZCore.Client.Core.Utils
{
    /// <summary>
    /// Abstract partial class responsible for managing player spawning in the game.
    /// Inherits from <see cref="BaseScript"/> for integration with the CitizenFX.Core framework.
    /// </summary>
    public abstract partial class Spawner : BaseScript
    {
        
        private static bool _spawnLock = false;
        
        /// <summary>
        /// Spawns the player at a specified location with a given model and heading.
        /// </summary>
        /// <param name="model">The model to apply to the player.</param>
        /// <param name="position">The position (coordinates) where the player will spawn.</param>
        /// <param name="heading">The heading (rotation) of the player at the spawn location, in degrees.</param>
        /// <remarks>
        /// This method handles the entire spawning process, including screen fading, model changing, collision loading, and player unfreezing.
        /// It ensures that the player is spawned correctly and safely.
        /// </remarks>
        public static async Task SpawnPlayer(Model model, Vector3 position, float heading)
        {
            if (_spawnLock) return;
            _spawnLock = true;

            DoScreenFadeOut(500);

            while (IsScreenFadingOut())
            {
                await Delay(1);
            }

            FreezePlayer(PlayerId(), true);
            await CitizenFX.Core.Game.Player.ChangeModel(model);
            SetPedDefaultComponentVariation(GetPlayerPed(-1));
            RequestCollisionAtCoord(position.X, position.Y, position.Z);

            var ped = GetPlayerPed(-1);

            SetEntityCoordsNoOffset(ped, position.X, position.Y, position.Z, false, false, false);
            NetworkResurrectLocalPlayer(position.X, position.Y, position.Z, heading, true, true);
            ClearPedTasksImmediately(ped);
            RemoveAllPedWeapons(ped, false);
            ClearPlayerWantedLevel(PlayerId());
            
            while (!HasCollisionLoadedAroundEntity(ped))
            {
                await Delay(1);
            }

            ShutdownLoadingScreen();
            DoScreenFadeIn(500);
            
            while (IsScreenFadingIn())
            {
                await Delay(1);
            }
            
            FreezePlayer(PlayerId(), false);

            //TriggerEvent("playerSpawned", PlayerId());
                
            _spawnLock = false;
        }

        /// <summary>
        /// Freezes or unfreezes the player, controlling their movement and visibility.
        /// </summary>
        /// <param name="playerId">The ID of the player to freeze or unfreeze.</param>
        /// <param name="freeze">If <c>true</c>, the player will be frozen; otherwise, the player will be unfrozen.</param>
        /// <remarks>
        /// This method also handles visibility, collision, and invincibility states of the player.
        /// </remarks>
        private static void FreezePlayer(int playerId, bool freeze)
        {
            var ped = GetPlayerPed(playerId);
            
            SetPlayerControl(playerId, !freeze, 0);

            if (!freeze)
            {
                if (!IsEntityVisible(ped))
                    SetEntityVisible(ped, true, false);
                
                if (!IsPedInAnyVehicle(ped, true))
                    SetEntityCollision(ped, true, true);

                FreezeEntityPosition(ped, false);
                //SetCharNeverTargetted(ped, false)
                SetPlayerInvincible(playerId, false);
            } 
            else 
            {
                if (IsEntityVisible(ped))
                    SetEntityVisible(ped, false, false);

                SetEntityCollision(ped, false, true);
                FreezeEntityPosition(ped, true);
                //SetCharNeverTargetted(ped, true)
                SetPlayerInvincible(playerId, true);
                
                if (IsPedFatallyInjured(ped))
                    ClearPedTasksImmediately(ped);
            }
        }
    }
}