using Il2Cpp;
using Il2CppGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Movement
{
    public static class TeleportManager
    {
        public static void TeleportTo(Vector3 targetPos)
        {
            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null) return;

            PlayerMovement myPlayer = gsm.MyPlayer;
            if (myPlayer == null) return;

            if (myPlayer.CharacterController != null)
            {
                myPlayer.CharacterController.enabled = false;
                myPlayer.transform.position = targetPos;
                myPlayer.CharacterController.enabled = true;
            }
            else
            {
                myPlayer.transform.position = targetPos;
            }
        }

        public static void TeleportToPlayer(PlayerMovement target)
        {
            if (target == null) return;
            TeleportTo(target.GetPosition() + Vector3.up * 0.2f);
        }

        public static void TeleportForward(float distance = 5.0f)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            forward.Normalize();

            GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
            if (gsm == null || gsm.MyPlayer == null) return;

            TeleportTo(gsm.MyPlayer.GetPosition() + forward * distance);
        }

        public static void TeleportToCrosshair()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                TeleportTo(hit.point + Vector3.up * 0.5f);
            }
        }
    }
}
