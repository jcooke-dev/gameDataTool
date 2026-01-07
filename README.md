# GameDataTool

GameDataTool is a sample/demo solution that could help with packaging game asset files for deployment. It's composed of a few projects: a Windows GUI application, a console application, and an underlying library, all written in VS 2026 with C#.

GameDataTool allows you to build up a data package, by adding asset files (images, textures, meshes, sounds, scripts, etc.) individually, from all within a folder, or from another already packaged data file.  You can then generate the Packaged Data File (.PKD), which will contain header/meta data and all of your assets, sequentially.

I created this tool primarily as an interesting side-project to better understand a game development data pipeline and to refresh my knowledge of wpf and xaml for Windows GUI apps.

## Developers
Justin Cooke

## Development Environment
I targeted this tool to modern Windows machines and used these IDEs, utilities, and frameworks:
* **Microsoft Visual Studio 2026 - Community Edition 18.1.1** - IDE (https://visualstudio.microsoft.com/)
* **Microsoft .NET 10.0** - Framework (https://dotnet.microsoft.com/en-us/)
* **Notepad++** - Powerful text editor (https://notepad-plus-plus.org/)
* **HxD** - Hex editor and viewer (https://mh-nexus.de/en/hxd/)

## Features
* Interact with the GameDataTool through either a Windows GUI or console application.
* Build up a data package, which is intended to contain all game assets for a single entity (such as a car or track in a racing game).  These assets may include images, textures, meshes, sounds, scripts, etc..
* You can add files individually or by importing all supported file types from within a folder.  You can also load all asset files from a previously generated Packaged Data File (.PKD).
* Once satisfied with the full entity's data package, you can generate that PKD file.  The PKD will include header/meta data describing the file contents and then all individual asset files from this package sequentially following the header.
* The Windows GUI application implements some simple tool state persistence by maintaining the last used window state/size/position and the most recently accessed folders and PKD files.
* The tool includes a somewhat simplistic checksum calculation stored with the PKD file's metadata and then used on subsequent imports to confirm that the file hasn't been modified.

## Usage

## Interesting Development Questions and Solutions

## License
Distributed under the MIT license here: [LICENSE](LICENSE)
