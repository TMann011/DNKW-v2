using HarmonyLib;
using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Roles
{
    public static class AbilityUnlocker
    {
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.HasUnlockedAbility))]
        public static class PlayerMovement_HasUnlockedAbility
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CanUnlockAbility))]
        public static class PlayerMovement_CanUnlockAbility
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.GetUnlockAbilityLevel))]
        public static class PlayerMovement_GetUnlockAbilityLevel
        {
            public static bool Prefix(ref int __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = 1;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.HasUsableCard))]
        public static class PlayerMovement_HasUsableCard
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.IsUsableCard))]
        public static class PlayerMovement_IsUsableCard
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.IsUsableCardRole))]
        public static class PlayerMovement_IsUsableCardRole
        {
            public static bool Prefix(ref bool __result)
            {
                if (Config.UnlockAllAbilities || Config.ForceMaxLevel)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CurrentLevel), MethodType.Getter)]
        public static class PlayerMovement_CurrentLevel_Getter
        {
            public static void Postfix(PlayerMovement __instance, ref int __result)
            {
                if ((Config.UnlockAllAbilities || Config.ForceMaxLevel) && __instance != null && __instance.IsMyPlayer)
                {
                    __result = 10;
                }
            }
        }
    }
}
