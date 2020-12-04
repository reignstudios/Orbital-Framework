using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using LONG = System.Int32;
using BYTE = System.Byte;

namespace Orbital.Input.DirectInput
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct DIJOYSTATE2
    {
        public LONG lX;                     /* x-axis position              */
        public LONG lY;                     /* y-axis position              */
        public LONG lZ;                     /* z-axis position              */
        public LONG lRx;                    /* x-axis rotation              */
        public LONG lRy;                    /* y-axis rotation              */
        public LONG lRz;                    /* z-axis rotation              */
        public fixed LONG rglSlider[2];           /* extra axes positions         */
        public fixed DWORD rgdwPOV[4];             /* POV directions               */
        public fixed BYTE rgbButtons[128];        /* 128 buttons                  */
        public LONG lVX;                    /* x-axis velocity              */
        public LONG lVY;                    /* y-axis velocity              */
        public LONG lVZ;                    /* z-axis velocity              */
        public LONG lVRx;                   /* x-axis angular velocity      */
        public LONG lVRy;                   /* y-axis angular velocity      */
        public LONG lVRz;                   /* z-axis angular velocity      */
        public fixed LONG rglVSlider[2];          /* extra axes velocities        */
        public LONG lAX;                    /* x-axis acceleration          */
        public LONG lAY;                    /* y-axis acceleration          */
        public LONG lAZ;                    /* z-axis acceleration          */
        public LONG lARx;                   /* x-axis angular acceleration  */
        public LONG lARy;                   /* y-axis angular acceleration  */
        public LONG lARz;                   /* z-axis angular acceleration  */
        public fixed LONG rglASlider[2];          /* extra axes accelerations     */
        public LONG lFX;                    /* x-axis force                 */
        public LONG lFY;                    /* y-axis force                 */
        public LONG lFZ;                    /* z-axis force                 */
        public LONG lFRx;                   /* x-axis torque                */
        public LONG lFRy;                   /* y-axis torque                */
        public LONG lFRz;                   /* z-axis torque                */
        public fixed LONG rglFSlider[2];          /* extra axes forces            */
    }
}
