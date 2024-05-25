- [Meterway](#Meterway)
- [Instalation](#Instalation)
- [Building](#Building)
  * [Prerequisites](#Prerequisites)
  * [Build](#Build)
  * [Add to game](#Add-to-game)


# Meterway
Dalamud-based FFXIV addon that display and help you analyze combat data. 

MeterWay is designed to be used in conjunction with [IINACT](https://www.iinact.com/) or Act and has the ability acts as a real-time log parser alowing to get and display elaborated statistics and data even during combat.

## Instalation
1. Click the "Experimental" tab.
2. Paste ```https://raw.githubusercontent.com/CondeSaheki/MeterWay/main/repo.json``` into the empty box at the bottom of the "Custom Plugin Repositories" section (the last box will always have a + next to it).
3. Click the + button.
4. Click the 💾 button.

## Building
### Prerequisites
- XIVLauncher, FINAL FANTASY XIV, and Dalamud installed and the game has been run with Dalamud at least once.
- DALAMUD_HOME environment variable must be set if you used an custom instalation paths.
- .NET Core 8 SDK installed and configured.

### Build
- Restore packages with ```dotnet restore```
- Run ```dotnet build -c Debug``` or ```dotnet build -c Release```

#### Add to game
Add the full path to the ```MeterWay.dll``` in list of Dev Plugins and then Install with Plugin Installer. You can disable, enable, or load your plugin on startup
