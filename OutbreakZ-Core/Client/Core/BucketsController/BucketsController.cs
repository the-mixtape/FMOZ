using CitizenFX.Core;
using CitizenFX.Core.Native;
using OutbreakZCore.Shared;

namespace OutbreakZCore.Client.Core
{
    public class BucketsController : BaseScript
    {
        public static void SetLocalPlayerRoutingBucket(RoutingBuckets bucketId)
        {
            int netId = API.GetPlayerServerId(API.PlayerId());
            TriggerServerEvent("BucketsController:SetPlayerRoutingBucket", netId, (int)bucketId);            
        }
    }
}