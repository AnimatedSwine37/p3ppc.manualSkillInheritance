# Translating The Mod
If you are interested in translating the mod to another language then awesome, this page details what needs to be done.

If you need help at any point or if you finish a translation then feel free to contact me on Discord (AnimatedSwine37#6932), by making an Issue on this repo, or anywhere else you can find me.

## Textures
The [assets](assets) folder contains the sprites that this mod uses in xcf format (for Gimp users) or as a plain png (anyone else). 
Note that the text is intentionally upside down in these sprites. 

For the two "Choose which skills to inherit" textures if you don't have a second font that you feel fits well feel free to only make a texture for one and I can disable the alternate text style for the particular language.
The only reason this exists is that there were two textures that people were divided between for the English version of the mod, if it isn't there for other languages I don't care.

### Testing In Game
If you aren't interested in doing this yourself that's fine, I can work with just the image, but if you are so inclined read on.

If you want to check that your textures look good in game you can replace the existing ones which are located in [Files\English\umd0\facility\combine\inheritance.spr](p3ppc.manualSkillInheritance/Files/English/umd0/facility/combine) using [Amicitia](https://github.com/tge-was-taken/Amicitia/releases/latest).
When doing so you will need to use [Tmx Converter](https://github.com/LTSophia/TMXConverter/releases/latest) to first convert your png to a tmx file.

## Text
Igor's message explaining Skill Inheritance in the Velvet Room's Talk menu needs to be altered to not mention skills being randomly assigned when selecting a Persona and the text "Please choose the skills you would like to pass on." needs to be translated (this is a brand new message).

### Testing In Game
If you aren't interested in doing this yourself that's fine, I can work with just the text, but if you are so inclined read on.

Both messages are located in [umd0.cpk\facility\combine.bin\msg_combine.bmd](p3ppc.manualSkillInheritance/Files/English/umd0/facility) which can be extracted from your game's files using [CriFsLib.GUI](https://github.com/Sewer56/CriFsV2Lib/releases/latest) and can be edited using [Atlus Script Compiler](https://github.com/tge-was-taken/Atlus-Script-Tools).
