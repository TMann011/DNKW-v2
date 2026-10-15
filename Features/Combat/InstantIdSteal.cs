using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Combat
{
    public static class InstantIdSteal
    {
        public static void MaximizeInfoProgress()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            gsm.MyPlayer.SetInfoProgressRate(100f, true);
        }

        public static void Update()
        {
            if (!Config.InstantIdSteal) return;

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            gsm.MyPlayer.SetInfoProgressRate(100f, false);
        }
    }
}
