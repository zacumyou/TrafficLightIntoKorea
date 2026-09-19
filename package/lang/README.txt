TLIK individual panel localization
Copy en-US.json to the target locale ID (for example ja-JP.json) and translate VALUES only. Preserve every TLK.Individual.TextNNN key. English/Korean files are built in; other languages use English until community translations override these keys.
I18NEveryWhere discovers lang/<localeId>.json beside the mod DLL. Cache edits can be lost during mod updates; distribute translations using its supported language-pack workflow for persistence. Its Restrict Mode prevents overwriting existing keys; allow overrides when replacing TLIK fallback text. Reopen the editing panel after a language change/reload.
Reference: https://github.com/baka-gourd/I18NEveryWhere
Only the custom individual panel is covered by these JSON files. Existing settings/tool localization remains registered through the game's localization system. In-game integration with I18NEveryWhere has not been verified.
