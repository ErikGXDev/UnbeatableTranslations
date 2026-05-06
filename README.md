# Unbeatable Translations

A mod that allows you to dump and load custom translations for UNBEATABLE.

The mod has several shortcuts that you can use to first dump, then edit, and finally load translations into the game.

## Installation

This mod requires you to have BepInEx 5 installed. You will most likely already have this when using the CustomBeatmaps mod.

Download the latest release from the [releases page](https://github.com/ErikGXDev/UnbeatableTranslations/releases) and merge the contents of the zip file with your BepInEx folder.

## Shortcuts

CTRL + Shift + 1 - enable/disable translations

CTRL + Shift + 2 - reload translations

CTRL + Shift + 3 - dump translations

CTRL + Shift + 4 - reload scene

---

Check the BepInEx console for more output when using these shortcuts.

Translations will be dumped into a `translations_dumped` folder in the game folder. The game will load translations from a `Translation` folder that you will have to create inside the game directory. **Please do not edit the translations inside the dumped folder, to avoid overwriting your work accidentally.**


## Notes

Several translations for UI-related elements are currently not supported. However, dialogue, speakers and notifications are already editable.
