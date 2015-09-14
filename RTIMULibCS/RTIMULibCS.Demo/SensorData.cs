using RichardsTech.Sensors;

namespace RTIMULibCS.Demo
{
	public class SensorData
	{
		public bool Initiated
		{ get; set; }

		public string ErrorMessage
		{ get; set; }

		public SensorReadings Readings
		{ get; set; }

		public int SampleRate
		{ get; set; }
	}
}