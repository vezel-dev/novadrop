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
| [![novadrop-launch][launch-img]][launch-pkg] | Provides the .NET global tool for launching the TERA client manually from the command line. | ![Downloads][launch-dls] |
| [![novadrop-scan][scan-img]][scan-pkg] | Provides the .NET global tool for extracting useful data from a running TERA client. | ![Downloads][scan-dls] |
| [![Vezel.Novadrop.Data][data-img]][data-pkg] | Provides support for creating, reading, modifying, and writing TERA's data center files. | ![Downloads][data-dls] |
| [![Vezel.Novadrop.Launcher][launcher-img]][launcher-pkg] | Provides APIs for interacting with the TERA client launcher. | ![Downloads][launcher-dls] |
| [![Vezel.Novadrop.Memory][memory-img]][memory-pkg] | Provides support for manipulating local and remote process memory
for the purpose of reverse engineering. | ![Downloads][memory-dls] |
| [![Vezel.Novadrop.Net][net-img]][net-pkg] | Provides support for TERA's network protocol. | ![Downloads][net-dls] |

[dc-pkg]: https://www.nuget.org/packages/novadrop-dc
[launch-pkg]: https://www.nuget.org/packages/novadrop-launch
[scan-pkg]: https://www.nuget.org/packages/novadrop-scan
[data-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Data
[launcher-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Launcher
[memory-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Memory
[net-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Net

[dc-img]: https://img.shields.io/nuget/v/novadrop-dc?label=novadrop-dc
[launch-img]: https://img.shields.io/nuget/v/novadrop-launch?label=novadrop-launch
[scan-img]: https://img.shields.io/nuget/v/novadrop-scan?label=novadrop-scan
[data-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Data?label=Vezel.Novadrop.Data
[launcher-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Launcher?label=Vezel.Novadrop.Launcher
[memory-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Memory?label=Vezel.Novadrop.Memory
[net-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Net?label=Vezel.Novadrop.Net

[dc-dls]: https://img.shields.io/nuget/dt/novadrop-dc?label=
[launch-dls]: https://img.shields.io/nuget/dt/novadrop-launch?label=
[scan-dls]: https://img.shields.io/nuget/dt/novadrop-scan?label=
[data-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Data?label=
[launcher-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Launcher?label=
[memory-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Memory?label=
[net-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Net?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.
