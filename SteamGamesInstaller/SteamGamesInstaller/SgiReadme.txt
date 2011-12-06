Copyright © 2011, Mesenion (ArahnaBytes). All rights reserved.
See copyright notice in SgiLicense.txt.


Steam Games Installer (SGI) helps install games downloaded from Steam.


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
form; or use syahmixp's GCF WinRAR Plugin (plugin have high probability to corrupt extracted files, so test extracted files!);
or use Nemesis's GCFScape (this program do not check control sums, so it useless for validating game files). After extracting
all game files you should have in this example three directory, each of them will contain one ncf file and "common\game_name"
directory with game files related only to this ncf file). SGI should be placed in directory which contains these three
directories.

USING FIXES FEATURE:
Not implemented yet.

USING UPDATES FEATURE:
Not implemented yet.

CHANGELOG:

1.0.4357.5291 "White Stone" December 5, 2011:
- First public release.