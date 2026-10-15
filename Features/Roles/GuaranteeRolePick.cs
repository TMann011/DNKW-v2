using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Roles
{
    public static class GuaranteeRolePick
    {
        public static string[] RoleNames = new string[] { 
            "Kira", 
            "Kira Helper", 
            "X-Kira (2nd Kira)", 
            "2nd Kira Helper", 
            "L Detective", 
            "Watari (L Helper)", 
            "Near (2nd Detective)", 
            "Investigator", 
            "Strong Investigator", 
            "Mello (Terrorist)" 
        };

        public static EPlayerRoles[] RoleEnums = new EPlayerRoles[] { 
            EPlayerRoles.Killer, 
            EPlayerRoles.Killer, 
            EPlayerRoles.Killer, 
            EPlayerRoles.Killer, 
            EPlayerRoles.Detective, 
            EPlayerRoles.Detective, 
            EPlayerRoles.Detective, 
            EPlayerRoles.Detective, 
            EPlayerRoles.Detective, 
            EPlayerRoles.Terrorist 
        };

        public static EPlayerRoleDetail[] DetailEnums = new EPlayerRoleDetail[] {
            EPlayerRoleDetail.Killer,
            EPlayerRoleDetail.KillerHelp,
            EPlayerRoleDetail.Killer_2,
            EPlayerRoleDetail.KillerHelp_2,
            EPlayerRoleDetail.Detective,
            EPlayerRoleDetail.Detective_Help,
            EPlayerRoleDetail.Detective_2,
            EPlayerRoleDetail.Investigator,
            EPlayerRoleDetail.Investigator_Strong,
            EPlayerRoleDetail.Terrorist
        };

        // Resolve the local player's network GameID across available network & scene managers
        public static int GetLocalGameID()
        {
            try
            {
                var selectWindow = UnityEngine.Object.FindObjectOfType<SelectRoleWindow>();
                if (selectWindow != null && selectWindow.myPlayer != null)
                {
                    return selectWindow.myPlayer.GameID;
                }
            }
            catch { }

            try
            {
                if (Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer != null)
                {
                    return Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
                }
            }
            catch { }

            try
            {
                var gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null && gsm.MyPlayer != null)
                {
                    return gsm.MyPlayer.ActorID;
                }
            }
            catch { }

            return -1;
        }

        // Accurately verify if a PlayerMovement instance is truly the local player, preventing false positives on prefabs
        public static bool IsTrulyLocalPlayer(PlayerMovement player)
        {
            if (player == null) return false;
            try
            {
                var gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null && gsm.MyPlayer != null)
                {
                    return gsm.MyPlayer.Pointer == player.Pointer;
                }

                if (Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer != null && player.ActorID > 0)
                {
                    return player.ActorID == Il2CppPhoton.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
                }

                var pv = player.GetComponent<Il2CppPhoton.Pun.PhotonView>();
                if (pv != null)
                {
                    return pv.IsMine;
                }
            }
            catch { }
            return player.IsMyPlayer;
        }

        // 1. Pre-selected initial role when UI opens
        [HarmonyPatch(typeof(SelectRoleContent), nameof(SelectRoleContent.GetInitialRoleData))]
        public static class GuaranteeRole_GetInitialRoleData_Patch
        {
            public static void Postfix(ref RoleData __result)
            {
                if (Config.GuaranteeRole)
                {
                    int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                    __result = new RoleData(RoleEnums[idx], DetailEnums[idx]);
                }
            }
        }

        // 2. Bypass UI validation: allow local player to select any role in the UI even if locked/full
        [HarmonyPatch(typeof(SelectRoleContent), nameof(SelectRoleContent.SelectRoleIsValid))]
        public static class GuaranteeRole_SelectRoleIsValid_Patch
        {
            public static bool Prefix(RoleData roleData, ref bool __result)
            {
                if (Config.GuaranteeRole)
                {
                    __result = true;
                    return false; // Bypass default lock/full check
                }
                return true;
            }
        }

        // 3. Pre-game auto-selection & decision in UI (safely triggers button click)
        [HarmonyPatch(typeof(SelectRoleContent), nameof(SelectRoleContent.Update))]
        public static class GuaranteeRole_SelectRoleContent_Update_Patch
        {
            private static float _nextTryTime = 0f;

            public static void Postfix(SelectRoleContent __instance)
            {
                if (!Config.GuaranteeRole || __instance == null || __instance.buttons == null) return;

                if (!__instance.IsSelectedRole && Time.time > _nextTryTime)
                {
                    _nextTryTime = Time.time + 0.5f;
                    int idx = Config.SelectedRoleIndex % DetailEnums.Length;
                    EPlayerRoleDetail targetDetail = DetailEnums[idx];
                    EPlayerRoles targetRole = RoleEnums[idx];

                    for (int i = 0; i < __instance.buttons.Count; i++)
                    {
                        var btn = __instance.buttons[i];
                        if (btn != null && btn.RoleType == targetDetail && !btn.IsRandom)
                        {
                            try
                            {
                                __instance.CurrentSelectButton = btn;
                                __instance.ButtonPressed(btn);
                                __instance.SetCurrentSelectRole(new RoleData(targetRole, targetDetail), true);
                                __instance.DecideSelectRole();
                                MelonLogger.Msg($"[Guarantee Role] Pre-game role auto-selected and locked with host: {targetDetail}");
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Warning($"[Guarantee Role] Button press error: {ex.Message}");
                            }
                            break;
                        }
                    }
                }
            }
        }

        // 3. Pre-game role assignment lottery interception:
        // Ensures local player's desired role is awarded and SWAPS roles with any competing player
        // so total team counts remain balanced and NEVER create duplicate Kiras or Ls.
        [HarmonyPatch(typeof(SelectRoleUtility), nameof(SelectRoleUtility.GetAssignRoleList))]
        public static class GuaranteeRole_GetAssignRoleList_Patch
        {
            public static void Prefix(
                Il2CppReferenceArray<SelectedRoleData> selectedRoleList, 
                int gamePlayerCount, 
                Il2CppSystem.Collections.Generic.List<SelectRoleButtonData> RoleButtonDataList)
            {
                if (!Config.GuaranteeRole || selectedRoleList == null) return;
                try
                {
                    int myId = GetLocalGameID();
                    if (myId < 0) return;

                    int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                    EPlayerRoles targetRole = RoleEnums[idx];
                    EPlayerRoleDetail targetDetail = DetailEnums[idx];

                    for (int i = 0; i < selectedRoleList.Length; i++)
                    {
                        var entry = selectedRoleList[i];
                        if (entry != null && entry.GameID == myId)
                        {
                            entry.RoleData = new RoleData(targetRole, targetDetail);
                            entry.IsSelfSelected = true;
                            MelonLogger.Msg($"[Guarantee Role] Pre-lottery pool entry set: Actor #{myId} -> {targetRole} ({targetDetail})");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[Guarantee Role] GetAssignRoleList Prefix error: {ex.Message}");
                }
            }

            public static void Postfix(
                Il2CppReferenceArray<SelectedRoleData> selectedRoleList, 
                int gamePlayerCount, 
                Il2CppSystem.Collections.Generic.List<SelectRoleButtonData> RoleButtonDataList, 
                ref Il2CppSystem.Collections.Generic.List<SelectedRoleData> __result)
            {
                if (!Config.GuaranteeRole || __result == null) return;
                try
                {
                    int myId = GetLocalGameID();
                    if (myId < 0) return;

                    int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                    EPlayerRoles targetRole = RoleEnums[idx];
                    EPlayerRoleDetail targetDetail = DetailEnums[idx];

                    SelectedRoleData? myEntry = null;
                    SelectedRoleData? targetHolderEntry = null;

                    for (int i = 0; i < __result.Count; i++)
                    {
                        var entry = __result[i];
                        if (entry == null) continue;

                        if (entry.GameID == myId)
                        {
                            myEntry = entry;
                        }
                        else if (entry.RoleData != null && entry.RoleData.RoleDetail == targetDetail)
                        {
                            targetHolderEntry = entry;
                        }
                    }

                    if (myEntry != null)
                    {
                        if (myEntry.RoleData != null && myEntry.RoleData.RoleDetail == targetDetail)
                        {
                            MelonLogger.Msg($"[Guarantee Role] Local player naturally assigned desired role: {targetRole} ({targetDetail})");
                            return;
                        }

                        // Swap roles to preserve clean match balance and prevent duplicate roles
                        if (targetHolderEntry != null)
                        {
                            RoleData? myOldRole = myEntry.RoleData;
                            myEntry.SetRoleData(new RoleData(targetRole, targetDetail));
                            myEntry.IsSelfSelected = true;

                            RoleData swappedRole = myOldRole ?? new RoleData(EPlayerRoles.Detective, EPlayerRoleDetail.Investigator);
                            targetHolderEntry.SetRoleData(swappedRole);

                            MelonLogger.Msg($"[Guarantee Role] Role Swap Resolved: Local player #{myId} awarded {targetRole} ({targetDetail}). Actor #{targetHolderEntry.GameID} swapped to {swappedRole.RoleDetail}. No duplicate roles created.");
                        }
                        else
                        {
                            myEntry.SetRoleData(new RoleData(targetRole, targetDetail));
                            myEntry.IsSelfSelected = true;
                            MelonLogger.Msg($"[Guarantee Role] Assigned desired role {targetRole} ({targetDetail}) to local player #{myId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[Guarantee Role] GetAssignRoleList Postfix error: {ex.Message}");
                }
            }
        }

        // 4. Pre-game network result synchronization hook (SelectRoleScene.SendResult)
        [HarmonyPatch(typeof(SelectRoleScene), nameof(SelectRoleScene.SendResult))]
        public static class GuaranteeRole_SendResult_Patch
        {
            public static void Prefix(Il2CppReferenceArray<SelectedRoleData> selectedRoleList)
            {
                if (!Config.GuaranteeRole || selectedRoleList == null) return;
                try
                {
                    int myId = GetLocalGameID();
                    if (myId < 0) return;

                    int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                    EPlayerRoles targetRole = RoleEnums[idx];
                    EPlayerRoleDetail targetDetail = DetailEnums[idx];

                    SelectedRoleData? myEntry = null;
                    SelectedRoleData? targetHolderEntry = null;

                    for (int i = 0; i < selectedRoleList.Length; i++)
                    {
                        var entry = selectedRoleList[i];
                        if (entry == null) continue;

                        if (entry.GameID == myId)
                        {
                            myEntry = entry;
                        }
                        else if (entry.RoleData != null && entry.RoleData.RoleDetail == targetDetail)
                        {
                            targetHolderEntry = entry;
                        }
                    }

                    if (myEntry != null && (myEntry.RoleData == null || myEntry.RoleData.RoleDetail != targetDetail))
                    {
                        if (targetHolderEntry != null)
                        {
                            RoleData? myOld = myEntry.RoleData;
                            myEntry.SetRoleData(new RoleData(targetRole, targetDetail));
                            myEntry.IsSelfSelected = true;

                            RoleData swapped = myOld ?? new RoleData(EPlayerRoles.Detective, EPlayerRoleDetail.Investigator);
                            targetHolderEntry.SetRoleData(swapped);
                            MelonLogger.Msg($"[Guarantee Role] SendResult broadcast swapped role with Actor #{targetHolderEntry.GameID}");
                        }
                        else
                        {
                            myEntry.SetRoleData(new RoleData(targetRole, targetDetail));
                            myEntry.IsSelfSelected = true;
                            MelonLogger.Msg($"[Guarantee Role] SendResult broadcast set local role to {targetRole} ({targetDetail})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[Guarantee Role] SendResult Prefix error: {ex.Message}");
                }
            }
        }

        // 5. InGameSettings synchronization
        [HarmonyPatch(typeof(InGameSettings), nameof(InGameSettings.SetRole))]
        public static class GuaranteeRole_InGameSettings_SetRole_Patch
        {
            public static void Prefix(ref EPlayerRoles role)
            {
                if (Config.GuaranteeRole)
                {
                    int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                    role = RoleEnums[idx];
                }
            }
        }

        [HarmonyPatch(typeof(InGameSettings), nameof(InGameSettings.SetRoleDetail))]
        public static class GuaranteeRole_InGameSettings_SetRoleDetail_Patch
        {
            public static void Prefix(ref EPlayerRoleDetail roleDetail)
            {
                if (Config.GuaranteeRole)
                {
                    int idx = Config.SelectedRoleIndex % DetailEnums.Length;
                    roleDetail = DetailEnums[idx];
                }
            }
        }

        // 6. Local Player Movement Role Initialization & Spawning (Guest & Host)
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.SetupInitialRole))]
        public static class GuaranteeRole_SetupInitialRole_Patch
        {
            public static void Prefix(PlayerMovement __instance, ref EPlayerRoles roles, ref EPlayerRoleDetail roleDetail)
            {
                if (!Config.GuaranteeRole || __instance == null) return;

                int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                EPlayerRoles targetRole = RoleEnums[idx];
                EPlayerRoleDetail targetDetail = DetailEnums[idx];

                if (IsTrulyLocalPlayer(__instance))
                {
                    roles = targetRole;
                    roleDetail = targetDetail;
                    MelonLogger.Msg($"[Guarantee Role] Spawn override: SetupInitialRole forced to {roles} ({roleDetail})");
                }
                else
                {
                    // If a remote player was assigned the same unique role, demote them to avoid duplicates
                    if (roleDetail == targetDetail && (targetRole == EPlayerRoles.Killer || targetRole == EPlayerRoles.Terrorist || targetDetail == EPlayerRoleDetail.Detective))
                    {
                        roles = EPlayerRoles.Detective;
                        roleDetail = EPlayerRoleDetail.Investigator;
                        MelonLogger.Msg($"[Guarantee Role] Demoted remote duplicate Actor #{__instance.ActorID} to Investigator in SetupInitialRole.");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.SetRoleAndRoleDetail))]
        public static class GuaranteeRole_SetRoleAndRoleDetail_Patch
        {
            public static void Prefix(PlayerMovement __instance, ref EPlayerRoles role, ref EPlayerRoleDetail roleDetail)
            {
                if (!Config.GuaranteeRole || __instance == null) return;

                int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                EPlayerRoles targetRole = RoleEnums[idx];
                EPlayerRoleDetail targetDetail = DetailEnums[idx];

                if (IsTrulyLocalPlayer(__instance))
                {
                    role = targetRole;
                    roleDetail = targetDetail;
                    MelonLogger.Msg($"[Guarantee Role] Spawn override: SetRoleAndRoleDetail forced to {role} ({roleDetail})");
                }
                else
                {
                    if (roleDetail == targetDetail && (targetRole == EPlayerRoles.Killer || targetRole == EPlayerRoles.Terrorist || targetDetail == EPlayerRoleDetail.Detective))
                    {
                        role = EPlayerRoles.Detective;
                        roleDetail = EPlayerRoleDetail.Investigator;
                        MelonLogger.Msg($"[Guarantee Role] Demoted remote duplicate Actor #{__instance.ActorID} to Investigator in SetRoleAndRoleDetail.");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.OnSetRoleAndRoleDetail))]
        public static class GuaranteeRole_OnSetRoleAndRoleDetail_Patch
        {
            public static void Prefix(PlayerMovement __instance, ref EPlayerRoles role, ref EPlayerRoleDetail roleDetail)
            {
                if (!Config.GuaranteeRole || __instance == null) return;

                int idx = Config.SelectedRoleIndex % RoleEnums.Length;
                EPlayerRoles targetRole = RoleEnums[idx];
                EPlayerRoleDetail targetDetail = DetailEnums[idx];

                if (IsTrulyLocalPlayer(__instance))
                {
                    role = targetRole;
                    roleDetail = targetDetail;
                    MelonLogger.Msg($"[Guarantee Role] Network override: OnSetRoleAndRoleDetail forced to {role} ({roleDetail})");
                }
                else
                {
                    if (roleDetail == targetDetail && (targetRole == EPlayerRoles.Killer || targetRole == EPlayerRoles.Terrorist || targetDetail == EPlayerRoleDetail.Detective))
                    {
                        role = EPlayerRoles.Detective;
                        roleDetail = EPlayerRoleDetail.Investigator;
                        MelonLogger.Msg($"[Guarantee Role] Demoted remote duplicate Actor #{__instance.ActorID} to Investigator in OnSetRoleAndRoleDetail.");
                    }
                }
            }
        }

        // Anti-ID Grab Patch 1: Block GivePlayerDoc
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.GivePlayerDoc), new Type[] { typeof(int), typeof(int) })]
        public static class AntiIdGrab_GivePlayerDoc
        {
            public static bool Prefix(PlayerMovement __instance, int fromActor, int docActor)
            {
                if (Config.AntiIdGrab && __instance != null && __instance.IsMyPlayer)
                {
                    MelonLogger.Msg($"[Anti-ID Grab] Blocked GivePlayerDoc from Actor #{fromActor}");
                    return false;
                }
                return true;
            }
        }

        // Anti-ID Grab Patch 2: Block AddPlayerInfoList for local player
        [HarmonyPatch(typeof(PlayerMovement), "AddPlayerInfoList", new Type[] { typeof(PlayerMovement) })]
        public static class AntiIdGrab_AddPlayerInfoList
        {
            public static bool Prefix(PlayerMovement __instance, PlayerMovement playerMovement)
            {
                if (Config.AntiIdGrab && playerMovement != null && playerMovement.IsMyPlayer)
                {
                    MelonLogger.Msg("[Anti-ID Grab] Blocked AddPlayerInfoList for local player");
                    return false;
                }
                return true;
            }
        }

        // Anti-ID Grab Patch 3: Block CanStealName interaction check
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CanStealName), MethodType.Getter)]
        public static class AntiIdGrab_CanStealName
        {
            public static bool Prefix(PlayerMovement __instance, ref bool __result)
            {
                if (Config.AntiIdGrab && __instance != null && __instance.IsMyPlayer)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Anti-ID Grab Patch 4: Block external SetInfoProgressRate
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.SetInfoProgressRate))]
        public static class AntiIdGrab_SetInfoProgressRate
        {
            public static bool Prefix(PlayerMovement __instance)
            {
                if (Config.AntiIdGrab && __instance != null && __instance.IsMyPlayer)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
