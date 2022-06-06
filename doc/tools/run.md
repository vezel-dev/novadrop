# novadrop-run

```text
novadrop-run <command> <arguments...> [options...]
```

The novadrop-run tool allows running TERA's launcher (`Tl.exe`) and client
(`TERA.exe`) from the command line, given a valid account name and session
ticket for the server(s) being connected to. A session ticket is usually
obtained from some sort of Web-based authentication service.

Notes:

* There is no support for running novadrop-run on platforms other than Windows.
* Only 64-bit versions of the launcher and client are supported.

## novadrop-run client

```text
novadrop-run client <executable> <language> <account-name> <session-ticket> <server-host> <server-port>
```

Runs the game client (`TERA.exe`) directly without going through `Tl.exe`. When
running the client in this way, only a single arbiter server can be connected
to as no server list is available.

The `executable` argument specifies the path to `TERA.exe`. The `language`
argument specifies the data center language to use (`EUR`, `FRA`, `GER`, etc).
The `account-name` and `session-ticket` arguments specify the credentials as
obtained from the relevant authentication service. The `server-host` and
`server-port` arguments specify the arbiter server to connect to.

| Option | Description |
| - | - |
| `--patch` | Enable memory patching to remove Themida integrity checks and telemetry. |

## novadrop-run launcher

```text
novadrop-run launcher <executable> <account-name> <session-ticket> <url> [options...]
```

Runs the game launcher (`Tl.exe`).

The `executable` argument specifies the path to `Tl.exe`. The `account-name` and
`session-ticket` arguments specify the credentials as obtained from the relevant
authentication service. The `url` argument specifies the URL from which the
server list should be retrieved.

| Option | Description |
| - | - |
| `--server-id <id>` | Specifies a preferred server ID to connect to, if available. |
