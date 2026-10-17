## About the [DC Report (Disaster and Crisis Management Report)](https://qzss.go.jp/overview/services/sv08_dc-report.html)

This is a service that delivers disaster-prevention information using the GPS L1S signal of the quasi-zenith satellite system "Michibiki" (QZSS).  
The service area is mainly Asia and Oceania.  
[Please check here for details about the service area.](https://qzss.go.jp/technical/system/dcr.html)

You need a device that supports L1S reception and NMEA output to a PC.  
[Please check here for compatible devices.](https://qzss.go.jp/usage/products/list.html)  
Depending on the device, module or board, you may need to change settings or use a USB-serial converter, so please be careful.  
At present, devices that output QZQSM sentences and output via the u-blox protocol are supported.

This software parses and displays the NMEA output by a GPS receiver.  
Connect a compatible device to your PC and specify the serial port recognized by the PC.  
If the receiver outputs over TCP, select TCP as the connection type and specify the host to connect to.
