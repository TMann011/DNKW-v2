using DeathNoteCheaterWithin.Drawing;
using Il2CppGame;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.Features.ESP
{
    public static class WorldEsp
    {
        private static GUIStyle? _worldLabelStyle;

        private static GUIStyle WorldLabelStyle
        {
            get
            {
                if (_worldLabelStyle == null)
                {
                    _worldLabelStyle = new GUIStyle();
                    _worldLabelStyle.fontSize = 11;
                    _worldLabelStyle.fontStyle = FontStyle.Normal;
                    _worldLabelStyle.alignment = TextAnchor.MiddleCenter;
                    _worldLabelStyle.normal.textColor = Palette.Task;
                }
                return _worldLabelStyle;
            }
        }

        public static void Render()
        {
            if (!Config.TaskEsp && !Config.CameraEsp) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 camPos = cam.transform.position;

            if (Config.TaskEsp)
            {
                EspCache.UpdateCache();
                if (EspCache.InteractableCache.Count > 0)
                {
                    foreach (var obj in EspCache.InteractableCache)
                    {
                        if (obj == null) continue;

                        Vector3 pos = obj.transform.position;
                        float dist = Vector3.Distance(camPos, pos);
                        if (dist > Config.EspMaxDistance || dist < 1.0f) continue;

                        if (!RenderHelper.WorldToScreen(cam, pos, out Vector2 screenPos))
                            continue;

                        string name = obj.name;
                        GUI.contentColor = Palette.Task;
                        GUI.Label(new Rect(screenPos.x - 75f, screenPos.y - 10f, 150f, 25f), $"{name} ({(int)dist}m)", WorldLabelStyle);
                    }
                }
            }

            GUI.contentColor = Color.white;
        }
    }
}
