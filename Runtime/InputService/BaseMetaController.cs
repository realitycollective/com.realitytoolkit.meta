// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Controllers;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Extensions;
using RealityToolkit.Input.Interfaces.Modules;
using RealityToolkit.MetaPlatform.Plugins;
using System;
using UnityEngine;

namespace RealityToolkit.MetaPlatform.InputService
{
    public abstract class BaseMetaController : BaseController
    {
        /// <inheritdoc />
        protected BaseMetaController() { }

        /// <inheritdoc />
        protected BaseMetaController(IControllerServiceModule controllerServiceModule, TrackingState trackingState, Handedness controllerHandedness, ControllerProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerServiceModule, trackingState, controllerHandedness, controllerMappingProfile)
        {
            ControllerType = controllerType;
            NodeType = nodeType;
        }

        /// <summary>
        /// The Meta Node Type.
        /// </summary>
        private OculusApi.Node NodeType { get; }

        /// <summary>
        /// The Meta Controller Type.
        /// </summary>
        private OculusApi.Controller ControllerType { get; }

        private OculusApi.ControllerState4 previousState = new OculusApi.ControllerState4();
        private OculusApi.ControllerState4 currentState = new OculusApi.ControllerState4();

        /// <inheritdoc />
        public override InteractionMapping[] DefaultInteractions => new[]
        {
            new InteractionMapping("Button.A Press", AxisType.Digital, "A", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.B Press", AxisType.Digital, "B", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.X Press", AxisType.Digital, "X", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Y Press", AxisType.Digital, "Y", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Back Press", AxisType.Digital, "Back", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.LShoulder Press", AxisType.Digital, "LShoulder", DeviceInputType.ButtonPress),
            new InteractionMapping("Axis1D.LIndexTrigger", AxisType.SingleAxis, "LIndexTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.LIndexTrigger Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerTouch),
            new InteractionMapping("Axis1D.LIndexTrigger Near Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerNearTouch),
            new InteractionMapping("Axis1D.LIndexTrigger Press", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis1D.LHandTrigger Press", AxisType.SingleAxis, "LHandTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis2D.LThumbstick", AxisType.DualAxis, "PrimaryThumbstick", DeviceInputType.ThumbStick),
            new InteractionMapping("Button.LThumbstick Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickTouch),
            new InteractionMapping("Button.LThumbstick Near Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Button.LThumbstick Press", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.RShoulder Press", AxisType.Digital, "RShoulder", DeviceInputType.ButtonPress),
            new InteractionMapping("Axis1D.RIndexTrigger", AxisType.SingleAxis, "RIndexTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.RIndexTrigger Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerTouch),
            new InteractionMapping("Axis1D.RIndexTrigger Near Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerNearTouch),
            new InteractionMapping("Axis1D.RIndexTrigger Press", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis1D.RHandTrigger Press", AxisType.SingleAxis, "RHandTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis2D.RThumbstick", AxisType.DualAxis, "SecondaryThumbstick", DeviceInputType.ThumbStick),
            new InteractionMapping("Button.RThumbstick Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickTouch),
            new InteractionMapping("Button.RThumbstick Near Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Button.RThumbstick Press", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Axis2D.Dpad", AxisType.DualAxis, DeviceInputType.ThumbStick),
            new InteractionMapping("Button.DpadUp Press", AxisType.Digital, "DpadUp", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.DpadDown Press", AxisType.Digital, "DpadDown", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.DpadLeft Press", AxisType.Digital, "DpadLeft", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.DpadRight Press", AxisType.Digital, "DpadRight", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.RTouchpad", AxisType.Digital, "RTouchpad", DeviceInputType.ThumbTouch)
        };

        /// <inheritdoc />
        public override InteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        /// <inheritdoc />
        public override InteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        private Pose currentPointerPose = Pose.identity;
        private Pose lastControllerPose = Pose.identity;
        private OculusApi.PoseStatef currentControllerVelocity = new OculusApi.PoseStatef();
        private float singleAxisValue = 0.0f;
        private Vector2 dualAxisPosition = Vector2.zero;

        /// <inheritdoc />
        public override void UpdateController()
        {
            if (!Enabled)
            {
                return;
            }

            UpdateControllerData();

            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for Meta Controller {ControllerHandedness}");
                Enabled = false;
            }

            for (int i = 0; i < Interactions?.Length; i++)
            {
                var interactionMapping = Interactions[i];

                switch (interactionMapping.InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        UpdatePoseData(interactionMapping);
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.ButtonPress:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.ThumbStickPress:
                        UpdateButtonDataPress(interactionMapping);
                        break;
                    case DeviceInputType.ButtonTouch:
                    case DeviceInputType.TriggerTouch:
                    case DeviceInputType.ThumbTouch:
                    case DeviceInputType.TouchpadTouch:
                    case DeviceInputType.ThumbStickTouch:
                        UpdateButtonDataTouch(interactionMapping);
                        break;
                    case DeviceInputType.ButtonNearTouch:
                    case DeviceInputType.TriggerNearTouch:
                    case DeviceInputType.ThumbNearTouch:
                    case DeviceInputType.TouchpadNearTouch:
                    case DeviceInputType.ThumbStickNearTouch:
                        UpdateButtonDataNearTouch(interactionMapping);
                        break;
                    case DeviceInputType.Trigger:
                        UpdateSingleAxisData(interactionMapping);
                        break;
                    case DeviceInputType.ThumbStick:
                    case DeviceInputType.Touchpad:
                        UpdateDualAxisData(interactionMapping);
                        break;
                    default:
                        Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                        break;
                }

                interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
            }

            base.UpdateController();
        }

        private void UpdateControllerData()
        {
            var lastState = TrackingState;
            lastControllerPose = Pose;
            previousState = currentState;

            currentState = OculusApi.GetControllerState4((uint)ControllerType);

            if (currentState.LIndexTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LIndexTrigger;
            }

            if (currentState.LHandTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LHandTrigger;
            }

            if (currentState.LThumbstick.y >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickUp;
            }

            if (currentState.LThumbstick.y <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickDown;
            }

            if (currentState.LThumbstick.x <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickLeft;
            }

            if (currentState.LThumbstick.x >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickRight;
            }

            if (currentState.RIndexTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RIndexTrigger;
            }

            if (currentState.RHandTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RHandTrigger;
            }

            if (currentState.RThumbstick.y >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickUp;
            }

            if (currentState.RThumbstick.y <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickDown;
            }

            if (currentState.RThumbstick.x <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickLeft;
            }

            if (currentState.RThumbstick.x >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickRight;
            }

            if (IsTrackedController(ControllerType))
            {
                // The source is either a hand or a controller that supports pointing.
                // We can now check for position and rotation.
                IsPositionAvailable = OculusApi.GetNodePositionTracked(NodeType);
                IsPositionApproximate = IsPositionAvailable && OculusApi.GetNodePositionValid(NodeType);
                IsRotationAvailable = OculusApi.GetNodeOrientationTracked(NodeType);

                // Devices are considered tracked if we receive position OR rotation data from the sensors.
                TrackingState = (IsPositionAvailable || IsRotationAvailable) ? Definitions.Devices.TrackingState.Tracked : Definitions.Devices.TrackingState.NotTracked;
            }
            else
            {
                // The input source does not support tracking.
                TrackingState = Definitions.Devices.TrackingState.NotApplicable;
            }

            Pose = OculusApi.GetNodePose(NodeType, OculusApi.stepType).ToUnityPoseFlippedQuaternionXY();
            currentControllerVelocity = OculusApi.GetNodeState(NodeType, OculusApi.stepType);
            Velocity = currentControllerVelocity.Velocity.ToVector3FlippedZ();
            AngularVelocity = currentControllerVelocity.AngularVelocity.ToVector3FlippedZ();

            // Raise input system events if it is enabled.
            if (lastState != TrackingState)
            {
                InputService?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }

            if (TrackingState == Definitions.Devices.TrackingState.Tracked && lastControllerPose != Pose)
            {
                if (IsPositionAvailable && IsRotationAvailable)
                {
                    InputService?.RaiseSourcePoseChanged(InputSource, this, Pose);
                }
                else if (IsPositionAvailable && !IsRotationAvailable)
                {
                    InputService?.RaiseSourcePositionChanged(InputSource, this, Pose.position);
                }
                else if (!IsPositionAvailable && IsRotationAvailable)
                {
                    InputService?.RaiseSourceRotationChanged(InputSource, this, Pose.rotation);
                }
            }
        }

        private static bool IsTrackedController(OculusApi.Controller controller)
        {
            return controller == OculusApi.Controller.Touch ||
                   controller == OculusApi.Controller.LTouch ||
                   controller == OculusApi.Controller.RTouch;
        }

        private void UpdateButtonDataPress(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawButton>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawButton.None)
            {
                if (((OculusApi.RawButton)currentState.Buttons & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawButton)currentState.Buttons & interactionButton) == 0 &&
                        ((OculusApi.RawButton)previousState.Buttons & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateButtonDataTouch(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawTouch>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawTouch.None)
            {
                if (((OculusApi.RawTouch)currentState.Touches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawTouch)currentState.Touches & interactionButton) == 0 &&
                        ((OculusApi.RawTouch)previousState.Touches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateButtonDataNearTouch(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawNearTouch>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawNearTouch.None)
            {
                if (((OculusApi.RawNearTouch)currentState.NearTouches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawNearTouch)currentState.NearTouches & interactionButton) == 0 &&
                        ((OculusApi.RawNearTouch)previousState.NearTouches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateSingleAxisData(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SingleAxis);

            Enum.TryParse<OculusApi.RawAxis1D>(interactionMapping.InputName, out var interactionAxis1D);

            if (interactionAxis1D != OculusApi.RawAxis1D.None)
            {
                switch (interactionAxis1D)
                {
                    case OculusApi.RawAxis1D.LIndexTrigger:
                        singleAxisValue = currentState.LIndexTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.LHandTrigger:
                        singleAxisValue = currentState.LHandTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.RIndexTrigger:
                        singleAxisValue = currentState.RIndexTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.RHandTrigger:
                        singleAxisValue = currentState.RHandTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                }
            }

            // Update the interaction data source
            interactionMapping.FloatData = singleAxisValue;
        }

        private void UpdateDualAxisData(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.DualAxis);

            Enum.TryParse<OculusApi.RawAxis2D>(interactionMapping.InputName, out var interactionAxis2D);

            if (interactionAxis2D != OculusApi.RawAxis2D.None)
            {
                switch (interactionAxis2D)
                {
                    case OculusApi.RawAxis2D.LThumbstick:
                        dualAxisPosition.x = currentState.LThumbstick.x;
                        dualAxisPosition.y = currentState.LThumbstick.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.LTouchpad:
                        dualAxisPosition.x = currentState.LTouchpad.x;
                        dualAxisPosition.y = currentState.LTouchpad.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.RThumbstick:
                        dualAxisPosition.x = currentState.RThumbstick.x;
                        dualAxisPosition.y = currentState.RThumbstick.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.RTouchpad:
                        dualAxisPosition.x = currentState.RTouchpad.x;
                        dualAxisPosition.y = currentState.RTouchpad.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                }
            }

            // Update the interaction data source
            interactionMapping.Vector2Data = dualAxisPosition;
        }

        private void UpdatePoseData(InteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

            if (interactionMapping.InputType != DeviceInputType.SpatialPointer)
            {
                Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                return;
            }

            currentPointerPose = Pose;

            // Update the interaction data source
            interactionMapping.PoseData = currentPointerPose;
        }
    }
}
