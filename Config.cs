using System;
using System.IO;
using UnityEngine;

namespace DeathNoteCheaterWithin
{
    public static class Config
    {
        public static string ConfigFilePath => Path.Combine(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData"), 
            "inkwell_config.cfg"
        );

        // Menu & Display
        public static bool MenuOpen = true;
        public static bool ShowKeybinds = false;
        public static Vector2 MenuPos = new Vector2(30, 30);
        public static Vector2 MenuSize = new Vector2(760, 600);

        // Customizable Colors (RGBA)
        public static Color MenuBg = new Color(0.08f, 0.08f, 0.15f, 0.94f);
        public static Color MenuText = Color.white;
        public static Color TabActive = new Color(0.20f, 0.65f, 0.40f, 1.0f);
        public static Color TabInactive = new Color(0.13f, 0.15f, 0.25f, 1.0f);
        public static Color ContentBg = new Color(0.05f, 0.06f, 0.12f, 0.96f);

        // Default OFF Features (All Cheats Off By Default)
        public static bool EspEnabled = false;
        public static bool EspBoxes2D = false;
        public static bool EspBoxes3D = false;
        public static bool EspSnaplines = false;
        public static bool EspShowRoles = false;
        public static bool EspShowDistance = false;
        public static bool EspShowItems = false;
        public static bool EspShowStatus = false;
        public static float EspMaxDistance = 150f;

        // ESP - NPCs & World (Off by default)
        public static bool NpcEsp = false;
        public static bool NpcCorpseEsp = false;
        public static bool NpcEliteOnly = false;
        public static bool CameraEsp = false;
        public static bool TaskEsp = false;

        // Locomotion & Physics (Off by default)
        public static bool SpeedHack = false;
        public static float SpeedMultiplier = 2.0f;
        public static bool NoClip = false;
        public static float NoClipSpeed = 12.0f;
        public static bool AntiSlowdown = false;

        // Camera
        public static bool CustomFovEnabled = false;
        public static float CustomFov = 90f;

        // Combat & Death Note (Off by default)
        public static bool DeathNoteAlwaysKill = false;
        public static bool InstantIdSteal = false;
        public static bool AutoIdTheftAura = false;

        // Roles & Shinigami Vision (Off by default)
        public static bool RevealRoles = false;
        public static bool UnlockAllAbilities = false;
        public static bool ForceMaxLevel = false;
        public static bool RemVision = false;

        // Meeting & Voting (Off by default)
        public static bool VoteManyTimes = false;

        // Self & Exploits (Off by default)
        public static bool GodMode = false;
        public static bool InfiniteStamina = false;
        public static bool AntiKickProtection = true; // Anti-Kick ON by default for safety
        public static bool HearGhostsForAll = false;
        public static bool GuaranteeRole = false;
        public static int SelectedRoleIndex = 1;
        public static bool AntiIdGrab = true;
        public static string CustomPlayerName = "";
        public static string CustomNameColorHex = "#00FFB0"; // Default Cyan/Mint
        public static Color CustomNameColor = new Color(0.0f, 1.0f, 0.69f, 1.0f); // Default Cyan/Mint RGBA
        public static float NameColorR = 0.0f;
        public static float NameColorG = 1.0f;
        public static float NameColorB = 0.69f;
        public static bool EnableDevNameColor = false;
        public static bool EnableAudioFix = true; // Audio Fix ON by default for stability
        public static bool BypassAuthority = false;

        // Game Optimizations & Platform (Off by default)
        public static bool UnlockCustomizations = false;
        public static bool SkipCinematics = false;
        public static bool AutoQte = false;
        public static bool SpoofPlayStation = false;

        // Identity & Level Tools
        public static string ManualSteamIdInput = "";
        public static float ExpBoostAmount = 500f;
        public static bool ShowLobbyPlayers = true;
        public static string TargetRoomCode = "";
        public static bool ShowDebugOverlay = false;
        public static bool BypassBlockRestriction = true; // Allow joining rooms even when blocked
        public static bool JoinActiveLobbies = true;      // Allow quick joining games in-progress
        public static bool ShowActiveLobbies = true;      // Show in-progress rooms in room search list

        public static void SaveConfig()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigFilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using (StreamWriter sw = new StreamWriter(ConfigFilePath, false))
                {
                    sw.WriteLine($"MenuOpen={MenuOpen}");
                    sw.WriteLine($"ShowKeybinds={ShowKeybinds}");
                    sw.WriteLine($"EspEnabled={EspEnabled}");
                    sw.WriteLine($"EspBoxes2D={EspBoxes2D}");
                    sw.WriteLine($"EspSnaplines={EspSnaplines}");
                    sw.WriteLine($"EspShowRoles={EspShowRoles}");
                    sw.WriteLine($"SpeedHack={SpeedHack}");
                    sw.WriteLine($"SpeedMultiplier={SpeedMultiplier}");
                    sw.WriteLine($"NoClip={NoClip}");
                    sw.WriteLine($"DeathNoteAlwaysKill={DeathNoteAlwaysKill}");
                    sw.WriteLine($"RevealRoles={RevealRoles}");
                    sw.WriteLine($"GodMode={GodMode}");
                    sw.WriteLine($"AntiKickProtection={AntiKickProtection}");
                    sw.WriteLine($"HearGhostsForAll={HearGhostsForAll}");
                    sw.WriteLine($"CustomPlayerName={CustomPlayerName}");
                    sw.WriteLine($"CustomNameColorHex={CustomNameColorHex}");
                    sw.WriteLine($"NameColorR={NameColorR}");
                    sw.WriteLine($"NameColorG={NameColorG}");
                    sw.WriteLine($"NameColorB={NameColorB}");
                    sw.WriteLine($"EnableDevNameColor={EnableDevNameColor}");
                    sw.WriteLine($"SpoofPlayStation={SpoofPlayStation}");
                    sw.WriteLine($"UnlockCustomizations={UnlockCustomizations}");
                    sw.WriteLine($"ShowDebugOverlay={ShowDebugOverlay}");
                    sw.WriteLine($"EnableAudioFix={EnableAudioFix}");
                    sw.WriteLine($"AntiIdGrab={AntiIdGrab}");
                    sw.WriteLine($"GuaranteeRole={GuaranteeRole}");
                    sw.WriteLine($"SelectedRoleIndex={SelectedRoleIndex}");
                    sw.WriteLine($"SavedSteamIdIndex={Features.Misc.Spoofers.CurrentSteamIdIndex}");
                    sw.WriteLine($"SavedPsnIndex={Features.Misc.Spoofers.CurrentPsnIndex}");
                    sw.WriteLine($"BypassBlockRestriction={BypassBlockRestriction}");
                    sw.WriteLine($"JoinActiveLobbies={JoinActiveLobbies}");
                    sw.WriteLine($"ShowActiveLobbies={ShowActiveLobbies}");
                }
                MelonLoader.MelonLogger.Msg($"[Config] Saved settings to '{ConfigFilePath}'");
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[Config] Failed to save config: {ex.Message}");
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath)) return;

                string[] lines = File.ReadAllLines(ConfigFilePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                    string[] parts = line.Split('=', 2);
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();

                    switch (key)
                    {
                        case "MenuOpen": bool.TryParse(val, out MenuOpen); break;
                        case "ShowKeybinds": bool.TryParse(val, out ShowKeybinds); break;
                        case "EspEnabled": bool.TryParse(val, out EspEnabled); break;
                        case "EspBoxes2D": bool.TryParse(val, out EspBoxes2D); break;
                        case "EspSnaplines": bool.TryParse(val, out EspSnaplines); break;
                        case "EspShowRoles": bool.TryParse(val, out EspShowRoles); break;
                        case "SpeedHack": bool.TryParse(val, out SpeedHack); break;
                        case "SpeedMultiplier": float.TryParse(val, out SpeedMultiplier); break;
                        case "NoClip": bool.TryParse(val, out NoClip); break;
                        case "DeathNoteAlwaysKill": bool.TryParse(val, out DeathNoteAlwaysKill); break;
                        case "RevealRoles": bool.TryParse(val, out RevealRoles); break;
                        case "GodMode": bool.TryParse(val, out GodMode); break;
                        case "AntiKickProtection": bool.TryParse(val, out AntiKickProtection); break;
                        case "HearGhostsForAll": bool.TryParse(val, out HearGhostsForAll); break;
                        case "EnableAudioFix": bool.TryParse(val, out EnableAudioFix); break;
                        case "AntiIdGrab": bool.TryParse(val, out AntiIdGrab); break;
                        case "GuaranteeRole": bool.TryParse(val, out GuaranteeRole); break;
                        case "SelectedRoleIndex": int.TryParse(val, out SelectedRoleIndex); break;
                        case "CustomPlayerName": CustomPlayerName = val; break;
                        case "CustomNameColorHex": CustomNameColorHex = val; break;
                        case "NameColorR": float.TryParse(val, out NameColorR); break;
                        case "NameColorG": float.TryParse(val, out NameColorG); break;
                        case "NameColorB": float.TryParse(val, out NameColorB); break;
                        case "EnableDevNameColor": bool.TryParse(val, out EnableDevNameColor); break;
                        case "SpoofPlayStation": bool.TryParse(val, out SpoofPlayStation); break;
                        case "UnlockCustomizations": bool.TryParse(val, out UnlockCustomizations); break;
                        case "ShowDebugOverlay": bool.TryParse(val, out ShowDebugOverlay); break;
                        case "BypassBlockRestriction": bool.TryParse(val, out BypassBlockRestriction); break;
                        case "JoinActiveLobbies": bool.TryParse(val, out JoinActiveLobbies); break;
                        case "ShowActiveLobbies": bool.TryParse(val, out ShowActiveLobbies); break;
                        case "SavedSteamIdIndex": if (int.TryParse(val, out int sIdx)) Features.Misc.Spoofers.SetSteamIdIndex(sIdx); break;
                        case "SavedPsnIndex": if (int.TryParse(val, out int pIdx)) Features.Misc.Spoofers.SetPsnIndex(pIdx); break;
                    }
                }
                MelonLoader.MelonLogger.Msg($"[Config] Loaded configuration from '{ConfigFilePath}'");
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Warning($"[Config] Could not load config: {ex.Message}");
            }
        }
    }
}
