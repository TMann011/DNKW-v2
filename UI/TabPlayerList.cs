using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeathNoteCheaterWithin.Drawing;
using DeathNoteCheaterWithin.Features.Combat;
using DeathNoteCheaterWithin.Features.Dev;
using DeathNoteCheaterWithin.Features.Movement;
using DeathNoteCheaterWithin.Features.Misc;
using DeathNoteCheaterWithin.Features.Game;
using Il2Cpp;
using Il2CppGame;
using Il2CppLunaOutGame;
using Il2CppGRNetwork;
using UnityEngine;

namespace DeathNoteCheaterWithin.UI
{
    public static class TabPlayerList
    {
        private static int _currentPage = 0;
        private const int PlayersPerPage = 4;

        public static void Draw(UiBuilder ui)
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                List<PlayerMovement> players = new List<PlayerMovement>();

                if (gsm != null && gsm.PlayerList != null && gsm.PlayerList.Count > 0)
                {
                    var enumerator = gsm.PlayerList.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        PlayerMovement p = enumerator.Current;
                        if (p != null) players.Add(p);
                    }
                }
                else
                {
                    PlayerMovement[] scenePlayers = UnityEngine.Object.FindObjectsOfType<PlayerMovement>();
                    if (scenePlayers != null && scenePlayers.Length > 0)
                    {
                        foreach (var p in scenePlayers)
                        {
                            if (p != null && !players.Contains(p)) players.Add(p);
                        }
                    }
                }

                // Check for RoomScene (OutGame Lobby Player List)
                RoomScene? roomScene = null;
                GameObject roomSceneObj = GameObject.Find("RoomSceneRoot");
                if (roomSceneObj != null)
                {
                    roomScene = roomSceneObj.GetComponent<RoomScene>();
                }

                if (players.Count == 0 && (roomScene == null || roomScene.Matchmaker == null || roomScene.Matchmaker.playerList == null || roomScene.Matchmaker.playerList.Count == 0))
                {
                    ui.Label("<color=#FF8800>No active players or lobby room members detected.</color>");
                    ui.Label("  Wait for room join / game start, or click [Scan Waiting Room] in Game tab.");
                    return;
                }

                // If in OutGame Lobby RoomScene, draw RoomScene Matchmaker Players
                if (roomScene != null && roomScene.Matchmaker != null && roomScene.Matchmaker.playerList != null && roomScene.Matchmaker.playerList.Count > 0)
                {
                    MatchmakerObject matchmaker = roomScene.Matchmaker;
                    ui.Header($"Lobby Room Roster ({matchmaker.playerList.Count} Players)", 22f);

                    var enumerator = matchmaker.playerList.GetEnumerator();
                    int idx = 0;
                    while (enumerator.MoveNext())
                    {
                        IPlayer current = enumerator.Current;
                        if (current == null) continue;
                        idx++;

                        string tag = current.IsLocal ? " <color=#00FFAA>(YOU)</color>" : "";
                        ui.Header($"#{idx} {current.NickName}{tag} (ID: {current.UserId})", 22f);

                        ui.BeginRow(24f);
                        if (!current.IsLocal && ui.RowButton("Turn Off PS (Kick) (H)", 150f))
                        {
                            matchmaker.KickPlayer(current.UserId);
                            MelonLoader.MelonLogger.Msg($"[+] Turned off PS console / Kicked player '{current.NickName}' (ID: {current.UserId}) from lobby!");
                        }
                        if (!current.IsLocal && ui.RowButton("Ban Player (H)", 110f))
                        {
                            try
                            {
                                matchmaker.KickPlayer(current.UserId);
                                MelonLoader.MelonLogger.Msg($"[+] Banned & Kicked player '{current.NickName}' (ID: {current.UserId}) from lobby session!");
                            }
                            catch (Exception ex)
                            {
                                MelonLoader.MelonLogger.Error($"[-] Ban failed for player '{current.NickName}': {ex.Message}");
                            }
                        }
                        if (ui.RowButton("PSN Account", 100f))
                        {
                            Spoofers.ViewPsnProfile(current.NickName);
                        }
                        if (ui.RowButton("Copy Name", 90f))
                        {
                            GUIUtility.systemCopyBuffer = current.NickName;
                        }
                        ui.EndRow();
                        ui.Space(4f);
                    }
                    return;
                }

                // Global In-Game Action Toolbar
                ui.Header($"In-Game Player Roster ({players.Count} Connected)", 22f);
                ui.BeginRow(28f);
                if (ui.RowButton("Kill All (Heart) (H)", 140f)) Executioner.ExecuteAll(true, EDeathType.HeartAttack);
                if (ui.RowButton("Kill All (Truck) (H)", 135f)) Executioner.ExecuteAll(true, EDeathType.Truck);
                if (ui.RowButton("Add All to Notebook", 160f)) InstantNames.AddAllNamesToDeathNote();
                if (ui.RowButton("Warp All To Me (H)", 140f)) Exploits.WarpAllToMe();
                if (ui.RowButton("Handcuff All (H)", 125f)) Exploits.HandcuffAll();
                ui.EndRow();
                ui.Space(4f);

                int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)players.Count / PlayersPerPage));
                if (_currentPage >= totalPages) _currentPage = totalPages - 1;
                if (_currentPage < 0) _currentPage = 0;

                // Pagination Row
                ui.BeginRow(24f);
                if (ui.RowButton("<< Prev", 90f)) if (_currentPage > 0) _currentPage--;
                ui.RowLabel($"<b>Page {_currentPage + 1} / {totalPages} (Total: {players.Count} Players)</b>", 220f);
                if (ui.RowButton("Next >>", 90f)) if (_currentPage < totalPages - 1) _currentPage++;
                ui.EndRow();
                ui.Space(6f);

                PlayerMovement? myPlayer = gsm != null ? gsm.MyPlayer : null;
                Vector3 myPos = myPlayer != null ? myPlayer.GetPosition() : Vector3.zero;

                int startIndex = _currentPage * PlayersPerPage;
                int endIndex = Mathf.Min(startIndex + PlayersPerPage, players.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    PlayerMovement p = players[i];
                    if (p == null) continue;

                    bool isSelf = myPlayer != null && p.ActorID == myPlayer.ActorID;
                    string rawName = p.GetPlayerName(false, true);
                    string cleanName = System.Text.RegularExpressions.Regex.Replace(rawName ?? "Player", "<.*?>", string.Empty);
                    string roleName = p.GetRoleName(false, true);
                    Color rColor = Palette.GetRoleColor(p.RoleDetail);
                    float dist = myPlayer != null ? Vector3.Distance(myPos, p.GetPosition()) : 0f;

                    string selfTag = isSelf ? " <color=#00FFAA>(YOU)</color>" : "";
                    string statusTag = p.IsDead ? " <color=#FF3333>[DEAD]</color>" : " <color=#33FF55>[ALIVE]</color>";
                    string roleTag = $"<color=#{ColorUtility.ToHtmlStringRGB(rColor)}>[{roleName}]</color>";

                    ui.Header($"#{p.ActorID} {cleanName} {roleTag}{selfTag}{statusTag} ({dist:F1}m)", 22f);

                    StringBuilder badges = new StringBuilder();
                    if (p.HasDeathNote) badges.Append("<color=#FF3333>[Death Note]</color> ");
                    if (p.HasOthersID) badges.Append("<color=#FFAA00>[Stolen ID]</color> ");
                    if (p.IsUsingTransceiver) badges.Append("<color=#00FFAA>[Talking]</color> ");

                    if (badges.Length > 0) ui.Label($"  Badges: {badges}", 18f);

                    ui.BeginRow(24f);
                    if (ui.RowButton("Steam Profile", 110f)) Spoofers.ViewSteamProfile(Spoofers.ActiveSpoofedSteamId);
                    if (ui.RowButton("PSN Account", 100f)) Spoofers.ViewPsnProfile(Spoofers.ActivePsnId);
                    if (ui.RowButton("Copy Name", 90f)) GUIUtility.systemCopyBuffer = cleanName;

                    if (!isSelf)
                    {
                        if (ui.RowButton("Kick (H)", 75f)) Exploits.PseudoKick(p);
                        if (ui.RowButton("Ban (H)", 75f)) Exploits.PseudoKick(p);
                        if (ui.RowButton("Warp", 55f)) TeleportManager.TeleportToPlayer(p);
                    }
                    ui.EndRow();

                    if (!isSelf && !p.IsDead)
                    {
                        ui.BeginRow(24f);
                        if (ui.RowButton("Heart Attack", 95f)) Executioner.ExecutePlayer(p, EDeathType.HeartAttack);
                        if (ui.RowButton("Truck", 55f)) Executioner.ExecutePlayer(p, EDeathType.Truck);
                        if (ui.RowButton("Gun", 50f)) Executioner.ExecutePlayer(p, EDeathType.Gun);
                        if (ui.RowButton("Arrest (H)", 75f)) PoliceActions.Arrest(p);
                        ui.EndRow();
                    }

                    ui.Space(4f);
                }
            }
            catch (Exception ex)
            {
                ui.Label($"<color=#FF4444>Player List Render Warning: {ex.Message}</color>");
            }
        }
    }
}
