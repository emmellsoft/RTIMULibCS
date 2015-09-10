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

using System.Threading.Tasks;

using RTIMULibCS;

namespace RTIMULibDrive
{
    public class RTIMUThread
    {
        public bool IMUInitComplete { get { return this.mImu.InitComplete; } }
        public string IMUErrorMessage { get { return this.mImu.ErrorMessage; } }
        public RTIMUData GetIMUData {  get { lock(this) { return this.mImuData; } } }
        public bool GyroBiasValid { get { return this.mImu.GyroBiasValid; } }
        public bool MagCalValid { get { return this.mImu.MagCalValid; } }
        public int SampleRate {  get { return this.mSampleRate; } }

        public bool HumidityInitComplete { get { return this.mHumidity.InitComplete; } }
        public string HumidityErrorMessage { get { return this.mHumidity.ErrorMessage; } }

        public bool PressureInitComplete { get { return this.mPressure.InitComplete; } }
        public string PressureErrorMessage { get { return this.mPressure.ErrorMessage; } }

        private RTIMULSM9DS1 mImu = new RTIMULSM9DS1();
        private RTFusionRTQF mFusion = new RTFusionRTQF();
        private RTHumidityHTS221 mHumidity = new RTHumidityHTS221();
        private RTPressureLPS25H mPressure = new RTPressureLPS25H();
        private RTIMUData mImuData = new RTIMUData();
        private int mSampleCount = 0;
        private int mSampleRate = 0;
        private long mStartTime = 0;

        public RTIMUThread()
        {
            mImu.IMUInit();
            mPressure.PressureInit();
            mHumidity.HumidityInit();

            mStartTime = System.DateTime.Now.Ticks;

            Task.Run(() =>
            {
                while (true) {
                    Task.Delay(2);
                    if (mImu.InitComplete) {
                        RTIMUData data;
                        while (mImu.IMURead(out data)) {
                            mSampleCount++;

                            // collect all of the data
                            mFusion.NewIMUData(ref data);
                            mHumidity.HumidityRead(ref data);
                            mPressure.PressureRead(ref data);
 
                            // data is now ready for procesing - just pass to display in this case

                            lock (this) {
                                mImuData = data;
                            }
                        }
                    }

                    if ((System.DateTime.Now.Ticks - mStartTime) >= 10000000) {
                        mStartTime = System.DateTime.Now.Ticks;
                        mSampleRate = mSampleCount;
                        mSampleCount = 0;
                    }
                }

            });
        }
    }
}
