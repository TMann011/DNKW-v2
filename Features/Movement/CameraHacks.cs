using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Movement
{
    public static class CameraHacks
    {
        private static float _originalFov = -1f;

        public static void Update()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            if (_originalFov < 0f)
            {
                _originalFov = cam.fieldOfView;
            }

            if (Config.CustomFovEnabled)
            {
                cam.fieldOfView = Config.CustomFov;
            }
            else if (_originalFov > 0f && Mathf.Abs(cam.fieldOfView - _originalFov) > 0.1f)
            {
                cam.fieldOfView = _originalFov;
            }
        }
    }
}
