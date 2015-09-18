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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RichardsTech.Sensors;

namespace RTIMULibCS.Demo
{
	public sealed partial class MainPage : Page
	{
		private readonly SensorThread _sensorThread = new SensorThread();
		private readonly DispatcherTimer _timer = new DispatcherTimer();

		public MainPage()
		{
			InitializeComponent();

			_timer.Interval = TimeSpan.FromSeconds(0.3);
			_timer.Tick += (s, e) => UpdateValues();
			_timer.Start();
		}

		private void UpdateValues()
		{
			ImuSensorData imuSensorData = _sensorThread.GetImuSensorData();
			SensorData pressureSensorData = _sensorThread.GetPressureSensorData();
			SensorData humiditySensorData = _sensorThread.GetHumiditySensorData();

			ImuInitiated.Text = imuSensorData.Initiated ? "Yes" : "No";
			ImuError.Text = imuSensorData.ErrorMessage;
			ImuSampleRate.Text = imuSensorData.SampleRate.ToString();
			ImuGyro.Text = ReadingToString(imuSensorData.Readings.GyroValid, imuSensorData.Readings.Gyro, false, "radians/s");
			ImuAccel.Text = ReadingToString(imuSensorData.Readings.AccelerationValid, imuSensorData.Readings.Acceleration, false, "g");
			ImuMag.Text = ReadingToString(imuSensorData.Readings.MagneticFieldValid, imuSensorData.Readings.MagneticField, false, "\u00B5T");
			ImuGyroBiasValid.Text = imuSensorData.GyroBiasValid ? "Yes" : "No";
			ImuMagCalValid.Text = imuSensorData.MagCalValid ? "Yes" : "No";

			FusionPose.Text = ReadingToString(imuSensorData.Readings.FusionPoseValid, imuSensorData.Readings.FusionPose, true, "");
			FusionQPose.Text = ReadingToString(imuSensorData.Readings.FusionQPoseValid, imuSensorData.Readings.FusionQPose, "");

			PressureInitiated.Text = pressureSensorData.Initiated ? "Yes" : "No";
			PressureError.Text = pressureSensorData.ErrorMessage;
			PressureSampleRate.Text = pressureSensorData.SampleRate.ToString();
			PressurePressure.Text = ReadingToString(pressureSensorData.Readings.PressureValid, pressureSensorData.Readings.Pressure, "hPa");
			PressureTemp.Text = ReadingToString(pressureSensorData.Readings.TemperatureValid, pressureSensorData.Readings.Temperature, "\u00B0C");

			HumidityInitiated.Text = humiditySensorData.Initiated ? "Yes" : "No";
			HumidityError.Text = humiditySensorData.ErrorMessage;
			HumiditySampleRate.Text = humiditySensorData.SampleRate.ToString();
			HumidityHumidity.Text = ReadingToString(humiditySensorData.Readings.HumidityValid, humiditySensorData.Readings.Humidity, "%RH");
			HumidityTemp.Text = ReadingToString(humiditySensorData.Readings.TemperatureValid, humiditySensorData.Readings.Temperature, "\u00B0C");
		}

		private static string ReadingToString(bool valid, Vector3 vector, bool asDegrees, string unit)
		{
			return !valid ? "N/A" : vector.ToString(asDegrees) + " " + unit;
		}

		private static string ReadingToString(bool valid, Quaternion quaternion, string unit)
		{
			return !valid ? "N/A" : quaternion + " " + unit;
		}

		private static string ReadingToString(bool valid, double value, string unit)
		{
			return !valid ? "N/A" : $"{value:0.0000} {unit}";
		}
	}
}
