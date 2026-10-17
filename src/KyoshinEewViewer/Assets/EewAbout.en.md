Earthquake Early Warnings have characteristics and limitations. For details, please [check the JMA website](https://www.data.jma.go.jp/svd/eew/data/nc/shikumi/whats-eew.html).

Reception through Kyoshin Monitor (the initial state) supports only one Earthquake Early Warning at a time, but this software supports receiving multiple Earthquake Early Warnings in a simulated way.
In that case, please keep the following points, which come from the specifications of Kyoshin Monitor, in mind.

- It cannot be received when the hypocenter is too deep, or when it becomes deeper in a follow-up report.
- Cancellation reports and the out-of-range state may not be handled.
- Some announcements may be missed, so the latest state may not always be kept.

### Feature table

SNP: SignalNowProfessional  
DM (forecast): DM-D.S.S Earthquake Early Warning (forecast)  
DM (warning): DM-D.S.S Earthquake Early Warning (warning)

|Feature \ Source|Kyoshin Monitor|SNP|DM (forecast)|DM (warning)|AXIS|
|-------------|---------|---|--------|-------|----|
|**Price**|Free|Paid|Paid|Paid|Free|
|**Delivery speed**|Slow|Fast|Fast|Fast|Fast|
|**Cancellation report**|Not supported*|Supported|Supported|Supported|Supported|
|**Deep earthquakes**|Not supported|Supported|Supported|Supported|Supported|
|**Forecast**|Supported|Supported|Supported|Not supported|Supported|
|**Max intensity / epicenter name**|Supported|Not supported|Supported|Supported|Supported|
|**Accuracy information**|Not supported|Supported|Supported|Supported|Not supported|
|**Warning areas**|Not supported|Supported|Supported|Supported|Not supported|
|**Max intensity per area**|Not supported|Not supported|Supported|Not supported|Supported|

* Kyoshin Monitor has a process that treats it as a cancellation in a simulated way.
