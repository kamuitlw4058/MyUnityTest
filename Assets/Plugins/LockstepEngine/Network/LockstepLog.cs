#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Lockstep.Network {
    public class LockstepLog {
        public static void Error(string msg){
#if UNITY_5_3_OR_NEWER
            Debug.LogError(msg);
#else
            Console.WriteLine(msg);
#endif
        }
    }
}