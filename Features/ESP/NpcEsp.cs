using System.Text;
using DeathNoteCheaterWithin.Drawing;
using Il2CppGame;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.Features.ESP
{
    public static class NpcEsp
    {
        private static GUIStyle? _npcLabelStyle;

        private static GUIStyle NpcLabelStyle
        {
            get
            {
                if (_npcLabelStyle == null)
                {
                    _npcLabelStyle = new GUIStyle();
                    _npcLabelStyle.fontSize = 11;
                    _npcLabelStyle.fontStyle = FontStyle.Normal;
                    _npcLabelStyle.alignment = TextAnchor.MiddleCenter;
                    _npcLabelStyle.normal.textColor = Palette.Npc;
                }
                return _npcLabelStyle;
            }
        }

        public static void Render()
        {
            if (!Config.NpcEsp && !Config.NpcCorpseEsp) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            EspCache.UpdateCache();
            if (EspCache.NpcCache.Count == 0) return;

            Vector3 camPos = cam.transform.position;

            foreach (var npc in EspCache.NpcCache)
            {
                if (npc == null) continue;

                Vector3 pos = npc.transform.position;
                float dist = Vector3.Distance(camPos, pos);
                if (dist > Config.EspMaxDistance) continue;

                bool isDead = npc.IsDead;
                if (isDead && !Config.NpcCorpseEsp) continue;
                if (!isDead && !Config.NpcEsp) continue;
                if (!isDead && Config.NpcEliteOnly && !npc.IsEliteNPC) continue;

                if (!RenderHelper.WorldToScreen(cam, pos + Vector3.up * 1.0f, out Vector2 screenPos))
                    continue;

                Color tagColor = isDead ? Palette.Corpse : (npc.IsEliteNPC ? Palette.Terrorist : Palette.Npc);

                StringBuilder sb = new StringBuilder();
                if (isDead)
                {
                    sb.Append("<color=#FF2222>[CORPSE]</color>");
                }
                else
                {
                    sb.Append("NPC");
                    if (npc.IsEliteNPC) sb.Append(" <color=#FFAA00>[ELITE]</color>");
                    if (npc.GathersInfo) sb.Append(" <color=#00FFDD>[INFO]</color>");
                }

                if (Config.EspShowDistance)
                {
                    sb.Append(" (").Append((int)dist).Append("m)");
                }

                GUI.contentColor = tagColor;
                GUI.Label(new Rect(screenPos.x - 100f, screenPos.y - 12f, 200f, 30f), sb.ToString(), NpcLabelStyle);
                GUI.contentColor = Color.white;
            }
        }
    }
}
