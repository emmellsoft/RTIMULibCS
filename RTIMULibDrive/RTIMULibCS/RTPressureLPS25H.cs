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
    public class RTPressureLPS25H
    {

        public string ErrorMessage = "";

        public bool InitComplete { get { return this.mInitComplete; } }
        private bool mInitComplete = false;                 // set true when init has completed                    

        public bool PressureValid { get { return this.mPressureValid; } }
        private bool mPressureValid = false;

        public bool TemperatureValid { get { return this.mTemperatureValid; } }
        private bool mTemperatureValid = false;

        private double mPressure;                           // the current pressure
        private double mTemperature;                        // the current temperature

        //  LPS25H I2C Slave Addresses

        public const byte LPS25H_ADDRESS0 = 0x5c;
        public const byte LPS25H_ADDRESS1 = 0x5d;
        public const byte LPS25H_REG_ID = 0x0f;
        public const byte LPS25H_ID = 0xbd;

        //	Register map

        public const byte LPS25H_REF_P_XL = 0x08;
        public const byte LPS25H_REF_P_XH = 0x09;
        public const byte LPS25H_RES_CONF = 0x10;
        public const byte LPS25H_CTRL_REG_1 = 0x20;
        public const byte LPS25H_CTRL_REG_2 = 0x21;
        public const byte LPS25H_CTRL_REG_3 = 0x22;
        public const byte LPS25H_CTRL_REG_4 = 0x23;
        public const byte LPS25H_INT_CFG = 0x24;
        public const byte LPS25H_INT_SOURCE = 0x25;
        public const byte LPS25H_STATUS_REG = 0x27;
        public const byte LPS25H_PRESS_OUT_XL = 0x28;
        public const byte LPS25H_PRESS_OUT_L = 0x29;
        public const byte LPS25H_PRESS_OUT_H = 0x2a;
        public const byte LPS25H_TEMP_OUT_L = 0x2b;
        public const byte LPS25H_TEMP_OUT_H = 0x2c;
        public const byte LPS25H_FIFO_CTRL = 0x2e;
        public const byte LPS25H_FIFO_STATUS = 0x2f;
        public const byte LPS25H_THS_P_L = 0x30;
        public const byte LPS25H_THS_P_H = 0x31;
        public const byte LPS25H_RPDS_L = 0x39;
        public const byte LPS25H_RPDS_H = 0x3a;

        private I2cDevice mPress;

        public async void PressureInit()
        {
            try {
                string aqsFilter = I2cDevice.GetDeviceSelector("I2C1");

                DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(aqsFilter);
                if (collection.Count == 0)
                    return;

                I2cConnectionSettings settings0 = new I2cConnectionSettings(LPS25H_ADDRESS0);
                settings0.BusSpeed = I2cBusSpeed.FastMode;
                mPress = await I2cDevice.FromIdAsync(collection[0].Id, settings0);

            } catch (Exception) {
                ErrorMessage = "Failed to connect to LPS25H";
                if (Debugger.IsAttached) {
                    Debugger.Break();
                }
                return;
            }

            if (!RTI2C.Write(mPress, LPS25H_CTRL_REG_1, 0xc4)) {
                ErrorMessage = "Failed to set LPS25H CTRL_REG_1";
                return;
            }

            if (!RTI2C.Write(mPress, LPS25H_RES_CONF, 0x05)) {
                ErrorMessage = "Failed to set LPS25H RES_CONF";
                return;
            }

            if (!RTI2C.Write(mPress, LPS25H_FIFO_CTRL, 0xc0)) {
                ErrorMessage = "Failed to set LPS25H FIFO_CTRL";
                return;
            }

            if (!RTI2C.Write(mPress, LPS25H_CTRL_REG_2, 0x40)) {
                ErrorMessage = "Failed to set LPS25H CTRL_REG_2";
                return;
            }

            mInitComplete = true;
            ErrorMessage = "LPS25H init complete";
            return;
        }


        public bool PressureRead(ref RTIMUData data)
        {
            byte[] oneByte = new byte[1];
            byte[] twoByte = new byte[2];
            byte[] threeByte = new byte[3];

            data.pressureValid = false;
            data.temperatureValid = false;
            data.temperature = 0;
            data.pressure = 0;

            if (!RTI2C.Read(mPress, LPS25H_STATUS_REG, oneByte)) {
                ErrorMessage = "Failed to read LPS25H status";
                return false;
            }

            if ((oneByte[0] & 2) == 2) {
                if (!RTI2C.Read(mPress, LPS25H_PRESS_OUT_XL + 0x80, threeByte)) {
                    ErrorMessage = "Failed to read LPS25H pressure";
                    return false;
                }

                mPressure = (double)((((UInt32)threeByte[2]) << 16) | (((UInt32)threeByte[1]) << 8) | (UInt32)threeByte[0]) / (double)4096;
                mPressureValid = true;
            }
            if ((oneByte[0] & 1) == 1) {
                if (!RTI2C.Read(mPress, LPS25H_TEMP_OUT_L + 0x80, twoByte)) {
                    ErrorMessage = "Failed to read LPS25H temperature";
                    return false;
                }

                mTemperature = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]) / (double)480 + (double)42.5;
                mTemperatureValid = true;
            }

            data.pressureValid = mPressureValid;
            data.pressure = mPressure;
            data.temperatureValid = mTemperatureValid;
            data.temperature = mTemperature;

            return true;
        }
    }
}
