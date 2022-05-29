# Launcher/Client Protocol

This page describes the communication protocol employed by `launcher.exe`,
`Tl.exe`, and `TERA.exe`.

* `launcher.exe`: The publisher-specific game launcher which performs
  authentication and server list URL resolution. Serves requests from `Tl.exe`
  and receives game events.
* `Tl.exe`: The publisher-agnostic game launcher which requests authentication
  data and the server list URL from `launcher.exe`. Serves requests and forwards
  game events from `TERA.exe`.
* `TERA.exe`: The actual game client. Sends requests and game events to
  `Tl.exe`.

These programs all communicate via
[window messages](https://docs.microsoft.com/en-us/windows/win32/winmsg/windowing),
specifically
[`WM_COPYDATA`](https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-copydata).
The `dwData` field specifies the message ID, while `lpData` and `cbData` contain
the payload pointer and length (if any).

## `Tl.exe` -> `launcher.exe` Messages

Messages sent between `Tl.exe` and `launcher.exe` consist of text encoded as
UTF-8. A NUL terminator is present in most messages, but not all. Responses from
`launcher.exe` should always use the same message ID that `Tl.exe` used in the
corresponding request message. Notably, only the Hello Handshake and Game Event
Notification messages have a static ID; the request messages use a message
counter as the message ID, so the contents must be parsed to understand them.

### Hello Handshake

The protocol starts off with a handshake sent from `Tl.exe`. This message has
the ID `0x0dbadb0a` and contains the NUL-terminated string `Hello!!`.

`launcher.exe` should respond with a NUL-terminated `Hello!!` or `Steam!!` to
indicate the method of authentication in use. The former uses classic
authentication while the latter uses [Steam](https://store.steampowered.com).
(Steam authentication will not be documented here.)

### Game Event Notification

`Tl.exe` will occasionally notify `launcher.exe` of various game events sent by
`TERA.exe`. These message have the message ID `0x0`. The format of these
messages can be described with the following
[regular expressions](https://en.wikipedia.org/wiki/Regular_expression):

* `gameEvent\((\d+)\)`: Indicates some kind of notable action taken by the user
  in the game. The value in parentheses is a code specifying the type of event.
* `endPopup\((\d+)\)`: Indicates that the client has exited. The value in
  parentheses is an exit reason code (not the same as the process exit code).

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
            <xsd:element name="id" type="xsd:unsignedInt" />
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
            <xsd:pattern value="0x[0-9A-Fa-f]{8}"/>
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:simpleType name="serverlist_server_server_stat">
        <xsd:restriction base="xsd:string">
            <xsd:pattern value="0x[0-9A-Fa-f]{8}"/>
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:element name="serverlist" type="serverlist" />
</xsd:schema>
```

### Authentication Info Request

TODO: `gamestr`, `ticket`, `last_svr`, `char_cnt`

#### Authentication Info Schema

The authentication JSON response can be described with the following
[JSON Schema](https://json-schema.org) definition:

```json
TODO
```

### Web Link URL Request

TODO: `getWebLinkUrl\((\d+),(.*)\)`

## `TERA.exe` -> `Tl.exe` Messages

TODO
