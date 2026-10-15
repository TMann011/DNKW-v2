using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Combat
{
    public static class Executioner
    {
        public static void ExecutePlayer(PlayerMovement target, EDeathType deathType = EDeathType.HeartAttack)
        {
            if (target == null || target.IsDead) return;

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            int killerId = gsm?.MyPlayer != null ? gsm.MyPlayer.ActorID : 0;

            target.KillPlayer(false, killerId, deathType, -1, (CharacterModelBase.EDeathAnim)0, false, false);
            target.IsDead = true;
        }

        public static void ExecuteAll(bool excludeSelf = true, EDeathType deathType = EDeathType.HeartAttack)
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.PlayerList == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            var enumerator = gsm.PlayerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerMovement p = enumerator.Current;
                if (p == null || p.IsDead) continue;
                if (excludeSelf && myPlayer != null && p.ActorID == myPlayer.ActorID) continue;

                ExecutePlayer(p, deathType);
            }
        }
    }
}
