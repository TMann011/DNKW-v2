using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppGR;
using Il2CppGame.Core;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class Spoofers
    {
        public static string BaseDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");

        public static string SpoofFile => Path.Combine(BaseDirectory, "steamidspoof.txt");
        public static string BackupFile => Path.Combine(BaseDirectory, "steamid_backup.txt");

        public static List<string> AvailableSteamIds { get; } = new List<string>();
        public static int CurrentSteamIdIndex { get; private set; } = 0;
        public static string OriginalSteamId { get; private set; } = "";
        public static bool IsSteamIdSpoofed => !string.IsNullOrWhiteSpace(ActiveSpoofedSteamId) && ActiveSpoofedSteamId != OriginalSteamId;

        public static string ActiveSpoofedSteamId
        {
            get
            {
                if (AvailableSteamIds.Count > 0 && CurrentSteamIdIndex >= 0 && CurrentSteamIdIndex < AvailableSteamIds.Count)
                {
                    return AvailableSteamIds[CurrentSteamIdIndex];
                }
                return "";
            }
        }

        // PSN Profile Presets & Custom Profiles
        public static List<string> PsnPresets { get; } = new List<string>()
        {
            "Shadow_Sniper99",
            "Vortex_Gamer_PS5",
            "Cyber_Ghost_X",
            "Apex_Predator_EU",
            "Kira_DeathNote_01",
            "Lawliet_Detective",
            "Ryuk_Apples_PS5",
            "Alpha_Phantom_007",
            "Pixel_Ninja_PRO",
            "Hyper_Drive_99",
            "MisaMisa_Amane",
            "Near_SPK_Leader",
            "Mello_Chocolate_JP",
            "Quantum_Strike",
            "Venom_Blade_PSN",
            "Zero_Latency_JP",
            "Spectre_Agent_88",
            "Blaze_Rider_NA",
            "LightYagami_God",
            "Titan_Slayer_X"
        };
        public static int CurrentPsnIndex { get; private set; } = 0;
        public static string ActivePsnId => PsnPresets.Count > 0 ? PsnPresets[CurrentPsnIndex % PsnPresets.Count] : "Kira_Official";

        static Spoofers()
        {
            LoadSteamIds();
        }

        public static void LoadSteamIds()
        {
            AvailableSteamIds.Clear();
            try
            {
                if (!Directory.Exists(BaseDirectory))
                {
                    Directory.CreateDirectory(BaseDirectory);
                }

                if (File.Exists(SpoofFile))
                {
                    string[] lines = File.ReadAllLines(SpoofFile);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
                        {
                            AvailableSteamIds.Add(trimmed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Spoofers] Failed to load Steam IDs from '{SpoofFile}': {ex.Message}");
            }

            if (AvailableSteamIds.Count == 0)
            {
                // Generate randomized 64-bit Steam ID pool dynamically (no hardcoded personal accounts)
                long basePrefix = 76561198000000000L;
                System.Random rng = new System.Random();
                for (int i = 0; i < 20; i++)
                {
                    long randomSuffix = (long)rng.Next(10000000, 99999999);
                    AvailableSteamIds.Add((basePrefix + randomSuffix).ToString());
                }

                try
                {
                    List<string> fileLines = new List<string>
                    {
                        "# Death Note Cheater Within - Steam ID Spoofing Pool",
                        "# Add your own 64-bit Steam IDs below (one per line):"
                    };
                    fileLines.AddRange(AvailableSteamIds);
                    File.WriteAllLines(SpoofFile, fileLines);
                    MelonLogger.Msg($"[Spoofers] Generated default '{SpoofFile}' with {AvailableSteamIds.Count} randomized Steam ID entries.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[Spoofers] Could not write default file '{SpoofFile}': {ex.Message}");
                }
            }

            MelonLogger.Msg($"[Spoofers] Loaded {AvailableSteamIds.Count} Steam IDs from pool.");
        }

        public static void SetSteamIdIndex(int index)
        {
            if (AvailableSteamIds.Count == 0) return;
            CurrentSteamIdIndex = Mathf.Clamp(index, 0, AvailableSteamIds.Count - 1);
        }

        public static void SetPsnIndex(int index)
        {
            if (PsnPresets.Count == 0) return;
            CurrentPsnIndex = Mathf.Clamp(index, 0, PsnPresets.Count - 1);
        }

        public static void NextSteamId()
        {
            if (AvailableSteamIds.Count == 0) return;
            CurrentSteamIdIndex = (CurrentSteamIdIndex + 1) % AvailableSteamIds.Count;
            Config.SaveConfig();
            MelonLogger.Msg($"[Spoofers] Switched to Steam ID [{CurrentSteamIdIndex + 1}/{AvailableSteamIds.Count}]: {ActiveSpoofedSteamId}");
        }

        public static void PrevSteamId()
        {
            if (AvailableSteamIds.Count == 0) return;
            CurrentSteamIdIndex = (CurrentSteamIdIndex - 1 + AvailableSteamIds.Count) % AvailableSteamIds.Count;
            Config.SaveConfig();
            MelonLogger.Msg($"[Spoofers] Switched to Steam ID [{CurrentSteamIdIndex + 1}/{AvailableSteamIds.Count}]: {ActiveSpoofedSteamId}");
        }

        public static void AddCustomSteamId(string newId)
        {
            if (string.IsNullOrWhiteSpace(newId)) return;
            string cleanId = newId.Trim();
            if (!AvailableSteamIds.Contains(cleanId))
            {
                AvailableSteamIds.Add(cleanId);
                CurrentSteamIdIndex = AvailableSteamIds.Count - 1;
                try
                {
                    File.AppendAllText(SpoofFile, "\n" + cleanId);
                }
                catch { }
                MelonLogger.Msg($"[Spoofers] Added & switched to new Steam ID: {cleanId}");
            }
        }

        public static void NextPsnId()
        {
            if (PsnPresets.Count == 0) return;
            CurrentPsnIndex = (CurrentPsnIndex + 1) % PsnPresets.Count;
            Config.SaveConfig();
            MelonLogger.Msg($"[Spoofers] Switched to PSN ID [{CurrentPsnIndex + 1}/{PsnPresets.Count}]: {ActivePsnId}");
        }

        public static void PrevPsnId()
        {
            if (PsnPresets.Count == 0) return;
            CurrentPsnIndex = (CurrentPsnIndex - 1 + PsnPresets.Count) % PsnPresets.Count;
            Config.SaveConfig();
            MelonLogger.Msg($"[Spoofers] Switched to PSN ID [{CurrentPsnIndex + 1}/{PsnPresets.Count}]: {ActivePsnId}");
        }

        public static void RandomizeSteamId()
        {
            long basePrefix = 76561198000000000L;
            long randomSuffix = (long)UnityEngine.Random.Range(10000000, 99999999);
            string newId = (basePrefix + randomSuffix).ToString();

            AvailableSteamIds.Add(newId);
            CurrentSteamIdIndex = AvailableSteamIds.Count - 1;
            MelonLogger.Msg($"[Spoofers] Generated & activated randomized Steam ID: {newId}");
        }

        public static void RevertToOriginal()
        {
            if (!string.IsNullOrEmpty(OriginalSteamId))
            {
                int idx = AvailableSteamIds.IndexOf(OriginalSteamId);
                if (idx >= 0)
                {
                    CurrentSteamIdIndex = idx;
                }
                else
                {
                    AvailableSteamIds.Insert(0, OriginalSteamId);
                    CurrentSteamIdIndex = 0;
                }
                MelonLogger.Msg($"[Spoofers] Reverted back to Original Steam ID: {OriginalSteamId}");
            }
        }

        // Profile Viewing & Web Launcher
        public static void ViewSteamProfile(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId)) return;
            string targetId = steamId.Trim();
            string url = targetId.StartsWith("7656") 
                ? $"https://steamcommunity.com/profiles/{targetId}" 
                : $"https://steamcommunity.com/id/{targetId}";

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                MelonLogger.Msg($"[ProfileViewer] Opening Steam profile: {url}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ProfileViewer] Failed to launch profile browser: {ex.Message}");
            }
        }

        public static void ViewPsnProfile(string psnId)
        {
            if (string.IsNullOrWhiteSpace(psnId)) return;
            string targetPsn = psnId.Trim();
            string url = $"https://psnprofiles.com/{targetPsn}";

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                MelonLogger.Msg($"[ProfileViewer] Opening PSN profile online: {url}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ProfileViewer] Failed to launch PSN profile browser: {ex.Message}");
            }
        }

        [HarmonyPatch(typeof(AppClient), nameof(AppClient.GetMyPlatform))]
        public static class AppClient_GetMyPlatform
        {
            public static bool Prefix(ref EPlatform __result)
            {
                if (Config.SpoofPlayStation)
                {
                    __result = (EPlatform)2; // PlayStation platform
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatformService), nameof(SteamPlatformService.GetPlatformUserID))]
        public static class SteamPlatformService_GetPlatformUserID
        {
            public static bool Prefix(ref string __result)
            {
                if (Config.SpoofPlayStation)
                {
                    __result = ActivePsnId; // Return active PSN handle when PSN spoof active
                    return false;
                }
                else if (IsSteamIdSpoofed)
                {
                    __result = ActiveSpoofedSteamId;
                    return false;
                }
                return true;
            }

            public static void Postfix(ref string __result)
            {
                if (string.IsNullOrEmpty(OriginalSteamId) && !string.IsNullOrEmpty(__result) && __result != "0" && !Config.SpoofPlayStation)
                {
                    OriginalSteamId = __result;
                    try
                    {
                        File.WriteAllText(BackupFile, $"OriginalSteamID={OriginalSteamId}\nSavedOn={DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
                        MelonLogger.Msg($"[Spoofers] Captured & backed up original Steam ID to '{BackupFile}': {OriginalSteamId}");
                    }
                    catch { }
                }
            }
        }

        // Generic Platform Account ID Hook (Applied conditionally)
        public static class GenericPlatformAccountPatch
        {
            public static bool Prefix(ref string __result)
            {
                if (Config.SpoofPlayStation)
                {
                    __result = ActivePsnId;
                    return false;
                }
                else if (IsSteamIdSpoofed)
                {
                    __result = ActiveSpoofedSteamId;
                    return false;
                }
                return true;
            }
        }

        // Profile Click Direct Redirect Hook (Intercepts game profile click actions)
        public static class ProfileClickRedirectPatch
        {
            public static bool Prefix(MethodBase __originalMethod, object[] __args)
            {
                if (Config.SpoofPlayStation)
                {
                    string targetPsn = ActivePsnId;
                    ViewPsnProfile(targetPsn);
                    MelonLogger.Msg($"[+] Intercepted profile click! Directing to PSN profile page: https://psnprofiles.com/{targetPsn}");
                    return false;
                }
                return true;
            }
        }
    }
}
