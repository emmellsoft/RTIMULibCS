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

namespace RTIMULibCS
{
    public class RTHumidityHTS221
    {
        public string ErrorMessage = "";

        public bool InitComplete { get { return this.mInitComplete; } }
        private bool mInitComplete = false;                   // set true when init has completed                    

        public bool HumidityValid { get { return this.mHumidityValid; } }
        private bool mHumidityValid = false;

        public bool TemperatureValid { get { return this.mTemperatureValid; } }
        private bool mTemperatureValid = false;

        private double mHumidity;                           // the current humidity
        private double mTemperature;                        // the current temperature
        private double mTemperature_m;                      // temperature calibration slope
        private double mTemperature_c;                      // temperature calibration y intercept
        private double mHumidity_m;                         // humidity calibration slope
        private double mHumidity_c;                         // humidity calibration y intercept

        //  HTS221 I2C Slave Address

        public const byte HTS221_ADDRESS = 0x5f;
        public const byte HTS221_REG_ID = 0x0f;
        public const byte HTS221_ID = 0xbc;

        //  Register map

        public const byte HTS221_WHO_AM_I = 0x0f;
        public const byte HTS221_AV_CONF = 0x10;
        public const byte HTS221_CTRL1 = 0x20;
        public const byte HTS221_CTRL2 = 0x21;
        public const byte HTS221_CTRL3 = 0x22;
        public const byte HTS221_STATUS = 0x27;
        public const byte HTS221_HUMIDITY_OUT_L = 0x28;
        public const byte HTS221_HUMIDITY_OUT_H = 0x29;
        public const byte HTS221_TEMP_OUT_L = 0x2a;
        public const byte HTS221_TEMP_OUT_H = 0x2b;
        public const byte HTS221_H0_H_2 = 0x30;
        public const byte HTS221_H1_H_2 = 0x31;
        public const byte HTS221_T0_C_8 = 0x32;
        public const byte HTS221_T1_C_8 = 0x33;
        public const byte HTS221_T1_T0 = 0x35;
        public const byte HTS221_H0_T0_OUT = 0x36;
        public const byte HTS221_H1_T0_OUT = 0x3a;
        public const byte HTS221_T0_OUT = 0x3c;
        public const byte HTS221_T1_OUT = 0x3e;

        private I2cDevice mHum;

        public async void HumidityInit()
        {
            byte[] oneByte = new byte[1];
            byte[] twoByte = new byte[2];
            byte H0_H_2 = 0;
            byte H1_H_2 = 0;
            UInt16 T0_C_8 = 0;
            UInt16 T1_C_8 = 0;
            Int16 H0_T0_OUT = 0;
            Int16 H1_T0_OUT = 0;
            Int16 T0_OUT = 0;
            Int16 T1_OUT = 0;
            double H0, H1, T0, T1;

            try {
                string aqsFilter = I2cDevice.GetDeviceSelector("I2C1");

                DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(aqsFilter);
                if (collection.Count == 0)
                    return;

                I2cConnectionSettings settings0 = new I2cConnectionSettings(HTS221_ADDRESS);
                settings0.BusSpeed = I2cBusSpeed.FastMode;
                mHum = await I2cDevice.FromIdAsync(collection[0].Id, settings0);

            } catch (Exception) {
                ErrorMessage = "Failed to connect to HTS221";
                if (Debugger.IsAttached) {
                    Debugger.Break();
                }
                return;
            }

            if (!RTI2C.Write(mHum, HTS221_CTRL1, 0x87)) {
                ErrorMessage = "Failed to set HTS221 CTRL_REG_1";
                return;
            }

            if (!RTI2C.Write(mHum, HTS221_AV_CONF, 0x1b)) {
                ErrorMessage = "Failed to set HTS221 AV_CONF";
                return;
            }

            // Get calibration data

            if (!RTI2C.Read(mHum, HTS221_T1_T0 + 0x80, oneByte)) {
                ErrorMessage = "Failed to read HTS221 T1_T0";
                return;
            }
            byte temp0 = oneByte[0];

            if (!RTI2C.Read(mHum, HTS221_T0_C_8 + 0x80, oneByte)) {
                ErrorMessage = "Failed to read HTS221 T0_C_8";
                return;
            }
            byte temp1 = oneByte[0];

            T0_C_8 = (UInt16)((((UInt16)temp1 & 0x3) << 8) | (UInt16)temp0);
            T0 = (double)T0_C_8 / 8.0;

            if (!RTI2C.Read(mHum, HTS221_T1_C_8 + 0x80, oneByte)) {
                ErrorMessage = "Failed to read HTS221 T1_C_8";
                return;
            }
            temp0 = oneByte[0];

            T1_C_8 = (UInt16)(((UInt16)(temp1 & 0xC) << 6) | (UInt16)temp0);
            T1 = (double)T1_C_8 / 8.0;

            if (!RTI2C.Read(mHum, HTS221_T0_OUT + 0x80, twoByte)) {
                ErrorMessage = "Failed to read HTS221 T0_OUT";
                return;
            }

            T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

            if (!RTI2C.Read(mHum, HTS221_T1_OUT + 0x80, twoByte)) {
                ErrorMessage = "Failed to read HTS221 T1_OUT";
                return;
            }

            T1_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

            if (!RTI2C.Read(mHum, HTS221_H0_H_2 + 0x80, oneByte)) {
                ErrorMessage = "Failed to read HTS221 H0_H_2";
                return;
            }

            H0_H_2 = oneByte[0];
            H0 = (double)H0_H_2 / 2.0;

            if (!RTI2C.Read(mHum, HTS221_H1_H_2 + 0x80, oneByte)) {
                ErrorMessage = "Failed to read HTS221 H1_H_2";
                return;
            }

            H1_H_2 = oneByte[0];
            H1 = (double)H1_H_2 / 2.0;

            if (!RTI2C.Read(mHum, HTS221_H0_T0_OUT + 0x80, twoByte)) {
                ErrorMessage = "Failed to read HTS221 H0_T_OUT";
                return;
            }

            H0_T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);


            if (!RTI2C.Read(mHum, HTS221_H1_T0_OUT + 0x80, twoByte)) {
                ErrorMessage = "Failed to read HTS221 H1_T_OUT";
                return;
            }

            H1_T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

            mTemperature_m = (T1 - T0) / (T1_OUT - T0_OUT);
            mTemperature_c = T0 - (mTemperature_m * T0_OUT);
            mHumidity_m = (H1 - H0) / (H1_T0_OUT - H0_T0_OUT);
            mHumidity_c = (H0) - (mHumidity_m * H0_T0_OUT);

            mInitComplete = true;
            ErrorMessage = "HTS221 init complete";
            return;
        }


        public bool HumidityRead(ref RTIMUData data)
        {
            byte[] oneByte = new byte[1];
            byte[] twoByte = new byte[2];

            data.humidityValid = false;
            data.temperatureValid = false;
            data.temperature = 0;
            data.humidity = 0;

            if (!RTI2C.Read(mHum, HTS221_STATUS, oneByte)) {
                ErrorMessage = "Failed to read HTS221 status";
                return false;
            }

            if ((oneByte[0] & 2) == 2) {
                if (!RTI2C.Read(mHum, HTS221_HUMIDITY_OUT_L + 0x80, twoByte)) {
                    ErrorMessage = "Failed to read HTS221 humidity";
                    return false;
                }

                mHumidity = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);
                mHumidity = mHumidity * mHumidity_m + mHumidity_c;
                mHumidityValid = true;
            }
            if ((oneByte[0] & 1) == 1) {
                if (!RTI2C.Read(mHum, HTS221_TEMP_OUT_L + 0x80, twoByte)) {
                    ErrorMessage = "Failed to read HTS221 temperature";
                    return false;
                }

                mTemperature = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);
                mTemperature = mTemperature * mTemperature_m + mTemperature_c;
                mTemperatureValid = true;
            }

            data.humidityValid = mHumidityValid;
            data.humidity = mHumidity;
            data.temperatureValid = mTemperatureValid;
            data.temperature = mTemperature;

            return true;
        }
    }
}
