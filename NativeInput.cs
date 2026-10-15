using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeathNoteCheaterWithin
{
    public static class NativeInput
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public const int VK_INSERT = 0x2D;
        public const int VK_SPACE = 0x20;
        public const int VK_SHIFT = 0x10;
        public const int VK_W = 0x57;
        public const int VK_A = 0x41;
        public const int VK_S = 0x53;
        public const int VK_D = 0x44;

        public const int VK_L = 0x4C;
        public const int VK_P = 0x50;
        public const int VK_LBRACKET = 0xDB; // '[' key

        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_F7 = 0x76;
        public const int VK_F8 = 0x77;
        public const int VK_F9 = 0x78;
        public const int VK_F10 = 0x79;

        private static readonly Dictionary<int, bool> KeyStates = new Dictionary<int, bool>();

        public static bool IsKeyDown(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }

        public static bool WasKeyPressed(int vKey)
        {
            bool isDown = (GetAsyncKeyState(vKey) & 0x8000) != 0;
            KeyStates.TryGetValue(vKey, out bool wasDown);

            if (isDown && !wasDown)
            {
                KeyStates[vKey] = true;
                return true;
            }

            if (!isDown)
            {
                KeyStates[vKey] = false;
            }

            return false;
        }
    }
}
