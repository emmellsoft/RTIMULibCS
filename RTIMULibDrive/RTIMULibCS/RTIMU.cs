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

using Windows.Devices.I2c;

namespace RTIMULibCS
{

    public class RTIMU {
        //  this sets the learning rate for compass running average calculation

        public double COMPASS_ALPHA = 0.2;

        //  this defines the accelerometer noise level

        public double RTIMU_FUZZY_GYRO_ZERO = 0.20;

        //  this defines the accelerometer noise level

        public double RTIMU_FUZZY_ACCEL_ZERO = 0.05;

        //  Axis rotation arrays

        //  Axis rotation defs
        //
        //  These allow the IMU to be virtually repositioned if it is in a non-standard configuration
        //  Standard configuration is X pointing at north, Y pointing east and Z pointing down
        //  with the IMU horizontal. There are 24 different possible orientations as defined
        //  below. Setting the axis rotation code to non-zero values performs the repositioning.

        public const int RTIMU_XNORTH_YEAST = 0;            // this is the default identity matrix
        public const int RTIMU_XEAST_YSOUTH = 1;
        public const int RTIMU_XSOUTH_YWEST = 2;
        public const int RTIMU_XWEST_YNORTH = 3;
        public const int RTIMU_XNORTH_YWEST = 4;
        public const int RTIMU_XEAST_YNORTH = 5;
        public const int RTIMU_XSOUTH_YEAST = 6;
        public const int RTIMU_XWEST_YSOUTH = 7;
        public const int RTIMU_XUP_YNORTH = 8;
        public const int RTIMU_XUP_YEAST = 9;
        public const int RTIMU_XUP_YSOUTH = 10;
        public const int RTIMU_XUP_YWEST = 11;
        public const int RTIMU_XDOWN_YNORTH = 12;
        public const int RTIMU_XDOWN_YEAST = 13;
        public const int RTIMU_XDOWN_YSOUTH = 14;
        public const int RTIMU_XDOWN_YWEST = 15;
        public const int RTIMU_XNORTH_YUP = 16;
        public const int RTIMU_XEAST_YUP = 17;
        public const int RTIMU_XSOUTH_YUP = 18;
        public const int RTIMU_XWEST_YUP = 19;
        public const int RTIMU_XNORTH_YDOWN = 20;
        public const int RTIMU_XEAST_YDOWN = 21;
        public const int RTIMU_XSOUTH_YDOWN = 22;
        public const int RTIMU_XWEST_YDOWN = 23;

        public const int RTIMU_AXIS_ROTATION_COUNT = 24;  

        protected static double[,] mAxisRotationArray = new double[,] {
            {1, 0, 0, 0, 1, 0, 0, 0, 1},                    // RTIMU_XNORTH_YEAST
            {0, -1, 0, 1, 0, 0, 0, 0, 1},                   // RTIMU_XEAST_YSOUTH
            {-1, 0, 0, 0, -1, 0, 0, 0, 1},                  // RTIMU_XSOUTH_YWEST
            {0, 1, 0, -1, 0, 0, 0, 0, 1},                   // RTIMU_XWEST_YNORTH

            {1, 0, 0, 0, -1, 0, 0, 0, -1},                  // RTIMU_XNORTH_YWEST
            {0, 1, 0, 1, 0, 0, 0, 0, -1},                   // RTIMU_XEAST_YNORTH
                {-1, 0, 0, 0, 1, 0, 0, 0, -1},                  // RTIMU_XSOUTH_YEAST
            {0, -1, 0, -1, 0, 0, 0, 0, -1},                 // RTIMU_XWEST_YSOUTH

            {0, 1, 0, 0, 0, -1, -1, 0, 0},                  // RTIMU_XUP_YNORTH
            {0, 0, 1, 0, 1, 0, -1, 0, 0},                   // RTIMU_XUP_YEAST
            {0, -1, 0, 0, 0, 1, -1, 0, 0},                  // RTIMU_XUP_YSOUTH
            {0, 0, -1, 0, -1, 0, -1, 0, 0},                 // RTIMU_XUP_YWEST

            {0, 1, 0, 0, 0, 1, 1, 0, 0},                    // RTIMU_XDOWN_YNORTH
            {0, 0, -1, 0, 1, 0, 1, 0, 0},                   // RTIMU_XDOWN_YEAST
            {0, -1, 0, 0, 0, -1, 1, 0, 0},                  // RTIMU_XDOWN_YSOUTH
            {0, 0, 1, 0, -1, 0, 1, 0, 0},                   // RTIMU_XDOWN_YWEST

            {1, 0, 0, 0, 0, 1, 0, -1, 0},                   // RTIMU_XNORTH_YUP
            {0, 0, -1, 1, 0, 0, 0, -1, 0},                  // RTIMU_XEAST_YUP
            {-1, 0, 0, 0, 0, -1, 0, -1, 0},                 // RTIMU_XSOUTH_YUP
            {0, 0, 1, -1, 0, 0, 0, -1, 0},                  // RTIMU_XWEST_YUP

            {1, 0, 0, 0, 0, -1, 0, 1, 0},                   // RTIMU_XNORTH_YDOWN
            {0, 0, 1, 1, 0, 0, 0, 1, 0},                    // RTIMU_XEAST_YDOWN
            {-1, 0, 0, 0, 0, 1, 0, 1, 0},                   // RTIMU_XSOUTH_YDOWN
            {0, 0, -1, -1, 0, 0, 0, 1, 0}                   // RTIMU_XWEST_YDOWN
        };

        public bool InitComplete = false;                   // set true when init has completed                    

        protected RTIMUData mImuData;                       // the retained version of the IMU data
        protected int mSampleRate;                          // samples per second
        protected long mSampleInterval;                     // interval between samples in microseonds

        protected double mGyroScale;                        // used to get result in rads/sec
        protected double mAccelScale;                       // used to get results in gs
        protected double mMagScale;                         // used to get results in uT

        double mGyroLearningAlpha;                          // gyro bias rapid learning rate
        double mGyroContinuousAlpha;                        // gyro bias continuous (slow) learning rate
        int mGyroSampleCount = 0;                           // number of gyro samples used


        public int AxisRotation { set { if ((value >= 0) && (value < RTIMU_AXIS_ROTATION_COUNT))
                    this.mAxisRotation = value;
                else
                    this.mAxisRotation = 0;
            } }
        private int mAxisRotation = 0;                      // defines the installed oerientation
    
        public bool MagCalValid {  get { return this.mMagCalValid; } }
        private bool mMagCalValid = false;                  // if we have mag cal valid data

        public bool GyroBiasValid { get { return this.mGyroBiasValid; } }
        private bool mGyroBiasValid = false;                // true if valid gyro bias has been obtained

        RTVector3 mPreviousAccel = new RTVector3();         // previous step accel for gyro learning
        RTVector3 mGyroBias = new RTVector3();              // previous step gyro bias for gyro learning
        RTVector3 mMagAverage = new RTVector3();            // a running average to smooth the mag outputs

        private double[] mMagMax = new double[3];           // max values seen for mag
        private double[] mMagMin = new double[3];           // min values seen for mag
        private double mMagMaxDelta;                        // max difference between max and min used for scale
        private double[] mMagCalScale = new double[3];      // used to scale the mag readings
        private double[] mMagCalOffset = new double[3];     // used to offset the mag readings

        public RTIMU()
        {
            for (int i = 0; i < 3; i++) {
                mMagMax[i] = -1000.0;
                mMagMin[i] = 1000.0;
            }
        }

        public RTIMUData getRTIMUData()
        {
            return mImuData;
        }

        public bool SetGyroContinuousLearningAlpha(double alpha)
        {
            if ((alpha < 0.0) || (alpha >= 1.0))
                return false;

            mGyroContinuousAlpha = alpha;
            return true;
        }


        protected void GyroBiasInit()
        {
            mGyroLearningAlpha = 2.0f / mSampleRate;
            mGyroContinuousAlpha = 0.01f / mSampleRate;
            mGyroSampleCount = 0;
        }

        //  Note - code assumes that this is the first thing called after axis swapping
        //  for each specific IMU chip has occurred.

        protected void HandleGyroBias()
        {
            // do axis rotation

            if ((mAxisRotation > 0) && (mAxisRotation < RTIMU_AXIS_ROTATION_COUNT)) {
                // need to do an axis rotation
                RTIMUData tempIMU = mImuData;

                // do new x value
                if (mAxisRotationArray[mAxisRotation, 0] != 0) {
                    mImuData.gyro.X = tempIMU.gyro.X * mAxisRotationArray[mAxisRotation, 0];
                    mImuData.accel.X = tempIMU.accel.X * mAxisRotationArray[mAxisRotation, 0];
                    mImuData.mag.X = tempIMU.mag.X * mAxisRotationArray[mAxisRotation, 0];
                } else if (mAxisRotationArray[mAxisRotation, 1] != 0) {
                    mImuData.gyro.X = tempIMU.gyro.Y * mAxisRotationArray[mAxisRotation, 1];
                    mImuData.accel.X = tempIMU.accel.Y * mAxisRotationArray[mAxisRotation, 1];
                    mImuData.mag.X = tempIMU.mag.Y * mAxisRotationArray[mAxisRotation, 1];
                } else if (mAxisRotationArray[mAxisRotation, 2] != 0) {
                    mImuData.gyro.X = tempIMU.gyro.Z * mAxisRotationArray[mAxisRotation, 2];
                    mImuData.accel.X = tempIMU.accel.Z * mAxisRotationArray[mAxisRotation, 2];
                    mImuData.mag.X = tempIMU.mag.Z * mAxisRotationArray[mAxisRotation, 2];
                }

                // do new y value
                if (mAxisRotationArray[mAxisRotation, 3] != 0) {
                    mImuData.gyro.Y = tempIMU.gyro.X * mAxisRotationArray[mAxisRotation, 3];
                    mImuData.accel.Y = tempIMU.accel.X * mAxisRotationArray[mAxisRotation, 3];
                    mImuData.mag.Y = tempIMU.mag.X * mAxisRotationArray[mAxisRotation, 3];
                } else if (mAxisRotationArray[mAxisRotation, 4] != 0) {
                    mImuData.gyro.Y = tempIMU.gyro.Y * mAxisRotationArray[mAxisRotation, 4];
                    mImuData.accel.Y = tempIMU.accel.Y * mAxisRotationArray[mAxisRotation, 4];
                    mImuData.mag.Y = tempIMU.mag.Y * mAxisRotationArray[mAxisRotation, 4];
                } else if (mAxisRotationArray[mAxisRotation, 5] != 0) {
                    mImuData.gyro.Y = tempIMU.gyro.Z * mAxisRotationArray[mAxisRotation, 5];
                    mImuData.accel.Y = tempIMU.accel.Z * mAxisRotationArray[mAxisRotation, 5];
                    mImuData.mag.Y = tempIMU.mag.Z * mAxisRotationArray[mAxisRotation, 5];
                }

                // do new z value
                if (mAxisRotationArray[mAxisRotation, 6] != 0) {
                    mImuData.gyro.Z = tempIMU.gyro.X * mAxisRotationArray[mAxisRotation, 6];
                    mImuData.accel.Z = tempIMU.accel.X * mAxisRotationArray[mAxisRotation, 6];
                    mImuData.mag.Z = tempIMU.mag.X * mAxisRotationArray[mAxisRotation, 6];
                } else if (mAxisRotationArray[mAxisRotation, 7] != 0) {
                    mImuData.gyro.Z = tempIMU.gyro.Y * mAxisRotationArray[mAxisRotation, 7];
                    mImuData.accel.Z = tempIMU.accel.Y * mAxisRotationArray[mAxisRotation, 7];
                    mImuData.mag.Z = tempIMU.mag.Y * mAxisRotationArray[mAxisRotation, 7];
                } else if (mAxisRotationArray[mAxisRotation, 8] != 0) {
                    mImuData.gyro.Z = tempIMU.gyro.Z * mAxisRotationArray[mAxisRotation, 8];
                    mImuData.accel.Z = tempIMU.accel.Z * mAxisRotationArray[mAxisRotation, 8];
                    mImuData.mag.Z = tempIMU.mag.Z * mAxisRotationArray[mAxisRotation, 8];
                }
            }

            RTVector3 deltaAccel = mPreviousAccel;
            deltaAccel -= mImuData.accel;   // compute difference
            mPreviousAccel = mImuData.accel;

            if ((deltaAccel.length() < RTIMU_FUZZY_ACCEL_ZERO) && (mImuData.gyro.length() < RTIMU_FUZZY_GYRO_ZERO)) {
                // what we are seeing on the gyros should be bias only so learn from this

                if (mGyroSampleCount < (5 * mSampleRate)) {
                    mGyroBias.X = (1.0 - mGyroLearningAlpha) * mGyroBias.X + mGyroLearningAlpha * mImuData.gyro.X;
                    mGyroBias.Y = (1.0 - mGyroLearningAlpha) * mGyroBias.Y + mGyroLearningAlpha * mImuData.gyro.Y;
                    mGyroBias.Z = (1.0 - mGyroLearningAlpha) * mGyroBias.Z + mGyroLearningAlpha * mImuData.gyro.Z;

                    mGyroSampleCount++;

                    if (mGyroSampleCount == (5 * mSampleRate)) {
                        // this could have been true already of course
                        mGyroBiasValid = true;
                     }
                } else {
                    mGyroBias.X = (1.0 - mGyroContinuousAlpha) * mGyroBias.X + mGyroContinuousAlpha * mImuData.gyro.X;
                    mGyroBias.Y = (1.0 - mGyroContinuousAlpha) * mGyroBias.Y + mGyroContinuousAlpha * mImuData.gyro.Y;
                    mGyroBias.Z = (1.0 - mGyroContinuousAlpha) * mGyroBias.Z + mGyroContinuousAlpha * mImuData.gyro.Z;
                }
            }

            mImuData.gyro -= mGyroBias;
        }

        public void CalibrateAverageCompass()
        {
            //  calibrate if required

            SetCalibrationData();

            if (mMagCalValid) {
                mImuData.mag.X = (mImuData.mag.X - mMagCalOffset[0]) * mMagCalScale[0];
                mImuData.mag.Y = (mImuData.mag.Y - mMagCalOffset[1]) * mMagCalScale[1];
                mImuData.mag.Z = (mImuData.mag.Z - mMagCalOffset[2]) * mMagCalScale[2];
            }

            //  update running average

            mMagAverage.X = mImuData.mag.X* COMPASS_ALPHA + mMagAverage.X * (1.0 - COMPASS_ALPHA);
            mMagAverage.Y = mImuData.mag.Y * COMPASS_ALPHA + mMagAverage.Y * (1.0 - COMPASS_ALPHA);
            mMagAverage.Z = mImuData.mag.Z * COMPASS_ALPHA + mMagAverage.Z * (1.0 - COMPASS_ALPHA);

            mImuData.mag = mMagAverage;
        }

        private void SetCalibrationData()
        {
            double delta;
            bool changed = false;

            // see if there is a new max or min

            if ((mMagMax[0] < mImuData.mag.X)) {
                mMagMax[0] = mImuData.mag.X;
                changed = true;
            }
            if ((mMagMax[1] < mImuData.mag.Y)) {
                mMagMax[1] = mImuData.mag.Y;
                changed = true;
            }
            if ((mMagMax[2] < mImuData.mag.Z)) {
                mMagMax[2] = mImuData.mag.Z;
                changed = true;
            }
            if ((mMagMin[0] > mImuData.mag.X)) {
                mMagMin[0] = mImuData.mag.X;
                changed = true;
            }
            if ((mMagMin[1] > mImuData.mag.Y)) {
                mMagMin[1] = mImuData.mag.Y;
                changed = true;
            }
            if ((mMagMin[2] > mImuData.mag.Z)) {
                mMagMin[2] = mImuData.mag.Z;
                changed = true;
            }
            if (changed) {
                if (!mMagCalValid) {
                    mMagCalValid = true;
                    for (int i = 0; i < 3; i++) {
                        delta = mMagMax[i] - mMagMin[i];
                        if ((delta < 30) || (mMagMin[i] > 0) || (mMagMax[i] < 0)) {
                            mMagCalValid = false;
                            break;
                        }
                    }
                }

                if (mMagCalValid) {
                    mMagMaxDelta = -1;

                    for (int i = 0; i < 3; i++) {
                        if ((mMagMax[i] - mMagMin[i]) > mMagMaxDelta)
                            mMagMaxDelta = mMagMax[i] - mMagMin[i];
                    }

                    // adjust for + and - range

                    mMagMaxDelta /= 2.0;
                }

                for (int i = 0; i < 3; i++) {
                    delta = (mMagMax[i] - mMagMin[i]) / 2.0;
                    mMagCalScale[i] = mMagMaxDelta / delta;
                    mMagCalOffset[i] = (mMagMax[i] + mMagMin[i]) / 2.0;
                }
            }
        }


        public bool IMUGyroBiasValid()
        {
            return mGyroBiasValid;
        }

        public void SetExtIMUData(double gx, double gy, double gz, double ax, double ay, double az,
        double mx, double my, double mz, long timestamp)
        {
            mImuData.gyro.X = gx;
            mImuData.gyro.Y = gy;
            mImuData.gyro.Z = gz;
            mImuData.accel.X = ax;
            mImuData.accel.Y = ay;
            mImuData.accel.Z = az;
            mImuData.mag.X = mx;
            mImuData.mag.Y = my;
            mImuData.mag.Z = mz;
            mImuData.timestamp = timestamp;
        }
    }
}