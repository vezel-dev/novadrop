# Novadrop

<div align="center">
    <img src="novadrop.svg"
         width="128"
         alt="Novadrop" />
</div>

<p align="center">
    <strong>
        A developer toolkit for interacting with and modifying the TERA game
        client.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/vezel-dev/novadrop?color=brown)](LICENSE-0BSD)
[![Commits](https://img.shields.io/github/commit-activity/m/vezel-dev/novadrop/master?label=commits&color=slateblue)](https://github.com/vezel-dev/novadrop/commits/master)
[![Build](https://img.shields.io/github/actions/workflow/status/vezel-dev/novadrop/build.yml?branch=master)](https://github.com/vezel-dev/novadrop/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/novadrop?color=teal)](https://github.com/vezel-dev/novadrop/discussions)
[![Discord](https://img.shields.io/badge/discord-chat-7289da?logo=discord)](https://discord.gg/wtzCfaX2Nj)
[![Zulip](https://img.shields.io/badge/zulip-chat-394069?logo=zulip)](https://vezel.zulipchat.com)

</div>

--------------------------------------------------------------------------------

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
| [![novadrop-rc][rc-img]][rc-pkg] | Provides the .NET global tool for manipulating TERA's resource container files. | ![Downloads][rc-dls] |
| [![Vezel.Novadrop.Formats][formats-img]][formats-pkg] | Provides support for TERA's various file formats. | ![Downloads][formats-dls] |
| [![Vezel.Novadrop.Client][client-img]][client-pkg] | Provides support for interacting with the TERA launcher and client. | ![Downloads][client-dls] |
| [![Vezel.Novadrop.Interop][interop-img]][interop-pkg] | Provides low-level bindings for in-memory interoperation with the TERA client. | ![Downloads][interop-dls] |

[dc-pkg]: https://www.nuget.org/packages/novadrop-dc
[rc-pkg]: https://www.nuget.org/packages/novadrop-rc
[formats-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Formats
[client-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Client
[interop-pkg]: https://www.nuget.org/packages/Vezel.Novadrop.Interop

[dc-img]: https://img.shields.io/nuget/v/novadrop-dc?label=novadrop-dc
[rc-img]: https://img.shields.io/nuget/v/novadrop-rc?label=novadrop-rc
[formats-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Formats?label=Vezel.Novadrop.Formats
[client-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Client?label=Vezel.Novadrop.Client
[interop-img]: https://img.shields.io/nuget/v/Vezel.Novadrop.Interop?label=Vezel.Novadrop.Interop

[dc-dls]: https://img.shields.io/nuget/dt/novadrop-dc?label=
[rc-dls]: https://img.shields.io/nuget/dt/novadrop-rc?label=
[formats-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Formats?label=
[client-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Client?label=
[interop-dls]: https://img.shields.io/nuget/dt/Vezel.Novadrop.Interop?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.

For more information, please visit the
[project home page](https://docs.vezel.dev/novadrop).

## Building

You will need the .NET SDK installed. Simply run `./cake`
(a [Bash](https://www.gnu.org/software/bash) script) to build artifacts. You can
also use `./cake pack` if you do not want to build the documentation (which
requires Node.js).

These commands will use the `Debug` configuration by default, which is suitable
for development and debugging. Pass `-c Release` instead to get an optimized
build.

## License

This project is licensed under the terms found in
[`LICENSE-0BSD`](LICENSE-0BSD).
