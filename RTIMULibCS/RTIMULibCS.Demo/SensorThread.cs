using System;
using System.Threading.Tasks;
using RichardsTech.Sensors;
using RichardsTech.Sensors.Devices.HTS221;
using RichardsTech.Sensors.Devices.LPS25H;
using RichardsTech.Sensors.Devices.LSM9DS1;

namespace RTIMULibCS.Demo
{
	public class SensorThread
	{
		private readonly object _syncObj = new object();

		private readonly LSM9DS1ImuSensor _imuSensor;
		private readonly HTS221HumiditySensor _humiditySensor;
		private readonly LPS25HPressureSensor _pressureSensor;
		private readonly SensorFusionRTQF _fusion = new SensorFusionRTQF();
		private SensorReadings _fusionReadings;
		private int _imuSampleCount;
		private int _pressureSampleCount;
		private int _humiditySampleCount;
		private int _imuSampleRate;
		private int _pressureSampleRate;
		private int _humiditySampleRate;
		private string _imuErrorMessage;
		private string _pressureErrorMessage;
		private string _humidityErrorMessage;

		public SensorThread()
		{
			var lsm9Ds1Config = new LSM9DS1Config();
			_imuSensor = new LSM9DS1ImuSensor(LSM9DS1Defines.ADDRESS0, LSM9DS1Defines.MAG_ADDRESS0, lsm9Ds1Config);

			_humiditySensor = new HTS221HumiditySensor(HTS221Defines.ADDRESS);

			_pressureSensor = new LPS25HPressureSensor(LPS25HDefines.ADDRESS0);

			Start();
		}

		private void Start()
		{
			InitDevices().ContinueWith(t => MainLoop());
		}

		private async Task InitDevices()
		{
			_imuErrorMessage = await InitSensorAsync(_imuSensor);
			_pressureErrorMessage = await InitSensorAsync(_pressureSensor);
			_humidityErrorMessage = await InitSensorAsync(_humiditySensor);
		}

		private static async Task<string> InitSensorAsync(Sensor sensor)
		{
			try
			{
				var initiated = await sensor.InitAsync();
				if (!initiated)
				{
					return "Initiation failed";
				}

				return string.Empty;
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}

		private void MainLoop()
		{
			DateTime startTime = DateTime.Now;
			DateTime oneSecondAfterStartTime = startTime.AddSeconds(1);

			while (true)
			{
				Task.Delay(2);

				SensorReadings readings;
				string errorMessage;

				if (TryReadSensor(_imuSensor, out readings, out errorMessage))
				{
					lock (_syncObj)
					{
						_fusion.ProcessNewImuReadings(ref readings);
						_fusionReadings = readings;
						_imuSampleCount++;
					}
				}

				_imuErrorMessage = errorMessage;

				if (TryReadSensor(_pressureSensor, out readings, out errorMessage))
				{
					lock (_syncObj)
					{
						_pressureSampleCount++;
					}
				}

				_pressureErrorMessage = errorMessage;

				if (TryReadSensor(_humiditySensor, out readings, out errorMessage))
				{
					lock (_syncObj)
					{
						_humiditySampleCount++;
					}
				}

				_humidityErrorMessage = errorMessage;

				if (DateTime.Now >= oneSecondAfterStartTime)
				{
					lock (_syncObj)
					{
						_imuSampleRate = _imuSampleCount;
						_pressureSampleRate = _pressureSampleCount;
						_humiditySampleRate = _humiditySampleCount;

						_imuSampleCount = 0;
						_pressureSampleCount = 0;
						_humiditySampleCount = 0;

						startTime = DateTime.Now;
						oneSecondAfterStartTime = startTime.AddSeconds(1);
					}
				}
			}
		}

		private static bool TryReadSensor(
			Sensor sensor,
			out SensorReadings readings,
			out string errorMessage)
		{
			errorMessage = string.Empty;

			if (sensor.Initiated)
			{
				try
				{
					if (sensor.Update())
					{
						readings = sensor.Readings;
						return true;
					}
				}
				catch (Exception exception)
				{
					errorMessage = exception.Message;
				}
			}

			readings = new SensorReadings();
			return false;
		}


		public ImuSensorData GetImuSensorData()
		{
			lock (_syncObj)
			{
				return new ImuSensorData
				{
					Initiated = _imuSensor.Initiated,
					ErrorMessage = _imuErrorMessage ?? string.Empty,
					Readings = _imuSensor.Readings,
					SampleRate = _imuSampleRate,
					GyroBiasValid = _imuSensor.GyroBiasValid,
					MagCalValid = _imuSensor.MagCalValid
				};
			}
		}

		public SensorData GetPressureSensorData()
		{
			lock (_syncObj)
			{
				return new SensorData
				{
					Initiated = _pressureSensor.Initiated,
					ErrorMessage = _pressureErrorMessage ?? string.Empty,
					Readings = _pressureSensor.Readings,
					SampleRate = _pressureSampleRate,
				};
			}
		}

		public SensorData GetHumiditySensorData()
		{
			lock (_syncObj)
            {
				return new SensorData
				{
					Initiated = _humiditySensor.Initiated,
					ErrorMessage = _humidityErrorMessage ?? string.Empty,
					Readings = _humiditySensor.Readings,
					SampleRate = _humiditySampleRate,
				};
			}
		}

		public SensorData GetFusionSensorData()
		{
			lock (_syncObj)
            {
				return new SensorData
				{
					Initiated = true,
					ErrorMessage = "",
					Readings = _fusionReadings,
					SampleRate = -1
				};
			}
		}
	}
}
