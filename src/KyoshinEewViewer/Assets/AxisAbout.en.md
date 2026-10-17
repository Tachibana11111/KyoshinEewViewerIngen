## About [AXIS](https://axis.prioris.jp/)

AXIS is a service that delivers earthquake and weather information in real time using WebSocket. (free)  
By default, KyoshinEewViewer for ingen receives earthquake information using the [JMA Disaster Prevention XML (PULL type)](http://xml.kishou.go.jp/xmlpull.html), but this method introduces a lag of **about 1 to 3 minutes** from announcement to reception.  
Using AXIS reduces it to **a few seconds**.

### Currently supported channels

- `eew`
    - Available in the Kyoshin Monitor tab.
    - The warning state may not be stable.
- Earthquake information, tsunami information and so on are planned for the future.

### Notes

- AXIS is in beta testing, and its stability has not been confirmed in this app either. Unexpected errors may occur.
- AXIS cannot receive past information, so information that has not been reflected in JMA's feed cannot be received.
- Use of the service is limited to non-commercial purposes.

> [!WARNING]
> Redistribution, reproduction, copying, modification and commercial use of information received through AXIS are prohibited.
> If you record or stream videos, please be careful.
