using RichardsTech.Sensors;

namespace RTIMULibCS.Demo
{
	public class ImuSensorData : SensorData
	{
		public bool GyroBiasValid
		{ get; set; }

		public bool MagCalValid
		{ get; set; }
	}
}