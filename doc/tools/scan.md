# novadrop-scan

```text
novadrop-scan <output> [pid]
```

The novadrop-scan tool allows extracting useful data from a running TERA client.
Currently, it can extract the following:

* The client versions reported in the `C_CHECK_VERSION` packet.
* The data center encryption key and initialization vector.
* The resource container encryption key.
* The system message table used for the `S_SYSTEM_MESSAGE` packet.

The `output` argument specifies a directory where result files will be written
by the scanners. The `pid` argument specifies the process ID of the target
process for the scan; if omitted, novadrop-scan will look for a process named
`TERA` and attach to that.

Notes:

* There is no support for running novadrop-scan on platforms other than Windows.
* Only 64-bit versions of the game client are supported.
