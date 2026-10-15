using System;
using System.Text;
using DeathNoteCheaterWithin.Drawing;
using Il2Cpp;
using Il2CppGame;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.Features.ESP
{
    public static class PlayerEsp
    {
        private static GUIStyle? _labelStyle;

        private static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle();
                    _labelStyle.fontSize = 12;
                    _labelStyle.fontStyle = FontStyle.Bold;
                    _labelStyle.alignment = TextAnchor.UpperLeft;
                    _labelStyle.normal.textColor = Color.white;
                }
                return _labelStyle;
            }
        }

        public static void Render()
        {
            if (!Config.EspEnabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.PlayerList == null) return;

            PlayerMovement? myPlayer = null;
            try { myPlayer = gsm.MyPlayer; } catch { }
            Vector3 myPos = myPlayer != null ? myPlayer.GetPosition() : cam.transform.position;

            var enumerator = gsm.PlayerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerMovement player = enumerator.Current;
                if (player == null || player.IsDead || player.IsSpectating) continue;
                if (myPlayer != null && player.ActorID == myPlayer.ActorID) continue;

                Vector3 targetPos = player.GetPosition();
                float dist = Vector3.Distance(myPos, targetPos);
                if (dist > Config.EspMaxDistance) continue;

                Vector3 headPos = targetPos + Vector3.up * 1.8f;
                Vector3 footPos = targetPos;

                if (!RenderHelper.WorldToScreen(cam, footPos, out Vector2 footScreen) ||
                    !RenderHelper.WorldToScreen(cam, headPos, out Vector2 headScreen))
                {
                    continue;
                }

                Color roleColor = Palette.GetRoleColor(player.RoleDetail);

                // Snaplines
                if (Config.EspSnaplines)
                {
                    RenderHelper.DrawSnapline(footScreen, roleColor, 1.2f);
                }

                // 2D Bounding Box
                float height = Mathf.Abs(footScreen.y - headScreen.y);
                float width = height * 0.5f;
                float left = footScreen.x - width * 0.5f;
                float top = headScreen.y;
                Rect boxRect = new Rect(left, top, width, height);

                if (Config.EspBoxes2D)
                {
                    RenderHelper.DrawBox2D(boxRect, roleColor, 1.5f);
                }

                // 3D Bounding Box
                if (Config.EspBoxes3D && player.CharacterController != null)
                {
                    Bounds bounds = player.CharacterController.bounds;
                    RenderHelper.DrawBox3D(bounds.center, bounds.size, player.GetRotation(), roleColor, 1.2f);
                }

                // Text Badge Info
                StringBuilder sb = new StringBuilder();
                string pName = player.GetPlayerName(false, true);
                sb.Append(pName).Append(" [").Append(player.ActorID).Append("]");

                if (Config.EspShowDistance)
                {
                    sb.Append(" (").Append((int)dist).Append("m)");
                }

                if (Config.EspShowRoles)
                {
                    string roleName = player.GetRoleName(false, true);
                    sb.Append("\n<color=#").Append(ColorUtility.ToHtmlStringRGB(roleColor)).Append(">").Append(roleName).Append("</color>");
                }

                if (Config.EspShowItems)
                {
                    if (player.HasDeathNote) sb.Append("\n<color=#FF3333>[DEATH NOTE]</color>");
                    if (player.HasOthersID) sb.Append("\n<color=#FFAA00>[STOLEN ID]</color>");
                }

                if (Config.EspShowStatus)
                {
                    if (player.IsUsingTransceiver) sb.Append("\n<color=#00FFAA>[TRANSCEIVER]</color>");
                }

                string infoText = sb.ToString();
                Vector2 labelPos = new Vector2(boxRect.xMax + 4f, boxRect.yMin);
                GUI.Label(new Rect(labelPos.x, labelPos.y, 220f, 100f), infoText, LabelStyle);
            }
        }
    }
}
