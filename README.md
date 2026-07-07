# Description

`UPMod.pw.dll` is a small mod for [Pillars of Eternity](https://store.steampowered.com/app/291650/Pillars_of_Eternity/) that fixes several in-game bugs. It is intended to be used with [Patchwork Launcher](https://github.com/GregRos/Patchwork) and patches several methods from `Assembly-CSharp.dll`.

## Versioning

- UPMod.1.01.306.pw.dll was generated for v3.06 of the game
- UPMod.1.01.370.pw.dll was tested with v3.70 of the game
- UPMod.1.01.393.pw.dll was generated for v3.9.3 of the game
- UPMod.1.01.394.pw.dll was generated for v3.9.4 of the game
- UPMod.1.01.395.pw.dll was generated for v3.9.5 of the game

## Patchwork Downloads

Patchwork Launcher can be downloaded from:

- [Nexus Mods (launcher + appinfo.dll)](https://www.nexusmods.com/pillarsofeternity/mods/335)
- [Nexus Mods (launcher + appinfo.dll + IEMod)](https://www.nexusmods.com/pillarsofeternity/mods/1)
- [GitHub (launcher only)](https://github.com/GregRos/Patchwork)

## Related Links

- [Base game method snapshots](https://github.com/naijaro/poe-upmod-basegame-snapshots)
- [UPMod on Nexus Mods](https://www.nexusmods.com/pillarsofeternity/mods/308)
- [UPMod forum thread](https://forums.obsidian.net/topic/92999-community-bug-fixes/#comment-1921632)

## Building from Source

To build `UPMod.pw.dll` from source:

1. Copy `Assembly-CSharp.dll` from the game directory, for example:

   ```text
   G:\platforms\Steam\steamapps\common\Pillars of Eternity\PillarsOfEternity_Data\Managed
   ```

2. Open it using `OpenAssemblyCreator.exe` from Patchwork:

   ```text
   OpenAssemblyCreator.exe Assembly-CSharp.dll Assembly-CSharp.Open.dll
   ```

3. Copy `Assembly-CSharp.Open.dll` into the project root and rename it to:

   ```text
   Assembly-CSharp.dll
   ```

4. Rebuild the solution in Visual Studio.

# License Note

© 2017–2026 MaxQuest (Naijaro)

"UPMod" is the original project name of this mod.

Derivative works and forks are permitted under the project license,
but may not use the name "UPMod" in a way that implies
official endorsement by or maintenance from the original author.

Please retain attribution to the original author
in derivative works, forks, and redistributions.

Only builds distributed from the official repository
are endorsed by the original author.

Forks and derivative works are maintained independently
and are the responsibility of their respective maintainers.

This software is not intended for use in malware,
credential theft, unauthorized surveillance,
or other malicious activity.

## Special Permission

Obsidian Entertainment, Inc. may use, modify,
redistribute, or incorporate this code into official
Pillars of Eternity products, updates, or patches
without attribution requirements.
