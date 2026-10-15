# DEATH NOTE Cheater Within 📓⚡

> **Advanced Internal Tactical Introspection & Enhancement Mod for *DEATH NOTE Killer Within***

A high-performance internal mod built on **MelonLoader (IL2CPP)** targeting **Unity 2022.3.15f1 x64** and **Photon Unity Networking (PUN2)**. Features a 14-tab dark-glass GUI, real-time 2D/3D ESP, role guarantee lottery interceptors, proximity voice occlusion bypasses, level progression unlocks, and identity spoofers.

---

## 🌟 Key Feature Modules

### 👁️ Visuals & ESP
- **2D / 3D Bounding Boxes**: Dynamic perspective projection on all player and NPC models.
- **Snaplines**: Tactical directional vectors pointing toward all active participants.
- **Role & Distance Markers**: Color-coded role badges (Kira, L, Investigators, Terrorist).
- **Proximity Radar & Nameplates**: Direct world-space text overlays with custom color support.

### 🎭 Role Selection & Guarantee
- **Pre-Game Role Swapper**: Guarantees selected role (Kira, L, Watari, Near, Terrorist, etc.) during pre-game matchmaking.
- **Atomic Balance Preservation**: Resolves role conflicts dynamically with zero duplicate roles and zero match desync.
- **Anti-ID Grab Protection**: Prevents enemy Kiras from discovering or stealing your ID badge.

### 🔊 Audio Engine & Voice Chat Fixes
- **Occlusion Bypass**: Completely eliminates proximity voice cutouts caused by walls, corners, or line-of-sight raycasts.
- **Zero-Thrashing Channel Engine**: Eliminates periodic ~30s voice drops by locking listen groups to match state transitions.
- **Engine Recovery**: One-click recovery and reinitialization of Unity, CriWare, and Vivox audio subsystems.

### 🌐 Lobby & Matchmaking Tools
- **Bypass All Block / Ban Restrictions**: Join active rooms regardless of host/player blocklists.
- **Active Match Quick-Join**: Join in-progress matches as an active player or live spectator.
- **Multi-Platform Identity Spoofing**: Spoof PlayStation Network cross-play identity or rotate across custom 64-bit Steam ID pools.

### ⚡ Progression & Tactical Cheats
- **Instant Role & BattlePass Leveling**: Instant Level 10 role perks and BattlePass tier completion.
- **SpeedHack & NoClip**: Adjustable movement velocity and full collision bypass.
- **GodMode / Anti-Death**: Auto-revival from killer executions.
- **Custom Name & Color Engine**: Rich-text colored display names synchronized across Photon and local UI.

---

## 🎮 Hotkey Quick Reference

| Key | Action |
| :--- | :--- |
| **`[INSERT]`** | Toggle Tactical Menu GUI |
| **`[L]`** | Toggle Floating Hotkey Reference HUD |
| **`[P]`** | Export Live Game State & Reflection to Desktop (`dnkw_dump.txt`) |
| **`[`** | Force Re-scan Active Scene Entities & Roles |
| **`[F1]`** | Quick-Toggle Player ESP |
| **`[F2]`** | Quick-Toggle NoClip |
| **`[F9]`** | Emergency Panic / Safe State |

---

## 🛠️ Requirements & Building from Source

### Prerequisites
1. **Windows 10 / 11 (64-bit)**
2. **.NET 6.0 SDK** (or later) — [Download .NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)
3. **MelonLoader v0.7.3+ (Open-Beta)** installed into your *DEATH NOTE Killer Within* directory.
4. Launch the game once with MelonLoader installed to generate all required `Il2CppAssemblies`.

### Build Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/<your-username>/DNKW.git
   cd DNKW
   ```

2. **Compile the Release DLL**:
   - Double-click `build.bat` or run:
     ```powershell
     dotnet build -c Release
     ```
   - If your game is located in a custom folder, pass the `GameDir` property:
     ```powershell
     dotnet build -c Release -p:GameDir="<PathToGameDirectory>"
     ```

3. **Deploy**:
   Copy the compiled binary to your game's `Mods` folder:
   ```powershell
   Copy-Item "bin\Release\net6.0\DeathNoteCheaterWithin.dll" -Destination "<PathToGameDirectory>\Mods\"
   ```

---

## ⚙️ Configuration & Customization

The mod automatically generates persistent configuration files in `<GameDir>\UserData\`:
- **`inkwell_config.cfg`**: Saves all active toggles, colors, slider values, and keybinds.
- **`steamidspoof.txt`**: List of custom 64-bit Steam IDs for rotation (one per line).

---

## 📜 Disclaimer
This software is intended strictly for reverse engineering research, educational purposes, and private game introspection. Use at your own discretion.
