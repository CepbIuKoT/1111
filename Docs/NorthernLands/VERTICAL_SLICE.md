# Riverholm campaign slice

The development branch now generates its first campaign scene before export instead of launching the old Boss Room arena.

## Included in the generated scene

- 500 × 500 metre traversable highland terrain with hills and a carved river valley.
- Riverholm: perimeter walls, south gate, roads, Nordic houses and the jarl's hall.
- A glacial river and timber bridge.
- Black-pine forest, hunter camp, old watchtower and a dormant Dead World portal.
- The Boss Room Tank Boy character art, unpacked and stripped of multiplayer behaviours for the local campaign.
- Character-controller locomotion, sprint, dodge, light/heavy animation triggers and a collision-aware orbit camera.
- Landscape Android HUD with a virtual joystick, attack, heavy attack, dodge and sprint controls.
- Desktop fallback controls: WASD, Shift, Space, F/G and right-mouse camera orbit.
- Twelve animated frost-imps placed across roads, forest landmarks and the portal route.
- Local enemy pursuit, melee attacks, hero/enemy health, hit reactions, death and hero respawn.
- Light/heavy area attacks and collectible northern-silver drops.
- HUD readouts for health and collected silver.
- Jarl Ingvar as an in-world quest giver with a contextual mobile interaction button.
- A complete first objective: accept the hunt, defeat four frost imps, return for gold and experience, then unlock the eastern portal.
- Persistent northern silver, health, quest progress and Riverholm position with ten-second autosave and pause/quit saving.
- A second explorable 360 × 360 metre scene, the Shore of the Forgotten in the Dead World, with a soul river, ruined shrines, bone fields, enemies and a sealed road to the Tower of Gods.
- A third playable scene, the Tower of Gods trial hall, with divine pillars, aether crystals, eight guardians and a Gate of Life unlocked by completing the trial.
- A persistent objective compass with a rotating gold arrow, destination name and distance to the jarl, nearest required enemy or world gate.
- The Voice of the Gods encounter in the Tower: the player can answer the original riddle («яма») or commit to the eight-guardian combat trial; gameplay pauses while the choice window is open, and a correct answer dismisses the guardians.
- Real world travel from Riverholm to the Dead World, onward to the Tower of Gods and back to Riverholm, plus the living-world/dead-world death loop.

## Build discipline

The Cloud Build hook rebuilds `Assets/Scenes/NorthernLands.unity`, `Assets/Scenes/DeadWorld.unity` and `Assets/Scenes/TowerOfGods.unity`, appends all three to Build Settings and then validates that `Startup` and `MainMenu` remain the first route. Cloud Build stays disabled until the branch has passed a compile/build validation, so free minutes are not spent on known-broken revisions.

## Next implementation block

1. Compile and build-validate both generated scenes without launching paid/free cloud minutes.
2. Add the first dungeon and boss encounter after the Tower of Gods trial.
3. Replace placeholder environment primitives with optimized modular art while retaining the generated layout and gameplay markers.
4. Add inventory presentation and race-specific starting abilities.
5. Spend one cloud-build slot only after validation is clean.
