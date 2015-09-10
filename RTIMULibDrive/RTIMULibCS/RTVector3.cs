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
    public struct RTVector3
    {
        private double mX;
        private double mY;
        private double mZ;

        public double X { get { return this.mX; } set { this.mX = value; } }
        public double Y { get { return this.mY; } set { this.mY = value; } }
        public double Z { get { return this.mZ; } set { this.mZ = value; } }

        public RTVector3(double x, double y, double z)
        {
            mX = x;
            mY = y;
            mZ = z;
        }

        public void Zero()
        {
            mX = 0;
            mY = 0;
            mZ = 0;
        }


        public static RTVector3 operator +(RTVector3 lhs, RTVector3 rhs)
        {
            return (new RTVector3(lhs.mX + rhs.mX, lhs.mY + rhs.mY, lhs.mZ + rhs.mZ));
        }

        public static RTVector3 operator -(RTVector3 lhs, RTVector3 rhs)
        {
            return (new RTVector3(lhs.mX - rhs.mX, lhs.mY - rhs.mY, lhs.mZ - rhs.mZ));
        }



        public static double DotProduct(RTVector3 a, RTVector3 b)
        {
            return a.mX * b.mX + a.mY * b.mY + a.mZ * b.mZ;
        }

        public static void CrossProduct(RTVector3 a, RTVector3 b, out RTVector3 c)
        {
            c = new RTVector3(a.mY * b.mZ - a.mZ * b.mY, a.mZ * b.mX - a.mX * b.mZ, a.mX * b.mY - a.mY * b.mX);
        }


        public void AccelToEuler(out RTVector3 rollPitchYaw)
        {
            RTVector3 normAccel = this;
            normAccel.Normalize();

            rollPitchYaw = new RTVector3(Math.Atan2(normAccel.mY, normAccel.mZ),
            -Math.Atan2(normAccel.mX, Math.Sqrt(normAccel.mY * normAccel.mY + normAccel.mZ * normAccel.mZ)), 0);
        }

        public void AccelToQuaternion(out RTQuaternion qPose)
        {
            RTVector3 normAccel = this;
            RTVector3 vec;
            RTVector3 z = new RTVector3(0, 0, 1.0);
            qPose = new RTQuaternion();

            normAccel.Normalize();

            double angle = Math.Acos(RTVector3.DotProduct(z, normAccel));
            RTVector3.CrossProduct(normAccel, z, out vec);
            vec.Normalize();

            qPose.FromAngleVector(angle, vec);
        }


        public void Normalize()
        {
            double length = Math.Sqrt(mX * mX + mY * mY + mZ * mZ);

            if ((length == 0) || (length == 1))
                return;

            mX /= length;
            mY /= length;
            mZ /= length;
        }

        public double length()
        {
            return Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
        }
    }
}
