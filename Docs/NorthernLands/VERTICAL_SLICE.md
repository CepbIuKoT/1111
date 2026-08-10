# Riverholm vertical slice

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

## Build discipline

The Cloud Build hook rebuilds `Assets/Scenes/NorthernLands.unity`, appends it to Build Settings and then validates that `Startup` and `MainMenu` remain the first route. No cloud build should be launched until the first quest, portal gate and a full save/load pass are connected.

## Next implementation block

1. Connect northern-silver pickups to the persistent inventory and first Riverholm quest.
2. Add a jarl NPC, quest dialogue and an objective marker.
3. Activate the Dead World portal only after the first quest gate.
4. Persist the hero's position, health and cleared objective state.
5. Run one compile/build validation, fix all errors, then spend a single cloud-build slot.
