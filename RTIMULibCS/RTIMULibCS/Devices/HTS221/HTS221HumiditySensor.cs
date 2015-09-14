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
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace RichardsTech.Sensors.Devices.HTS221
{
	public class HTS221HumiditySensor : HumiditySensor
	{
		private readonly byte _i2CAddress;

		private I2cDevice _i2CDevice;

		private double _temperatureM;       // temperature calibration slope
		private double _temperatureC;       // temperature calibration y intercept
		private double _humidityM;          // humidity calibration slope
		private double _humidityC;          // humidity calibration y intercept

		public HTS221HumiditySensor(byte i2CAddress)
		{
			_i2CAddress = i2CAddress;
		}

		protected override async Task<bool> InitDeviceAsync()
		{
			await ConnectToI2CDevices();

			byte[] oneByte = new byte[1];
			byte[] twoByte = new byte[2];

			if (!I2CSupport.Write(_i2CDevice, HTS221Defines.CTRL1, 0x87))
			{
				throw new SensorException("Failed to set HTS221 CTRL_REG_1");
			}

			if (!I2CSupport.Write(_i2CDevice, HTS221Defines.AV_CONF, 0x1b))
			{
				throw new SensorException("Failed to set HTS221 AV_CONF");
			}

			// Get calibration data

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.T1_T0 + 0x80, oneByte))
			{
				throw new SensorException("Failed to read HTS221 T1_T0");
			}

			byte temp0 = oneByte[0];

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.T0_C_8 + 0x80, oneByte))
			{
				throw new SensorException("Failed to read HTS221 T0_C_8");
			}

			byte temp1 = oneByte[0];

			UInt16 T0_C_8 = (UInt16)((((UInt16)temp1 & 0x3) << 8) | (UInt16)temp0);
			double T0 = T0_C_8 / 8.0;

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.T1_C_8 + 0x80, oneByte))
			{
				throw new SensorException("Failed to read HTS221 T1_C_8");
			}

			temp0 = oneByte[0];

			UInt16 T1_C_8 = (UInt16)(((UInt16)(temp1 & 0xC) << 6) | (UInt16)temp0);
			double T1 = T1_C_8 / 8.0;

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.T0_OUT + 0x80, twoByte))
			{
				throw new SensorException("Failed to read HTS221 T0_OUT");
			}

			Int16 T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.T1_OUT + 0x80, twoByte))
			{
				throw new SensorException("Failed to read HTS221 T1_OUT");
			}

			Int16 T1_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.H0_H_2 + 0x80, oneByte))
			{
				throw new SensorException("Failed to read HTS221 H0_H_2");
			}

			byte H0_H_2 = oneByte[0];
			double H0 = H0_H_2 / 2.0;

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.H1_H_2 + 0x80, oneByte))
			{
				throw new SensorException("Failed to read HTS221 H1_H_2");
			}

			byte H1_H_2 = oneByte[0];
			double H1 = H1_H_2 / 2.0;

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.H0_T0_OUT + 0x80, twoByte))
			{
				throw new SensorException("Failed to read HTS221 H0_T_OUT");
			}

			Int16 H0_T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.H1_T0_OUT + 0x80, twoByte))
			{
				throw new SensorException("Failed to read HTS221 H1_T_OUT");
			}

			Int16 H1_T0_OUT = (Int16)((((UInt16)twoByte[1]) << 8) | (UInt16)twoByte[0]);

			_temperatureM = (T1 - T0) / (T1_OUT - T0_OUT);
			_temperatureC = T0 - (_temperatureM * T0_OUT);
			_humidityM = (H1 - H0) / (H1_T0_OUT - H0_T0_OUT);
			_humidityC = H0 - (_humidityM * H0_T0_OUT);

			return true;
		}

		private async Task ConnectToI2CDevices()
		{
			try
			{
				string aqsFilter = I2cDevice.GetDeviceSelector("I2C1");

				DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(aqsFilter);
				if (collection.Count == 0)
				{
					throw new SensorException("I2C device not found");
				}

				I2cConnectionSettings i2CSettings = new I2cConnectionSettings(_i2CAddress)
				{
					BusSpeed = I2cBusSpeed.FastMode
				};

				_i2CDevice = await I2cDevice.FromIdAsync(collection[0].Id, i2CSettings);
			}
			catch (Exception exception)
			{
				throw new SensorException("Failed to connect to HTS221", exception);
			}
		}

		public override bool Update()
		{
			byte[] oneByte = new byte[1];
			byte[] twoByte = new byte[2];

			if (!I2CSupport.Read(_i2CDevice, HTS221Defines.STATUS, oneByte))
			{
				throw new SensorException("Failed to read HTS221 status");
			}

			var readings = new SensorReadings
			{
				Timestamp = DateTime.Now
			};

			if ((oneByte[0] & 0x02) == 0x02)
			{
				if (!I2CSupport.Read(_i2CDevice, HTS221Defines.HUMIDITY_OUT_L + 0x80, twoByte))
				{
					throw new SensorException("Failed to read HTS221 humidity");
				}

				Int16 humidity = (Int16)(((twoByte[1]) << 8) | twoByte[0]);
				readings.Humidity = humidity * _humidityM + _humidityC;
				readings.HumidityValid = true;
			}

			if ((oneByte[0] & 0x01) == 0x01)
			{
				if (!I2CSupport.Read(_i2CDevice, HTS221Defines.TEMP_OUT_L + 0x80, twoByte))
				{
					throw new SensorException("Failed to read HTS221 temperature");
				}

				Int16 temperature = (Int16)(((twoByte[1]) << 8) | twoByte[0]);
				readings.Temperature = temperature * _temperatureM + _temperatureC;
				readings.TemperatureValid = true;
			}

			if (readings.HumidityValid || readings.TemperatureValid)
			{
				AssignNewReadings(readings);
				return true;
			}

			return false;
		}
	}
}
