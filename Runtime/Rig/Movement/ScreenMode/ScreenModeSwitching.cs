using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace KadenZombie8.BIMOS.Rig.Movement
{
    public class ScreenModeSwitching : MonoBehaviour
    {
        [SerializeField]
        private TrackedPoseDrivers _trackedPoseDrivers;

        [SerializeField]
        private ScreenModeCamera _camera;

        [SerializeField]
        private Controllers _controllers;

        [SerializeField]
        private InputAction _userPresenceAction;

        private void Awake()
        {
            _userPresenceAction.Enable();
        }

        private void OnEnable()
        {
            _userPresenceAction.performed += VRMode;
            _userPresenceAction.canceled += ScreenMode;
        }

        private void OnDisable()
        {
            _userPresenceAction.performed -= VRMode;
            _userPresenceAction.canceled -= ScreenMode;
        }

        private void Start() => ScreenMode();

        private void VRMode(InputAction.CallbackContext _) => VRMode();

        private void VRMode()
        {
            _camera.enabled = false;
            _controllers.LeftController.enabled = false;
            _controllers.RightController.enabled = false;

            _trackedPoseDrivers.Headset.enabled = true;
            _trackedPoseDrivers.LeftController.enabled = true;
            _trackedPoseDrivers.RightController.enabled = true;
        }

        private void ScreenMode(InputAction.CallbackContext _) => ScreenMode();

        private void ScreenMode()
        {
            _camera.enabled = true;
            _controllers.LeftController.enabled = true;
            _controllers.RightController.enabled = true;

            _trackedPoseDrivers.Headset.enabled = false;
            _trackedPoseDrivers.LeftController.enabled = false;
            _trackedPoseDrivers.RightController.enabled = false;
        }

        [Serializable]
        private struct TrackedPoseDrivers
        {
            public TrackedPoseDriver Headset;
            public TrackedPoseDriver LeftController;
            public TrackedPoseDriver RightController;
        }

        [Serializable]
        private struct Controllers
        {
            public ScreenModeController LeftController;
            public ScreenModeController RightController;
        }
    }
}
