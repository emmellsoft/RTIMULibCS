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
    public struct RTQuaternion
    {
        private double mScalar;
        private double mX;
        private double mY;
        private double mZ;

        public double Scalar { get { return this.mScalar; } set { this.mScalar = value; } }
        public double X { get { return this.mX; } set { this.mX = value; } }
        public double Y { get { return this.mY; } set { this.mY = value; } }
        public double Z { get { return this.mZ; } set { this.mZ = value; } }

        public RTQuaternion(double scalar, double x, double y, double z)
        {
            mScalar = scalar;
            mX = x;
            mY = y;
            mZ = z;
        }

        public double data(int i)
        {
            if (i == 0)
                return mScalar;
            else if (i == 1)
                return mX;
            else if (i == 2)
                return mY;
            return mZ;
        }

        public static RTQuaternion operator +(RTQuaternion lhs, RTQuaternion rhs)
        {
            return new RTQuaternion(lhs.mScalar + rhs.mScalar, lhs.mX + rhs.mX,
                                    lhs.mY + rhs.mY, lhs.mZ + rhs.mZ);
        }

        public static RTQuaternion operator -(RTQuaternion lhs, RTQuaternion rhs)
        {
            return new RTQuaternion(lhs.mScalar - rhs.mScalar, lhs.mX - rhs.mX,
                                    lhs.mY - rhs.mY, lhs.mZ - rhs.mZ);
        }

        public static RTQuaternion operator *(RTQuaternion qa, RTQuaternion qb)
        {
            return new RTQuaternion(
                    qa.mScalar * qb.mScalar - qa.mX * qb.mX - qa.mY * qb.mY - qa.mZ * qb.mZ,
                    qa.mScalar * qb.mX + qa.mX * qb.mScalar + qa.mY * qb.mZ - qa.mZ * qb.mY,
                    qa.mScalar * qb.mY - qa.mX * qb.mZ + qa.mY * qb.mScalar + qa.mZ * qb.mX,
                    qa.mScalar * qb.mZ + qa.mX * qb.mY - qa.mY * qb.mX + qa.mZ * qb.mScalar);
        }


        public static RTQuaternion operator *(RTQuaternion lhs, double rhs)
        {
            return new RTQuaternion(lhs.mScalar * rhs, lhs.mX * rhs, lhs.mY * rhs, lhs.mZ * rhs);
        }

        public void Normalize()
        {
            double length = Math.Sqrt(mScalar * mScalar + mX * mX +
                                    mY * mY + mZ * mZ);

            if ((length == 0) || (length == 1))
                return;

            mScalar /= length;
            mX /= length;
            mY /= length;
            mZ /= length;
        }

        public void ToEuler(out RTVector3 vec)
        {
            vec = new RTVector3(Math.Atan2(2.0 * (mY * mZ + mScalar * mX), 1 - 2.0 * (mX * mX + mY * mY)),
                    Math.Asin(2.0 * (mScalar * mY - mX * mZ)),
                    Math.Atan2(2.0 * (mX * mY + mScalar * mZ), 1 - 2.0 * (mY * mY + mZ * mZ)));
        }

        public void FromEuler(RTVector3 vec)
        {
            double cosX2 = Math.Cos(vec.X / 2.0f);
            double sinX2 = Math.Sin(vec.X / 2.0f);
            double cosY2 = Math.Cos(vec.Y / 2.0f);
            double sinY2 = Math.Sin(vec.Y / 2.0f);
            double cosZ2 = Math.Cos(vec.Z / 2.0f);
            double sinZ2 = Math.Sin(vec.Z / 2.0f);

            mScalar = cosX2 * cosY2 * cosZ2 + sinX2 * sinY2 * sinZ2;
            mX = sinX2 * cosY2 * cosZ2 - cosX2 * sinY2 * sinZ2;
            mY = cosX2 * sinY2 * cosZ2 + sinX2 * cosY2 * sinZ2;
            mZ = cosX2 * cosY2 * sinZ2 - sinX2 * sinY2 * cosZ2;
            Normalize();
        }

        public RTQuaternion Conjugate()
        {
            return new RTQuaternion(mScalar, -mX, -mY, -mZ);
        }

        public void ToAngleVector(out double angle, out RTVector3 vec)
        {
            double halfTheta;
            double sinHalfTheta;

            halfTheta = Math.Acos(mScalar);
            sinHalfTheta = Math.Sin(halfTheta);

            if (sinHalfTheta == 0) {
                vec = new RTVector3(1.0, 0, 0);
            } else {
                vec = new RTVector3(mX / sinHalfTheta, mY / sinHalfTheta, mZ / sinHalfTheta);
            }
            angle = 2.0 * halfTheta;
        }

        public void FromAngleVector(double angle, RTVector3 vec)
        {
            double sinHalfTheta = Math.Sin(angle / 2.0);
            mScalar = Math.Cos(angle / 2.0);
            mX = vec.X * sinHalfTheta;
            mY = vec.Y * sinHalfTheta;
            mZ = vec.Z * sinHalfTheta;
        }
    }
}
