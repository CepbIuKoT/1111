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

## Build discipline

The Cloud Build hook rebuilds `Assets/Scenes/NorthernLands.unity`, appends it to Build Settings and then validates that `Startup` and `MainMenu` remain the first route. No cloud build should be launched until combat targets, health/feedback, loot and a full save/load pass are connected.

## Next implementation block

1. Add real enemy prefabs with local AI and navigation.
2. Add damage, hit reactions, health bars and death/respawn.
3. Add loot pickup and the first Riverholm quest.
4. Activate the Dead World portal only after the first quest gate.
5. Run one compile/build validation, fix all errors, then spend a single cloud-build slot.
