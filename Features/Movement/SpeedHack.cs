using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Movement
{
    [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.Update))]
    public static class SpeedHack
    {
        private static Camera? _cachedCam;
        private static float _lastCamCheck = 0f;

        public static void Postfix(PlayerMovement __instance)
        {
            if (!Config.SpeedHack || __instance == null || !__instance.IsMyPlayer) return;

            // CharacterController movement boost
            if (__instance.CharacterController != null && __instance.inputDir.sqrMagnitude > 0.01f)
            {
                if (_cachedCam == null || Time.time > _lastCamCheck + 2.0f)
                {
                    _cachedCam = Camera.main;
                    _lastCamCheck = Time.time;
                }

                Vector3 forward = _cachedCam != null ? _cachedCam.transform.forward : __instance.transform.forward;
                forward.y = 0;
                forward.Normalize();
                Vector3 right = _cachedCam != null ? _cachedCam.transform.right : __instance.transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 moveDir = (forward * __instance.inputDir.y + right * __instance.inputDir.x).normalized;
                float bonusSpeed = (Config.SpeedMultiplier - 1.0f) * 4.5f * Time.deltaTime;
                __instance.CharacterController.Move(moveDir * bonusSpeed);
            }
        }
    }
}
