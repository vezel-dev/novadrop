# Launcher/Client Protocol

This page describes the communication protocol employed by `launcher.exe`,
`Tl.exe`, and `TERA.exe`.

* C/C++-like primitive types and `struct`s will be used.
* Integers (`uint8_t`, `int8_t`, `uint16_t`, `int16_t`, etc) are little endian.
* Characters (i.e. `char8_t` and `char16_t`) are UTF-8 and UTF-16 respectively,
  and little endian.
* Strings (i.e. `u8string` and `u16string`) are a series of valid `char8_t` and
  `char16_t` characters respectively, followed by a NUL character.
* Fields are laid out in the declared order with no implied padding anywhere.

Note that strings are not always NUL-terminated. For this reason, the document
will explicitly call out whether a NUL terminator is present.

## Communication

The role of each program is as follows:

* `launcher.exe`: The publisher-specific game launcher which performs
  authentication and server list URL resolution. Serves requests from `Tl.exe`
  and receives game events.
* `Tl.exe`: The (mostly) publisher-agnostic game launcher which requests
  authentication data and the server list URL from `launcher.exe`. Serves
  requests and forwards game events from `TERA.exe`.
* `TERA.exe`: The actual game client. Sends requests and game events to
  `Tl.exe`.

These programs all communicate via
[window messages](https://docs.microsoft.com/en-us/windows/win32/winmsg/windowing),
specifically
[`WM_COPYDATA`](https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-copydata).
The `dwData` field specifies the message ID, while `lpData` and `cbData` contain
the payload pointer and length.

## `Tl.exe` -> `launcher.exe` Messages

Messages sent between `Tl.exe` and `launcher.exe` consist of text encoded as
UTF-8. A NUL terminator is present in most messages, but not all. Responses from
`launcher.exe` should always use the same message ID that `Tl.exe` used in the
corresponding request message. Notably, only the Hello Handshake and Game Event
Notification messages have a static ID; the request messages use a message
counter as the message ID, so the contents must be parsed to understand them.

### Hello Handshake (`0x0dbadb0a`)

The protocol starts off with a handshake sent from `Tl.exe`. This message
contains the NUL-terminated string `Hello!!`.

`launcher.exe` should respond with a NUL-terminated `Hello!!` or `Steam!!` to
indicate the method of authentication in use. The former uses classic
authentication while the latter uses [Steam](https://store.steampowered.com).
(Steam authentication will not be documented here.)

### Game Event Notification (`0x0`)

`Tl.exe` will occasionally notify `launcher.exe` of various game events sent by
`TERA.exe`. The format of these messages can be described with the following
[regular expressions](https://en.wikipedia.org/wiki/Regular_expression):

* `^csPopup\(\)$`: Signals that `launcher.exe` should open the customer support
  website (e.g. in the default Web browser).
* `^gameEvent\((\d+)\)$`: Indicates some kind of notable action taken by the
  user in the game. The value in parentheses is a code specifying the type of
  event; the possible values are documented in the section on `TERA.exe`
  messages.
* `^endPopup\((\d+)\)$`: Indicates that the client has exited. The value in
  parentheses is an exit reason code (not the same as the process exit code);
  the possible values are documented in the section on `TERA.exe` messages.
* `^promoPopup\(\d+\)$`: The exact purpose of this notification is currently
  unknown.

`launcher.exe` should *not* respond to these messages.

### Server List URL Request

`Tl.exe` will request the server list URL from `launcher.exe`. This message does
not have a static message ID. The message contains the NUL-terminated string
`slsurl`.

`launcher.exe` should respond with the NUL-terminated server list URL.

The Web server at the URL should be prepared to receive an
[HTTP GET](https://datatracker.ietf.org/doc/html/rfc2616#section-9.3) request,
potentially with a
[query string](https://url.spec.whatwg.org/#url-query-string) (which can be
ignored). The response should use the
[`application/xml` media type](https://datatracker.ietf.org/doc/html/rfc2376#section-3.2).
Note that `Tl.exe` does *not* support
[chunked transfer encoding](https://datatracker.ietf.org/doc/html/rfc2616#section-3.6.1).

#### Server List Schema

The response received from the server list URL should be in
[XML](https://www.w3.org/TR/xml) format. It can be described with the following
[XSD](https://www.w3.org/TR/xmlschema-1) schema:

```xml
<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema"
            xmlns="https://vezel.dev/novadrop/tl/ServerList"
            targetNamespace="https://vezel.dev/novadrop/tl/ServerList"
            elementFormDefault="qualified">
    <xsd:complexType name="serverlist">
        <xsd:sequence>
            <xsd:element name="server" type="serverlist_server" maxOccurs="unbounded" />
        </xsd:sequence>
    </xsd:complexType>

    <xsd:complexType name="serverlist_server">
        <xsd:sequence>
            <xsd:element name="id" type="serverlist_server_id" />
            <xsd:element name="ip" type="serverlist_server_ip" />
            <xsd:element name="port" type="unsignedShort" />
            <xsd:element name="category" type="serverlist_server_category" />
            <xsd:element name="name" type="serverlist_server_name" />
            <xsd:element name="crowdness" type="serverlist_server_crowdness" />
            <xsd:element name="open" type="serverlist_server_open" />
            <xsd:element name="permission_mask" type="serverlist_server_permission_mask" />
            <xsd:element name="server_stat" type="serverlist_server_server_stat" />
            <xsd:element name="popup" type="xsd:string" />
            <xsd:element name="language" type="xsd:string" />
        </xsd:sequence>
    </xsd:complexType>

    <xsd:simpleType name="serverlist_server_id">
        <xsd:restriction base="xsd:unsignedInt">
            <xsd:minInclusive value="1" />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:simpleType name="serverlist_server_ip">
        <xsd:restriction base="xsd:string">
            <xsd:pattern value="\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}" />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:complexType name="serverlist_server_category">
        <xsd:simpleContent>
            <xsd:extension base="xsd:string">
                <xsd:attribute name="sort" type="xsd:unsignedInt" />
            </xsd:extension>
        </xsd:simpleContent>
    </xsd:complexType>

    <xsd:complexType name="serverlist_server_name">
        <xsd:simpleContent>
            <xsd:extension base="xsd:string">
                <xsd:attribute name="raw_name" type="xsd:string" />
            </xsd:extension>
        </xsd:simpleContent>
    </xsd:complexType>

    <xsd:complexType name="serverlist_server_crowdness">
        <xsd:simpleContent>
            <xsd:extension base="xsd:string">
                <xsd:attribute name="sort" type="xsd:unsignedInt" />
            </xsd:extension>
        </xsd:simpleContent>
    </xsd:complexType>

    <xsd:complexType name="serverlist_server_open">
        <xsd:simpleContent>
            <xsd:extension base="xsd:string">
                <xsd:attribute name="sort" type="xsd:unsignedInt" />
            </xsd:extension>
        </xsd:simpleContent>
    </xsd:complexType>

    <xsd:simpleType name="serverlist_server_permission_mask">
        <xsd:restriction base="xsd:string">
            <xsd:pattern value="0x[0-9A-Fa-f]{8}" />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:simpleType name="serverlist_server_server_stat">
        <xsd:restriction base="xsd:string">
            <xsd:pattern value="0x[0-9A-Fa-f]{8}" />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:element name="serverlist" type="serverlist" />
</xsd:schema>
```

### Authentication Info Request

`Tl.exe` will request [JSON](https://www.json.org)-encoded authentication data
from `launcher.exe`. This message does not have a static message ID. The message
contains one of several NUL-terminated strings.

`gamestr` is only sent when the game is initially launched. This request asks
for the full authentication data.

`ticket`, `last_svr`, and `char_cnt` are only sent when returning to the server
list from an arbiter server.

* `ticket`: Requests the authentication ticket. This can be the same ticket as
  before, but `launcher.exe` may also choose to authenticate anew and retrieve a
  fresh ticket.
* `last_svr`: Requests the ID of the last server the the account connected to.
* `char_cnt`: Requests character counts for each server in the server list.

`launcher.exe` should respond with the NUL-terminated JSON payload.

#### Authentication Info Schema

The authentication JSON response can be described with the following
[JSON Schema](https://json-schema.org) definition:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://vezel.dev/novadrop/tl/authentication.schema.json",
  "title": "AuthenticationInfo",
  "type": "object",
  "properties": {
    "result-message": {
      "type": "string"
    },
    "result-code": {
      "type": "integer",
      "minimum": 100,
      "maximum": 599
    },
    "account_bits": {
      "type": "string",
      "pattern": "^0x[0-9A-Fa-f]{10}$"
    },
    "ticket": {
      "type": "string"
    },
    "last_connected_server_id": {
      "type": "integer",
      "minimum": 0
    },
    "access_level": {
      "type": "integer",
      "minimum": 0
    },
    "master_account_name": {
      "type": "string"
    },
    "chars_per_server": {
      "anyOf": [
        {
          "type": "object",
          "properties": {},
          "additionalProperties": false
        },
        {
          "$ref": "#/$defs/chars_on_server"
        },
        {
          "type": "array",
          "items": {
            "$ref": "#/$defs/chars_on_server"
          }
        }
      ]
    },
    "user_permission": {
      "type": "integer",
      "minimum": 0
    },
    "game_account_name": {
      "type": "string"
    }
  },
  "required": [
    "result-code",
    "master_account_name",
    "game_account_name"
  ],
  "$defs": {
    "chars_on_server": {
      "type": "object",
      "properties": {
        "id": {
          "type": "string",
          "pattern": "^[1-9]\\d*$"
        },
        "char_count": {
          "type": "string",
          "pattern": "^\\d+$"
        }
      },
      "additionalProperties": false,
      "required": [
        "id",
        "char_count"
      ]
    }
  }
}
```

Some properties are only required depending on the message:

* `gamestr`: All properties are required.
* `ticket`: The `ticket` property is required.
* `last_svr`: The `last_connected_server_id` property is required.
* `char_cnt`: The `chars_per_server` property is required.

`result-message`, `account_bits`, `access_level`, and `user_permission` are
completely optional.

### Web Link URL Request

`Tl.exe` will request a URL to be opened in the client's embedded Web browser.
This message does not have a static message ID and is *not* NUL-terminated.

The message can be described by the regular expression
`^getWebLinkUrl\((\d+),(.*)\)$`. The first group is the ID of a `UIWindow` node
under the `CoherentGTWeb` data center sheet, and the second group is a set of
arguments specific to that link.

`launcher.exe` should respond with a URL to be opened (*without* a NUL
terminator), or an empty payload to reject the request.

## `TERA.exe` -> `Tl.exe` Messages

Messages sent between `TERA.exe` and `Tl.exe` use a simple binary protocol. All
messages have static message IDs; responses have different IDs from requests.

### Account Name Request (`0x1`)

`TERA.exe` will request the (game) account name from `Tl.exe`.

```cpp
struct LauncherAccountNameRequest
{
};
```

#### Account Name Response (`0x2`)

`Tl.exe` should respond with the account name.

```cpp
struct LauncherAccountNameResponse
{
    u16string account_name;
};
```

`account_name` is the name of the game account. It is *not* NUL-terminated.

### Session Ticket Request (`0x3`)

`TERA.exe` will request the session ticket from `Tl.exe`.

```cpp
struct LauncherSessionTicketRequest
{
};
```

#### Session Ticket Response (`0x4`)

`Tl.exe` should respond with the session ticket.

```cpp
struct LauncherAccountNameResponse
{
    u8string session_ticket;
};
```

`session_ticket` is the authentication session ticket. It is *not*
NUL-terminated.

### Server List Request (`0x5`)

`TERA.exe` will request the server list from `Tl.exe`.

```cpp
struct LauncherServerListRequest
{
    LauncherServerListSortCriteria sort_criterion;
};
```

`sorting` specifies how `Tl.exe` should sort the server list. Valid values are
as follows:

```cpp
enum LauncherServerListSortCriteria : int32_t
{
    LAUNCHER_SERVER_LIST_SORT_CRITERIA_NONE = -1,
    LAUNCHER_SERVER_LIST_SORT_CRITERIA_CHARACTERS = 0,
    LAUNCHER_SERVER_LIST_SORT_CRITERIA_CATEGORY = 1,
    LAUNCHER_SERVER_LIST_SORT_CRITERIA_NAME = 2,
    LAUNCHER_SERVER_LIST_SORT_CRITERIA_POPULATION = 4,
};
```

A few notes on sorting:

* The sort should be stable.
* `LAUNCHER_SERVER_LIST_SORT_CRITERIA_NONE` indicates that `Tl.exe` is free to
  sort the list arbitrarily.
* The resultant list should be maintained between requests and further sorted
  on each request, unless `LAUNCHER_SERVER_LIST_SORTING_NONE` is sent to reset
  the sorting.
* If the same sorting is requested multiple times and is any sorting other than
  `LAUNCHER_SERVER_LIST_SORT_CRITERIA_NONE`, the list should simply be reversed
  without applying sorting.

#### Server List Response (`0x6`)

`Tl.exe` should respond with the server list encoded with
[Protocol Buffers](https://developers.google.com/protocol-buffers).

```cpp
struct LauncherServerListResponse
{
    uint8_t data[];
};
```

##### Server List Structures

The server list response can be described with the following message
definitions:

```protobuf
syntax = "proto2";

message ServerList
{
    message ServerInfo
    {
        required fixed32 id = 1;
        required bytes name = 2;
        required bytes category = 3;
        required bytes title = 4;
        required bytes queue = 5;
        required bytes population = 6;
        required fixed32 address = 7;
        required fixed32 port = 8;
        required fixed32 available = 9;
        required bytes unavailable_message = 10;
        optional bytes host = 11;
    }

    repeated ServerInfo servers = 1;
    required fixed32 last_server_id = 2;
    required fixed32 sort_criterion = 3;
}
```

A few notes on these definitions:

* They will not work correctly as `proto3` due to `required` semantics.
* `bytes` fields are really `u16string` *without* NUL terminators.
* `id` must be a positive (non-zero) value.
* `port` should be in the `uint16_t` range.
* `available` is really a `bool`, so only the values `0` and `1` are allowed.
* Either `address` or `host` must be set; not neither and not both.
    * For `address`, a value of `0` has 'not set' semantics since the field is
      not marked `optional`.
* `sort_criterion` should be the `LauncherServerListSortCriteria` value that was
  sent in the request.

### Enter Lobby/World Notification (`0x7`)

`TERA.exe` will notify `Tl.exe` when entering the lobby (i.e. successfully
connecting to an arbiter server) or when entering the world on a particular
character.

```cpp
struct LauncherEnterLobbyNotification
{
};

struct LauncherEnterWorldNotification
{
    u16string character_name;
};
```

The two cases can be distinguished by looking at the payload length.

`character_name` is the NUL-terminated name of the character that the user is
entering the world on.

### Voice Chat Requests

There is a set of requests for interacting with
[TeamSpeak](https://www.teamspeak.com):

* Create Room (`0x8`)
* Join Room (`0xa`)
* Leave Room (`0xc`)
* Set Volume (`0x13`)
* Set Microphone (`0x14`)
* Silence User (`0x15`)

These messages were only present in some regions and were likely never actually
used. It is currently unknown what their payloads contain.

#### Voice Chat Responses

The following responses exist for the above TeamSpeak requests:

* Create Room Result (`0x9`)
* Join Room Result (`0xb`)
* Leave Room Result (`0xd`)

### Open Website Command (`0x19`)

`TERA.exe` asks `Tl.exe` to open a website in the default Web browser.

```cpp
struct LauncherOpenWebsiteCommand
{
    uint32_t id;
};
```

`id` specifies the kind of website that should be opened. The possible values
are currently unknown.

### Web Link URL Request (`0x1a`)

This request is the `TERA.exe` equivalent of the request sent by `Tl.exe` to
`launcher.exe`.

```cpp
struct LauncherWebLinkURLRequest
{
    uint32_t id;
    u16string arguments;
};
```

`id` refers to a `UIWindow` node under the `CoherentGTWeb` data center sheet.

`arguments` specifies the NUL-terminated arguments specific to the link.

#### Web Link URL Response (`0x1b`)

```cpp
struct LauncherWebLinkURLResponse
{
    uint32_t id;
    u16string url;
};
```

`id` is the same value that was sent in the request.

`url` is the NUL-terminated URL to open. It can be the special string `|` to
indicate that no URL should be opened.

### Game Start Notification (`0x3e8`)

`TERA.exe` will notify `Tl.exe` that it has launched.

```cpp
struct LauncherGameStartNotification
{
    uint32_t source_revision;
    uint32_t unknown_1;
    u16string windows_account_name;
};
```

`source_revision` is the `SrcRegVer` value from the client's
`ReleaseRevision.txt` file.

The meaning of `unknown_1` is currently unknown.

`windows_account_name` is the NUL-terminated name of the current Windows user
account.

### Game Event Notification (`0x3e9` - `0x3f8`)

`TERA.exe` will occasionally notify `Tl.exe` of various notable actions taken by
the user.

```cpp
struct LauncherGameEventNotification
{
    LauncherGameEvent event;
};
```

`event` specifies the kind of event that occurred. Valid values are as follows:

```cpp
enum LauncherGameEvent : uint32_t
{
    LAUNCHER_GAME_EVENT_ENTERED_INTO_CINEMATIC = 1001,
    LAUNCHER_GAME_EVENT_ENTERED_SERVER_LIST = 1002,
    LAUNCHER_GAME_EVENT_ENTERING_LOBBY = 1003,
    LAUNCHER_GAME_EVENT_ENTERED_LOBBY = 1004,
    LAUNCHER_GAME_EVENT_ENTERING_CHARACTER_CREATION = 1005,
    LAUNCHER_GAME_EVENT_LEFT_LOBBY = 1006,
    LAUNCHER_GAME_EVENT_DELETED_CHARACTER = 1007,
    LAUNCHER_GAME_EVENT_CANCELED_CHARACTER_CREATION = 1008,
    LAUNCHER_GAME_EVENT_ENTERED_CHARACTER_CREATION = 1009,
    LAUNCHER_GAME_EVENT_CREATED_CHARACTER = 1010,
    LAUNCHER_GAME_EVENT_ENTERED_WORLD = 1011,
    LAUNCHER_GAME_EVENT_FINISHED_LOADING_SCREEN = 1012,
    LAUNCHER_GAME_EVENT_LEFT_WORLD = 1013,
    LAUNCHER_GAME_EVENT_MOUNTED_PEGASUS = 1014,
    LAUNCHER_GAME_EVENT_DISMOUNTED_PEGASUS = 1015,
    LAUNCHER_GAME_EVENT_CHANGED_CHANNEL = 1016,
};
```

### Game Exit Notification (`0x3fc`)

`TERA.exe` will notify `Tl.exe` that it is exiting.

```cpp
struct LauncherGameExitNotification
{
    uint32_t length;
    uint32_t code;
    LauncherGameExitReason reason;
};
```

`length` is the length of the payload. It must be `12`.

`code` is the exit code of the `TERA.exe` process. This can be `0` or `1`.

`reason` provides a more specific exit reason. Some known values are as follows:

```cpp
enum LauncherGameExitReason : uint32_t
{
    LAUNCHER_GAME_EXIT_REASON_SUCCESS = 0x0,
    LAUNCHER_GAME_EXIT_REASON_INVALID_DATA_CENTER = 0x6,
    LAUNCHER_GAME_EXIT_REASON_CONNECTION_DROPPED = 0x8,
    LAUNCHER_GAME_EXIT_REASON_INVALID_AUTHENTICATION_INFO = 0x9,
    LAUNCHER_GAME_EXIT_REASON_OUT_OF_MEMORY = 0xa,
    LAUNCHER_GAME_EXIT_REASON_SHADER_MODEL_3_UNAVAILABLE = 0xc,
    LAUNCHER_GAME_EXIT_REASON_SPEED_HACK_DETECTED = 0x10,
    LAUNCHER_GAME_EXIT_REASON_UNSUPPORTED_VERSION = 0x13,
    LAUNCHER_GAME_EXIT_REASON_ALREADY_ONLINE = 0x106,
};
```

(The above enumeration is an incomplete list.)

### Game Crash Notification (`0x3fd`)

`TERA.exe` will notify `Tl.exe` that it has crashed (e.g. because of a memory
access violation). Note that, since a crash could mean things are arbitrarily
broken, this message may not be produced if the crash is sufficiently severe.

```cpp
struct LauncherGameCrashNotification
{
    u16string details;
};
```

`details` contains various details about the crash such as the instruction
pointer and exception type. It is *not* NUL-terminated.

### Anti-Cheat Event Notifications (`0x3fe` - `0x400`)

`TERA.exe` will notify `Tl.exe` of various events relating to the anti-cheat
module (e.g. [XIGNCODE3](https://www.wellbia.com) or
[GameGuard](https://gameguard.nprotect.com)). Note that only some regions have a
client build with an anti-cheat module.

#### Anti-Cheat Starting Notification

`TERA.exe` will notify `Tl.exe` that the anti-cheat module is starting.

```cpp
struct LauncherAntiCheatStartingNotification
{
};
```

#### Anti-Cheat Started Notification

`TERA.exe` will notify `Tl.exe` that the anti-cheat module has successfully
started.

```cpp
struct LauncherAntiCheatStartedNotification
{
};
```

#### Anti-Cheat Error Notification

`TERA.exe` will notify `Tl.exe` that the anti-cheat module failed to start.

```cpp
struct LauncherAntiCheatStartedNotification
{
    uint64_t error;
};
```

`error` contains the error code from the anti-cheat module.

### Unknown Command 1025 (`0x401`)

The exact purpose of this message is currently unknown.

### Unknown Command 1027 (`0x403`)

The exact purpose of this message is currently unknown.
