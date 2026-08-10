# Northern Lands XIV implementation status

This branch turns the Boss Room sample into the foundation for the single-player Android action RPG **Northern Lands XIV**. It is intentionally isolated from `main` while the campaign is under construction so Unity Build Automation does not consume free minutes for incomplete milestones.

## Locked product direction

- Third-person real-time action RPG for Android.
- A connected campaign with seven explorable worlds, cities, dungeons and portals rather than a single combat arena.
- Forty-five permanent race choices with distinct stats and active abilities.
- Melee, magic, blocking, dash, equipment, talents and living-item progression.
- Death moves the hero to the Dead World. Five soul kills and two soul ash unlock the Tower of Gods. Completing its boss or riddle returns the hero to life. A second death inside the Dead World resets run progress but keeps the permanent race.
- Local, versioned save data suitable for offline single-player play.

## Implemented in this milestone

- Versioned content manifest containing all 45 races and all 7 worlds.
- Validated content catalog with unique identifiers.
- Run progression and permanent-race state kept as separate save domains.
- Dead World, soul ash, Tower of Gods unlock and return-to-life rules.
- Atomic local JSON persistence with backup behavior.
- Hero levels, experience thresholds, two talent choices per level and stat growth.
- Equipment data for weapons, armor and rings across five rarities.
- Living-item soul experience, soul levels, kill tracking and item consumption.
- Riverholm first-hunt quest progression and one-time reward handling.
- Per-world city reputation, crime status and both intermediary redemption paths.
- Gated world travel with autosave before every transition.
- Dependency injection through the existing Boss Room `ApplicationController`.
- Runtime tests for content counts, the death loop, progression, quests, living items and reputation.

## Planned campaign scenes

| Scene | Purpose |
|---|---|
| `NL_NorthernLands` | Riverholm, forests, roads, camps, first dungeons and portal hub |
| `NL_AshenWorld` | Ash Harbor, lava fields, ruined forge and fire enemies |
| `NL_StarWastes` | Astralis, crystal desert, observatory and astral enemies |
| `NL_DeadWorld` | Soul hunt, ash collection and entrance to the Tower |
| `NL_AncientDungeon` | Multi-floor combat and treasure dungeon |
| `NL_TowerOfGods` | Boss path and the riddle trial |
| `NL_QuietDimension` | Hidden race-gated world with unique resources |

## Definition of the next playable milestone

The next cloud build is not started until the branch contains a real vertical slice: a controllable character, third-person camera, touch controls, Riverholm outdoor terrain, enemies with navigation, combat feedback, loot, a portal to the Dead World and a working save/load loop.
