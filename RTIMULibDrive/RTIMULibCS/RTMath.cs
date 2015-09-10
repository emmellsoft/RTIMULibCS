////////////////////////////////////////////////////////////////////////////
//
//  This file is part of RTIMULibCS
//
//  Copyright (c) 2015, richards-tech, LLC
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of 
//  this software and associated documentation files (the "Software"), to deal in 
//  the Software without restriction, including without limitation the rights to use, 
//  copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
//  Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all 
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;

namespace RTIMULibCS
{
    public class RTMath
    {
        public const double RTMATH_PI = 3.1415926535;
        public const double RTMATH_DEGREE_TO_RAD = RTMATH_PI / 180.0;
        public const double RTMATH_RAD_TO_DEGREE = 180.0 / RTMATH_PI;

        public static string DisplayRadians(string label, RTVector3 vec)
        {
            return string.Format("{0}: x:{1:F4}, y: {2:F4}, z: {3:F4}", label, vec.X, vec.Y, vec.Z);
        }

        public static string DisplayDegrees(string label, RTVector3 vec)
        {
            return string.Format("{0}: roll: {1:F4}, pitch: {2:F4}, yaw: {3:F4}", label, vec.X * RTMATH_RAD_TO_DEGREE,
                vec.Y * RTMATH_RAD_TO_DEGREE, vec.Z * RTMATH_RAD_TO_DEGREE);
        }

        public static string Display(string label, RTQuaternion quat)
        {
            return string.Format("{0}: scalar: {1:F4}, x: {2:F4}, y: {3:F4}, z: {4:F4}", label, quat.Scalar, quat.X, quat.Y, quat.Z);
        }

        //  convertPressureToHeight() - the conversion uses the formula:
        //
        //  h = (T0 / L0) * ((p / P0)**(-(R* * L0) / (g0 * M)) - 1)
        //
        //  where:
        //  h  = height above sea level
        //  T0 = standard temperature at sea level = 288.15
        //  L0 = standard temperatur elapse rate = -0.0065
        //  p  = measured pressure
        //  P0 = static pressure = 1013.25 (but can be overridden)
        //  g0 = gravitational acceleration = 9.80665
        //  M  = mloecular mass of earth's air = 0.0289644
        //  R* = universal gas constant = 8.31432
        //
        //  Given the constants, this works out to:
        //
        //  h = 44330.8 * (1 - (p / P0)**0.190263)

        public static double convertPressureToHeight(double pressure, double staticPressure)
        {
            return 44330.8 * (1 - Math.Pow(pressure / staticPressure, (double)0.190263));
        }


        public static RTVector3 PoseFromAccelMag(RTVector3 accel, RTVector3 mag)
        {
            RTVector3 result;
            RTQuaternion m;
            RTQuaternion q;

            accel.AccelToEuler(out result);

            //  q.fromEuler(result);
            //  since result.z() is always 0, this can be optimized a little

            double cosX2 = Math.Cos(result.X / 2.0f);
            double sinX2 = Math.Sin(result.X / 2.0f);
            double cosY2 = Math.Cos(result.Y / 2.0f);
            double sinY2 = Math.Sin(result.Y / 2.0f);

            q = new RTQuaternion(cosX2 * cosY2, sinX2 * cosY2, cosX2 * sinY2, -sinX2 * sinY2);
            m = new RTQuaternion(0, mag.X, mag.Y, mag.Z);

            m = q * m * q.Conjugate();
            result.Z = -Math.Atan2(m.Y, m.X);
            return result;
        }

        public static void ConvertToVector(byte[] rawData, out RTVector3 vec, double scale, bool bigEndian)
        {
            if (bigEndian) {
                vec = new RTVector3((double)((UInt16)(((UInt16)rawData[0] << 8) | (UInt16)rawData[1])) * scale,
                (double)((Int16)(((UInt16)rawData[2] << 8) | (UInt16)rawData[3])) * scale,
                (double)((Int16)(((UInt16)rawData[4] << 8) | (UInt16)rawData[5])) * scale);
            } else {
                vec = new RTVector3((double)((Int16)(((UInt16)rawData[1] << 8) | (UInt16)rawData[0])) * scale,
                (double)((Int16)(((UInt16)rawData[3] << 8) | (UInt16)rawData[2])) * scale,
                (double)((Int16)(((UInt16)rawData[5] << 8) | (UInt16)rawData[4])) * scale);
            }
        }
    }
}

