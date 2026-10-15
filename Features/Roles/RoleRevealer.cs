using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Roles
{
    public static class RoleRevealer
    {
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.GetRoleName), new Type[] { typeof(bool), typeof(bool) })]
        public static class PlayerMovement_GetRoleName
        {
            public static void Prefix(bool withoutColor, ref bool isRealName)
            {
                if (Config.RevealRoles)
                {
                    isRealName = true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CanSeePlayerRole))]
        public static class PlayerMovement_CanSeePlayerRole
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.RevealRoles)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        public static void RefreshNames()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.PlayerList == null) return;

            var enumerator = gsm.PlayerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerMovement p = enumerator.Current;
                if (p != null && p.nameUI != null)
                {
                    p.nameUI.RefreshName_Normal();
                }
            }
        }
    }
}
