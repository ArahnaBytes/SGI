Copyright © 2011-2012, Mesenion (ArahnaBytes). All rights reserved.
See copyright notice in SgiLicense.txt.


Steam Games Installer (SGI) helps install games downloaded from Steam.
All source code available on https://github.com/ArahnaBytes/SGI/


REQUIREMENTS:
1. PC with Windows XP OS or later version of Windows OS.
2. Microsoft .NET Framework 3.5 Service Pack 1 (Full Package), update KB959209 and in some cases update KB967190:
http://www.microsoft.com/download/en/details.aspx?id=22#instructions

PREPARING GAME FILES:
Typical example: some game has three ncf files stored in "C:\Program Files (x86)\Steam\SteamApps\" directory:
"game content.ncf", "game binaries.ncf" and "game english.ncf". All games files stored in
"C:\Program Files (x86)\Steam\SteamApps\common\game_name\" directory. All game files related to "game content.ncf"
("game content.ncf" itself and some portion of files from "C:\Program Files (x86)\Steam\SteamApps\common\game_name\" directory)
should be placed in "game content" or "game content.v17" directory ("game content.v17" means that "game content.ncf" file have
17 version in this example; such form of versioning is strongly recommended). You can use steamCooker's CF Toolbox (recommended)
for downloading game files for single ncf or gcf file to specified directory, validating game files and creating updates in gcf
format; or use syahmixp's GCF WinRAR Plugin (plugin have high probability to corrupt extracted files, so test extracted files!);
or use Nemesis's GCFScape (this program do not check control sums, so it useless for validating game files). After extracting
all game files you should have in this example three directory, each of them will contain one ncf file and "common\game_name"
directory with game files related only to this ncf file). SGI should be placed in directory which contains these three
directories.

USING SHARED FILES FEATURE:
This feature allows share files of one depot to other depots.
If some depots use the same files and such files logically belongs to one depot then better share such files and do not waste
depot space. Good candidates for sharing are very big files which unlikely will changed in other depots. Bed candidates are
binaries, config files, small files (several megabytes) etc.

USING FIXES FEATURE:
This feature allow to user replace or add files of installed game.
If user selected install fixes and directory where placed SGI contains "_Fixes" directory and if "_Fixes" directory contains
directory which name is the same as name of directory used for installation game files, then content of such directory will be
copied to installation directory and if some files in such directory has the same names as files in installation directory, then
original files will be renamed by adding ".original" to the end of file name.
In previous example if directory where placed SGI has "game english.v7\" and "_Fixes\game english.v7\" directories then all files
from "_Fixes\game english.v7\" directory will be copied to installation directory ("C:\Program Files (x86)\Steam\SteamApps\")
with renaming original files if "game english.v7\" directory was used for installation of game.

USING UPDATES FEATURE:
Not implemented yet.

FAQ:

Q: WTF?! I don't understand a single word of what you saying! Is it English or what?
A: Yeees! This is my awesome and perfect English! But some times... my teacher say that it is "shame" and teaching me
"is wasting of time". I agree. Use issue tracker if you want help to fix this.

Q: Why I should save game files to separate directories when my installed game use only one directory
("C:\Program Files (x86)\Steam\SteamApps\common\game_name\" for example)?
A: We live in world where people talk on diffrent languages. To minimize size of installed game most of developers prefer
to divide files on localizable part which contains files translated to one of languages and non localizable which contains
files independent from language. In some cases localizable files for diffrent languages have same names, so it is impossible
to save such files in the same directory.

CHANGELOG:

1.0.*
- Added to games list: The Elder Scrolls V: Skyrim Creation Kit and Skyrim High Resolution Texture Pack.

v1.0.4404.25 "White Stone" Sunday, 22 January 2012:
- SGI now search directories for depots in current directory and in directories of current directory (2 level depth).
- E.Y.E added to games list.
- Shared files for "X3AP russian" depot added to games list.
- Added languages for Terraria.

v1.0.4379.41550 "White Stone" Wednesday, 28 December 2011:
- Fixed bug: if install script have language condition in registry node then keys will not saved in registry.
- Implemented installation of shared files.
- Added to games list: RAGE, Supreme Ruler: Cold War and Dead Island.

v1.0.4371.28373 "White Stone" Tuesday, 20 December 2011:
- Fixed typos in depots list for X3: Albion Prelude.
- Added russian depots for X3: Terran Conflict and X3: Albion Prelude.

v1.0.4370.26919 "White Stone" Monday, 19 December 2011:
- Fixed bug: if install script does not have "HasRunKey" token in "Run Process" token tree, then default registry key will not
created or checked and process will not runned.
- Added to games list: DEFCON, X3: Terran Conflict and X3: Albion Prelude.

v1.0.4363.41913 "White Stone" Monday, 12 December 2011:
- Fixed crash if not exists HKEY_CURRENT_USER\Software\Valve\Steam\SteamPath registry key.

v1.0.4363.13647 "White Stone" Monday, 12 December 2011:
- Fixed bug since v1.0.4358.1416: if install two or more fix directories and some files in such directories have same names then
renamed original file in installation directory will be replaced with file from penultimate fix directory.
- User now have option to do these actions in any combinations: install application, install fixes, execute installation script.
- Front end now call methods from back end through interface ISgiManager.
- Implemented files maps model (dictionaries of key-values where key is relative path to file in installation directory and
value is file in depot).
- SGI now use terminology used in https://partner.steamgames.com/documentation/running_on_steam and
http://udn.epicgames.com/Three/Steam.html pages.
- Depots now have separate version\directory dictionaries for common and fixes directories.

v1.0.4359.10862 "White Stone" Thursday, 08 December 2011:
- Fixed possible bug if in valve data file token have value with \" string.
- Added support of multicultural components.
- You can assign to GameInstallMethod delegates all game install methods which now have matching signature.
- Terraria added to games list.
- Program.FixChangeLogVersion method improvement.
- SgiManager.ComponentsCheckDefaultMethod method code cleaning.

v1.0.4358.1416 "White Stone" December 7, 2011:
- Implemented installation of fixes.
- Replacing version stump in SgiReadme.txt if command line have /fixchangelogversion argument.
- Removed some duplicate code.

v1.0.4357.5291 "White Stone" December 5, 2011:
- First public release.
