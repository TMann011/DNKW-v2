using System;
using DeathNoteCheaterWithin.Drawing;
using DeathNoteCheaterWithin.Features.Combat;
using DeathNoteCheaterWithin.Features.Dev;
using DeathNoteCheaterWithin.Features.Game;
using DeathNoteCheaterWithin.Features.Misc;
using DeathNoteCheaterWithin.Features.Movement;
using DeathNoteCheaterWithin.Features.Roles;
using Il2Cpp;
using Il2CppGame;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.UI
{
    public static class TacticalMenu
    {
        private static int _currentTab = 0;
        private static readonly string[] Tabs = {
            "Info", "ESP", "Move", "Combat", "Roles", "Game", "Players", "Dev", "Fixes", "Self", "Global", "Exploit", "Settings", "Support"
        };

        private static bool _isDragging = false;
        private static Vector2 _dragOffset;
        private static bool _isResizing = false;

        public static void Draw()
        {
            if (!Config.MenuOpen) return;

            HandleInputEvents();

            float w = Mathf.Max(Config.MenuSize.x, 600f);
            float h = Mathf.Max(Config.MenuSize.y, 480f);

            // Background Panel
            Color oldBg = GUI.backgroundColor;
            Color oldCol = GUI.color;

            GUI.backgroundColor = Config.MenuBg;
            GUI.Box(new Rect(Config.MenuPos.x, Config.MenuPos.y, w, h), GUIContent.none);

            // Title Bar
            GUI.color = Config.MenuText;
            GUI.Box(new Rect(Config.MenuPos.x, Config.MenuPos.y, w, 28f),
                "<b><color=#FFFFFF>DEATH NOTE Cheater Within v3.0</color> <color=#33FFAA>[PRO TACTICAL IL2CPP]</color> <color=#BBBBBB>Inkwell</color></b>");

            // Tab Navigation Grid (2 rows)
            int tabsPerRow = 7;
            float tabH = 24f;
            float tabW = (w - 20f) / tabsPerRow;

            for (int i = 0; i < Tabs.Length; i++)
            {
                int row = i / tabsPerRow;
                int col = i % tabsPerRow;
                float tx = Config.MenuPos.x + 10f + (col * tabW);
                float ty = Config.MenuPos.y + 32f + (row * (tabH + 3f));

                GUI.backgroundColor = (i == _currentTab) ? Config.TabActive : Config.TabInactive;
                GUI.color = Config.MenuText;

                string label = (i == _currentTab) ? $"<b><color=#00FFB0>[{Tabs[i]}]</color></b>" : Tabs[i];
                if (GUI.Button(new Rect(tx, ty, tabW - 2f, tabH), label))
                {
                    _currentTab = i;
                }
            }

            GUI.backgroundColor = oldBg;
            GUI.color = oldCol;

            // Content Panel Area
            float contentTop = Config.MenuPos.y + 88f;
            float contentH = h - 114f;
            Rect contentBox = new Rect(Config.MenuPos.x + 10f, contentTop, w - 20f, contentH);

            GUI.backgroundColor = Config.ContentBg;
            GUI.Box(contentBox, GUIContent.none);
            GUI.backgroundColor = oldBg;

            UiBuilder ui = new UiBuilder(new Rect(contentBox.x + 8f, contentBox.y + 8f, contentBox.width - 16f, contentBox.height - 16f));

            try
            {
                switch (_currentTab)
                {
                    case 0: DrawInfoTab(ui); break;
                    case 1: DrawEspTab(ui); break;
                    case 2: DrawMoveTab(ui); break;
                    case 3: DrawCombatTab(ui); break;
                    case 4: DrawRolesTab(ui); break;
                    case 5: DrawGameTab(ui); break;
                    case 6: TabPlayerList.Draw(ui); break;
                    case 7: DrawDevTab(ui); break;
                    case 8: DrawFixesTab(ui); break;
                    case 9: DrawSelfTab(ui); break;
                    case 10: DrawGlobalTab(ui); break;
                    case 11: DrawExploitTab(ui); break;
                    case 12: DrawSettingsTab(ui); break;
                    case 13: DrawSupportTab(ui); break;
                }
            }
            catch (Exception ex)
            {
                ui.Label($"<color=#FF3333>Tab execution error: {ex.Message}</color>");
            }

            // Footer Status Bar
            GUI.Label(new Rect(Config.MenuPos.x + 12f, Config.MenuPos.y + h - 22f, w - 40f, 20f),
                "<color=#888888>[INSERT] Menu | [L] Keys | [P] Dump | v3.0 Native Zero-Strip | Inkwell</color>");

            // Resize Grip (bottom-right corner)
            GUI.Box(new Rect(Config.MenuPos.x + w - 16f, Config.MenuPos.y + h - 16f, 16f, 16f), "<color=#666666>//</color>");
        }

        private static void HandleInputEvents()
        {
            Event e = Event.current;
            if (e == null) return;

            float w = Mathf.Max(Config.MenuSize.x, 600f);
            float h = Mathf.Max(Config.MenuSize.y, 480f);

            Rect headerRect = new Rect(Config.MenuPos.x, Config.MenuPos.y, w - 24f, 28f);
            Rect resizeRect = new Rect(Config.MenuPos.x + w - 18f, Config.MenuPos.y + h - 18f, 18f, 18f);

            if (e.type == EventType.MouseDown)
            {
                if (headerRect.Contains(e.mousePosition))
                {
                    _isDragging = true;
                    _dragOffset = e.mousePosition - Config.MenuPos;
                    e.Use();
                }
                else if (resizeRect.Contains(e.mousePosition))
                {
                    _isResizing = true;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
                _isResizing = false;
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (_isDragging)
                {
                    Config.MenuPos = e.mousePosition - _dragOffset;
                    e.Use();
                }
                else if (_isResizing)
                {
                    Config.MenuSize = new Vector2(
                        Mathf.Max(600f, e.mousePosition.x - Config.MenuPos.x),
                        Mathf.Max(480f, e.mousePosition.y - Config.MenuPos.y)
                    );
                    e.Use();
                }
            }
        }

        private static void DrawInfoTab(UiBuilder ui)
        {
            ui.Header("Match & Network Status");
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null)
            {
                ui.Label("<color=#FFAA00>Waiting for match / In Lobby or Title Screen.</color>");
                ui.Label("Managers found: GameStateManager=No");
            }
            else
            {
                PlayerMovement myPlayer = gsm.MyPlayer;
                string pName = myPlayer != null ? myPlayer.GetPlayerName(false, true) : "Unknown";
                string roleName = myPlayer != null ? myPlayer.GetRoleName(false, true) : "None";
                int pCount = gsm.PlayerList != null ? gsm.PlayerList.Count : 0;

                ui.Label($"<b>Game State:</b> <color=#55FFAA>{gsm.State}</color> | <b>Total Players:</b> <color=#55FFAA>{pCount}</color>");
                ui.Label($"<b>Local Player:</b> <color=#FFFFFF>{pName}</color> (Actor #{myPlayer?.ActorID}) | <b>True Role:</b> <color=#FFD700>{roleName}</color>");
                ui.Label($"<b>Status:</b> {(myPlayer != null && myPlayer.IsDead ? "<color=#FF3333>DEAD</color>" : "<color=#33FF55>ALIVE</color>")} | <b>Death Note In Hand:</b> {(myPlayer != null && myPlayer.HasDeathNote ? "YES" : "NO")}");
            }

            ui.Space(8f);
            ui.Header("Quick Hotkeys Reference (Press [L] for Full HUD)");
            ui.Label("  <b>[INSERT]</b> - Toggle Mod Menu");
            ui.Label("  <b>[L]</b> - Toggle Floating Keybind HUD");
            ui.Label("  <b>[P]</b> - Export Full Game State to Desktop");
            ui.Label("  <b>[ [ ]</b> - Force Refresh Roles & Entity Cache");
            ui.Label("  <b>[F1]</b> - Master ESP Toggle | <b>[F2]</b> - NoClip Flight");
            ui.Label("  <b>[F3]</b> - Speed Multiplier | <b>[F4]</b> - Always-Kill Bypass");
            ui.Label("  <b>[F5]</b> - 100% ID Theft Aura | <b>[F6]</b> - Inject Names into Notebook");
            ui.Label("  <b>[F9]</b> - Execute All Enemies | <b>[F10]</b> - Force Kira Win");
        }

        private static void DrawEspTab(UiBuilder ui)
        {
            ui.Header("ESP Master & Visual Introspection");
            ui.Toggle(ref Config.EspEnabled, " Enable Master Visual ESP");
            ui.Space(4f);

            ui.Header("Player ESP Primitives");
            ui.BeginRow(22f);
            ui.RowToggle(ref Config.EspBoxes2D, " 2D Boxes", 140f);
            ui.RowToggle(ref Config.EspBoxes3D, " 3D World Boxes", 150f);
            ui.RowToggle(ref Config.EspSnaplines, " Snaplines", 140f);
            ui.EndRow();

            ui.BeginRow(22f);
            ui.RowToggle(ref Config.EspShowRoles, " Exact Roles", 140f);
            ui.RowToggle(ref Config.EspShowDistance, " Distance (m)", 150f);
            ui.RowToggle(ref Config.EspShowItems, " Item Badges", 140f);
            ui.EndRow();

            ui.Toggle(ref Config.EspShowStatus, " Transceiver Voice / Communication State Indicator");
            ui.Space(4f);

            ui.Slider("Maximum ESP Render Distance", ref Config.EspMaxDistance, 20f, 500f, "F0");
            ui.Space(6f);

            ui.Header("World, Entities & Objective ESP");
            ui.BeginRow(22f);
            ui.RowToggle(ref Config.NpcEsp, " NPC Markers", 140f);
            ui.RowToggle(ref Config.NpcCorpseEsp, " Dead Corpses", 150f);
            ui.RowToggle(ref Config.NpcEliteOnly, " Elite NPCs Only", 160f);
            ui.EndRow();

            ui.BeginRow(22f);
            ui.RowToggle(ref Config.TaskEsp, " Task Objectives", 140f);
            ui.RowToggle(ref Config.CameraEsp, " Security Cameras", 150f);
            ui.EndRow();
        }

        private static void DrawMoveTab(UiBuilder ui)
        {
            ui.Header("Locomotion & Physics Bypasses");
            ui.BeginRow(24f);
            ui.RowToggle(ref Config.SpeedHack, " Speed Multiplier", 160f);
            ui.RowToggle(ref Config.NoClip, " True NoClip (WASD/Space/Shift)", 250f);
            ui.RowToggle(ref Config.AntiSlowdown, " Anti-Slowdown (Zero Hindrance)", 240f);
            ui.EndRow();
            ui.Space(4f);

            ui.Slider("Speed Hack Multiplier", ref Config.SpeedMultiplier, 1.0f, 10.0f, "F1");
            ui.Slider("NoClip Flight Speed", ref Config.NoClipSpeed, 2.0f, 50.0f, "F1");
            ui.Space(6f);

            ui.Header("Teleportation & Instant Warping");
            ui.BeginRow(28f);
            if (ui.RowButton("Dash Forward (5m)", 150f)) TeleportManager.TeleportForward(5.0f);
            if (ui.RowButton("Dash Forward (15m)", 150f)) TeleportManager.TeleportForward(15.0f);
            if (ui.RowButton("Dash Forward (30m)", 150f)) TeleportManager.TeleportForward(30.0f);
            ui.EndRow();

            ui.BeginRow(28f);
            if (ui.RowButton("Teleport to Crosshair / Surface Raycast", 280f)) TeleportManager.TeleportToCrosshair();
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Camera Optics & Field of View");
            ui.Toggle(ref Config.CustomFovEnabled, " Override Camera Field of View (FOV)");
            if (Config.CustomFovEnabled)
            {
                ui.Slider("Camera Field of View", ref Config.CustomFov, 60f, 140f, "F0");
            }
        }

        private static void DrawCombatTab(UiBuilder ui)
        {
            ui.Header("Death Note & Rule Manipulation");
            ui.Toggle(ref Config.DeathNoteAlwaysKill, " Always-Kill Bypass (Bypass cooldown, range & cantKillType)");
            ui.Toggle(ref Config.InstantIdSteal, " Instant 100% ID Gather Aura (Auto-harvest nearby IDs)");
            ui.Toggle(ref Config.AntiIdGrab, " Anti-ID Grab Protection (Prevent enemy players from stealing your ID)");
            ui.Space(6f);

            ui.BeginRow(28f);
            if (ui.RowButton("Add All Names into Death Note", 240f)) InstantNames.AddAllNamesToDeathNote();
            if (ui.RowButton("Instant Max Info Gauge", 190f)) InstantIdSteal.MaximizeInfoProgress();
            ui.EndRow();
            ui.Space(8f);

            ui.Header("Mass Executioners & Police Authority");
            ui.BeginRow(30f);
            if (ui.RowButton("Execute All (Heart Attack) (H)", 210f)) Executioner.ExecuteAll(true, EDeathType.HeartAttack);
            if (ui.RowButton("Execute All (Truck) (H)", 175f)) Executioner.ExecuteAll(true, EDeathType.Truck);
            if (ui.RowButton("Execute All (Gun) (H)", 165f)) Executioner.ExecuteAll(true, EDeathType.Gun);
            ui.EndRow();
        }

        private static void DrawRolesTab(UiBuilder ui)
        {
            ui.Header("Role Espionage & Shinigami Vision");
            ui.Toggle(ref Config.RevealRoles, " Reveal All Roles in Real-Time");
            ui.Toggle(ref Config.UnlockAllAbilities, " Unlock All Role Perks & Passive Abilities");
            ui.Toggle(ref Config.RemVision, " Infinite Rem Aura Vision (Shinigami Eyes)");
            ui.Space(4f);

            ui.Header("Guarantee Role Pick Enforcer");
            ui.Toggle(ref Config.GuaranteeRole, " Enable Guarantee Selected Role Pick (Pre-Game Auto-Select & Balance Swap)");
            ui.BeginRow(24f);
            for (int r = 0; r < GuaranteeRolePick.RoleNames.Length; r++)
            {
                string rName = GuaranteeRolePick.RoleNames[r];
                bool isSel = Config.SelectedRoleIndex == r;
                if (ui.RowButton(isSel ? $"<b><color=#00FFAA>[{rName}]</color></b>" : rName, 125f))
                {
                    Config.SelectedRoleIndex = r;
                }
                if (r == 4)
                {
                    ui.EndRow();
                    ui.BeginRow(24f);
                }
            }
            ui.EndRow();
            ui.Space(4f);

            if (ui.Button("Force Refresh Role Names & Badges", 260f, 26f))
            {
                RoleRevealer.RefreshNames();
                RemVision.EnableAllVisions();
            }
            ui.Space(6f);

            ui.Header("Instant Role Switcher (Host Authority) (H)");
            ui.BeginRow(26f);
            if (ui.RowButton("Kira", 90f)) RoleSwitcher.SwitchRole(EPlayerRoles.Killer, EPlayerRoleDetail.Killer);
            if (ui.RowButton("Kira Helper", 110f)) RoleSwitcher.SwitchRole(EPlayerRoles.Killer, EPlayerRoleDetail.KillerHelp);
            if (ui.RowButton("X-Kira (2nd Kira)", 140f)) RoleSwitcher.SwitchRole(EPlayerRoles.Killer, EPlayerRoleDetail.Killer_2);
            if (ui.RowButton("2nd Kira Helper", 130f)) RoleSwitcher.SwitchRole(EPlayerRoles.Killer, EPlayerRoleDetail.KillerHelp_2);
            ui.EndRow();

            ui.BeginRow(26f);
            if (ui.RowButton("L (Detective)", 110f)) RoleSwitcher.SwitchRole(EPlayerRoles.Detective, EPlayerRoleDetail.Detective);
            if (ui.RowButton("Watari (L Helper)", 140f)) RoleSwitcher.SwitchRole(EPlayerRoles.Detective, EPlayerRoleDetail.Detective_Help);
            if (ui.RowButton("Near (2nd Detective)", 150f)) RoleSwitcher.SwitchRole(EPlayerRoles.Detective, EPlayerRoleDetail.Detective_2);
            if (ui.RowButton("Investigator", 110f)) RoleSwitcher.SwitchRole(EPlayerRoles.Detective, EPlayerRoleDetail.Investigator);
            ui.EndRow();

            ui.BeginRow(26f);
            if (ui.RowButton("Strong Invest.", 120f)) RoleSwitcher.SwitchRole(EPlayerRoles.Detective, EPlayerRoleDetail.Investigator_Strong);
            if (ui.RowButton("Mello (Terrorist)", 140f)) RoleSwitcher.SwitchRole(EPlayerRoles.Terrorist, EPlayerRoleDetail.Terrorist);
            if (ui.RowButton("Spectator", 100f)) RoleSwitcher.SwitchRole(EPlayerRoles.Spectator, EPlayerRoleDetail.Spectator);
            ui.EndRow();
        }

        private static void DrawGameTab(UiBuilder ui)
        {
            ui.Header("Level, EXP & Progression Manipulator");
            ui.BeginRow(28f);
            if (ui.RowButton("+500 Local EXP", 130f)) LevelManager.AddPlayerExp(500f);
            if (ui.RowButton("+2500 Local EXP", 140f)) LevelManager.AddPlayerExp(2500f);
            if (ui.RowButton("+10,000 EXP", 120f)) LevelManager.AddPlayerExp(10000f);
            if (ui.RowButton("+50k Pass EXP", 120f)) LevelManager.AddPlayerExp(50000f);
            ui.EndRow();
            ui.Space(4f);

            ui.BeginRow(28f);
            if (ui.RowButton("Instant Max Level (Lv 10 / Pass 100)", 260f)) LevelManager.SetInstantMaxLevel();
            if (ui.RowButton("EXP to All Players (H)", 180f)) LevelManager.AddExpToAll(2500f);
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Title Screen Matchmaking Filters & Lobby Search");
            ui.Toggle(ref Config.BypassBlockRestriction, " Bypass All Block / Ban Restrictions (Join rooms even when blocked)");
            ui.Toggle(ref Config.ShowActiveLobbies, " Show Active In-Progress Matches in Room Browser");
            ui.Toggle(ref Config.JoinActiveLobbies, " Force Allow Joining Active / Full Matches");
            ui.Space(4f);

            ui.BeginRow(28f);
            if (ui.RowButton("Quick-Join Open Lobby", 175f)) LobbyHacks.ForceJoinActiveLobby(false, false);
            if (ui.RowButton("Quick-Join Active Match (In-Progress)", 255f)) LobbyHacks.ForceJoinActiveLobby(true, false);
            if (ui.RowButton("Quick-Join Active (Spectator)", 195f)) LobbyHacks.ForceJoinActiveLobby(true, true);
            ui.EndRow();

            ui.BeginRow(26f);
            if (ui.RowButton("Scan Waiting Room", 150f)) LobbyHacks.ScanWaitingLobbyPlayers();
            if (ui.RowButton("Force Start Match (H)", 175f)) LobbyHacks.ForceStartMatch();
            ui.EndRow();
            ui.Space(4f);

            Config.TargetRoomCode = ui.TextField("Room Code:", Config.TargetRoomCode ?? "", 24f);
            ui.BeginRow(26f);
            if (ui.RowButton("Join as Player", 130f)) LobbyHacks.JoinPrivateRoom(Config.TargetRoomCode, false);
            if (ui.RowButton("Join Full/Active as Spectator", 230f)) LobbyHacks.JoinPrivateRoom(Config.TargetRoomCode, true);
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Steam Friends In-Game Sessions");
            ui.BeginRow(26f);
            if (ui.RowButton("Refresh Steam Friends Playing DNKW", 270f)) LobbyHacks.RefreshFriendsList();
            ui.EndRow();
            ui.Space(4f);

            if (LobbyHacks.OnlineFriends.Count == 0)
            {
                ui.Label("<color=#888888><i>Click 'Refresh' above to query online friends playing DNKW</i></color>");
            }
            else
            {
                for (int f = 0; f < LobbyHacks.OnlineFriends.Count; f++)
                {
                    var friend = LobbyHacks.OnlineFriends[f];
                    ui.BeginRow(24f);
                    string codeDisp = !string.IsNullOrEmpty(friend.RoomCode) ? $"[Room: {friend.RoomCode}]" : "[In Game]";
                    ui.Label($"<b>{friend.Name}</b> <color=#00FFAA>{codeDisp}</color>");
                    if (ui.RowButton("Join Player", 100f)) LobbyHacks.JoinFriendGame(friend, false);
                    if (ui.RowButton("Spectate", 90f)) LobbyHacks.JoinFriendGame(friend, true);
                    ui.EndRow();
                }
            }
            ui.Space(6f);

            ui.Header("Meeting & Voting Exploits");
            ui.Toggle(ref Config.VoteManyTimes, " Vote Multiple Times (Bypass 1-vote limit)");
            ui.Space(4f);

            if (ui.Button("Force Call Emergency Meeting (H)", 260f, 28f))
            {
                MeetingHacks.CallEmergencyMeeting();
            }
            ui.Space(6f);

            ui.Header("Team Progression & Objectives");
            ui.BeginRow(28f);
            if (ui.RowButton("Complete Active Task", 180f)) TaskHacks.CompleteActiveTask();
            if (ui.RowButton("Add +10% Progress (H)", 170f)) TaskHacks.AddTeamProgress(10f);
            if (ui.RowButton("Add +25% Progress (H)", 170f)) TaskHacks.AddTeamProgress(25f);
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Match Outcome Control");
            ui.BeginRow(30f);
            if (ui.RowButton("Force Kira Victory (H)", 175f)) InstantWin.WinAsKira();
            if (ui.RowButton("Force L Victory (H)", 165f)) InstantWin.WinAsL();
            if (ui.RowButton("Force Terrorist Victory (H)", 195f)) InstantWin.WinAsTerrorist();
            ui.EndRow();
        }

        private static void DrawDevTab(UiBuilder ui)
        {
            ui.Header("Developer Inspection & Diagnostics");
            ui.Toggle(ref Config.ShowDebugOverlay, " Live On-Screen Debug Telemetry Overlay (Show Real-time State & FPS)");
            ui.Space(4f);
            ui.BeginRow(28f);
            if (ui.RowButton("Dump State to Desktop (P)", 220f)) StateDumper.DumpStateToDesktop();
            if (ui.RowButton("Dump Reflection to Log", 200f)) StateDumper.DumpGameStateManagerReflection();
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Entity & Authority Control");
            ui.BeginRow(28f);
            if (ui.RowButton("Kill Self", 110f))
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null && gsm.MyPlayer != null) Executioner.ExecutePlayer(gsm.MyPlayer, EDeathType.HeartAttack);
            }
            if (ui.RowButton("Kill All Others (H)", 160f)) Executioner.ExecuteAll(true, EDeathType.HeartAttack);
            if (ui.RowButton("Revive Self", 120f)) Exploits.ReviveSelf();
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Custom Name & Universal Color Picker (Shows for All)", 24f);
            ui.Toggle(ref Config.EnableDevNameColor, " Enable Custom Name Color (Broadcasts color to all players)");
            ui.Space(4f);

            string currentNameInput = Config.CustomPlayerName ?? "";
            string newNameInput = ui.TextField("Custom Display Name:", currentNameInput, 24f);
            if (newNameInput != currentNameInput)
            {
                Config.CustomPlayerName = newNameInput;
                Config.SaveConfig();
            }
            ui.Space(4f);

            string previewName = !string.IsNullOrEmpty(Config.CustomPlayerName) ? Config.CustomPlayerName : "Player";
            ui.Label($"Live Name Tag Preview: <b><color={Config.CustomNameColorHex}>{previewName}</color></b>   (Hex: <color=#00FFAA>{Config.CustomNameColorHex}</color>)");
            ui.Space(4f);

            // Preset Swatches
            ui.Header("Quick Color Presets (16 Vibrant Themes - 1-Click Apply)");
            ui.BeginRow(24f);
            for (int p = 0; p < CustomNameColor.Presets.Length; p++)
            {
                var preset = CustomNameColor.Presets[p];
                if (ui.RowButton($"<color={preset.Hex}>{preset.Name}</color>", 125f))
                {
                    Config.EnableDevNameColor = true;
                    CustomNameColor.SetColorHex(preset.Hex);
                    CustomNameColor.ApplyAndRefresh();
                }
                if (p % 4 == 3 && p < CustomNameColor.Presets.Length - 1)
                {
                    ui.EndRow();
                    ui.BeginRow(24f);
                }
            }
            ui.EndRow();
            ui.Space(6f);

            // RGBA Color Picker Slider
            ui.Header("Interactive RGBA Color Slider");
            Color oldCol = Config.CustomNameColor;
            Color newCol = ui.ColorSlider("Color Mixer (R / G / B / A)", oldCol);
            if (newCol != oldCol)
            {
                Config.CustomNameColor = newCol;
                Config.NameColorR = newCol.r;
                Config.NameColorG = newCol.g;
                Config.NameColorB = newCol.b;
                Config.CustomNameColorHex = "#" + ColorUtility.ToHtmlStringRGB(newCol);
                Config.SaveConfig();
            }
            ui.Space(4f);

            // Manual Hex Input Field
            string currentHex = Config.CustomNameColorHex ?? "#00FFAA";
            string newHex = ui.TextField("Manual Hex Code (#RRGGBB):", currentHex, 24f);
            if (newHex != currentHex)
            {
                CustomNameColor.SetColorHex(newHex);
            }
            ui.Space(4f);

            ui.BeginRow(26f);
            if (ui.RowButton("Apply & Sync to Network", 190f))
            {
                CustomNameColor.ApplyAndRefresh();
            }
            if (ui.RowButton("Apply [DEV] Staff Preset", 180f))
            {
                CustomNameColor.EnableDevMode();
            }
            ui.EndRow();
            ui.Space(8f);

            ui.Header("Steam & Platform ID Spoofing Matrix");
            ui.Toggle(ref Config.SpoofPlayStation, " Spoof Platform as PlayStation (Display PS Icon / PSN Account to network)");
            ui.Label($"<b>Active Steam ID:</b> <color=#55FFAA>{Spoofers.ActiveSpoofedSteamId}</color> (Pool: {Spoofers.AvailableSteamIds.Count} IDs)");
            ui.Label($"<b>Original ID Backup:</b> <color=#FFD700>{(string.IsNullOrEmpty(Spoofers.OriginalSteamId) ? "Not captured yet" : Spoofers.OriginalSteamId)}</color>");
            ui.Label($"<b>Active PSN ID:</b> <color=#00DDFF>{Spoofers.ActivePsnId}</color>");

            ui.BeginRow(26f);
            if (ui.RowButton("<< Prev Steam ID", 140f)) Spoofers.PrevSteamId();
            if (ui.RowButton("Next Steam ID >>", 140f)) Spoofers.NextSteamId();
            if (ui.RowButton("Randomize ID", 110f)) Spoofers.RandomizeSteamId();
            if (ui.RowButton("Revert Original", 120f)) Spoofers.RevertToOriginal();
            ui.EndRow();

            ui.BeginRow(26f);
            if (ui.RowButton("<< Prev PSN", 110f)) Spoofers.NextPsnId();
            if (ui.RowButton("Next PSN >>", 110f)) Spoofers.PrevPsnId();
            if (ui.RowButton("View My Steam Profile", 170f)) Spoofers.ViewSteamProfile(Spoofers.ActiveSpoofedSteamId);
            if (ui.RowButton("View My PSN Profile", 170f)) Spoofers.ViewPsnProfile(Spoofers.ActivePsnId);
            ui.EndRow();
            ui.Space(6f);

            ui.Header("Audio Subsystem Diagnostics & Recovery", 22f);
            ui.Toggle(ref Config.EnableAudioFix, " Audio Drop Prevention Active");
            ui.BeginRow(26f);
            if (ui.RowButton("Re-init Audio Engine (512 DSP)", 220f)) AudioFixer.ResetAudioEngine();
            if (ui.RowButton("Unmute Scene Audio", 180f)) AudioFixer.UnmuteAllSceneAudio();
            ui.EndRow();
            ui.Space(4f);

            ui.Header("Match Control");
            ui.BeginRow(28f);
            if (ui.RowButton("Force Win Match (H)", 160f)) InstantWin.WinAsKira();
            if (ui.RowButton("Emergency Meeting (H)", 175f)) MeetingHacks.CallEmergencyMeeting();
            if (ui.RowButton("Skip Meeting Timer (H)", 175f)) MeetingHacks.SkipMeetingTimer();
            ui.EndRow();
        }

        private static void DrawFixesTab(UiBuilder ui)
        {
            ui.Header("Audio Fixes & Anti-Dropout System", 24f);
            ui.Toggle(ref Config.EnableAudioFix, " Master Audio Drop Fix (Intercept Mute/Stop/Pause Calls)");
            ui.Space(4f);

            ui.Label("<color=#AAAAAA>Prevents casual audio drops, Vivox proximity cutouts, and meeting transition drops.</color>");
            ui.Label($"<color=#55FFAA>AudioListener State:</color> Pause={(AudioListener.pause ? "<color=#FF4444>PAUSED</color>" : "<color=#00FFB0>ACTIVE</color>")} | Volume={(AudioListener.volume * 100f):F0}% | RunInBackground={(Application.runInBackground ? "<color=#00FFB0>YES</color>" : "<color=#FF4444>NO</color>")}");
            ui.Space(6f);

            ui.BeginRow(28f);
            if (ui.RowButton("Re-init Audio Engine (512 DSP)", 240f))
            {
                AudioFixer.ResetAudioEngine();
            }
            if (ui.RowButton("Unmute All Scene AudioSources", 240f))
            {
                int count = AudioFixer.UnmuteAllSceneAudio();
                MelonLoader.MelonLogger.Msg($"[+] Unmuted {count} AudioSource instances in scene.");
            }
            ui.EndRow();
            ui.Space(8f);

            ui.Header("Identity & Anti-ID Theft Protection", 24f);
            ui.Toggle(ref Config.AntiIdGrab, " Anti-ID Grab Protection (Block enemy theft, prompts, & docs)");
            ui.Label("<color=#AAAAAA>Blocks GivePlayerDoc, AddPlayerInfoList, and get_CanStealName prompts for local player.</color>");
            ui.Space(8f);

            ui.Header("Role Enforcement & Guarantee", 24f);
            ui.Toggle(ref Config.GuaranteeRole, " Guarantee Selected Role Pick (Enforces choice on match start)");
            ui.Label($"<color=#AAAAAA>Active target role:</color> <b><color=#00FFAA>{GuaranteeRolePick.RoleNames[Config.SelectedRoleIndex % GuaranteeRolePick.RoleNames.Length]}</color></b>");
            ui.Space(4f);

            ui.BeginRow(26f);
            for (int r = 0; r < GuaranteeRolePick.RoleNames.Length; r++)
            {
                string rName = GuaranteeRolePick.RoleNames[r];
                bool isSel = Config.SelectedRoleIndex == r;
                if (ui.RowButton(isSel ? $"<b><color=#00FFB0>[{rName}]</color></b>" : rName, 110f))
                {
                    Config.SelectedRoleIndex = r;
                    Config.SaveConfig();
                }
            }
            ui.EndRow();
            ui.Space(8f);

            ui.Header("Network & Console Tools", 24f);
            ui.BeginRow(28f);
            if (ui.RowButton("Scan Waiting Room", 160f))
            {
                LobbyHacks.ScanWaitingLobbyPlayers();
            }
            if (ui.RowButton("Save Current Settings", 180f))
            {
                Config.SaveConfig();
            }
            ui.EndRow();
        }

        private static void DrawSelfTab(UiBuilder ui)
        {
            ui.Header("Local Player Attributes & Cheats");
            ui.Toggle(ref Config.GodMode, " God Mode / Anti-Death (Auto-revive if killed)");
            ui.Toggle(ref Config.InfiniteStamina, " Infinite Stamina (Run without fatigue)");
            ui.Toggle(ref Config.AntiKickProtection, " Anti-Kick Protection (Block incoming host kick commands)");
            ui.Toggle(ref Config.UnlockCustomizations, " Unlock All Cosmetics, Accessories & Outfits");
            ui.Toggle(ref Config.EnableAudioFix, " Audio Cutout & Voice Dropout Stabilization Fix");
            ui.Space(4f);

            ui.BeginRow(26f);
            if (ui.RowButton("Revive Local Player", 160f)) Exploits.ReviveSelf();
            if (ui.RowButton("Refresh Customizations UI", 190f)) Features.Misc.CustomizationUnlocker.RefreshUI();
            if (ui.RowButton("Reset Audio Engine", 150f)) Features.Misc.AudioFixer.ResetAudioEngine();
            ui.EndRow();
            ui.Space(8f);

            ui.Header("Player Identity & Custom Name / Color Changer");
            ui.Toggle(ref Config.SpoofPlayStation, " Display PlayStation Platform Icon (PSN Spoof)");
            ui.Toggle(ref Config.EnableDevNameColor, " Enable Custom Name Color Override (Network Visible)");
            ui.Space(4f);
            
            string currentNameInput = Config.CustomPlayerName ?? "";
            string newNameInput = ui.TextField("Custom Name:", currentNameInput, 24f);
            if (newNameInput != currentNameInput)
            {
                Config.CustomPlayerName = newNameInput;
                Config.SaveConfig();
            }

            Color oldCol = Config.CustomNameColor;
            Color newCol = ui.ColorSlider("Name Tag Color (RGBA Picker)", oldCol);
            if (newCol != oldCol)
            {
                Config.CustomNameColor = newCol;
                Config.CustomNameColorHex = "#" + ColorUtility.ToHtmlStringRGBA(newCol);
                Config.SaveConfig();
            }
            ui.Space(4f);

            string currentHex = Config.CustomNameColorHex ?? "#00FFB0";
            string newHex = ui.TextField("Hex Code:", currentHex, 24f);
            if (newHex != currentHex)
            {
                Config.CustomNameColorHex = newHex;
                if (ColorUtility.TryParseHtmlString(newHex, out Color parsedCol))
                {
                    Config.CustomNameColor = parsedCol;
                }
                Config.SaveConfig();
            }

            ui.Header("Quick Color Swatches (16 Vibrant Themes - 1-Click Apply)");
            ui.BeginRow(24f);
            for (int p = 0; p < CustomNameColor.Presets.Length; p++)
            {
                var preset = CustomNameColor.Presets[p];
                if (ui.RowButton($"<color={preset.Hex}>{preset.Name}</color>", 125f))
                {
                    Config.EnableDevNameColor = true;
                    CustomNameColor.SetColorHex(preset.Hex);
                    CustomNameColor.ApplyAndRefresh();
                }
                if (p % 4 == 3 && p < CustomNameColor.Presets.Length - 1)
                {
                    ui.EndRow();
                    ui.BeginRow(24f);
                }
            }
            ui.EndRow();
            ui.Space(4f);

            ui.BeginRow(26f);
            if (ui.RowButton("Apply & Broadcast Name / Color", 220f))
            {
                CustomNameColor.ApplyAndRefresh();
            }
            if (ui.RowButton("Apply [DEV] Staff Preset", 180f))
            {
                CustomNameColor.EnableDevMode();
            }
            ui.EndRow();
            ui.Space(4f);

            ui.Header("Developer Special Names & Staff Tags");
            ui.BeginRow(26f);
            if (ui.RowButton("[DEV] Inkwell", 120f)) Features.Misc.CustomNameColor.EnableDevMode();
            if (ui.RowButton("[PRO] Kira", 110f)) { Config.CustomPlayerName = "<color=#00FFAA>[PRO]</color> Kira"; Config.SaveConfig(); }
            if (ui.RowButton("[VIP] Lawliet", 120f)) { Config.CustomPlayerName = "<color=#FFD700>[VIP]</color> L"; Config.SaveConfig(); }
            if (ui.RowButton("[MOD] Ryuk", 110f)) { Config.CustomPlayerName = "<color=#00DDFF>[MOD]</color> Ryuk"; Config.SaveConfig(); }
            ui.EndRow();
        }

        private static void DrawGlobalTab(UiBuilder ui)
        {
            ui.Header("Global Map & Mass Player Actions");
            ui.BeginRow(28f);
            if (ui.RowButton("Warp All Players to Me (H)", 220f)) Exploits.WarpAllToMe();
            if (ui.RowButton("Handcuff All Enemies (H)", 200f)) Exploits.HandcuffAll();
            ui.EndRow();
            ui.Space(6f);

            ui.BeginRow(28f);
            if (ui.RowButton("Execute All (Heart Attack) (H)", 220f)) Executioner.ExecuteAll(true, EDeathType.HeartAttack);
            if (ui.RowButton("Execute All (Truck) (H)", 175f)) Executioner.ExecuteAll(true, EDeathType.Truck);
            if (ui.RowButton("Execute All (Gun) (H)", 165f)) Executioner.ExecuteAll(true, EDeathType.Gun);
            ui.EndRow();
        }

        private static void DrawExploitTab(UiBuilder ui)
        {
            ui.Header("Game Rule & Server Authority Exploits");
            ui.Toggle(ref Config.BypassAuthority, " Force Authority Bypass (Send actions without host checks)");
            ui.Toggle(ref Config.AntiKickProtection, " Anti-Kick Protection (Block host kick commands)");
            ui.Toggle(ref Config.HearGhostsForAll, " Hear Ghosts / Intercept All Voice Chat");
            ui.Toggle(ref Config.DeathNoteAlwaysKill, " Always-Kill Exploit (Kill anyone from any distance/cooldown)");
            ui.Toggle(ref Config.InstantIdSteal, " Instant 100% ID Theft Aura");
            ui.Toggle(ref Config.VoteManyTimes, " Multi-Vote Exploit (Vote unlimited times in meetings)");
            ui.Space(6f);

            ui.Header("Objective & Victory Bypasses");
            ui.BeginRow(28f);
            if (ui.RowButton("Complete Active Task", 180f)) TaskHacks.CompleteActiveTask();
            if (ui.RowButton("Force Kira Victory (H)", 180f)) InstantWin.WinAsKira();
            if (ui.RowButton("Force L Victory (H)", 170f)) InstantWin.WinAsL();
            ui.EndRow();
        }

        private static void DrawSettingsTab(UiBuilder ui)
        {
            ui.Header("Menu Configuration & Display");
            ui.Toggle(ref Config.ShowKeybinds, " Show Floating Keybinds HUD Overlay [L]");
            ui.Space(4f);

            ui.BeginRow(28f);
            if (ui.RowButton("Save Configuration to File", 200f)) Config.SaveConfig();
            if (ui.RowButton("Load Configuration from File", 200f)) Config.LoadConfig();
            ui.EndRow();
            ui.Space(6f);

            if (ui.Button("Reset Menu Position & Size", 220f, 26f))
            {
                Config.MenuPos = new Vector2(30, 30);
                Config.MenuSize = new Vector2(760, 600);
            }
            ui.Space(8f);

            ui.Header("GUI Color Customizer (RGBA Sliders)");
            Config.MenuBg = ui.ColorSlider("Menu Background", Config.MenuBg);
            Config.MenuText = ui.ColorSlider("Text Color", Config.MenuText);
            Config.TabActive = ui.ColorSlider("Tab Active Color", Config.TabActive);
            Config.TabInactive = ui.ColorSlider("Tab Inactive Color", Config.TabInactive);
            Config.ContentBg = ui.ColorSlider("Content Background", Config.ContentBg);

            ui.Space(6f);
            if (ui.Button("Reset Colors to Default Theme", 240f, 26f))
            {
                Config.MenuBg = new Color(0.08f, 0.08f, 0.15f, 0.94f);
                Config.MenuText = Color.white;
                Config.TabActive = new Color(0.20f, 0.65f, 0.40f, 1.0f);
                Config.TabInactive = new Color(0.13f, 0.15f, 0.25f, 1.0f);
                Config.ContentBg = new Color(0.05f, 0.06f, 0.12f, 0.96f);
            }
        }

        private static void DrawSupportTab(UiBuilder ui)
        {
            ui.Header("About & Credits");
            ui.Label("<b>DEATH NOTE Cheater Within v3.0 - Ultimate Edition</b>");
            ui.Label("Designed for: <b>DEATH NOTE Killer Within (Steam IL2CPP)</b>");
            ui.Label("Developed by: <b>Inkwell</b>");
            ui.Label("Framework: <b>MelonLoader v0.6.0+ / v0.7.3+ (.NET 6.0 IL2CPP)</b>");
            ui.Space(8f);

            ui.Header("Project Information");
            ui.Label("  <b>Open Source Release</b> - DEATH NOTE Cheater Within");
            ui.Label("  <b>Architecture</b> - Tactical Internal Mod Engine");
            ui.Space(8f);

            ui.Header("Keybinds Reference");
            ui.Label("  <b>[INSERT]</b> - Toggle Tactical Menu");
            ui.Label("  <b>[L]</b> - Toggle Floating Hotkey Reference HUD");
            ui.Label("  <b>[P]</b> - Export Game State to Desktop (dnkw_dump.txt)");
            ui.Label("  <b>[ [ ]</b> - Force Re-scan Game State & Roles");
            ui.Label("  <b>[F1]-[F10]</b> - Instant Action Shortcuts (F1=ESP, F2=NoClip, F9=Kill All)");
        }
    }
}
