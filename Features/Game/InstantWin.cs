using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Game
{
    public static class InstantWin
    {
        public static void WinAsKira()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm != null)
            {
                gsm.SetWinner(GameStateManager.EWinner.Killer, GameStateManager.EWinReason.AchieveMaxLevelKiller, 0, false);
            }
        }

        public static void WinAsL()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm != null)
            {
                gsm.SetWinner(GameStateManager.EWinner.Detective, GameStateManager.EWinReason.AchieveMaxLevelDetective, 0, false);
            }
        }

        public static void WinAsTerrorist()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm != null)
            {
                gsm.SetWinner(GameStateManager.EWinner.Terrorist, GameStateManager.EWinReason.OpponentKilled_ByTerrorist, 0, false);
            }
        }
    }
}
