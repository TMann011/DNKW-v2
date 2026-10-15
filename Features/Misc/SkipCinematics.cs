using HarmonyLib;
using Il2CppGame;
using Il2CppLunaOutGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class SkipCinematics
    {
        [HarmonyPatch(typeof(LogoScene), nameof(LogoScene.PostInit))]
        public static class LogoScene_PostInit
        {
            public static bool Prefix()
            {
                if (Config.SkipCinematics)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.StartOpeningMovie))]
        public static class TitleScene_StartOpeningMovie
        {
            public static bool Prefix(TitleScene __instance)
            {
                if (Config.SkipCinematics)
                {
                    __instance.StartTitle();
                    return false;
                }
                return true;
            }
        }

        private static float _lastTitlePush = 0f;

        [HarmonyPatch(typeof(TitleUI), nameof(TitleUI.Update))]
        public static class TitleUI_Update
        {
            public static void Prefix(TitleUI __instance)
            {
                if (Config.SkipCinematics && Time.time > _lastTitlePush + 1.0f)
                {
                    _lastTitlePush = Time.time;
                    try
                    {
                        __instance.PushedAnyButton();
                    }
                    catch { }
                }
            }
        }

        [HarmonyPatch(typeof(CommandInput), nameof(CommandInput.TickInput))]
        public static class CommandInput_TickInput
        {
            public static void Postfix(CommandInput __instance)
            {
                if (Config.AutoQte)
                {
                    __instance.SucceedCurrentCommand();
                }
            }
        }
    }
}
