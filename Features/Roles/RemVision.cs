using HarmonyLib;
using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Roles
{
    public static class RemVision
    {
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.GetRemVisionPlayers))]
        public static class PlayerMovement_GetRemVisionPlayers
        {
            public static void Postfix(ref Il2CppSystem.Collections.Generic.List<PlayerMovement> __result)
            {
                if (Config.RemVision)
                {
                    GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                    if (gsm != null && gsm.PlayerList != null)
                    {
                        __result = gsm.PlayerList;
                    }
                }
            }
        }

        public static void EnableAllVisions()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null) return;

            gsm.SetCanSeeKiller(true);
            gsm.SetCanSeeDetective(true);
        }
    }
}
