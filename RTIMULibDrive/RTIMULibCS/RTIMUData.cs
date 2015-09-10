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

namespace RTIMULibCS
{
    public struct RTIMUData
    {
        public long timestamp;                              // sample timestamp in microseconds
        public bool fusionPoseValid;                        // true if fusion pose valid
        public RTVector3 fusionPose;                        // the fusion pose
        public bool fusionQPoseValid;                       // true if the fusion quaternion is valid
        public RTQuaternion fusionQPose;                    // the fusion quaternion
        public bool gyroValid;                              // true if gyro data is valid
        public RTVector3 gyro;                              // gyro data in radians/sec
        public bool accelValid;                             // true if accel data valid
        public RTVector3 accel;                             // accel data in g
        public bool magValid;                               // true if mag data valid
        public RTVector3 mag;                               // mag data in uT
        public bool pressureValid;                          // true if pressure data valid
        public double pressure;                             // pressure in hPa                   
        public bool temperatureValid;                       // true if temperature data valid
        public double temperature;                          // temperature in degree C
        public bool humidityValid;                          // true if humidity data valid
        public double humidity;                             // humidity in %RH
    }
}