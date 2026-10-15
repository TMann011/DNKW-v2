using System;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace DeathNoteCheaterWithin.UI
{
    public class UiBuilder
    {
        private readonly Rect _bounds;
        private float _curX;
        private float _curY;
        private bool _inRow;
        private float _rowHeight;

        public float CurrentX => _curX;
        public float CurrentY => _curY;
        public float RemainingHeight => Mathf.Max(0, (_bounds.y + _bounds.height) - _curY);

        public UiBuilder(Rect bounds)
        {
            _bounds = bounds;
            _curX = bounds.x;
            _curY = bounds.y;
            _inRow = false;
            _rowHeight = 0f;
        }

        public void Space(float pixels = 6f)
        {
            if (!_inRow)
            {
                _curY += pixels;
            }
            else
            {
                _curX += pixels;
            }
        }

        public void Header(string title, float height = 24f)
        {
            GUI.Box(new Rect(_bounds.x, _curY, _bounds.width, height), $"<b><color=#55FFAA>{title}</color></b>");
            _curY += height + 4f;
        }

        public void Label(string text, float height = 20f)
        {
            GUI.Label(new Rect(_bounds.x, _curY, _bounds.width, height), text);
            _curY += height + 2f;
        }

        public string TextField(string label, string current, float height = 24f)
        {
            GUI.Label(new Rect(_bounds.x, _curY, 120f, height), label);
            string val = GUI.TextField(new Rect(_bounds.x + 125f, _curY, _bounds.width - 130f, height), current ?? "");
            _curY += height + 4f;
            return val;
        }

        public bool Toggle(ref bool value, string label, float height = 22f)
        {
            string indicator = value ? "<color=#00FFAA>[ON]</color>" : "<color=#FF4444>[OFF]</color>";
            string formattedText = $"  {indicator} {label}";
            value = GUI.Toggle(new Rect(_bounds.x, _curY, _bounds.width, height), value, formattedText);
            _curY += height + 2f;
            return value;
        }

        public float Slider(string label, ref float value, float min, float max, string format = "F1", float height = 36f)
        {
            GUI.Label(new Rect(_bounds.x, _curY, _bounds.width, 18f), $"<b>{label}:</b> {value.ToString(format)}");
            _curY += 18f;
            value = GUI.HorizontalSlider(new Rect(_bounds.x, _curY, _bounds.width, 16f), value, min, max);
            _curY += 18f + 4f;
            return value;
        }

        public bool Button(string text, float width = -1f, float height = 26f)
        {
            float w = width > 0 ? width : _bounds.width;
            bool pressed = GUI.Button(new Rect(_bounds.x, _curY, w, height), text);
            _curY += height + 4f;
            return pressed;
        }

        public void BeginRow(float height = 26f)
        {
            _inRow = true;
            _curX = _bounds.x;
            _rowHeight = height;
        }

        public bool RowButton(string text, float width)
        {
            bool pressed = GUI.Button(new Rect(_curX, _curY, width, _rowHeight), text);
            _curX += width + 4f;
            return pressed;
        }

        public bool RowToggle(ref bool value, string text, float width)
        {
            value = GUI.Toggle(new Rect(_curX, _curY, width, _rowHeight), value, text);
            _curX += width + 4f;
            return value;
        }

        public void RowLabel(string text, float width)
        {
            GUI.Label(new Rect(_curX, _curY, width, _rowHeight), text);
            _curX += width + 4f;
        }

        public void EndRow()
        {
            _inRow = false;
            _curY += _rowHeight + 4f;
            _curX = _bounds.x;
        }

        public Color ColorSlider(string label, Color c)
        {
            GUI.Label(new Rect(_bounds.x, _curY, _bounds.width, 18f), $"<b>{label}:</b> R={c.r:F2} G={c.g:F2} B={c.b:F2} A={c.a:F2}");
            _curY += 18f;

            float w = (_bounds.width - 24f) / 4f;
            c.r = GUI.HorizontalSlider(new Rect(_bounds.x + 0 * (w + 6f), _curY, w, 14f), c.r, 0f, 1f);
            c.g = GUI.HorizontalSlider(new Rect(_bounds.x + 1 * (w + 6f), _curY, w, 14f), c.g, 0f, 1f);
            c.b = GUI.HorizontalSlider(new Rect(_bounds.x + 2 * (w + 6f), _curY, w, 14f), c.b, 0f, 1f);
            c.a = GUI.HorizontalSlider(new Rect(_bounds.x + 3 * (w + 6f), _curY, w, 14f), c.a, 0f, 1f);
            _curY += 18f;

            return c;
        }
    }
}
