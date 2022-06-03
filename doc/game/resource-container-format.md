# Resource Container Format

This page describes the encrypted resource container format used by TERA.

* C/C++-like primitive types and `struct`s will be used.
* Integers (`uint8_t`, `int8_t`, `uint16_t`, `int16_t`, etc) are little endian.
* Characters (i.e. `char16_t`) are UTF-16 and little endian.
* Strings (i.e. `u16string`) are a series of valid `char16_t` characters
  followed by a NUL character.
* Fields are laid out in the declared order with no implied padding anywhere.

## Encryption

Portions of resource container files are encrypted with a 256-bit
[XOR cipher](https://en.wikipedia.org/wiki/XOR_cipher).

The encryption key can be extracted from a running TERA client, e.g. with the
[novadrop-scan](../tools/scan.md) tool. The key appears to be static; it does
not change with each client build.

## Physical Structure

The overall structure can be described like this:

```cpp
struct ResourceContainerFile
{
    uint8_t data[];
    ResourceContainerDirectory directory;
    ResourceContainerFooter footer;
};
```

Reading a resource container file requires knowing the full size of the file in
advance. A reader must first seek to the footer, read it, then seek to the
directory, read and decrypt it, and finally seek to the data where each file
entry can be read and decrypted according to the offsets in the directory.

### File Footer

A resource container file ends with this footer:

```cpp
struct ResourceContainerFooter
{
    uint32_t directory_size;
    uint32_t magic;
};
```

`directory_size` is the size of the directory. This enables a reader to know how
far back it must seek in order to read the directory correctly.

`magic` must be `0x01001fff`.

### Directory

Before the footer, there is a directory listing all contained files:

```cpp
struct ResourceContainerDirectory
{
    ResourceContainerEntry entries[];
};
```

There is no way to know the number of entries in advance; a reader must keep
reading entries until it has read the amount of bytes indicated in the footer's
`directory_size` field.

Note that the entire directory is encrypted with the XOR cipher.

#### Entries

Each entry in the directory is of the form:

```cpp
struct ResourceContainerEntry
{
    uint32_t size;
    uint32_t offset;
    u16string name;
};
```

`size` is the size of the contained file.

`offset` indicates where in the `data` section the file's contents are located.
`size` bytes must be readable at this location. Note that the contents are
encrypted with the XOR cipher.

`name` is the name of the file on disk.
