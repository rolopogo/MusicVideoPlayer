using IllusionPlugin;
using System;
using UnityEngine;
using VRUIControls;

namespace MusicVideoPlayer
{
    public class MoverPointer : MonoBehaviour
    {
        protected const float MinScrollDistance = 0.25f;
        protected const float MaxScrollDistance = 49f;
        protected const float MaxScale = 100f;
        protected const float MinScale = 0.3f;
        protected const float MaxLaserDistance = 50;
        protected const float positionSmooth = 4f;
        protected const float rotationSmooth = 2f;
        protected const float scaleSmooth = 1.5f;

        protected VRPointer _vrPointer;
        protected Transform _moveable;
        protected VRController _grabbingController;
        protected Vector3 _grabPos;
        protected Quaternion _grabRot;
        protected Vector3 _realPos;
        protected Quaternion _realRot;
        protected float _realScale;

        public event Action<Vector3, Quaternion, float> wasMoved;

        public virtual void Init(Transform moveable)
        {
            _moveable = moveable;
            _realPos = moveable.position;
            _realRot = moveable.rotation;
            _realScale = moveable.localScale.x;
            _vrPointer = GetComponent<VRPointer>();
        }

        
        protected virtual void Update()
        {
            if (_vrPointer.vrController != null)
                if (GetGrip())
                {
                    if (_grabbingController != null) return;
                    RaycastHit hit;
                    if (Physics.Raycast(_vrPointer.vrController.position, _vrPointer.vrController.forward, out hit, MaxLaserDistance))
                    {
                        if (hit.transform != _moveable) return;
                        _grabbingController = _vrPointer.vrController;
                        _grabPos = _vrPointer.vrController.transform.InverseTransformPoint(_moveable.position);
                        _grabRot = Quaternion.Inverse(_vrPointer.vrController.transform.rotation) * _moveable.rotation;
                    }
                }

            if (_grabbingController == null || GetGrip()) return;
            if (_grabbingController == null) return;
            wasMoved?.Invoke(_realPos, _realRot, _realScale);
            _grabbingController = null;
        }

        protected virtual void LateUpdate()
        {
            if (_grabbingController != null)
            {
                var diff = _grabbingController.verticalAxisValue * Time.unscaledDeltaTime;
                if (_grabPos.magnitude > MinScrollDistance && _grabPos.magnitude < MaxScrollDistance)
                {
                    _grabPos -= Vector3.forward * diff * 3;
                }
                else
                {
                    _grabPos -= Vector3.forward * Mathf.Clamp(diff * 3, float.MinValue, 0);
                }
                _realPos = _grabbingController.transform.TransformPoint(_grabPos);
                _realRot = _grabbingController.transform.rotation * _grabRot;

                var diffH = _grabbingController.horizontalAxisValue * Time.unscaledDeltaTime;
                if (_grabPos.magnitude > MinScrollDistance)
                {
                    _realScale -=  diffH;
                }
                else
                {
                    _realScale -= Mathf.Clamp(diffH, float.MinValue, 0);
                }
                
                _moveable.position = Vector3.Lerp(_moveable.position, _realPos,
                    positionSmooth * Time.unscaledDeltaTime);

                _moveable.rotation = Quaternion.Slerp(_moveable.rotation, _realRot,
                    rotationSmooth * Time.unscaledDeltaTime);

                _moveable.localScale = Vector3.Lerp(_moveable.localScale, _realScale * Vector3.one,
                    scaleSmooth * Time.unscaledDeltaTime);
            }
        }

        protected bool GetGrip()
        {
            if (Environment.CommandLine.Contains("fpfc"))
            {
                return Input.GetKey(KeyCode.G);
            }
            if (_vrPointer.vrController.name.Equals("ControllerLeft"))
            {
                return GetLeftGrip();
            }
            else if (_vrPointer.vrController.name.Equals("ControllerRight"))
            {
                return GetRightGrip();
            }
            return false;
        }

        public static bool GetRightGrip()
        {
            if (VRPlatformHelper.instance.vrPlatformSDK == VRPlatformHelper.VRPlatformSDK.OpenVR)
            {
                return OpenVRRightGrip();
            }
            else if (VRPlatformHelper.instance.vrPlatformSDK == VRPlatformHelper.VRPlatformSDK.Oculus)
            {
                return OculusRightGrip();
            }
            else
            {
                return false;
            }
        }

        public static bool GetLeftGrip()
        {
            if (VRPlatformHelper.instance.vrPlatformSDK == VRPlatformHelper.VRPlatformSDK.OpenVR)
            {
                return OpenVRLeftGrip();
            }
            else if (VRPlatformHelper.instance.vrPlatformSDK == VRPlatformHelper.VRPlatformSDK.Oculus)
            {
                return OculusLeftGrip();
            }
            
            else
            {
                return false;
            }
        }

        private static bool OculusRightGrip()
        {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.85f;
        }

        private static bool OculusLeftGrip()
        {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.85f;
        }

        private static bool OpenVRRightGrip()
        {
            return SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost)).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        }

        private static bool OpenVRLeftGrip()
        {
            return SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost)).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        }
    }
}