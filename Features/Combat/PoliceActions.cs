using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Combat
{
    public static class PoliceActions
    {
        public static void Arrest(PlayerMovement target)
        {
            if (target == null) return;

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            myPlayer.ArrestPlayer(target);
            target.SetToArrested(myPlayer);
            gsm.ArrestPlayer(myPlayer.ActorID, target.ActorID, true);
        }

        public static void Release(PlayerMovement target)
        {
            if (target == null) return;

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            gsm.ArrestPlayer(gsm.MyPlayer.ActorID, target.ActorID, false);
            gsm.ClearArrest();
        }
    }
}
