using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter.Models
{
    public class CameraShot
    {
        public const float LOWEST_ACCURACY = 15.0f;         // 15m  Coarse. Not found
        public const float LOW_ACCURACY = 5.0f;         // 5m       Q=5
        public const float HIGH_ACCURACY = 0.02f;       // 2cm      Q=1
        public const float MEDIUM_ACCURACY = 0.3f;       // 30cm    Q=2

        public double CoarseLat;
        public double CoarseLon;
        public double CoarseAlt;

        public float RollB;
        public float PitchB;
        public float YawB;

        public float ManualRoll;
        public float ManualPitch;
        public float ManualYaw;

        public float OffsetX;
        public float OffsetY;
        public float OffsetZ;

        // USed for DJI
        public bool skipRotationsAndApplyDirectOffset = false;

        public float RollH;
        public float PitchH;
        public float YawH;

        public double PreciseLatNodalPoint;
        public double PreciseLonNodalPoint;
        public double PreciseAltNodalPoint;

        public double PreciseLatGPSAntenna;
        public double PreciseLonGPSAntenna;
        public double PreciseAltGPSAntenna;

        public int GPSWeek;
        public double GPSSecond;
        public long MicrosTime;

        public int shutterDelay;

        public double GPSTrackYaw;

        public float Accuracy;

        public string Name;

        public string fileOriginalUri;

        public double distanceCameraAntenna;

        public CameraShot()
        {
            Accuracy = LOWEST_ACCURACY;
        }
    }
}
