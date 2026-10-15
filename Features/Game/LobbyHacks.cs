using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using Il2CppGR;
using Il2CppGRNetwork;
using Il2CppLunaOutGame;
using Il2CppPhoton.Pun;
using Il2CppPhoton.Realtime;
using Il2CppSteamworks;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Game
{
    public static class LobbyHacks
    {
        public static string CustomRoomCode { get; set; } = "";
        public static bool JoinAsSpectator { get; set; } = false;

        public struct FriendSessionInfo
        {
            public string Name;
            public CSteamID SteamId;
            public string RoomCode;
            public bool IsPlayingDNKW;
        }

        public static List<FriendSessionInfo> OnlineFriends { get; } = new List<FriendSessionInfo>();

        // 1. Harmony patch to override private room password / lock checks (safe non-blocking)
        [HarmonyPatch(typeof(MatchmakerObject), nameof(MatchmakerObject.IsTargetInRoom))]
        public static class PrivateRoomBypassPatch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = true; // Force room entry access / unlock
                return false;
            }
        }

        // 2. Allows entering rooms regardless of capacity, password, or active match state
        [HarmonyPatch(typeof(MatchmakerObject), nameof(MatchmakerObject.EvaluatePlayerCanEnterRoom), new Type[] { typeof(IPlayer) })]
        public static class EvaluatePlayerCanEnterRoomPatch
        {
            public static bool Prefix(MatchmakerObject __instance, IPlayer player, ref bool __result)
            {
                if (Config.BypassBlockRestriction || Config.JoinActiveLobbies)
                {
                    __result = true; // Bypass lock / capacity / block / game-in-progress restrictions
                    return false;
                }
                return true;
            }
        }

        // 3. Platform Block Introspection Bypass - Host or player blocked check
        [HarmonyPatch(typeof(MatchmakerObject), nameof(MatchmakerObject.IsPlatformBlocked_PlayerAnyBlock))]
        public static class MatchmakerObject_IsPlatformBlocked_PlayerAnyBlock_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.BypassBlockRestriction)
                {
                    __result = false; // Never report as blocked
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MatchmakerObject), nameof(MatchmakerObject.IsPlatformBlocked_AnyPlayerBlock))]
        public static class MatchmakerObject_IsPlatformBlocked_AnyPlayerBlock_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.BypassBlockRestriction)
                {
                    __result = false; // Never report as blocked
                    return false;
                }
                return true;
            }
        }

        // 4. Suppress Blocked Player error dialogs
        [HarmonyPatch(typeof(Il2CppGR.ErrorUtils), nameof(Il2CppGR.ErrorUtils.ShowErrorMessage_BlockedPlayer), new Type[] { typeof(IPlayer) })]
        public static class ErrorUtils_ShowErrorMessage_BlockedPlayer_Patch1
        {
            public static bool Prefix()
            {
                return !Config.BypassBlockRestriction;
            }
        }

        [HarmonyPatch(typeof(Il2CppGR.ErrorUtils), nameof(Il2CppGR.ErrorUtils.ShowErrorMessage_BlockedPlayer), new Type[] { typeof(string), typeof(EPlatform), typeof(string) })]
        public static class ErrorUtils_ShowErrorMessage_BlockedPlayer_Patch2
        {
            public static bool Prefix()
            {
                return !Config.BypassBlockRestriction;
            }
        }

        // 5. Force RoomSearchUI to display all rooms including in-progress / active matches
        [HarmonyPatch(typeof(RoomSearchUI), nameof(RoomSearchUI.IsMatchRoomCondition))]
        public static class RoomSearchUI_IsMatchRoomCondition_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (Config.ShowActiveLobbies || Config.JoinActiveLobbies)
                {
                    __result = true;
                }
            }
        }

        // 6. Intercept Photon JoinRoom / Room verification to allow joining full and active games
        [HarmonyPatch(typeof(MatchmakerObject), nameof(MatchmakerObject.JoinRoom), new Type[] { typeof(string), typeof(EWantRoleType), typeof(bool) })]
        public static class MatchmakerObject_JoinRoom_Patch
        {
            public static void Prefix(MatchmakerObject __instance, ref string roomName, ref EWantRoleType wantRole, ref bool enableViewCode)
            {
                try
                {
                    if (JoinAsSpectator)
                    {
                        wantRole = (EWantRoleType)10; // Spectator role index
                        MelonLogger.Msg($"[LobbyHacks] Overriding JoinRoom '{roomName}' with Spectator flag (Role 10).");
                    }
                }
                catch { }
            }
        }

        public static void ForceJoinActiveLobby(bool activeOnly = false, bool asSpectator = false)
        {
            try
            {
                JoinAsSpectator = asSpectator;
                string modeLabel = activeOnly ? "ACTIVE IN-PROGRESS MATCH" : "AVAILABLE LOBBY";
                string roleLabel = asSpectator ? "as SPECTATOR" : "as PLAYER";
                MelonLogger.Msg($"[LobbyHacks] Searching and quick-joining {modeLabel} {roleLabel}...");

                EWantRoleType roleType = asSpectator ? (EWantRoleType)10 : (EWantRoleType)0;

                // 1. Check RoomSearchUI cached room list
                RoomSearchUI roomSearch = UnityEngine.Object.FindObjectOfType<RoomSearchUI>();
                if (roomSearch != null)
                {
                    var roomList = roomSearch.roomInfoList ?? roomSearch.lastRoomList;
                    if (roomList != null && roomList.Count > 0)
                    {
                        IRoomInfo? selectedRoom = null;
                        for (int i = 0; i < roomList.Count; i++)
                        {
                            var r = roomList[i];
                            if (r == null) continue;
                            if (activeOnly)
                            {
                                if (!r.IsOpen || r.PlayerCount >= 4)
                                {
                                    selectedRoom = r;
                                    break;
                                }
                            }
                            else
                            {
                                if (r.IsOpen && r.PlayerCount < r.MaxPlayers)
                                {
                                    selectedRoom = r;
                                    break;
                                }
                            }
                        }

                        // If no exact match, take first available room for active mode
                        if (selectedRoom == null && activeOnly && roomList.Count > 0)
                        {
                            selectedRoom = roomList[0];
                        }

                        if (selectedRoom != null)
                        {
                            MelonLogger.Msg($"[+] Found room '{selectedRoom.Name}' (Players: {selectedRoom.PlayerCount}/{selectedRoom.MaxPlayers}, IsOpen: {selectedRoom.IsOpen}). Joining {roleLabel}...");
                            JoinPrivateRoom(selectedRoom.Name, asSpectator);
                            return;
                        }
                    }

                    // Trigger room search refresh
                    try
                    {
                        var searchMethod = typeof(RoomSearchUI).GetMethod("OnClickedRoomSearchButton", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (searchMethod != null)
                        {
                            searchMethod.Invoke(roomSearch, null);
                            MelonLogger.Msg("[+] OnClickedRoomSearchButton triggered to refresh room list.");
                        }
                    }
                    catch { }
                }

                // 2. Direct MatchmakerObject (works from any scene)
                var matchmaker = UnityEngine.Object.FindObjectOfType<MatchmakerObject>();
                if (matchmaker != null)
                {
                    MelonLogger.Msg("[LobbyHacks] Found MatchmakerObject directly.");

                    // Try cached room list from matchmaker
                    try
                    {
                        matchmaker.RequestRoomList();
                        MelonLogger.Msg("[+] Dispatched RequestRoomList to MatchmakerObject.");
                    }
                    catch { }

                    // If connected to a server, try joining any available room
                    try
                    {
                        if (matchmaker.IsConnectedServer)
                        {
                            // For active matches, try joining a random room via Photon directly
                            if (PhotonNetwork.IsConnectedAndReady)
                            {
                                MelonLogger.Msg("[+] Photon connected. Attempting JoinRandomRoom...");
                                TryPhotonJoinRandom();
                                return;
                            }
                        }
                        else
                        {
                            matchmaker.ConnectToServer();
                            MelonLogger.Msg("[+] Not connected. Dispatched ConnectToServer. Try again in a moment.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[LobbyHacks] MatchmakerObject join attempt: {ex.Message}");
                    }
                }

                // 3. RoomScene MatchmakerObject fallback
                try
                {
                    GameObject roomSceneObj = GameObject.Find("RoomSceneRoot");
                    if (roomSceneObj != null)
                    {
                        RoomScene roomScene = roomSceneObj.GetComponent<RoomScene>();
                        if (roomScene != null && roomScene.Matchmaker != null)
                        {
                            if (!roomScene.Matchmaker.IsConnectedServer)
                            {
                                roomScene.Matchmaker.ConnectToServer();
                            }
                            roomScene.Matchmaker.RequestRoomList();
                            MelonLogger.Msg("[+] Dispatched RequestRoomList via RoomScene.Matchmaker.");
                        }
                    }
                }
                catch { }

                // 4. Photon direct fallback (both active and open modes)
                try
                {
                    if (PhotonNetwork.IsConnectedAndReady)
                    {
                        MelonLogger.Msg("[+] Photon connected. Attempting JoinRandomRoom fallback...");
                        TryPhotonJoinRandom();
                        return;
                    }
                    else
                    {
                        MelonLogger.Msg("[LobbyHacks] Photon not connected. Ensure you are on Room Search / Matchmaking screen first.");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[LobbyHacks] Photon JoinRandomRoom: {ex.Message}");
                }

                MelonLogger.Msg("[LobbyHacks] Room query dispatched. Navigate to Room Search screen and try again.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] ForceJoinActiveLobby failed: {ex.Message}");
            }
        }

        private static void TryPhotonJoinRandom()
        {
            try
            {
                // Use reflection to call JoinRandomRoom to avoid Hashtable assembly dependency
                var pnType = typeof(PhotonNetwork);
                
                // Try zero-arg overload first
                var m0 = pnType.GetMethod("JoinRandomRoom", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (m0 != null)
                {
                    m0.Invoke(null, null);
                    MelonLogger.Msg("[+] JoinRandomRoom() dispatched via Photon.");
                    return;
                }

                // Try any overload and pass nulls/defaults
                var methods = pnType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (var m in methods)
                {
                    if (m.Name == "JoinRandomRoom")
                    {
                        var parms = m.GetParameters();
                        object?[] args = new object?[parms.Length];
                        for (int i = 0; i < parms.Length; i++)
                        {
                            var pt = parms[i].ParameterType;
                            if (pt.IsValueType) args[i] = Activator.CreateInstance(pt);
                            else args[i] = null;
                        }
                        m.Invoke(null, args);
                        MelonLogger.Msg($"[+] JoinRandomRoom({parms.Length} args) dispatched via Photon.");
                        return;
                    }
                }

                MelonLogger.Warning("[LobbyHacks] JoinRandomRoom method not found on PhotonNetwork.");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[LobbyHacks] TryPhotonJoinRandom failed: {ex.Message}");
            }
        }

        public static void JoinPrivateRoom(string roomCode, bool asSpectator = false)
        {
            if (string.IsNullOrWhiteSpace(roomCode)) return;
            string cleanCode = roomCode.Trim().ToUpper();
            try
            {
                JoinAsSpectator = asSpectator;
                EWantRoleType roleType = asSpectator ? (EWantRoleType)10 : (EWantRoleType)0;
                string modeStr = asSpectator ? "as SPECTATOR" : "as PLAYER";
                MelonLogger.Msg($"[LobbyHacks] Joining room '{cleanCode}' {modeStr}...");
                
                // 1. RoomScene MatchmakerObject
                GameObject roomSceneObj = GameObject.Find("RoomSceneRoot");
                if (roomSceneObj != null)
                {
                    RoomScene roomScene = roomSceneObj.GetComponent<RoomScene>();
                    if (roomScene != null && roomScene.Matchmaker != null)
                    {
                        MelonLogger.Msg($"[+] Invoking MatchmakerObject.JoinRoom('{cleanCode}', {roleType})...");
                        roomScene.Matchmaker.JoinRoom(cleanCode, roleType, false);
                        MelonLogger.Msg("[+] JoinRoom request sent.");
                        return;
                    }
                }

                // 2. Direct MatchmakerObject scan
                var matchmaker = UnityEngine.Object.FindObjectOfType<MatchmakerObject>();
                if (matchmaker != null)
                {
                    MelonLogger.Msg($"[+] Found MatchmakerObject directly. Invoking JoinRoom('{cleanCode}')...");
                    matchmaker.JoinRoom(cleanCode, roleType, false);
                    return;
                }

                // 3. RoomSearchUI
                RoomSearchUI roomSearch = UnityEngine.Object.FindObjectOfType<RoomSearchUI>();
                if (roomSearch != null)
                {
                    var reqMethod = typeof(RoomSearchUI).GetMethod("OnRequestJoinRoomFromInputCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (reqMethod != null)
                    {
                        reqMethod.Invoke(roomSearch, new object[] { cleanCode, roleType });
                        MelonLogger.Msg($"[+] OnRequestJoinRoomFromInputCode('{cleanCode}') executed.");
                        return;
                    }
                }

                // 4. PhotonNetwork direct fallback
                try
                {
                    PhotonNetwork.JoinRoom(cleanCode, null);
                    MelonLogger.Msg($"[+] Dispatched direct PhotonNetwork.JoinRoom('{cleanCode}')!");
                    return;
                }
                catch { }

                MelonLogger.Warning("[-] Navigation required: Please navigate to Room Search / Matchmaking screen.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] JoinPrivateRoom failed: {ex.Message}");
            }
        }

        // Query Steam friends playing Death Note Killer Within and retrieve session codes
        public static void RefreshFriendsList()
        {
            OnlineFriends.Clear();
            try
            {
                int count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                MelonLogger.Msg($"[LobbyHacks] Scanning {count} Steam friends for active DNKW games...");

                for (int i = 0; i < count; i++)
                {
                    CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    string name = SteamFriends.GetFriendPersonaName(friendId);
                    FriendGameInfo_t gameInfo = default;

                    bool inGame = SteamFriends.GetFriendGamePlayed(friendId, out gameInfo);
                    if (inGame)
                    {
                        string richRoom = SteamFriends.GetFriendRichPresence(friendId, "room");
                        if (string.IsNullOrEmpty(richRoom))
                        {
                            richRoom = SteamFriends.GetFriendRichPresence(friendId, "connect");
                        }
                        if (string.IsNullOrEmpty(richRoom))
                        {
                            richRoom = SteamFriends.GetFriendRichPresence(friendId, "status");
                        }

                        OnlineFriends.Add(new FriendSessionInfo
                        {
                            Name = name,
                            SteamId = friendId,
                            RoomCode = richRoom ?? "",
                            IsPlayingDNKW = true
                        });
                        MelonLogger.Msg($"  -> Friend '{name}' playing game (Presence: '{richRoom}')");
                    }
                }
                MelonLogger.Msg($"[LobbyHacks] Scan complete: Found {OnlineFriends.Count} friends in game.");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[LobbyHacks] Steam friends scan exception: {ex.Message}");
            }
        }

        public static void JoinFriendGame(FriendSessionInfo friend, bool asSpectator = false)
        {
            try
            {
                MelonLogger.Msg($"[LobbyHacks] Attempting join for friend '{friend.Name}' ({friend.SteamId.m_SteamID})...");

                if (!string.IsNullOrWhiteSpace(friend.RoomCode))
                {
                    JoinPrivateRoom(friend.RoomCode, asSpectator);
                    return;
                }

                // If no direct room string in rich presence, trigger platform invitation / friend join
                var matchmaker = UnityEngine.Object.FindObjectOfType<MatchmakerObject>();
                if (matchmaker != null)
                {
                    MelonLogger.Msg("[+] Dispatching platform join through MatchmakerObject...");
                    matchmaker.JoinRoom(friend.Name, asSpectator ? (EWantRoleType)10 : (EWantRoleType)0, false);
                }
                else
                {
                    MelonLogger.Warning("[-] Matchmaker inactive. Open Room Search screen.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] JoinFriendGame failed: {ex.Message}");
            }
        }

        public static void ForceStartMatch()
        {
            try
            {
                MelonLogger.Msg("[LobbyHacks] Bypassing host checks to force start match...");
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null)
                {
                    MelonLogger.Msg("[+] Dispatched GameStart trigger to active room!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] ForceStartMatch failed: {ex.Message}");
            }
        }

        public static void ScanWaitingLobbyPlayers()
        {
            try
            {
                PlayerMovement[] players = UnityEngine.Object.FindObjectsOfType<PlayerMovement>();
                int count = players != null ? players.Length : 0;
                MelonLogger.Msg($"[LobbyHacks] Scanned Waiting Room: Found {count} player entities in lobby.");
                if (players != null)
                {
                    foreach (var p in players)
                    {
                        if (p != null)
                        {
                            string pName = p.GetPlayerName(false, true);
                            MelonLogger.Msg($"  -> Lobby Member #{p.ActorID}: '{pName}' | Pos: {p.GetPosition()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] ScanWaitingLobbyPlayers failed: {ex.Message}");
            }
        }
    }
}
