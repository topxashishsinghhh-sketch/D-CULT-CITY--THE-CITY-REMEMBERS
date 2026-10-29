# D-CULT CITY – The City That Remembers

A 3D AI-driven open-world simulation built in **Unity (C#, Universal Render Pipeline - URP)** for Desktop (Windows/macOS) and Mobile (Android/iOS).

NPCs in D-Cult City remember the player's actions, spread reputation through gossip, and alter their behavior, dialogues, and missions based on trust and faction standing.

---

## 🌟 Key Features

* **AI Memory & Trust System:**
  * Central generic decoupled **EventBus** (`Publish` / `Subscribe<T>`).
  * Action events capture **who**, **what**, **where**, **when**, and **witnesses** with reputation and trust deltas.
  * **NPCMemory** stores events with decay over time; vivid events fade slower.
  * **TrustSystem** categorizes relationships into **Trusted (+0.5 to +1.0)**, **Neutral (0.0 to +0.5)**, **Suspicious (-0.5 to 0.0)**, and **Hostile (-1.0 to -0.5)**.
  * **GossipSystem** spreads memory between nearby NPCs every 8 seconds within a 12m radius.

* **50 Total NPCs:**
  * **10 Story NPCs** with individual daily routines (schedules), branching trust-gated dialogue trees, and memory-driven reactions:
    1. **Elena** – Florist (Central Park)
    2. **Officer Marcus** – Patrol Officer (Police Precinct)
    3. **Victor** – Casino Floor Manager (Grand Lux Casino)
    4. **Layla** – Food Cart Vendor (Metro Shopping Plaza)
    5. **Danny** – Gangster Enforcer (Back Alley)
    6. **Rosa** – Park Jogger / Witness (Fountain Trail)
    7. **Sam** – Mechanic & Racer (Roadside Garage)
    8. **Chief Henderson** – Veteran Police Chief (Precinct Courtyard)
    9. **Boss Vance** – Syndicate Kingpin (Casino VIP Lounge)
    10. **Dr. Aris** – Resident Botanist & Wildlife Vet (Park / Forest Gate)
  * **40 Ambient Crowd NPCs** with procedural wandering, dynamic crowd reactions (stare, flee, or call police), and simple trust tracking.

* **Open World Biomes & Highways:**
  * **Downtown City:** Grand Lux Casino, Precinct 4 Police Station, Metro Shopping Plaza, Skyline Suites, and Central Park with fountain.
  * **Coastal Beach:** 80m golden sandy shore, ocean water, and boardwalk wooden pier.
  * **Deep Pine Forest:** Clustered pine trees, clearings, and wildlife sanctuary.
  * **Mountain Perimeter Range:** 7 rocky peaks with snow caps encircling the north/east boundary.
  * **Highways:** Paved roads linking Downtown to the Beach, Forest, and Mountain pass.

* **Physics-Driven Vehicles:**
  * Four-wheel arcade physics using Unity's `WheelCollider` and `Rigidbody`.
  * Real-time RWD torque, speed-sensitive Ackermann steering, handbrake drift, and automatic brake lamps.
  * Full enter/exit mechanics with smart camera follow and dedicated mobile touch driving HUD (Gas, Reverse, Steer, Brake).

* **Storylines & Missions:**
  * **Police Storyline:** *Thin Blue Line* (gather docks evidence) & *Operation Lockdown* (disable gang lookouts).
  * **Gangster Storyline:** *First Contact* (contraband lockup delivery) & *The Takeover* (extort commercial stalls & pressure casino).
  * **Side Quests:** *Street Legend* (4-checkpoint circuit in under 90s), *Hot Delivery* (3-minute supply run), *Wild Heart* (guide injured forest deer), *High Roller* (roulette challenge).

* **Living Atmosphere & Mini-Games:**
  * **24-Hour Day/Night Cycle:** Synchronized sun rotation, ambient color grading (golden dawn, noon sun, dusk amber, moonlight), and automatic dusk toggling of streetlights and vehicle headlights.
  * **Grand Lux Casino Roulette Mini-Game:** Interactive roulette table betting on Red (1:1), Black (1:1), or Green Zero (35:1) with cash bankroll.
  * **Police Pursuit System:** Flashing red & blue cruiser interceptor with sirens chasing criminals; evasion mechanics (break away for 12s to clear Wanted stars).
  * **Procedural Audio Engine:** Runtime audio synthesis for engine hums, sirens, footsteps, casino win chimes, and crime alerts without external assets.

* **Cross-Platform User Flow & Save System:**
  * Flow: Splash → Login (Guest or Email) → Main Menu → Character Creator (gender, skin tone, hair style/color, clothing palettes) → Game.
  * Full JSON persistence (3 slots + 120s auto-save) storing player state, appearance, faction scores, and all NPC memory entries with decay strength.
  * Low / Medium / High graphics presets (LOD bias, shadow distance, 30/60/120 FPS targets) and distance-based NPC culling for optimal mobile battery life.

---

## 🎮 Controls

| Action | Desktop | Mobile |
| :--- | :--- | :--- |
| **Move** | `W`, `A`, `S`, `D` / Arrow Keys | Left Virtual Joystick |
| **Sprint** | Hold `Left Shift` | `RUN / WALK` Button |
| **Jump** | `Spacebar` | `JUMP` Button |
| **Camera Orbit** | Right Mouse Button + Drag | Drag on Right Half of Screen |
| **Camera Zoom** | Mouse Scroll Wheel | Two-Finger Pinch |
| **Talk to NPC** | Press `E` near NPC | Tap `[E] TALK` Button |
| **Enter / Exit Car** | Press `F` near Car | Tap `[F] CAR` Button |
| **Drive Controls** | `W` (Gas), `S` (Brake/Rev), `A`/`D` (Steer), `Space` (Handbrake) | `GAS▲`, `REV▼`, Steer Pad, `BRAKE` |
| **Title Menu** | Press `Esc` or Click `TITLE MENU` | Tap `MENU` Button |
| **Missions Journal** | Click `Missions ▶` (bottom-right) | Tap `Missions ▶` Button |
| **Quick-Save / Load** | `F5` (QuickSave) / `F9` (QuickLoad) | `MENU` → `SAVE` / `LOAD` |
| **Customize Appearance** | Press `C` | Tap `CUSTOMIZE` |
| **Play Roulette** | Press `E` at Casino Table | Tap `[E] TALK` at Table |

---

## 🛠️ Project Structure

```
Assets/
├── Scenes/
│   └── TestCity_Milestone1.unity
└── Scripts/
    ├── Core/          (EventBus, SaveLoadManager, QualitySettings, ProceduralAudioEngine, SceneBootstrap)
    ├── Player/        (CharacterCustomization, CharacterVisual, PlayerInputReader, ThirdPersonController, Camera)
    ├── Vehicles/      (VehicleDefinition, VehicleController, VehicleVisual, VehicleMobileHUD)
    ├── NPC/           (NPCBase, NPCMemory, TrustSystem, Schedules, DialogueTree, StoryNPC, AmbientNPC, DistanceCuller)
    ├── Reputation/    (FactionReputation, PolicePursuitManager)
    ├── Missions/      (MissionManager, MissionPolice, MissionGangster, SideQuests)
    ├── World/         (FullWorldBuilder, TestCityVehicleSpawner, DayNightCycle, CasinoMiniGame)
    ├── Wildlife/      (AnimalBase, WildlifeSpawner)
    └── UI/            (GameFlowManager, MainMenuUI, CharacterCreatorUI, MobileControlsOverlay, MissionUI, SaveLoadUI)
```

---

## 🚀 How to Run in Unity

1. Open **Unity Hub** and select **Add > Add project from disk**.
2. Select this repository folder.
3. Open with **Unity 2022.3 LTS** (or newer).
4. In the Project window, double-click `Assets/Scenes/TestCity_Milestone1.unity`.
5. Press **Play**!
