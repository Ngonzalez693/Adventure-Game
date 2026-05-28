# Futuristic Protocol

A 4-player cooperative escape room set in a sci-fi simulation. Four crew members must coordinate across isolated rooms to solve four puzzles before time runs out — or die in the simulation.

---

## 📖 Plot

A space crew was sent on a mission and ended up trapped inside a **virtual simulation**. Although they appear to be in the same facility, each crew member is actually in their own isolated room, unable to physically reach the others. Each one has been assigned a different terminal with a unique objective.

The only way out is to **complete all four challenges within the time limit**, communicating purely by voice. Fail, and the simulation consumes them.

---

## 🎮 Features

- **Asymmetric 4-player cooperation** — every player has a unique role in each match
- **Random puzzle assignment** — roles are shuffled every game, no two matches feel the same
- **Four distinct puzzle types**:
  - **Code Terminal** — enter a 4-digit code based on a sequence of colors. Other players hold the colored digits.
  - **Pattern Lights** — flip 3 binary levers in the correct combination, guided by the player who can see the status lights.
  - **Memory Sequence** — watch a sequence of colors on a teammate's console, then dictate it to the solver.
  - **Pressure Valves** — keep a pressure gauge stable in the target range by toggling valves spread across the team.
- **LAN auto-discovery** — host on one machine, clients on the same network find the lobby automatically. No codes, no IP entry.
- **Global timer** with win / lose conditions
- **In-game HUD** with task list, player roles, and a live timer
- **Sound design**: ambient music, button feedback, valve creaks, error buzzes, victory chimes

---

## 🕹️ Controls

| Action | Key |
|---|---|
| Move forward | `W` |
| Rotate left / right | `A` / `D` |
| Turn around 180° | `S` |
| Interact | `E` |
| Options menu | `P` |

---

## 🌐 How to play

This is a **LAN-only** game (auto-discovery over UDP broadcast). All players must be on the same local network.

1. **Host** clicks **HOST** in the main menu
2. **Clients** click **CLIENT** — they'll automatically find and join the host's session
3. Once **4 players** are connected in the lobby, the host interacts with the central console to start the game
4. Each player spawns in their own room with a unique role
5. **Communicate by voice** (Discord, Zoom, in-person, etc.) to coordinate
6. Solve all 4 puzzles before the timer hits zero to win

> 💡 The game does **not** include built-in voice chat. Use any external voice software.

---

## 💻 Requirements

- **OS**: Windows 10 / 11 (64-bit)
- **CPU**: Any modern dual-core
- **RAM**: 4 GB minimum
- **GPU**: any DX11-capable card
- **Network**: All players on the same LAN
- **Players**: Exactly 4 (one host + three clients)

---

## 📦 Installation

1. Download **`FuturisticProtocol.zip`**
2. Extract the entire folder anywhere on your PC
3. Open the extracted folder and run **`Futuristic Protocol.exe`**

> ⚠️ **Don't** run the .exe from inside the .zip — extract the full folder first. The game needs `Futuristic Protocol_Data/` and `UnityPlayer.dll` next to the .exe.

### Firewall

On first launch Windows may ask you to allow network access. **Allow it on Private networks** (LAN). Without this, host/client auto-discovery is blocked.

---

## 🛠️ Built with

- **Unity** `6000.3.7f1`
- **Netcode for GameObjects** (networking)
- **Universal Render Pipeline** (URP)
- **Cinemachine** (camera follow)
- **TextMeshPro** (UI)
- Custom UDP LAN discovery layer

---

## 🎨 Credits

- **Development**: Nicolás González
- **3D models / textures**: Juliana Botina - Sara Figueroa
- **Sound design**: Laura Ríos
- **Original concept and narrative**: Laura Ríos

This project was developed as part of Videogame production class (USB Cali).

---

## 📂 Repository structure

```
Adventure_Puzzle/
├── Assets/             # Unity project assets
│   ├── Scripts/        # All gameplay scripts
│   ├── Scenes/         # MenuScene, Lobby, GameMap, LoadingScene
│   └── ...
├── ProjectSettings/    # Unity project settings
├── Packages/           # Unity package manifest
└── README.md
```

The `Library/`, `Temp/`, `Logs/` and other generated folders are excluded via `.gitignore`.

---

## 🐛 Known limitations

- LAN-only — no internet matchmaking
- No reconnection if a player disconnects mid-game (they have to be replaced)
- Voice chat is not built in
- The match always requires exactly 4 players to start
