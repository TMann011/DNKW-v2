using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Combat
{
    public static class InstantNames
    {
        [HarmonyPatch(typeof(AdviceConditionData), nameof(AdviceConditionData.CheckEqual), new Type[] { typeof(float) })]
        public static class AdviceConditionData_Patch
        {
            public static bool Prefix(float value, ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        public static void AddAllNamesToDeathNote()
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null || gsm.PlayerList == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            var playerInfoList = myPlayer.GetPlayerInfoList();

            var enumerator = gsm.PlayerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerMovement target = enumerator.Current;
                if (target == null) continue;

                if (playerInfoList == null || !playerInfoList.Contains(target))
                {
                    myPlayer.AddPlayerInfoList(target);
                }
            }
        }
    }
}
