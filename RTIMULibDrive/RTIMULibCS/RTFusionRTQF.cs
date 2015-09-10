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

    public class RTFusionRTQF : RTFusion
    {

        public RTFusionRTQF()
        {
            Reset();
        }

        public void Reset()
        {
            mFirstTime = true;
            mFusionPose = new RTVector3();
            mFusionQPose.FromEuler(mFusionPose);
            mGyro = new RTVector3();
            mAccel = new RTVector3();
            mMag = new RTVector3();
            mMeasuredPose = new RTVector3();
            mMeasuredQPose.FromEuler(mMeasuredPose);
            mSampleNumber = 0;
        }

        private void Predict()
        {
            double x2, y2, z2;
            double qs, qx, qy, qz;

            if (!EnableGyro)
                return;

            qs = mStateQ.Scalar;
            qx = mStateQ.X;
            qy = mStateQ.Y;
            qz = mStateQ.Z;

            x2 = mGyro.X / (double)2.0;
            y2 = mGyro.Y / (double)2.0;
            z2 = mGyro.Z / (double)2.0;

            // Predict new state

            mStateQ.Scalar = qs + (-x2 * qx - y2 * qy - z2 * qz) * mTimeDelta;
            mStateQ.X = qx + (x2 * qs + z2 * qy - y2 * qz) * mTimeDelta;
            mStateQ.Y = qy + (y2 * qs - z2 * qx + x2 * qz) * mTimeDelta;
            mStateQ.Z = qz + (z2 * qs + y2 * qx - x2 * qy) * mTimeDelta;
            mStateQ.Normalize();
        }


        private void Update()
        {
            RTQuaternion rotationDelta;
            RTQuaternion rotationPower;
            RTVector3 rotationUnitVector;

            if (EnableMag || EnableAccel) {

                // calculate rotation delta

                rotationDelta = mStateQ.Conjugate() * mMeasuredQPose;
                rotationDelta.Normalize();

                // take it to the power (0 to 1) to give the desired amount of correction

                double theta = Math.Acos(rotationDelta.Scalar);

                double sinPowerTheta = Math.Sin(theta * SlerpPower);
                double cosPowerTheta = Math.Cos(theta * SlerpPower);

                rotationUnitVector = new RTVector3(rotationDelta.X, rotationDelta.Y, rotationDelta.Z);
                rotationUnitVector.Normalize();

                rotationPower = new RTQuaternion(cosPowerTheta,
                            sinPowerTheta * rotationUnitVector.X,
                            sinPowerTheta * rotationUnitVector.Y,
                            sinPowerTheta * rotationUnitVector.Z);
                rotationPower.Normalize();

                //  multiple this by predicted value to get result

                mStateQ *= rotationPower;
                mStateQ.Normalize();
            }
        }

        public void NewIMUData(ref RTIMUData data)
        {
            mSampleNumber++;

            if (EnableGyro)
                mGyro = data.gyro;
            else
                mGyro = new RTVector3();
            mAccel = data.accel;
            mMag = data.mag;
            mMagValid = data.magValid;

            if (mFirstTime) {
                mLastFusionTime = data.timestamp;
                CalculatePose(mAccel, mMag, CompassAdjDeclination);

                //  initialize the poses

                mStateQ.FromEuler(mMeasuredPose);
                mFusionQPose = mStateQ;
                mFusionPose = mMeasuredPose;
                mFirstTime = false;
            } else {
                mTimeDelta = (double)(data.timestamp - mLastFusionTime) / (double)1000000;
                if (mTimeDelta > 0) {
                    CalculatePose(mAccel, mMag, CompassAdjDeclination);
                    Predict();
                    Update();
                    mStateQ.ToEuler(out mFusionPose);
                    mFusionQPose = mStateQ;
                } 
                mLastFusionTime = data.timestamp;
            }
            data.fusionPoseValid = true;
            data.fusionQPoseValid = true;
            data.fusionPose = mFusionPose;
            data.fusionQPose = mFusionQPose;
        }
    }
}
