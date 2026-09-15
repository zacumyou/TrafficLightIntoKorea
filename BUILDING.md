# Building Traffic Light Into Korea

Version: 2.3.28. Target game: Cities: Skylines II 1.6.*.

Install the game's official Modding Toolchain through the game's options and use the environment variables it configures. Game and Unity assemblies are not distributed in this repository.

Official setup and publication documentation:
https://cs2.paradoxwikis.com/Modding_Toolchain

```powershell
dotnet build src/TrafficLightIntoKorea.csproj -c Release
node verify-ui.cjs
```

The official build runs ModPostProcessor and Burst. Output is written to `src/bin/Release/net48`. This project's `DeployWIP` target does not automatically install the result.

For a manual local install, copy the mod DLL, three generated native libraries, `IndividualSignals.js`, `IndividualSignal.svg`, `KoreanJunction.svg`, and `Icons` to the game's local `Mods/TrafficLightIntoKorea` folder. Do not load a local copy and a subscribed copy at the same time.

The independent rule tests require .NET 9:

```powershell
dotnet run --project tests/RecoveryTests.csproj
dotnet run --project fixed-tests/FixedTests.csproj
dotnet run --project accessory-tests/AccessoryTests.csproj
dotnet run --project merge-tests/MergeTests.csproj
dotnet run --project signal-state-tests/SignalStateTests.csproj
dotnet run --project three-lamp-tests/ThreeLampTests.csproj
```

Automated tests do not replace in-game checks. The 2.3.28 missing-asset dialog fix requires a game restart for live verification.
