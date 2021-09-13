# RimWorldImmersiveResearch

![Version](https://img.shields.io/badge/Rimworld-1.2-brightgreen.svg)

A mod that overhauls the vanilla game research mechanics of the game RimWorld.

**Notice:** Immersive Research requires Harmony to work correctly. You can Download it from its respective Steam Workshop page.

**Xtra Notice** Pushed final update of work in progress code that I was working on last year. Deemed the mod to be deprecated, both by my enthusiasm for it, and the fact that other mods of similar style were starting to crop up (And now the fact it is being maintained by somebody else).

It is VERY UNSTABLE, and I cannot guarantee that it can work with the latest versions of RimWorld (or any at all).

Was working on:
- Added major functionality to incorporate other mods with research options.
- Implemented ability to generate patch xml based on the user's installed mods (was in progress, currently incorporates all mods regardless if active)
	- This was in very early stage, and would break the mod if you uninstalled a mod from your game, requiring you to delete patch files manually.
	- Would need this to be done within the game, probably within a mod settings window.
- Seperate Mod Experiment Window, with adjusted material costs based on the length of the respective research. Experiments were sorted by the authors.
	- This was as far as I could think to go, as you are unable to determine an experiment type from a ResearchProjectDef (I decided them myself).
	- Was also in early stage, needs significant string editing to make it more pretty (currently has packageID as the mod name instead of the mod name itself).
- Added ability to take specific experiments from the Filing Cabinet, either manually or by a colonist during a Bill.
- 
## Features:

### Experiment System
- Intellectual colonists can now perform research experiments into various scientific fields. These experiments, when completed, will unlock research projects based on the scientific field you choose, and the size of the experiment.
- Experiments can be performed at a new Experiment Bench structure, which requires no research to build.
- Each potential type of experiment you can perform have specific resource costs to be able to be completed. They are projects designed to be completed over time, and can be safely paused and resumed when needed.

### 'Brain Drain' - Education System
- Your colonists are able to 'study' completed experiments, which will mark them as 'researchers'.
- Your colony researchers are the backbone of your colony's collective knowledge, and losing them can bring dire consequences.
- If a researcher happens to die, the effects can range from loss of progress on research projects, all the way to losing them entirely.

### Experiment Filing Cabinet
- To make the process of storing completed experiments more feasible, you are able to construct a filing cabinet building.
- You can store and retrieve any experiment at any time.

### Ancient Datadisks
- Scattered throughout the (rim)world are old Glitterworld datadisks, which must be decoded to discover their contents.
- Datadisks can be sometimes be obtained from exotic traders, and from dissasembling mechanoids.
- Datadisk contents can range from invaluable research data, all the way to completely useless and extremely valuable.
- Random flavour text is added to some types of datadisks for better immersion.

### Datadisk Analyzer
- This is a miscellaneous construct that links to either type of research bench, similar to the Multi-Analyzer. When sufficiently powered and connected, you will be able to decode any locked datadisks you own, and load any research disks you own.

## Changes to Research Progression
- Locked research options are now hidden from view, and are only visible when unlocked via finishing experiments or loading research datadisks.
- The main gameplay loop this mod provides is as follows:
	- Complete experiments to unlock new research projects, or obtain them through research datadisks. Research projects obtained through datadisks are exempt from the effects of a researchers death.
	- Colonists will study completed experiments to decrease negative effects of researcher death. The more colonists that study the same project, the less damage it will cause.
	
## Compatibility
This mod should work correctly with any mods that add new research projects.
**Notice:** Any mod that either completely overrides or makes significant changes to the research window is not compatible with this mod.
	- This could change in future, although it would probably require removing some features which could detract from the mods overall experience.
