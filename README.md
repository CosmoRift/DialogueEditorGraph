# Dialogue Graph Editor
by ???
Dialogue Graph Editor is an editor for designing dialogues for your games and projects using nodes. They are easier to use than raw `ScriptableObject` lists and references. This project is made by one guy that is bellow average programmer so if you encounter any bugs report them!
## Installation
You can download this package by pressing green button with `Code` and downloading package and unpack it, then in Unity go to package manager, press `+` in top left and add from disk, select `package.json` inside and wait.
Alternative downloading option will come soon.
## Setup after downloading
Because this package is holding on tape you need to fix some stuff before using it.
### Part 1: Downloading
After you downloaded this package you might have seen some errors. They appear because you need to add dependencies.
List:
 - **UltEvents** by [Kybernetik](https://forum.unity.com/members/kybernetik.174098/) - You can download it from [Asset Store](https://assetstore.unity.com/packages/tools/gui/ultevents-111307?aid=1100l8ah5&utm_source=aff) or [itch.io](https://kybernetik.itch.io/ultevents),
 - **SerializableCallback** by [Siccity](https://github.com/Siccity) - You can download it from [github](https://github.com/Siccity/SerializableCallback) (read `ReadMe.md` to download it using `manifest.json`),
 - **NaughtyAttributes** by [dbrizov](https://github.com/dbrizov) - You can download it from [github](https://github.com/dbrizov/NaughtyAttributes) (read `ReadMe.md` to download it using `manifest.json`).
### Part 2: Fixes
**UltEvents** add new action from current class which isn't helpful in ours case. To change it, look into folder **Fixes** in this package. 
Replace contents of `UltEvents\Inspector\UltEventDraver.cs` with contents of `DialogueGraphEditor\Fixes\UltEventDraver.txt` and  
contents of `UltEvents\Events\UltEventBase.cs` with contents of `DialogueGraphEditor\Fixes\UltEventBase.txt`.
It will fix this issue.
### Done!
You have *maybe* fixed all the setup errors! Great job!
## Documentation
Documentation will come soon.