# GpioSampler
Application to sample and visualize protocols on the GPIO interface

![image](https://user-images.githubusercontent.com/46497296/215291077-e60de3bb-6903-4a66-bf2f-5af2322f0f7e.png)

## Usage

Build the project and deploy the application to a server with a GPIO sysfs.

Edit the appsettings.json and specify the lines to debug, the interval to sample, and the duration.

```json
{
  "Sampler": {
    "Duration": "00:00:03.000",
    "Interval": "00:00:00.020",
    "Lines": [
      {
        "Label": "DATA",
        "FilePath": "/sys/class/gpio/gpio102/value"
      },
      {
        "Label": "CLOCK",
        "FilePath": "/sys/class/gpio/gpio101/value"
      }
    ]
  }
}

```
