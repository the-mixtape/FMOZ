using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Server.Core.BucketsController
{
    public class BucketsController : BaseScript
    {
        private static readonly HashSet<int> UsedBuckets = new HashSet<int>();

        private int GetUnusedBucketId()
        {
            ClearEmptyBuckets();
            int newBucket = 1;
            while (UsedBuckets.Contains(newBucket))
            {
                newBucket++;
            }
            UsedBuckets.Add(newBucket);
            return newBucket;
        }

        private void FreeBucket(int bucketId)
        {
            Debug.WriteLine($"Free bucket {bucketId}");
            UsedBuckets.Remove(bucketId);
        }

        private void ClearEmptyBuckets()
        {
            HashSet<int> used = new HashSet<int>();
            foreach (CitizenFX.Core.Player player in Players)
            {
                if(player == null || player.Character == null) continue;
                var id = API.GetPlayerRoutingBucket(player.Character.Handle.ToString());
                used.Add(id);
            }
            
            var forDelete = new HashSet<int>(UsedBuckets);
            forDelete.ExceptWith(used);

            foreach (var id in forDelete)
            {
                FreeBucket(id);
            }
        }

        private int GetBucketIdForType(RoutingBuckets type)
        {
            if (type == RoutingBuckets.Uniq)
            {
                return GetUnusedBucketId();
            }

            return 0;
        }
        
        [EventHandler("BucketsController:SetPlayerRoutingBucket")]
        private void OnSetPlayerRoutingBucket([FromSource] CitizenFX.Core.Player source, int netId, int bucketType)
        {
            var entityId = API.NetworkGetEntityFromNetworkId(netId);
            var type = (RoutingBuckets)bucketType;

            var id = GetBucketIdForType(type);
            API.SetPlayerRoutingBucket(entityId.ToString(), id);
            Debug.WriteLine($"Set player#{netId} routing bucket {id}");   
        }
    }
}