using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.UI
{
    public static class KeybindOverlay
    {
        public static void Draw()
        {
            if (!Config.ShowKeybinds) return;

            float w = 310f;
            float h = 310f;
            Rect r = new Rect(Screen.width - w - 20f, 40f, w, h);

            GUI.Box(r, "<b><color=#55FFAA>Tactical Hotkeys [L = Close]</color></b>");

            string helpText =
                "<b><color=#FFFFFF>[INSERT]</color></b>  Toggle Tactical Menu\n" +
                "<b><color=#FFFFFF>[L]</color></b>       Toggle This Help Overlay\n" +
                "<b><color=#FFFFFF>[P]</color></b>       Dump Game State to Desktop\n" +
                "<b><color=#FFFFFF>[ [ ]</color></b>     Force Refresh Roles & Managers\n" +
                "<b><color=#00FFAA>[F1]</color></b>      Master ESP Toggle\n" +
                "<b><color=#00FFAA>[F2]</color></b>      NoClip Flight Toggle\n" +
                "<b><color=#00FFAA>[F3]</color></b>      Speed Hack Toggle\n" +
                "<b><color=#00FFAA>[F4]</color></b>      Always-Kill Bypass Toggle\n" +
                "<b><color=#00FFAA>[F5]</color></b>      Instant 100% ID Theft Aura\n" +
                "<b><color=#00FFAA>[F6]</color></b>      Add All Names to Death Note\n" +
                "<b><color=#00FFAA>[F7]</color></b>      Dash Forward 15m\n" +
                "<b><color=#00FFAA>[F8]</color></b>      Force Call Emergency Meeting\n" +
                "<b><color=#FF3333>[F9]</color></b>      Execute All Enemies (Heart Attack)\n" +
                "<b><color=#FFD700>[F10]</color></b>     Force Instant Victory (Kira)";

            GUI.Label(new Rect(r.x + 12f, r.y + 26f, r.width - 24f, r.height - 30f), helpText);
        }
    }
}
