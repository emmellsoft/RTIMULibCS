using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RichardsTech.Sensors;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RTIMULibCS.Demo
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
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
			SensorData fusionSensorData = _sensorThread.GetFusionSensorData();

			ImuInitiated.Text = imuSensorData.Initiated ? "Yes" : "No";
			ImuError.Text = imuSensorData.ErrorMessage;
			ImuSampleRate.Text = imuSensorData.SampleRate.ToString();
			ImuGyro.Text = ReadingToString(imuSensorData.Readings.GyroValid, imuSensorData.Readings.Gyro, false, "radians/s");
			ImuAccel.Text = ReadingToString(imuSensorData.Readings.AccelerationValid, imuSensorData.Readings.Acceleration, false, "g");
			ImuMag.Text = ReadingToString(imuSensorData.Readings.MagneticFieldValid, imuSensorData.Readings.MagneticField, false, "\u00B5T");
			ImuGyroBiasValid.Text = imuSensorData.GyroBiasValid ? "Yes" : "No";
			ImuMagCalValid.Text = imuSensorData.MagCalValid ? "Yes" : "No";

			FusionPose.Text = ReadingToString(fusionSensorData.Readings.FusionPoseValid, fusionSensorData.Readings.FusionPose, true, "");
			FusionQPose.Text = ReadingToString(fusionSensorData.Readings.FusionQPoseValid, fusionSensorData.Readings.FusionQPose, "");

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
