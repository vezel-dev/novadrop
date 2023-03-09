# novadrop-dc

```text
novadrop-dc <command> <arguments...> [options...]
```

The novadrop-dc tool allows manipulation of TERA's data center files. It
supports the following tasks:

* Extraction of data center files to [XML](https://www.w3.org/TR/xml) data
  sheets with corresponding [XSD](https://www.w3.org/TR/xmlschema-1) schemas.
* Reasonably accurate inference of XSD schemas from a packed data center file.
* Validation of data sheets against schemas, preventing many mistakes when
  authoring data sheets.
* Packing of data sheets in accordance with type and key information in schemas
  to a fresh data center file usable by the client.
* Format integrity verification of data center files, optionally with strict
  compliance checks.

In general, novadrop-dc is quite fast: It exploits as many cores as are
available for parallel extraction, validation, and packing. On a modern system,
unpacking an official data center file takes around 20 seconds, while packing
the resulting data sheets takes around 40 seconds.

## novadrop-dc pack

```text
novadrop-dc pack <input> <output> [options...]
```

Packs the data sheets in a directory to a data center file. If validation of the
data sheets fails during packing, a list of problems will be printed and the
exit code will be non-zero.

The `input` argument specifies a directory containing data sheets and schemas.
The `output` argument specifies the path of the resulting data center file.

| Option | Description |
| - | - |
| `--revision <value>` | Specifies the data tree revision number (defaults to latest known revision). |
| `--compression <level>` | Specifies a compression level (defaults to `Optimal`). |
| `--encryption-key <key>` | Specifies an encryption key (defaults to the latest known key). |
| `--encryption-iv <iv>` | Specifies an encryption IV (defaults to the latest known IV). |

## novadrop-dc repack

```text
novadrop-dc repack <input> <output> [options...]
```

Repacks a data center file without unpacking to disk. This command is primarily
useful for development of novadrop-dc.

The `input` argument specifies the input data center file. The `output` argument
specifies the path of the resulting data center file.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--decryption-iv <iv>` | Specifies a decryption IV (defaults to the latest known IV). |
| `--strict` | Enables strict format compliance checks while reading the input file. |
| `--revision <value>` | Specifies the data tree revision number (defaults to latest known revision). |
| `--compression <level>` | Specifies a compression level (defaults to `Optimal`). |
| `--encryption-key <key>` | Specifies an encryption key (defaults to the latest known key). |
| `--encryption-iv <iv>` | Specifies an encryption IV (defaults to the latest known IV). |

## novadrop-dc schema

```text
novadrop-dc schema <input> <output> [options...]
```

Performs best-effort inference of the schemas for the data tree in a data center
file. This may be useful when working with older data centers for which
Novadrop's included schemas are not correct.

Please note that, regardless of whether the `Conservative` or `Aggressive`
inference strategy is used, the generated schemas will require a human touch
after they have been generated. Among other things, this command cannot tell
whether a given path in the tree with repeated node names represents a truly
recursive structure.

The `input` argument specifies the input data center file. The `output` argument
specifies the path of the directory to write inferred schemas to.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--decryption-iv <iv>` | Specifies a decryption IV (defaults to the latest known IV). |
| `--strict` | Enables strict format compliance checks while reading the input file. |
| `--strategy <level>` | Specifies a schema inference strategy (defaults to `Conservative`). |
| `--subdirectories` | Enables using output subdirectories based on data sheet names. |

## novadrop-dc unpack

```text
novadrop-dc unpack <input> <output> [options...]
```

Unpacks the data tree in a data center file to a series of data sheets and
schemas in a directory. This command is primarily intended for unpacking
official data center files into a form that is easily maintainable by humans.

The `input` argument specifies the input data center file. The `output` argument
specifies the path of the directory to extract data sheets and schemas to.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--decryption-iv <iv>` | Specifies a decryption IV (defaults to the latest known IV). |
| `--strict` | Enables strict format compliance checks while reading the input file. |

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
novadrop-dc verify <input> [options...]
```

Verifies the format integrity of a data center file. This means traversing the
entire data tree, reading all nodes and attributes, and ensuring that no errors
occur.

The `input` argument specifies the input data center file.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--decryption-iv <iv>` | Specifies a decryption IV (defaults to the latest known IV). |
| `--strict` | Enables strict format compliance checks while reading the input file. |
