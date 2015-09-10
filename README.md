# RTIMULibCS

A version of RTIMULib in C# for Windows IoT. It currently only supports the LSM9DS1, HTS221 and LPS25H as fitted on the Raspberry Pi Sense HAT.

Note that magnetometer calibration has changed on this version. There is no separate calibration process. Instead, each time the code is started, the calibration parameters are captured dynamically. For best results, make sure that the device moves though the maxima and minima on all axes to ensure that correct readings are obtained. Accelerometer calibration is not supported.
