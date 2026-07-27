# LordCaptor

Single-player module for Mount & Blade II: Bannerlord. It overhauls prisoner management for lords, giving you mass-action capabilities right after battles and on the campaign map.

## What it does

- **Post-Battle Management**: Intercepts the vanilla lord capture logic. After a battle, instead of dealing with prisoners one by one, you are presented with a convenient UI.
- **Mass Actions**: Execute, release, or take all captured enemy lords as prisoners with a single click.
- **Campaign Map Access**: Press `Shift+O` anytime on the map to open the prisoner management UI and manage the lords you currently hold captive.

## Release contents

The build scripts produce a self-contained module in the `dist/LordCaptor` directory.

## Compatibility and dependencies

- Single-player only.
- Harmony is bundled in `bin/Win64_Shipping_Client` (if built manually) and used for patching the core game logic; do not install a separate copy for this module.
- Safe to add to or remove from existing saves.

## Build from WSL/Linux

You must provide the path to your Bannerlord installation using the `BANNERLORD_DIR` environment variable. With Mono installed, run:

```bash
chmod +x build-linux.sh
BANNERLORD_DIR="/path/to/Mount & Blade II Bannerlord" ./build-linux.sh
```

**Important note for WSL users:** Windows restricts executing `.dll` files generated or copied from WSL environments (Mark of the Web issue). After copying the module to your game directory on Windows, you must unblock the files. Open PowerShell and run:

```powershell
Get-ChildItem -Path "C:\Your\Path\To\Mount & Blade II Bannerlord\Modules\LordCaptor" -Recurse | Unblock-File
```

## Build from Windows

Run the PowerShell script. If your game is not in the default Steam directory, specify the `-BannerlordDir` parameter:

```powershell
.\build-windows.ps1 -BannerlordDir "\path\to\Mount & Blade II Bannerlord"
```

## Install

Copy the contents of `dist/LordCaptor` to:

```text
Mount & Blade II Bannerlord/Modules/LordCaptor
```

Then enable **LordCaptor** in the Bannerlord launcher, after the official single-player modules.
