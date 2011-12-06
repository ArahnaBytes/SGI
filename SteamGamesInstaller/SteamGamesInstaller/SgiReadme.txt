Copyright � 2011, Mesenion (ArahnaBytes). All rights reserved.
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
form; or use syahmixp's GCF WinRAR Plugin (plugin have high probability to corrupt extracted files, so test extracted files!);
or use Nemesis's GCFScape (this program do not check control sums, so it useless for validating game files). After extracting
all game files you should have in this example three directory, each of them will contain one ncf file and "common\game_name"
directory with game files related only to this ncf file). SGI should be placed in directory which contains these three
directories.

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

1.0.* December 7, 2011:
- Implemented installation of fixes.
- Replacing version stump in SgiReadme.txt if command line have /fixchangelogversion argument.
- Removed some duplicate code.

v1.0.4357.5291 "White Stone" December 5, 2011:
- First public release.
