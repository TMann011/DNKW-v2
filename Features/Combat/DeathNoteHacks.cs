using System;
using HarmonyLib;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Combat
{
    public static class DeathNoteHacks
    {
        [HarmonyPatch(typeof(DeathNoteUI), nameof(DeathNoteUI.Update))]
        public static class DeathNoteUI_Update
        {
            public static void Prefix(DeathNoteUI __instance)
            {
                if (Config.DeathNoteAlwaysKill)
                {
                    __instance.hasScrapNoteCommandCount = 1;
                }
            }
        }

        [HarmonyPatch(typeof(DeathNotePlayerSelectUI), nameof(DeathNotePlayerSelectUI.Update))]
        public static class DeathNotePlayerSelectUI_Update
        {
            public static void Postfix(DeathNotePlayerSelectUI __instance)
            {
                if (Config.DeathNoteAlwaysKill)
                {
                    __instance.cantKillType = (DeathNotePlayerSelectUI.CantKillType)0;
                    if (__instance.CantKillRoot != null)
                    {
                        __instance.CantKillRoot.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CanKillPlayer), new Type[] { typeof(PlayerMovement) })]
        public static class PlayerMovement_CanKillPlayer
        {
            public static bool Prefix(PlayerMovement __instance, PlayerMovement target, ref bool __result)
            {
                if (Config.DeathNoteAlwaysKill)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
