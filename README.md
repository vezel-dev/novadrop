# Novadrop

<p align="center">
    <strong>
        A developer toolkit for interacting with and modifying the TERA game
        client.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/vezel-dev/novadrop?color=brown)](LICENSE.md)
[![Commits](https://img.shields.io/github/commit-activity/m/vezel-dev/novadrop/master?label=commits&color=slateblue)](https://github.com/vezel-dev/novadrop/commits/master)
[![Build](https://img.shields.io/github/workflow/status/vezel-dev/novadrop/Build/master)](https://github.com/vezel-dev/novadrop/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/novadrop?color=teal)](https://github.com/vezel-dev/novadrop/discussions)
[![Discord](https://img.shields.io/discord/960716713136095232?color=peru&label=discord)](https://discord.gg/SdBCrRuNxY)

</div>

---

**Novadrop** provides a collection of libraries, tools, and documentation for
modifying the [TERA](https://en.wikipedia.org/wiki/TERA_(video_game)) game
client.

Development of TERA stopped on April 20, 2022. Official game servers shut down
on June 30 in the same year. Some of the knowledge and functionality provided by
**Novadrop** was previously held in relative secrecy by a small portion of the
game's third-party modding community for the sake of preserving game integrity.
With the game effectively defunct, **Novadrop** now enables the game's community
to freely analyze and modify the client, e.g. for the purposes of creating
unofficial server emulators.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![novadrop-dc][dc-img]][dc-pkg] | Provides the .NET global tool for manipulating TERA's data center files. | ![Downloads][dc-dls] |
| [![novadrop-gpk][gpk-img]][gpk-pkg] | Provides the .NET global tool for manipulating TERA's GPK archive files. | ![Downloads][gpk-dls] |
| [![novadrop-rsc][rsc-img]][rsc-pkg] | Provides the .NET global tool for manipulating TERA's resource container files. | ![Downloads][rsc-dls] |
| [![novadrop-run][run-img]][run-pkg] | Provides the .NET global tool for launching the TERA client manually from the command line. | ![Downloads][run-dls] |
| [![novadrop-scan][scan-img]][scan-pkg] | Provides the .NET global tool for extracting useful data from a running TERA client. | ![Downloads][scan-dls] |
| [![Vezel.Novadrop.Data][data-img]][data-pkg] | Provides support for creating, reading, modifying, and writing TERA's data center files. | ![Downloads][data-dls] |
| [![Vezel.Novadrop.IO][io-img]][io-pkg] | Provides support for creating, reading, modifying, and writing
TERA's various archive formats. | ![Downloads][io-dls] |
| [![Vezel.Novadrop.Launcher][launcher-img]][launcher-pkg] | Provides support for interacting with the TERA client launcher. | ![Downloads][launcher-dls] |
| [![Vezel.Novadrop.Memory][memory-img]][memory-pkg] | Provides support for manipulating local and remote process memory
for the purpose of reverse engineering. | ![Downloads][memory-dls] |
| [![Vezel.Novadrop.Net][net-img]][net-pkg] | Provides support for TERA's network protocol. | ![Downloads][net-dls] |

[dc-pkg]: https://www.nuget.org/packages/novadrop-dc
[gpk-pkg]: https://www.nuget.org/packages/novadrop-gpk
[rsc-pkg]: https://www.nuget.org/packages/novadrop-rsc
[run-pkg]: https://www.nuget.org/packages/novadrop-run
[scan-pkg]: https://www.nuget.org/packages/novadrop-scan
[data-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Data
[io-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.IO
[launcher-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Launcher
[memory-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Memory
[net-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Net

[dc-img]: https://img.shields.io/nuget/v/novadrop-dc?label=novadrop-dc
[gpk-img]: https://img.shields.io/nuget/v/novadrop-gpk?label=novadrop-gpk
[rsc-img]: https://img.shields.io/nuget/v/novadrop-rsc?label=novadrop-rsc
[run-img]: https://img.shields.io/nuget/v/novadrop-run?label=novadrop-run
[scan-img]: https://img.shields.io/nuget/v/novadrop-scan?label=novadrop-scan
[data-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Data?label=Vezel.Novadrop.Data
[io-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.IO?label=Vezel.Novadrop.IO
[launcher-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Launcher?label=Vezel.Novadrop.Launcher
[memory-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Memory?label=Vezel.Novadrop.Memory
[net-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Net?label=Vezel.Novadrop.Net

[dc-dls]: https://img.shields.io/nuget/dt/novadrop-dc?label=
[gpk-dls]: https://img.shields.io/nuget/dt/novadrop-gpk?label=
[rsc-dls]: https://img.shields.io/nuget/dt/novadrop-rsc?label=
[run-dls]: https://img.shields.io/nuget/dt/novadrop-run?label=
[scan-dls]: https://img.shields.io/nuget/dt/novadrop-scan?label=
[data-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Data?label=
[io-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.IO?label=
[launcher-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Launcher?label=
[memory-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Memory?label=
[net-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Net?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.
