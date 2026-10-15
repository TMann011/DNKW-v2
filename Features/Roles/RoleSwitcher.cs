using Il2Cpp;
using Il2CppGame;

namespace DeathNoteCheaterWithin.Features.Roles
{
    public static class RoleSwitcher
    {
        public static void SwitchRole(EPlayerRoles role, EPlayerRoleDetail detail)
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            myPlayer.ChangeRole(role, detail);
            if (myPlayer.nameUI != null)
            {
                myPlayer.nameUI.RefreshName_Normal();
            }
        }
    }
}
