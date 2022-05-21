# novadrop-rsc

```text
novadrop-rsc <command> <arguments...> [options...]
```

The novadrop-rsc tool allows manipulation of TERA's resource container files. It
supports the following tasks:

* Extraction of files contained within resource container files.
* Packing of files to a fresh resource container file usable by the client.
* Format integrity verification of resource container files, optionally with
  strict compliance checks.

## novadrop-rsc pack

```text
novadrop-rsc pack <input> <output> [options...]
```

Packs the files in a directory to a resource container file.

The `input` argument specifies a directory containing files. The `output`
argument specifies the path of the resulting resource container file.

| Option | Description |
| - | - |
| `--encryption-key <key>` | Specifies an encryption key (defaults to the latest known key). |

## novadrop-rsc repack

```text
novadrop-rsc repack <input> <output> [options...]
```

Repacks a resource container file without unpacking to disk. This command is
primarily useful for development of novadrop-rsc.

The `input` argument specifies the input resource container file. The `output`
argument specifies the path of the resulting resource container file.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--encryption-key <key>` | Specifies an encryption key (defaults to the latest known key). |
| `--strict` | Enables strict format compliance checks while reading the input file. |

## novadrop-rsc unpack

```text
novadrop-rsc unpack <input> <output> [options...]
```

Unpacks the files in a resource container file to a directory.

The `input` argument specifies the input resource container file. The `output`
argument specifies the path of the directory to extract files to.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--strict` | Enables strict format compliance checks while reading the input file. |

## novadrop-rsc verify

```text
novadrop-rsc validate <input> [options...]
```

Verifies the format integrity of a resource container file. This means loading
all contained files into memory and verifying that no errors occur.

The `input` argument specifies the input resource container file.

| Option | Description |
| - | - |
| `--decryption-key <key>` | Specifies a decryption key (defaults to the latest known key). |
| `--strict` | Enables strict format compliance checks while reading the input file. |
