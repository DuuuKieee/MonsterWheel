namespace MonsterWheel.Core
{
    public struct WheelSettings
    {
        /// <summary>Ring light speed while spinning, in slots per second.</summary>
        public float RingSpinSpeed;
        /// <summary>Reel speed while spinning, in icons per second.</summary>
        public float ReelSpinSpeed;
        /// <summary>Time to ramp from rest to full spin speed.</summary>
        public float SpinUpTime;
        /// <summary>Overshoot amount of the reel's OutBack stop.</summary>
        public float ReelBackOvershoot;
        /// <summary>Pause between the reel stopping and the ring starting to stop.</summary>
        public float HandoffDelay;
        /// <summary>Upper bound for a single Tick's dt.</summary>
        public float MaxDeltaTime;
        /// <summary>Stops never begin slower than this fraction of full speed (avoids endless stops during spin-up).</summary>
        public float MinStopSpeedRatio;

        public static WheelSettings Default => new WheelSettings
        {
            RingSpinSpeed = 18f,
            ReelSpinSpeed = 24f,
            SpinUpTime = 0.3f,
            ReelBackOvershoot = 1.2f,
            HandoffDelay = 0.15f,
            MaxDeltaTime = 0.1f,
            MinStopSpeedRatio = 0.5f
        };
    }
}
