using System;

namespace Lockstep.Util {
    public class LTime {
        /// The total number of frames that have passed (Read Only).
        public static int FrameCount { get; private set; }

        /// The time in seconds it took to complete the last frame (Read Only).
        public static float DeltaTime { get; private set; }

        /// The time this frame has started (Read Only). This is the time in seconds since the last level has been loaded.
        public static float TimeSinceLevelLoad { get; private set; }
        
        public static bool IsClientMode { get; private set; }
        public static long RealtimeSinceStartupMs =>  (long)(DateTime.Now - m_InitTime).TotalMilliseconds;

        private static DateTime m_InitTime;
        private static DateTime m_LastFrameTime;

        public static void DoStart(bool isClientMode)
        {
            m_InitTime = DateTime.Now;
            IsClientMode = isClientMode;
        }

        public static void DoUpdate(float fDeltaTime)
        {
            var now = DateTime.Now;
            DeltaTime = (float) ((now - m_LastFrameTime).TotalSeconds);
            TimeSinceLevelLoad = (float) ((now - m_InitTime).TotalSeconds);
            FrameCount++;
            m_LastFrameTime = now;
        }
    }
}