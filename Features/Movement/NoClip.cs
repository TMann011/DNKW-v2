using System;
using Il2Cpp;
using Il2CppGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Movement
{
    public static class NoClip
    {
        private static bool _wasNoClip = false;

        public static void Update()
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm == null) return;

                PlayerMovement? myPlayer = null;
                try
                {
                    myPlayer = gsm.MyPlayer;
                }
                catch
                {
                    // IL2CPP getter throws NullReferenceException when outside active match field phase
                    return;
                }

                if (myPlayer == null) return;

                if (Config.NoClip)
                {
                    if (!_wasNoClip)
                    {
                        _wasNoClip = true;
                        try
                        {
                            if (myPlayer.CharacterController != null)
                            {
                                myPlayer.CharacterController.enabled = false;
                            }
                            myPlayer.IgnoreCollisionWithOtherCharacters();
                        }
                        catch { }
                    }

                    Transform camT = Camera.main != null ? Camera.main.transform : myPlayer.transform;
                    Vector3 move = Vector3.zero;

                    if (NativeInput.IsKeyDown(NativeInput.VK_W)) move += camT.forward;
                    if (NativeInput.IsKeyDown(NativeInput.VK_S)) move -= camT.forward;
                    if (NativeInput.IsKeyDown(NativeInput.VK_D)) move += camT.right;
                    if (NativeInput.IsKeyDown(NativeInput.VK_A)) move -= camT.right;
                    if (NativeInput.IsKeyDown(NativeInput.VK_SPACE)) move += Vector3.up;
                    if (NativeInput.IsKeyDown(NativeInput.VK_SHIFT)) move -= Vector3.up;

                    if (move.sqrMagnitude > 0.01f)
                    {
                        myPlayer.transform.position += move.normalized * (Config.NoClipSpeed * Time.deltaTime);
                    }
                }
                else if (_wasNoClip)
                {
                    _wasNoClip = false;
                    try
                    {
                        if (myPlayer.CharacterController != null)
                        {
                            myPlayer.CharacterController.enabled = true;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
