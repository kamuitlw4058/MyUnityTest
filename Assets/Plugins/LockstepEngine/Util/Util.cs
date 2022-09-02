using UnityGameFramework.Runtime;

namespace Lockstep.Util {
    public class Utils {
        public static void StartServices()
        {
            LTime.DoStart(false);
        }

        public static void UpdateServices(float fDeltaTime)
        {
            LTime.DoUpdate(fDeltaTime);
        }
    }
}