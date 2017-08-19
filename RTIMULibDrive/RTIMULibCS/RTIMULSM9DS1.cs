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
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RTIMULibCS
{
    public class RTIMULSM9DS1 : RTIMU
    {
        public string ErrorMessage = "";

        public const byte LSM9DS1_ADDRESS0 = 0x6a;
        public const byte LSM9DS1_ADDRESS1 = 0x6b;
        public const byte LSM9DS1_ID = 0x68;

        public const byte LSM9DS1_MAG_ADDRESS0 = 0x1c;
        public const byte LSM9DS1_MAG_ADDRESS1 = 0x1d;
        public const byte LSM9DS1_MAG_ADDRESS2 = 0x1e;
        public const byte LSM9DS1_MAG_ADDRESS3 = 0x1f;
        public const byte LSM9DS1_MAG_ID = 0x3d;

        //  LSM9DS1 Register map

        public const byte LSM9DS1_ACT_THS = 0x04;
        public const byte LSM9DS1_ACT_DUR = 0x05;
        public const byte LSM9DS1_INT_GEN_CFG_XL = 0x06;
        public const byte LSM9DS1_INT_GEN_THS_X_XL = 0x07;
        public const byte LSM9DS1_INT_GEN_THS_Y_XL = 0x08;
        public const byte LSM9DS1_INT_GEN_THS_Z_XL = 0x09;
        public const byte LSM9DS1_INT_GEN_DUR_XL = 0x0A;
        public const byte LSM9DS1_REFERENCE_G = 0x0B;
        public const byte LSM9DS1_INT1_CTRL = 0x0C;
        public const byte LSM9DS1_INT2_CTRL = 0x0D;
        public const byte LSM9DS1_WHO_AM_I = 0x0F;
        public const byte LSM9DS1_CTRL1 = 0x10;
        public const byte LSM9DS1_CTRL2 = 0x11;
        public const byte LSM9DS1_CTRL3 = 0x12;
        public const byte LSM9DS1_ORIENT_CFG_G = 0x13;
        public const byte LSM9DS1_INT_GEN_SRC_G = 0x14;
        public const byte LSM9DS1_OUT_TEMP_L = 0x15;
        public const byte LSM9DS1_OUT_TEMP_H = 0x16;
        public const byte LSM9DS1_STATUS = 0x17;
        public const byte LSM9DS1_OUT_X_L_G = 0x18;
        public const byte LSM9DS1_OUT_X_H_G = 0x19;
        public const byte LSM9DS1_OUT_Y_L_G = 0x1A;
        public const byte LSM9DS1_OUT_Y_H_G = 0x1B;
        public const byte LSM9DS1_OUT_Z_L_G = 0x1C;
        public const byte LSM9DS1_OUT_Z_H_G = 0x1D;
        public const byte LSM9DS1_CTRL4 = 0x1E;
        public const byte LSM9DS1_CTRL5 = 0x1F;
        public const byte LSM9DS1_CTRL6 = 0x20;
        public const byte LSM9DS1_CTRL7 = 0x21;
        public const byte LSM9DS1_CTRL8 = 0x22;
        public const byte LSM9DS1_CTRL9 = 0x23;
        public const byte LSM9DS1_CTRL10 = 0x24;
        public const byte LSM9DS1_INT_GEN_SRC_XL = 0x26;
        public const byte LSM9DS1_STATUS2 = 0x27;
        public const byte LSM9DS1_OUT_X_L_XL = 0x28;
        public const byte LSM9DS1_OUT_X_H_XL = 0x29;
        public const byte LSM9DS1_OUT_Y_L_XL = 0x2A;
        public const byte LSM9DS1_OUT_Y_H_XL = 0x2B;
        public const byte LSM9DS1_OUT_Z_L_XL = 0x2C;
        public const byte LSM9DS1_OUT_Z_H_XL = 0x2D;
        public const byte LSM9DS1_FIFO_CTRL = 0x2E;
        public const byte LSM9DS1_FIFO_SRC = 0x2F;
        public const byte LSM9DS1_INT_GEN_CFG_G = 0x30;
        public const byte LSM9DS1_INT_GEN_THS_XH_G = 0x31;
        public const byte LSM9DS1_INT_GEN_THS_XL_G = 0x32;
        public const byte LSM9DS1_INT_GEN_THS_YH_G = 0x33;
        public const byte LSM9DS1_INT_GEN_THS_YL_G = 0x34;
        public const byte LSM9DS1_INT_GEN_THS_ZH_G = 0x35;
        public const byte LSM9DS1_INT_GEN_THS_ZL_G = 0x36;
        public const byte LSM9DS1_INT_GEN_DUR_G = 0x37;

        //  Gyro sample rate defines

        public const byte LSM9DS1_GYRO_SAMPLERATE_14_9 = 0;
        public const byte LSM9DS1_GYRO_SAMPLERATE_59_5 = 1;
        public const byte LSM9DS1_GYRO_SAMPLERATE_119 = 2;
        public const byte LSM9DS1_GYRO_SAMPLERATE_238 = 3;
        public const byte LSM9DS1_GYRO_SAMPLERATE_476 = 4;
        public const byte LSM9DS1_GYRO_SAMPLERATE_952 = 5;

        //  Gyro banwidth defines

        public const byte LSM9DS1_GYRO_BANDWIDTH_0 = 0;
        public const byte LSM9DS1_GYRO_BANDWIDTH_1 = 1;
        public const byte LSM9DS1_GYRO_BANDWIDTH_2 = 2;
        public const byte LSM9DS1_GYRO_BANDWIDTH_3 = 3;

        //  Gyro FSR defines

        public const byte LSM9DS1_GYRO_FSR_250 = 0;
        public const byte LSM9DS1_GYRO_FSR_500 = 1;
        public const byte LSM9DS1_GYRO_FSR_2000 = 3;

        //  Gyro high pass filter defines

        public const byte LSM9DS1_GYRO_HPF_0 = 0;
        public const byte LSM9DS1_GYRO_HPF_1 = 1;
        public const byte LSM9DS1_GYRO_HPF_2 = 2;
        public const byte LSM9DS1_GYRO_HPF_3 = 3;
        public const byte LSM9DS1_GYRO_HPF_4 = 4;
        public const byte LSM9DS1_GYRO_HPF_5 = 5;
        public const byte LSM9DS1_GYRO_HPF_6 = 6;
        public const byte LSM9DS1_GYRO_HPF_7 = 7;
        public const byte LSM9DS1_GYRO_HPF_8 = 8;
        public const byte LSM9DS1_GYRO_HPF_9 = 9;

        //  Mag Register Map

        public const byte LSM9DS1_MAG_OFFSET_X_L = 0x05;
        public const byte LSM9DS1_MAG_OFFSET_X_H = 0x06;
        public const byte LSM9DS1_MAG_OFFSET_Y_L = 0x07;
        public const byte LSM9DS1_MAG_OFFSET_Y_H = 0x08;
        public const byte LSM9DS1_MAG_OFFSET_Z_L = 0x09;
        public const byte LSM9DS1_MAG_OFFSET_Z_H = 0x0A;
        public const byte LSM9DS1_MAG_WHO_AM_I = 0x0F;
        public const byte LSM9DS1_MAG_CTRL1 = 0x20;
        public const byte LSM9DS1_MAG_CTRL2 = 0x21;
        public const byte LSM9DS1_MAG_CTRL3 = 0x22;
        public const byte LSM9DS1_MAG_CTRL4 = 0x23;
        public const byte LSM9DS1_MAG_CTRL5 = 0x24;
        public const byte LSM9DS1_MAG_STATUS = 0x27;
        public const byte LSM9DS1_MAG_OUT_X_L = 0x28;
        public const byte LSM9DS1_MAG_OUT_X_H = 0x29;
        public const byte LSM9DS1_MAG_OUT_Y_L = 0x2A;
        public const byte LSM9DS1_MAG_OUT_Y_H = 0x2B;
        public const byte LSM9DS1_MAG_OUT_Z_L = 0x2C;
        public const byte LSM9DS1_MAG_OUT_Z_H = 0x2D;
        public const byte LSM9DS1_MAG_INT_CFG = 0x30;
        public const byte LSM9DS1_MAG_INT_SRC = 0x31;
        public const byte LSM9DS1_MAG_INT_THS_L = 0x32;
        public const byte LSM9DS1_MAG_INT_THS_H = 0x33;

        //  Accel sample rate defines

        public const byte LSM9DS1_ACCEL_SAMPLERATE_14_9 = 1;
        public const byte LSM9DS1_ACCEL_SAMPLERATE_59_5 = 2;
        public const byte LSM9DS1_ACCEL_SAMPLERATE_119 = 3;
        public const byte LSM9DS1_ACCEL_SAMPLERATE_238 = 4;
        public const byte LSM9DS1_ACCEL_SAMPLERATE_476 = 5;
        public const byte LSM9DS1_ACCEL_SAMPLERATE_952 = 6;

        //  Accel FSR

        public const byte LSM9DS1_ACCEL_FSR_2 = 0;
        public const byte LSM9DS1_ACCEL_FSR_16 = 1;
        public const byte LSM9DS1_ACCEL_FSR_4 = 2;
        public const byte LSM9DS1_ACCEL_FSR_8 = 3;

        //  Accel filter bandwidth

        public const byte LSM9DS1_ACCEL_LPF_408 = 0;
        public const byte LSM9DS1_ACCEL_LPF_211 = 1;
        public const byte LSM9DS1_ACCEL_LPF_105 = 2;
        public const byte LSM9DS1_ACCEL_LPF_50 = 3;

        //  Compass sample rate defines

        public const byte LSM9DS1_COMPASS_SAMPLERATE_0_625 = 0;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_1_25 = 1;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_2_5 = 2;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_5 = 3;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_10 = 4;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_20 = 5;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_40 = 6;
        public const byte LSM9DS1_COMPASS_SAMPLERATE_80 = 7;

        //  Compass FSR

        public const byte LSM9DS1_COMPASS_FSR_4 = 0;
        public const byte LSM9DS1_COMPASS_FSR_8 = 1;
        public const byte LSM9DS1_COMPASS_FSR_12 = 2;
        public const byte LSM9DS1_COMPASS_FSR_16 = 3;

        public byte AccelGyroAddress = LSM9DS1_ADDRESS0;
        public byte MagAddress = LSM9DS1_MAG_ADDRESS0;

        public byte mLSM9DS1GyroSampleRate;                 // the gyro sample rate
        public byte mLSM9DS1GyroBW;                         // the gyro bandwidth code
        public byte mLSM9DS1GyroHpf;                        // the gyro high pass filter cutoff code
        public byte mLSM9DS1GyroFsr;                        // the gyro full scale range

        public byte mLSM9DS1AccelSampleRate;                // the accel sample rate
        public byte mLSM9DS1AccelFsr;                       // the accel full scale range
        public byte mLSM9DS1AccelLpf;                       // the accel low pass filter

        public byte mLSM9DS1CompassSampleRate;              // the compass sample rate
        public byte mLSM9DS1CompassFsr;                     // the compass full scale range

        I2cDevice mAccelGyro;                               // the accel/gyro I2C device
        I2cDevice mMag;                                     // the mag I2C device

        public RTIMULSM9DS1()
        {
            mSampleRate = 100;
            mLSM9DS1GyroSampleRate = LSM9DS1_GYRO_SAMPLERATE_119;
            mLSM9DS1GyroBW = LSM9DS1_GYRO_BANDWIDTH_1;
            mLSM9DS1GyroHpf = LSM9DS1_GYRO_HPF_4;
            mLSM9DS1GyroFsr = LSM9DS1_GYRO_FSR_500;

            mLSM9DS1AccelSampleRate = LSM9DS1_ACCEL_SAMPLERATE_119;
            mLSM9DS1AccelFsr = LSM9DS1_ACCEL_FSR_8;
            mLSM9DS1AccelLpf = LSM9DS1_ACCEL_LPF_50;

            mLSM9DS1CompassSampleRate = LSM9DS1_COMPASS_SAMPLERATE_20;
            mLSM9DS1CompassFsr = LSM9DS1_COMPASS_FSR_4;
        }

        public async void IMUInit()
        {
            byte[] oneByte = new byte[1];
            
            //  open the I2C devices

            try {
                string aqsFilter = I2cDevice.GetDeviceSelector("I2C1");

                DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(aqsFilter);
                if (collection.Count == 0)
                    return;

                I2cConnectionSettings settings0 = new I2cConnectionSettings(AccelGyroAddress);
                settings0.BusSpeed = I2cBusSpeed.FastMode;
                mAccelGyro = await I2cDevice.FromIdAsync(collection[0].Id, settings0);

                I2cConnectionSettings settings1 = new I2cConnectionSettings(MagAddress);
                settings1.BusSpeed = I2cBusSpeed.FastMode;
                mMag = await I2cDevice.FromIdAsync(collection[0].Id, settings1);
            } catch (Exception) {
                ErrorMessage = "Failed to connect to IMU";
                if (Debugger.IsAttached) {
                    Debugger.Break();
                }
                return;
            }

            //  Set up the gyro/accel

            if (!RTI2C.Write(mAccelGyro, LSM9DS1_CTRL8, 0x81)) {
                ErrorMessage = "Failed to boot LSM9DS1";
                return;
            }

            await Task.Delay(100);

            if (!RTI2C.Read(mAccelGyro, LSM9DS1_WHO_AM_I, oneByte)) {
                ErrorMessage = "Failed to read LSM9DS1 accel/gyro id";
                return;
            }

            if (oneByte[0] != LSM9DS1_ID) {
                ErrorMessage = string.Format("Incorrect LSM9DS1 gyro id {0}", oneByte[0]);
                return;
            }

            if (!SetGyroSampleRate())
                return;

            if (!SetGyroCTRL3())
                return;

            //  Set up the mag

            if (!RTI2C.Read(mMag, LSM9DS1_MAG_WHO_AM_I, oneByte)) {
                ErrorMessage = "Failed to read LSM9DS1 accel/mag id";
                return;
            }

            if (oneByte[0] != LSM9DS1_MAG_ID) {
                ErrorMessage = string.Format("Incorrect LSM9DS1 accel/mag id {0}", oneByte[0]);
                return;
            }

            if (!SetAccelCTRL6())
                return;

            if (!SetAccelCTRL7())
                return;

            if (!SetMagCTRL1())
                return;

            if (!SetMagCTRL2())
                return;

            if (!SetMagCTRL3())
                return;

            GyroBiasInit();
            ErrorMessage = "IMU init completed";
            InitComplete = true;
            return;
        }

        bool SetGyroSampleRate()
        {
            byte ctrl1;

            switch (mLSM9DS1GyroSampleRate) {
                case LSM9DS1_GYRO_SAMPLERATE_14_9:
                    ctrl1 = 0x20;
                    mSampleRate = 15;
                    break;

                case LSM9DS1_GYRO_SAMPLERATE_59_5:
                    ctrl1 = 0x40;
                    mSampleRate = 60;
                    break;

                case LSM9DS1_GYRO_SAMPLERATE_119:
                    ctrl1 = 0x60;
                    mSampleRate = 119;
                    break;

                case LSM9DS1_GYRO_SAMPLERATE_238:
                    ctrl1 = 0x80;
                    mSampleRate = 238;
                    break;

                case LSM9DS1_GYRO_SAMPLERATE_476:
                    ctrl1 = 0xa0;
                    mSampleRate = 476;
                    break;

                case LSM9DS1_GYRO_SAMPLERATE_952:
                    ctrl1 = 0xc0;
                    mSampleRate = 952;
                    break;

                default:
                    ErrorMessage = string.Format("Illegal LSM9DS1 gyro sample rate code {0}", mLSM9DS1GyroSampleRate);
                    return false;
            }

            mSampleInterval = (long)1000000 / mSampleRate;

            switch (mLSM9DS1GyroBW) {
                case LSM9DS1_GYRO_BANDWIDTH_0:
                    ctrl1 |= 0x00;
                    break;

                case LSM9DS1_GYRO_BANDWIDTH_1:
                    ctrl1 |= 0x01;
                    break;

                case LSM9DS1_GYRO_BANDWIDTH_2:
                    ctrl1 |= 0x02;
                    break;

                case LSM9DS1_GYRO_BANDWIDTH_3:
                    ctrl1 |= 0x03;
                    break;
            }

            switch (mLSM9DS1GyroFsr) {
                case LSM9DS1_GYRO_FSR_250:
                    ctrl1 |= 0x00;
                    mGyroScale = (double)0.00875 * RTMath.RTMATH_DEGREE_TO_RAD;
                    break;

                case LSM9DS1_GYRO_FSR_500:
                    ctrl1 |= 0x08;
                    mGyroScale = (double)0.0175 * RTMath.RTMATH_DEGREE_TO_RAD;
                    break;

                case LSM9DS1_GYRO_FSR_2000:
                    ctrl1 |= 0x18;
                    mGyroScale = (double)0.07 * RTMath.RTMATH_DEGREE_TO_RAD;
                    break;

                default:
                    ErrorMessage = string.Format("Illegal LSM9DS1 gyro FSR code {0}", mLSM9DS1GyroFsr);
                    return false;
            }
            if (!RTI2C.Write(mAccelGyro, LSM9DS1_CTRL1, ctrl1)) {
                ErrorMessage = "Failed to set LSM9DS1 gyro CTRL1";
                return false;
            }
            return true;
        }

        bool SetGyroCTRL3()
        {
            byte ctrl3;

            if ((mLSM9DS1GyroHpf < LSM9DS1_GYRO_HPF_0) || (mLSM9DS1GyroHpf > LSM9DS1_GYRO_HPF_9)) {
                ErrorMessage = string.Format("Illegal LSM9DS1 gyro high pass filter code {0}", mLSM9DS1GyroHpf);
                return false;
            }
            ctrl3 = mLSM9DS1GyroHpf;

            //  Turn on hpf
            ctrl3 |= 0x40;

            if (!RTI2C.Write(mAccelGyro, LSM9DS1_CTRL3, ctrl3)) {
                ErrorMessage = "Failed to set LSM9DS1 gyro CTRL3";
                return false;
            }
            return true;
        }

        bool SetAccelCTRL6()
        {
            byte ctrl6;

            if ((mLSM9DS1AccelSampleRate < 0) || (mLSM9DS1AccelSampleRate > 6)) {
                ErrorMessage = string.Format("Illegal LSM9DS1 accel sample rate code {0}", mLSM9DS1AccelSampleRate);
                return false;
            }

            ctrl6 = (byte)(mLSM9DS1AccelSampleRate << 5);

            if ((mLSM9DS1AccelLpf < 0) || (mLSM9DS1AccelLpf > 3)) {
                ErrorMessage = string.Format("Illegal LSM9DS1 accel low pass fiter code {0}", mLSM9DS1AccelLpf);
                return false;
            }

            switch (mLSM9DS1AccelFsr) {
                case LSM9DS1_ACCEL_FSR_2:
                    mAccelScale = (double)0.000061;
                    break;

                case LSM9DS1_ACCEL_FSR_4:
                    mAccelScale = (double)0.000122;
                    break;

                case LSM9DS1_ACCEL_FSR_8:
                    mAccelScale = (double)0.000244;
                    break;

                case LSM9DS1_ACCEL_FSR_16:
                    mAccelScale = (double)0.000732;
                    break;

                default:
                    ErrorMessage = string.Format("Illegal LSM9DS1 accel FSR code {0}", mLSM9DS1AccelFsr);
                    return false;
            }

            ctrl6 |= (byte)((mLSM9DS1AccelLpf) | (mLSM9DS1AccelFsr << 3));

            if (!RTI2C.Write(mAccelGyro, LSM9DS1_CTRL6, ctrl6)) {
                ErrorMessage = "Failed to set LSM9DS1 accel CTRL6";
                return false;
            }
            return true;
        }

        bool SetAccelCTRL7()
        {
            byte ctrl7;

            ctrl7 = 0x00;
            //Bug: Bad things happen.
            //ctrl7 = 0x05;

            if (!RTI2C.Write(mAccelGyro, LSM9DS1_CTRL7, ctrl7)) {
                ErrorMessage = "Failed to set LSM9DS1 accel CTRL7";
                return false;
            }
            return true;
        }


        bool SetMagCTRL1()
        {
            byte ctrl1;

            if ((mLSM9DS1CompassSampleRate < 0) || (mLSM9DS1CompassSampleRate > 5)) {
                ErrorMessage = string.Format("Illegal LSM9DS1 compass sample rate code {0}", mLSM9DS1CompassSampleRate);
                return false;
            }

            ctrl1 = (byte)(mLSM9DS1CompassSampleRate << 2);

            if (!RTI2C.Write(mMag, LSM9DS1_MAG_CTRL1, ctrl1)) {
                ErrorMessage = "Failed to set LSM9DS1 compass CTRL5";
                return false;
            }
            return true;
        }

        bool SetMagCTRL2()
        {
            byte ctrl2;

            //  convert FSR to uT

            switch (mLSM9DS1CompassFsr) {
                case LSM9DS1_COMPASS_FSR_4:
                    ctrl2 = 0;
                    mMagScale = (double)0.014;
                    break;

                case LSM9DS1_COMPASS_FSR_8:
                    ctrl2 = 0x20;
                    mMagScale = (double)0.029;
                    break;

                case LSM9DS1_COMPASS_FSR_12:
                    ctrl2 = 0x40;
                    mMagScale = (double)0.043;
                    break;

                case LSM9DS1_COMPASS_FSR_16:
                    ctrl2 = 0x60;
                    mMagScale = (double)0.058;
                    break;

                default:
                    ErrorMessage = string.Format("Illegal LSM9DS1 compass FSR code {0}", mLSM9DS1CompassFsr);
                    return false;
            }

            if (!RTI2C.Write(mMag, LSM9DS1_MAG_CTRL2, ctrl2)) {
                ErrorMessage = "Failed to set LSM9DS1 compass CTRL6";
                return false;
            }
            return true;
        }

        bool SetMagCTRL3()
        {
            if (!RTI2C.Write(mMag, LSM9DS1_MAG_CTRL3, 0x00)) {
                ErrorMessage = "Failed to set LSM9DS1 compass CTRL3";
                return false;
            }
            return true;
        }

        public int IMUGetPollInterval()
        {
            return (400 / mSampleRate);
        }

        public bool IMURead(out RTIMUData data)
        {
            byte[] status = new byte[1];
            byte[] gyroData = new byte[6];
            byte[] accelData = new byte[6];
            byte[] magData = new byte[6];

            mImuData = new RTIMUData();
            data = mImuData;
                        
            // set validity flags

            mImuData.fusionPoseValid = false;
            mImuData.fusionQPoseValid = false;
            mImuData.gyroValid = true;
            mImuData.accelValid = true;
            mImuData.magValid = true;
            mImuData.pressureValid = false;
            mImuData.temperatureValid = false;
            mImuData.humidityValid = false;

            if (!RTI2C.Read(mAccelGyro, LSM9DS1_STATUS, status)) {
                ErrorMessage = "Failed to read LSM9DS1 status";
                return false;
            }
            if ((status[0] & 0x3) != 3)
                return false;

            if (!RTI2C.Read(mAccelGyro, 0x80 + LSM9DS1_OUT_X_L_G, gyroData)) {
                ErrorMessage = "Failed to read LSM9DS1 gyro data";
                return false;
            }

            if (!RTI2C.Read(mAccelGyro, 0x80 + LSM9DS1_OUT_X_L_XL, accelData)) {
                ErrorMessage = "Failed to read LSM9DS1 accel data";
                return false;
            }

            if (!RTI2C.Read(mMag, 0x80 + LSM9DS1_MAG_OUT_X_L, magData)) {
                ErrorMessage = "Failed to read LSM9DS1 compass data";
                return false;
            }

            mImuData.timestamp = System.DateTime.Now.Ticks / (long)10;

            RTMath.ConvertToVector(gyroData, out mImuData.gyro, mGyroScale, false);
            RTMath.ConvertToVector(accelData, out mImuData.accel, mAccelScale, false);
            RTMath.ConvertToVector(magData, out mImuData.mag, mMagScale, false);

            //  sort out gyro axes and correct for bias

            mImuData.gyro.Z = -mImuData.gyro.Z;

            //  sort out accel data;

            mImuData.accel.X = -mImuData.accel.X;
            mImuData.accel.Y = -mImuData.accel.Y;

            //  sort out mag axes

            mImuData.mag.X = -mImuData.mag.X;
            mImuData.mag.Z = -mImuData.mag.Z;

            //  now do standard processing

            HandleGyroBias();
            CalibrateAverageCompass();
            data = mImuData;
            return true;
        }
    }
}