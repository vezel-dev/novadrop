# novadrop-dc

```text
novadrop-dc <command> <arguments...> [options...]
```

The novadrop-dc tool allows manipulation of TERA's data center files. It
supports the following tasks:

* Extraction of data center files to [XML](https://www.w3.org/TR/xml) data
  sheets with corresponding [XSD](https://www.w3.org/TR/xmlschema-1) schemas.
* Validation of data sheets against schemas, preventing many mistakes when
  authoring data sheets.
* Packing of data sheets in accordance with type and key information in schemas
  to a fresh data center file usable by the client.
* Format integrity verification of data center files, optionally with strict
  compliance checks.
* Incremental packing of data sheets to a data center file by way of a file
  system watcher, for a faster development loop.

In general, novadrop-dc is quite fast: It exploits as many cores as are
available for parallel extraction, validation, and packing. On a modern system,
unpacking an official data center file takes around 20 seconds, while packing
the resulting data sheets takes around 1 minute.

## novadrop-dc pack

```text
novadrop-dc pack <input> <output> [--compression <level>]
```

Packs the data sheets in a directory to a data center file. Uses a medium level
of compression by default. If validation of the data sheets fails during
packing, a list of problems will be printed and the exit code will be non-zero.

The `input` argument specifies a directory containing data sheets and schemas.
The `output` argument specifies the path of the resulting data center file.

The `compression` option can be used to override the default compression level.

## novadrop-dc repack

```text
novadrop-dc repack <input> <output> [--strict] [--compression <level>]
```

Repacks a data center file without unpacking to disk. Uses a medium level of
compression by default. This command is primarily useful for development of
novadrop-dc.

The `input` argument specifies the input data center file. The `output` argument
specifies the path of the resulting data center file.

The `compression` option can be used to override the default compression level.
The `strict` option enables strict format compliance checks while reading the
input data center file.

## novadrop-dc unpack

```text
novadrop-dc unpack <input> <output> [--strict]
```

Unpacks the data tree in a data center file to a series of data sheets and
schemas in a directory. This command is primarily intended for unpacking
official data center files into a form that is easily maintainable by humans.

The `input` argument specifies the input data center file. The `output` argument
specifies the path of the directory to extract data sheets and schemas to.

The `strict` option enables strict format compliance checks while reading the
input data center file.

## novadrop-dc validate

```text
novadrop-dc validate <input>
```

Validates the data sheets in a directory against their schemas. If validation
of the data sheets fails, a list of problems will be printed and the exit code
will be non-zero.

The `input` argument specifies a directory containing data sheets and schemas.

## novadrop-dc verify

```text
novadrop-dc validate <input> [--strict]
```

Verifies the format integrity of a data center file. This means traversing the
entire data tree, reading all nodes and attributes, and ensuring that no errors
occur.

The `input` argument specifies the input data center file.

The `strict` option enables strict format compliance checks while reading the
input data center file.

## novadrop-dc watch

```text
novadrop-dc watch <input> <output> [--compression <level>]
```

Incrementally packs the data sheets in a directory to a data center file. The
data tree will be kept in memory and continually updated according to file
system changes detected in the given directory, thus reducing the time required
to pack the data center file each time. If validation of changed data sheets
fails, no packing will be attempted until the problems are addressed. Uses a low
level of compression by default, for the sake of speed.

The `input` argument specifies a directory containing data sheets and schemas.
The `output` argument specifies the path of the resulting data center file.

The `compression` option can be used to override the default compression level.
