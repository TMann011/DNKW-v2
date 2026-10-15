using System;
using UnityEngine;

namespace DeathNoteCheaterWithin.Drawing
{
    public static class RenderHelper
    {
        private static Material? _drawMaterial;

        public static Material DrawMaterial
        {
            get
            {
                if (_drawMaterial == null)
                {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");
                    _drawMaterial = new Material(shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    _drawMaterial.SetInt("_SrcBlend", 5);  // SrcAlpha
                    _drawMaterial.SetInt("_DstBlend", 10); // OneMinusSrcAlpha
                    _drawMaterial.SetInt("_Cull", 0);     // Off
                    _drawMaterial.SetInt("_ZWrite", 0);   // Off
                }
                return _drawMaterial;
            }
        }

        public static bool WorldToScreen(Camera camera, Vector3 worldPos, out Vector2 screenPos)
        {
            screenPos = Vector2.zero;
            if (camera == null) return false;

            Vector3 viewPos = camera.WorldToScreenPoint(worldPos);
            if (viewPos.z <= 0.01f) return false;

            // GUI coordinates: y is inverted compared to ScreenPoint
            screenPos = new Vector2(viewPos.x, Screen.height - viewPos.y);
            return true;
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1.5f)
        {
            width /= 1200f;
            GL.PushMatrix();
            DrawMaterial.SetPass(0);
            GL.LoadOrtho();

            Vector2 s = new Vector2(start.x / Screen.width, (Screen.height - start.y) / Screen.height);
            Vector2 e = new Vector2(end.x / Screen.width, (Screen.height - end.y) / Screen.height);

            Vector2 perp = new Vector2(s.y - e.y, e.x - s.x).normalized * (width * 0.5f);

            GL.Begin(7); // GL.QUADS
            GL.Color(color);
            GL.Vertex3(s.x - perp.x, s.y - perp.y, 0f);
            GL.Vertex3(s.x + perp.x, s.y + perp.y, 0f);
            GL.Vertex3(e.x + perp.x, e.y + perp.y, 0f);
            GL.Vertex3(e.x - perp.x, e.y - perp.y, 0f);
            GL.End();

            GL.PopMatrix();
        }

        public static void DrawBox2D(Rect rect, Color color, float width = 1.5f)
        {
            Vector2 tl = new Vector2(rect.xMin, rect.yMin);
            Vector2 tr = new Vector2(rect.xMax, rect.yMin);
            Vector2 br = new Vector2(rect.xMax, rect.yMax);
            Vector2 bl = new Vector2(rect.xMin, rect.yMax);

            DrawLine(tl, tr, color, width);
            DrawLine(tr, br, color, width);
            DrawLine(br, bl, color, width);
            DrawLine(bl, tl, color, width);
        }

        public static void DrawSnapline(Vector2 screenPos, Color color, float width = 1.5f)
        {
            Vector2 screenBottomCenter = new Vector2(Screen.width * 0.5f, Screen.height);
            DrawLine(screenBottomCenter, screenPos, color, width);
        }

        public static void DrawBox3D(Vector3 center, Vector3 size, Quaternion rotation, Color color, float width = 1.5f)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 half = size * 0.5f;
            Vector3[] vertices = new Vector3[8]
            {
                center + rotation * new Vector3(-half.x, -half.y, -half.z),
                center + rotation * new Vector3( half.x, -half.y, -half.z),
                center + rotation * new Vector3( half.x, -half.y,  half.z),
                center + rotation * new Vector3(-half.x, -half.y,  half.z),
                center + rotation * new Vector3(-half.x,  half.y, -half.z),
                center + rotation * new Vector3( half.x,  half.y, -half.z),
                center + rotation * new Vector3( half.x,  half.y,  half.z),
                center + rotation * new Vector3(-half.x,  half.y,  half.z),
            };

            Vector2[] s = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                if (!WorldToScreen(cam, vertices[i], out s[i]))
                    return; // Part of box behind camera
            }

            // Bottom base
            DrawLine(s[0], s[1], color, width);
            DrawLine(s[1], s[2], color, width);
            DrawLine(s[2], s[3], color, width);
            DrawLine(s[3], s[0], color, width);

            // Top base
            DrawLine(s[4], s[5], color, width);
            DrawLine(s[5], s[6], color, width);
            DrawLine(s[6], s[7], color, width);
            DrawLine(s[7], s[4], color, width);

            // Vertical pillars
            DrawLine(s[0], s[4], color, width);
            DrawLine(s[1], s[5], color, width);
            DrawLine(s[2], s[6], color, width);
            DrawLine(s[3], s[7], color, width);
        }
    }
}
