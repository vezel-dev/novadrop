# Network Protocol

This page describes TERA's network protocol, which is built on top of TCP. It is
a structured protocol that can be serialized and deserialized automatically
based on uniform packet definitions; no special serialization logic is required
for any fields.

* C/C++-like primitive types and `struct`s will be used.
* `bool` is equivalent to `uint8_t` but only allows the values `true` (`1`) and
  `false` (`0`).
* Integers (`uint8_t`, `int8_t`, `uint16_t`, `int16_t`, etc) are little endian.
* `offset_t` values are `uint16_t` indexes into a packet, including the header.
* `float` and `double` are IEEE 754 `binary32` and `binary64`, respectively.
* Characters (i.e. `char16_t`) are UTF-16 and little endian.
* Strings (i.e. `u16string`) are a series of valid `char16_t` characters
  followed by a NUL character.
* Fields are laid out in the declared order with no implied padding anywhere.

## Encryption

TODO: Describe the (insecure) key exchange and the PIKE algorithm.

## Packet Header

A packet starts with a simple header:

```cpp
struct PacketHeader
{
    uint16_t length;
    uint16_t code;
};
```

`length` specifies the full length of the packet, including the header. Thus,
the maximum length of a packet is `65535` bytes (or `65531` bytes for the
payload). For certain large packets (e.g. achievements and inventory), the game
works around this by sending 'continuation' packets after the first packet.

`code` is the operation code. This tells the client or server what the structure
of the payload is.

## Packet Body

The packet body consists of a series of fields. Some examples:

```cpp
struct CJoinPrivateChannelPacket
{
    PacketHeader header;
    u16string channel_name;
    uint16_t password;
};

struct CEditPrivateChannelPacket
{
    struct
    {
        uint32_t player_id;
    } members[];
    u16string channel_name;
    uint16_t password;
};
```

Fields are written in the order that they are declared. However, complex fields
(strings, object arrays, and byte arrays) are written as `offset_t` values that
point to the actual data elsewhere in the payload. Primitive types such as
integers, floating point numbers, and Boolean values are written in the obvious
way as they appear.

## Complex Types

Complex types are written after all primitive types in the current 'object' (be
that the root packet body, or an object nested arbitrarily within arrays). At
the place where the complex field appears, an `offset_t` value is written
pointing to where the actual data for the field is written in the payload. For
object arrays and byte arrays, this value is accompanied by a `uint16_t` value
representing the number of elements in the array.

When there are multiple complex fields in an object, they are written at the end
of the object in the order that they appear in the structure. For example, in
`CEditPrivateChannelPacket`, the contents of the `members` array are written
after `password`, and the `channel_name` string contents after that.

### Strings

String pointers are represented as follows:

```cpp
struct PacketStringPointer
{
    offset_t start;
};
```

When writing the string contents, the characters are written contiguously,
followed by a NUL character.

### Object Arrays

Object array pointers are represented as follows:

```cpp
struct PacketObjectArrayPointer
{
    uint16_t count;
    offset_t start;
};
```

Each element within the array has an object pointer that links to the next
element:

```cpp
struct PacketObjectPointer
{
    offset_t here;
    offset_t next;
};
```

`start` points to a `PacketObjectPointer`, which is immediately followed by the
contents of the first element. The `next` pointer points to another
`PacketObjectPointer`, which is immediately followed by the contents of the
second element, and so on. This continues `count` times. The `next` value for
the last element is `0`. In each element, `here` is just a pointer to itself;
it is not clear what purpose it serves.

Due to the way that arrays are serialized, in theory, it would be possible to
spread the elements all over the place in a payload, in whatever way would make
the most sense for compactness and locality. In practice, the client and
official servers almost always do the straightforward thing, with only a few
curious (and seemingly nonsensical) exceptions. Regardless, neither party
actually cares how arrays are laid out for the purposes of serialization.

### Byte Arrays

Byte array pointers are represented as follows:

```cpp
struct PacketByteArrayPointer
{
    offset_t start;
    uint16_t count;
};
```

Note that `count` comes after `start`, unlike object arrays.

Byte arrays are serialized in a more compact fashion than object arrays: `start`
points to an area in the payload containing `count` bytes, making up the
contents of the byte array. There are no pointer values to follow for each
individual element.
