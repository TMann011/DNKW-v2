using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using Il2CppGR;
using Il2CppLunaOutGame;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class CustomNameColor
    {
        // Preset color swatches (16 vibrant themes)
        public static readonly (string Name, string Hex)[] Presets = new[]
        {
            ("Neon Cyan", "#00FFAA"),
            ("Neon Pink", "#FF1493"),
            ("Blood Red", "#FF2233"),
            ("Electric Blue", "#0088FF"),
            ("Acid Green", "#39FF14"),
            ("VIP Gold", "#FFD700"),
            ("Shinigami Purple", "#BF00FF"),
            ("Hazard Orange", "#FF8800"),
            ("Ice White", "#FFFFFF"),
            ("Shadow Violet", "#8A2BE2"),
            ("Emerald Green", "#00FF7F"),
            ("Deep Amber", "#FFBF00"),
            ("Cyber Yellow", "#FFFF00"),
            ("Crimson Flame", "#DC143C"),
            ("Sky Blue", "#00BFFF"),
            ("Magenta", "#FF00FF")
        };

        public static string GetFormattedName(string fallbackName = "Player")
        {
            string baseName = !string.IsNullOrWhiteSpace(Config.CustomPlayerName) 
                ? Config.CustomPlayerName.Trim() 
                : fallbackName;
            string cleanName = Regex.Replace(baseName, "<.*?>", string.Empty);

            if (Config.EnableDevNameColor && !string.IsNullOrEmpty(Config.CustomNameColorHex))
            {
                string hex = Config.CustomNameColorHex.StartsWith("#") ? Config.CustomNameColorHex : "#" + Config.CustomNameColorHex;
                return $"<color={hex}>{cleanName}</color>";
            }
            return cleanName;
        }

        // 1. Hook PlayerMovement.GetPlayerName to apply custom name and color tags in-game
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.GetPlayerName), new Type[] { typeof(bool), typeof(bool) })]
        public static class PlayerMovement_GetPlayerName_Patch
        {
            public static void Postfix(PlayerMovement __instance, bool withIcon, bool withColor, ref string __result)
            {
                try
                {
                    if (__instance != null && __instance.IsMyPlayer)
                    {
                        if (Config.EnableDevNameColor)
                        {
                            __instance.NameColor = Config.CustomNameColor;
                        }

                        if (!string.IsNullOrWhiteSpace(Config.CustomPlayerName) || Config.EnableDevNameColor)
                        {
                            string baseName = !string.IsNullOrWhiteSpace(Config.CustomPlayerName) 
                                ? Config.CustomPlayerName.Trim() 
                                : (__result ?? "Player");
                            string cleanName = Regex.Replace(baseName, "<.*?>", string.Empty);

                            if (Config.EnableDevNameColor && !string.IsNullOrEmpty(Config.CustomNameColorHex))
                            {
                                string hex = Config.CustomNameColorHex.StartsWith("#") ? Config.CustomNameColorHex : "#" + Config.CustomNameColorHex;
                                __result = $"<color={hex}>{cleanName}</color>";
                            }
                            else
                            {
                                __result = cleanName;
                            }
                        }
                    }
                }
                catch { }
            }
        }



        // Apply selected color from RGB floats
        public static void SetColorRGB(float r, float g, float b)
        {
            r = Mathf.Clamp01(r);
            g = Mathf.Clamp01(g);
            b = Mathf.Clamp01(b);
            Config.NameColorR = r;
            Config.NameColorG = g;
            Config.NameColorB = b;
            Config.CustomNameColor = new Color(r, g, b, 1.0f);

            int ir = Mathf.RoundToInt(r * 255f);
            int ig = Mathf.RoundToInt(g * 255f);
            int ib = Mathf.RoundToInt(b * 255f);
            Config.CustomNameColorHex = $"#{ir:X2}{ig:X2}{ib:X2}";
            Config.SaveConfig();
            ApplyAndRefresh();
        }

        // Apply selected color from Hex string
        public static void SetColorHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return;
            hex = hex.Trim();
            if (!hex.StartsWith("#")) hex = "#" + hex;
            Config.CustomNameColorHex = hex;

            if (ColorUtility.DoTryParseHtmlColor(hex, out Color32 parsed32))
            {
                Color parsed = parsed32;
                Config.CustomNameColor = parsed;
                Config.NameColorR = parsed.r;
                Config.NameColorG = parsed.g;
                Config.NameColorB = parsed.b;
            }
            Config.SaveConfig();
            ApplyAndRefresh();
        }

        // Refresh UI and synchronize across network, save data, and game managers
        public static void ApplyAndRefresh()
        {
            try
            {
                string formattedName = GetFormattedName("Player");

                // 1. In-game 3D name tag refresh
                try
                {
                    GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                    if (gsm != null && gsm.MyPlayer != null)
                    {
                        gsm.MyPlayer.CharacterName = formattedName;
                        if (Config.EnableDevNameColor)
                        {
                            gsm.MyPlayer.NameColor = Config.CustomNameColor;
                        }
                        gsm.MyPlayer.UpdatePlayerName();
                        if (gsm.MyPlayer.nameUI != null)
                        {
                            if (gsm.MyPlayer.nameUI.PlayerNameSetter != null)
                            {
                                if (gsm.MyPlayer.nameUI.PlayerNameSetter.PlayerName != null)
                                {
                                    gsm.MyPlayer.nameUI.PlayerNameSetter.PlayerName.text = formattedName;
                                    if (Config.EnableDevNameColor)
                                    {
                                        gsm.MyPlayer.nameUI.PlayerNameSetter.PlayerName.color = Config.CustomNameColor;
                                    }
                                }
                                if (Config.EnableDevNameColor && gsm.MyPlayer.nameUI.PlayerNameSetter.ColorImg != null)
                                {
                                    gsm.MyPlayer.nameUI.PlayerNameSetter.ColorImg.color = Config.CustomNameColor;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 2. Photon network nickname synchronization
                try
                {
                    Il2CppPhoton.Pun.PhotonNetwork.NickName = formattedName;
                    if (Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer != null)
                    {
                        Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer.NickName = formattedName;
                    }
                    MelonLogger.Msg($"[CustomNameColor] Synchronized network name: {formattedName}");
                }
                catch { }

                // 3. Out-Game Lobby refresh
                try
                {
                    var roomPlayerUIs = UnityEngine.Object.FindObjectsOfType<Il2CppLunaOutGame.RoomPlayerNameUI>();
                    if (roomPlayerUIs != null)
                    {
                        foreach (var ui in roomPlayerUIs)
                        {
                            if (ui == null) continue;
                            bool isLocal = (ui.CharacterModel != null && ui.CharacterModel.IsMyPlayer) ||
                                           (ui.Player != null && ui.Player.IsLocal);
                            if (isLocal)
                            {
                                ui.NickName = formattedName;
                                if (ui.PlayerName != null)
                                {
                                    ui.PlayerName.text = formattedName;
                                    if (Config.EnableDevNameColor)
                                    {
                                        ui.PlayerName.color = Config.CustomNameColor;
                                    }
                                }
                                if (Config.EnableDevNameColor)
                                {
                                    ui.MyPlayerColor = Config.CustomNameColor;
                                    ui.PlayerColor = Config.CustomNameColor;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 4. Save to persistent save data
                try
                {
                    var sdm = UnityEngine.Object.FindObjectOfType<SaveDataManager>();
                    if (sdm != null)
                    {
                        string rawName = !string.IsNullOrWhiteSpace(Config.CustomPlayerName) ? Config.CustomPlayerName.Trim() : "Player";
                        sdm.SavePlayerName(rawName);
                        sdm.SaveRealName(rawName);
                        sdm.StartSaveGame();
                        sdm.StartSaveSystem();
                    }
                }
                catch { }

                // 5. OutGame Matchmaker name sync
                try
                {
                    var mm = UnityEngine.Object.FindObjectOfType<Il2CppLunaOutGame.MatchmakerObject>();
                    if (mm != null)
                    {
                        mm.ChangeMyRealName(formattedName);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[CustomNameColor] Refresh error: {ex.Message}");
            }
        }

        private static float _lastUpdateCheck = 0f;

        public static void Update()
        {
            if (!Config.EnableDevNameColor && string.IsNullOrWhiteSpace(Config.CustomPlayerName)) return;

            if (Time.time > _lastUpdateCheck + 1.0f)
            {
                _lastUpdateCheck = Time.time;

                string formatted = GetFormattedName("Player");

                // 1. In-game: enforce NameColor, nameUI text/color, and CharacterName on local player
                try
                {
                    var gsm = EngineStrictSingleton<GameStateManager>.Instance;
                    if (gsm != null && gsm.MyPlayer != null)
                    {
                        var myPlayer = gsm.MyPlayer;

                        if (Config.EnableDevNameColor)
                        {
                            myPlayer.NameColor = Config.CustomNameColor;
                        }

                        if (!string.IsNullOrWhiteSpace(Config.CustomPlayerName))
                        {
                            string cleanName = Regex.Replace(Config.CustomPlayerName.Trim(), "<.*?>", string.Empty);
                            if (myPlayer.CharacterName != formatted)
                            {
                                myPlayer.CharacterName = formatted;
                            }
                        }

                        // Direct nameplate text mesh enforcement (replaces PlayerNameUI.RefreshName + PlayerNameSetter.Setup hooks)
                        if (myPlayer.nameUI != null && myPlayer.nameUI.PlayerNameSetter != null)
                        {
                            var setter = myPlayer.nameUI.PlayerNameSetter;
                            setter.NameWithColor = true;
                            if (setter.PlayerName != null)
                            {
                                setter.PlayerName.richText = true;
                                if (setter.PlayerName.text != formatted)
                                {
                                    setter.PlayerName.text = formatted;
                                }
                                if (Config.EnableDevNameColor)
                                {
                                    setter.PlayerName.color = Config.CustomNameColor;
                                }
                            }
                            if (Config.EnableDevNameColor && setter.ColorImg != null)
                            {
                                setter.ColorImg.color = Config.CustomNameColor;
                            }
                        }
                    }
                }
                catch { }

                // 2. Lobby: enforce RoomPlayerNameUI text/color for local player (replaces RoomPlayerNameUI.RefreshPlayerName hook)
                try
                {
                    var roomPlayerUIs = UnityEngine.Object.FindObjectsOfType<Il2CppLunaOutGame.RoomPlayerNameUI>();
                    if (roomPlayerUIs != null)
                    {
                        foreach (var ui in roomPlayerUIs)
                        {
                            if (ui == null) continue;
                            bool isLocal = (ui.CharacterModel != null && ui.CharacterModel.IsMyPlayer) ||
                                           (ui.Player != null && ui.Player.IsLocal);
                            if (isLocal)
                            {
                                if (ui.PlayerName != null)
                                {
                                    ui.PlayerName.richText = true;
                                    if (ui.PlayerName.text != formatted)
                                    {
                                        ui.PlayerName.text = formatted;
                                    }
                                    if (Config.EnableDevNameColor)
                                    {
                                        ui.PlayerName.color = Config.CustomNameColor;
                                    }
                                }
                                if (Config.EnableDevNameColor)
                                {
                                    ui.MyPlayerColor = Config.CustomNameColor;
                                    ui.PlayerColor = Config.CustomNameColor;
                                }
                            }
                        }
                    }
                }
                catch { }

                // 3. Photon NickName drift check
                try
                {
                    if (Il2CppPhoton.Pun.PhotonNetwork.NickName != formatted)
                    {
                        Il2CppPhoton.Pun.PhotonNetwork.NickName = formatted;
                    }
                }
                catch { }
            }
        }

        // Quick developer mode setup
        public static void EnableDevMode()
        {
            Config.EnableDevNameColor = true;
            SetColorHex("#FF0055");
            Config.CustomPlayerName = "[DEV] Inkwell";
            Config.SaveConfig();
            ApplyAndRefresh();
            MelonLogger.Msg("[+] DEV Name Color mode activated: [DEV] Inkwell (#FF0055)");
        }
    }
}
