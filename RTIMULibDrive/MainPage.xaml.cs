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

using Windows.UI.Xaml.Controls;
using System.Threading;

using RTIMULibCS;

namespace RTIMULibDrive
{
    public sealed partial class MainPage : Page
    {
        private RTIMUThread thread;
        private Timer periodicTimer;

        public MainPage()
        {
            this.InitializeComponent();

            thread = new RTIMUThread();
            periodicTimer = new Timer(this.TimerCallback, null, 0, 100);
         }

        private void TimerCallback(object state)
        {
            RTIMUData data = thread.GetIMUData;

            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextGyro.Text = string.Format("Gyro (radians/sec): x: {0:F4}, y: {1:F4}, z: {2:F4}",
                                data.gyro.X, data.gyro.Y, data.gyro.Z);
                TextAccel.Text = string.Format("Accel (g): x: {0:F4}, y: {1:F4}, z: {2:F4}",
                                data.accel.X, data.accel.Y, data.accel.Z);
                TextMag.Text = string.Format("Mag (uT): x: {0:F4}, y: {1:F4}, z: {2:F4}",
                                data.mag.X, data.mag.Y, data.mag.Z);

                TextPose.Text = RTMath.DisplayDegrees("Pose: ", data.fusionPose);
                TextQPose.Text = RTMath.Display("QPose: ", data.fusionQPose);

                TextStatus.Text = string.Format("Gyro bias: {0}, Mag cal: {1}",
                    thread.GyroBiasValid ? "Valid" : "Invalid", thread.MagCalValid ? "Valid" : "Invalid");

                if (thread.InitComplete)
                    TextRate.Text = string.Format("Rate: {0} samples per second", thread.SampleRate);
                else
                    TextStatus.Text = thread.ErrorMessage;
                
            });
        }
    }
}
