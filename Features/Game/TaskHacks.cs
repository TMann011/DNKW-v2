using System;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

namespace DeathNoteCheaterWithin.Features.Game
{
    public static class TaskHacks
    {
        public static void CompleteActiveTask()
        {
            try
            {
                TaskManager tm = EngineStrictSingleton<TaskManager>.Instance;
                if (tm != null)
                {
                    tm.EndCompletedTask();
                    MelonLogger.Msg("[+] Completed current active task!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] CompleteActiveTask failed: {ex.Message}");
            }
        }

        public static void CompleteAllMyTasks()
        {
            try
            {
                TaskManager tm = EngineStrictSingleton<TaskManager>.Instance;
                if (tm == null) return;

                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm == null || gsm.MyPlayer == null) return;

                // Grant max info progress and complete tasks
                gsm.MyPlayer.SetInfoProgressRate(1.0f);
                tm.EndCompletedTask();
                MelonLogger.Msg("[+] Dispatched 100% task completion to local player state!");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] CompleteAllMyTasks failed: {ex.Message}");
            }
        }

        public static void AddTeamProgress(float percent = 10f)
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null && gsm.MyPlayer != null)
                {
                    gsm.MyPlayer.AddExp(percent, (InGameExpType)0);
                    MelonLogger.Msg($"[+] Added +{percent}% team investigation progress!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] AddTeamProgress failed: {ex.Message}");
            }
        }
    }
}
