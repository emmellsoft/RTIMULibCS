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
    public class RTFusion
    {


        //  The slerp power valule controls the influence of the measured state to correct the predicted state
        //  0 = measured state ignored (just gyros), 1 = measured state overrides predicted state.
        //  In between 0 and 1 mixes the two conditions

        public double SlerpPower = 0.02;

        //  Change this value to set the magnetic declination adjustment

        public double CompassAdjDeclination = 0;

        public bool EnableGyro = true;                      // if true, gyro data used
        public bool EnableAccel = true;                     // if true, accel data used
        public bool EnableMag = true;                       // if true, mag data used

        protected bool mMagValid;

        protected bool mFirstTime = true;

        protected RTQuaternion mGravity = new RTQuaternion(0, 0, 0, 1);

        protected RTQuaternion mStateQ = new RTQuaternion();

        protected double mTimeDelta = 0;

        protected long mLastFusionTime = 0;

        protected RTVector3 mMeasuredPose = new RTVector3();
        protected RTQuaternion mMeasuredQPose = new RTQuaternion();
        protected RTVector3 mFusionPose = new RTVector3();
        protected RTQuaternion mFusionQPose = new RTQuaternion();

        protected RTVector3 mGyro = new RTVector3();
        protected RTVector3 mAccel = new RTVector3();
        protected RTVector3 mMag = new RTVector3();

        protected int mSampleNumber = 0;

        public void CalculatePose(RTVector3 accel, RTVector3 mag, double magDeclination)
        {
            RTQuaternion m;
            RTQuaternion q = new RTQuaternion();

            if (EnableAccel) {
                accel.AccelToEuler(out mMeasuredPose);
            } else {
                mMeasuredPose = mFusionPose;
                mMeasuredPose.Z = 0;
            }

            if (EnableMag && mMagValid) {
                q.FromEuler(mMeasuredPose);
                m = new RTQuaternion(0, mag.X, mag.Y, mag.Z);
                m = q * m * q.Conjugate();
                mMeasuredPose.Z = -Math.Atan2(m.Y, m.X) - magDeclination;
            } else {
                mMeasuredPose.Z = mFusionPose.Z;
            }

            mMeasuredQPose.FromEuler(mMeasuredPose);

            //  check for quaternion aliasing. If the quaternion has the wrong sign
            //  the filter will be very unhappy.

            int maxIndex = -1;
            double maxVal = -1000;

            for (int i = 0; i < 4; i++) {
                if (Math.Abs(mMeasuredQPose.data(i)) > maxVal) {
                    maxVal = Math.Abs(mMeasuredQPose.data(i));
                    maxIndex = i;
                }
            }

            //  if the biggest component has a different sign in the measured and kalman poses,
            //  change the sign of the measured pose to match.

            if (((mMeasuredQPose.data(maxIndex) < 0) && (mFusionQPose.data(maxIndex) > 0)) ||
                    ((mMeasuredQPose.data(maxIndex) > 0) && (mFusionQPose.data(maxIndex) < 0))) {
                mMeasuredQPose.Scalar = -mMeasuredQPose.Scalar;
                mMeasuredQPose.X = -mMeasuredQPose.X;
                mMeasuredQPose.Y = -mMeasuredQPose.Y;
                mMeasuredQPose.Z = -mMeasuredQPose.Z;
                mMeasuredQPose.ToEuler(out mMeasuredPose);
            }
        }

        public RTVector3 GetAccelResiduals()
        {
            RTQuaternion rotatedGravity;
            RTQuaternion fusedConjugate;
            RTQuaternion qTemp;
            RTVector3 residuals;

            //  do gravity rotation and subtraction

            // create the conjugate of the pose

            fusedConjugate = mFusionQPose.Conjugate();

            // now do the rotation - takes two steps with qTemp as the intermediate variable

            qTemp = mGravity * mFusionQPose;
            rotatedGravity = fusedConjugate * qTemp;

            // now adjust the measured accel and change the signs to make sense

            residuals = new RTVector3(-(mAccel.X - rotatedGravity.X), -(mAccel.Y - rotatedGravity.Y), -(mAccel.Z - rotatedGravity.Z));
            return residuals;
        }
    }
}
