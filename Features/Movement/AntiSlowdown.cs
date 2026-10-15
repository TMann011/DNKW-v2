using HarmonyLib;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Movement
{
    [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.CalcMoveHindranceRate))]
    public static class AntiSlowdown
    {
        public static bool Prefix(ref float __result)
        {
            if (Config.AntiSlowdown)
            {
                __result = 0.0f;
                return false;
            }
            return true;
        }
    }
}
