# Novadrop

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

This project offers the following packages:

* [novadrop-dc](https://www.nuget.org/packages/novadrop-dc): Provides the .NET
  global tool for manipulating TERA's data center files.
* [novadrop-launch](https://www.nuget.org/packages/novadrop-launch): Provides
  the .NET global tool for launching the TERA client manually from the command
  line.
* [novadrop-scan](https://www.nuget.org/packages/novadrop-scan): Provides the
  .NET global tool for extracting useful data from a running TERA client.
* [Vezel.Novadrop.Data](https://www.nuget.org/packages/Vezel.Novadrop.Data):
  Provides support for creating, reading, modifying, and writing TERA's data
  center files.
* [Vezel.Novadrop.Launcher](https://www.nuget.org/packages/Vezel.Novadrop.Launcher):
  Provides APIs for interacting with the TERA client launcher.
* [Vezel.Novadrop.Memory](https://www.nuget.org/packages/Vezel.Novadrop.Memory):
  Provides support for manipulating local and remote process memory for the
  purpose of reverse engineering
* [Vezel.Novadrop.Net](https://www.nuget.org/packages/Vezel.Novadrop.Net):
  Provides support for TERA's network protocol.

For more information, please visit the
[project page](https://github.com/vezel-dev/novadrop).
