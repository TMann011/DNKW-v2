using Il2CppGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Drawing
{
    public static class Palette
    {
        public static readonly Color Kira = new Color(0.95f, 0.2f, 0.2f, 1.0f);        // Crimson Red
        public static readonly Color KiraHelper = new Color(0.95f, 0.45f, 0.2f, 1.0f);  // Red-Orange
        public static readonly Color L = new Color(0.2f, 0.55f, 1.0f, 1.0f);           // Deep Sky Blue
        public static readonly Color Investigator = new Color(0.25f, 0.85f, 0.95f, 1.0f);// Cyan
        public static readonly Color Terrorist = new Color(1.0f, 0.65f, 0.0f, 1.0f);   // Bright Amber
        public static readonly Color Spectator = new Color(0.6f, 0.6f, 0.6f, 1.0f);     // Grey
        public static readonly Color Unknown = new Color(0.85f, 0.85f, 0.85f, 1.0f);   // White/Light Grey

        public static readonly Color Npc = new Color(0.9f, 0.9f, 0.3f, 1.0f);          // Light Yellow
        public static readonly Color Corpse = new Color(0.6f, 0.1f, 0.1f, 1.0f);       // Dark Red
        public static readonly Color Camera = new Color(0.7f, 0.3f, 0.9f, 1.0f);       // Purple
        public static readonly Color Task = new Color(0.2f, 0.85f, 0.3f, 1.0f);         // Lime Green

        public static Color GetRoleColor(EPlayerRoleDetail detail)
        {
            return detail switch
            {
                EPlayerRoleDetail.Investigator => Investigator,
                EPlayerRoleDetail.Detective => L,
                EPlayerRoleDetail.Killer => Kira,
                EPlayerRoleDetail.KillerHelp => KiraHelper,
                EPlayerRoleDetail.Terrorist => Terrorist,
                EPlayerRoleDetail.Detective_Help => L, // Watari (L Faction)
                EPlayerRoleDetail.Killer_2 => Kira,    // X-Kira
                EPlayerRoleDetail.KillerHelp_2 => KiraHelper,
                EPlayerRoleDetail.Investigator_Strong => Investigator,
                EPlayerRoleDetail.Detective_2 => L,    // Near
                EPlayerRoleDetail.Spectator => Spectator,
                _ => Unknown
            };
        }
    }
}
