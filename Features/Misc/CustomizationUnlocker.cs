using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppGR;
using Il2CppLunaOutGame;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class CustomizationUnlocker
    {
        [HarmonyPatch(typeof(SaveData), nameof(SaveData.HasItem), new Type[] { typeof(ECharacterEditContentType), typeof(int) })]
        public static class SaveData_HasItem_Patch
        {
            public static bool Prefix(SaveData __instance, ECharacterEditContentType contentType, int itemId, ref bool __result)
            {
                if (Config.UnlockCustomizations)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SaveData), nameof(SaveData.IsEnableItem), new Type[] { typeof(ECharacterEditContentType), typeof(int) })]
        public static class SaveData_IsEnableItem_Patch
        {
            public static bool Prefix(SaveData __instance, ECharacterEditContentType contentType, int itemId, ref bool __result)
            {
                if (Config.UnlockCustomizations)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterEditContentItemListItem), nameof(CharacterEditContentItemListItem.SetLocked))]
        public static class Item_SetLocked_Patch
        {
            public static void Prefix(ref bool isLocked)
            {
                if (Config.UnlockCustomizations)
                {
                    isLocked = false;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterEditContentItemListItem), nameof(CharacterEditContentItemListItem.SetPremium))]
        public static class Item_SetPremium_Patch
        {
            public static void Prefix(ref bool isPremium)
            {
                if (Config.UnlockCustomizations)
                {
                    isPremium = false;
                }
            }
        }

        public static void RefreshUI()
        {
            try
            {
                if (EngineSingleton<CharacterEdit>.Instance != null)
                {
                    EngineSingleton<CharacterEdit>.Instance.RefreshUI();
                }
            }
            catch { }
        }
    }
}
