// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces.Modules;
using RealityToolkit.MetaPlatform.Plugins;

namespace RealityToolkit.MetaPlatform.InputService
{
    [System.Runtime.InteropServices.Guid("1898974A-DBCD-4C88-8E03-726689848D52")]
    public class MetaTouchController : BaseMetaController
    {
        /// <inheritdoc />
        public MetaTouchController() { }

        /// <inheritdoc />
        public MetaTouchController(IControllerServiceModule controllerServiceModule, TrackingState trackingState, Handedness controllerHandedness, ControllerProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerServiceModule, trackingState, controllerHandedness, controllerMappingProfile, controllerType, nodeType)
        {
        }

        /// <inheritdoc />
        /// <remarks> Note, MUST use RAW button types as that is what the API works with, DO NOT use Virtual!</remarks>
        public override InteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new InteractionMapping("Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new InteractionMapping("Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, "LIndexTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerTouch),
            new InteractionMapping("Axis1D.PrimaryIndexTrigger Near Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerNearTouch),
            new InteractionMapping("Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, "LHandTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.PrimaryHandTrigger Press", AxisType.Digital, "LHandTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis2D.PrimaryThumbstick", AxisType.DualAxis, "LThumbstick", DeviceInputType.ThumbStick),
            new InteractionMapping("Button.PrimaryThumbstick Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickTouch),
            new InteractionMapping("Button.PrimaryThumbstick Near Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Button.PrimaryThumbstick Press", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.Three Press", AxisType.Digital, "X", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Four Press", AxisType.Digital, "Y", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Three Touch", AxisType.Digital, "X", DeviceInputType.ButtonTouch),
            new InteractionMapping("Button.Four Touch", AxisType.Digital, "Y", DeviceInputType.ButtonTouch),
            new InteractionMapping("Axis2D.PrimaryThumbRest", AxisType.DualAxis, "LTouchpad", DeviceInputType.ThumbStick),
            new InteractionMapping("Touch.PrimaryThumbRest Touch", AxisType.Digital, "LThumbRest", DeviceInputType.ThumbTouch),
            new InteractionMapping("Touch.PrimaryThumbRest Near Touch", AxisType.Digital, "LThumbRest", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Grip Pose", AxisType.SixDof, DeviceInputType.SpatialGrip)
        };

        /// <inheritdoc />
        public override InteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new InteractionMapping("Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new InteractionMapping("Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, "RIndexTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerTouch),
            new InteractionMapping("Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerNearTouch),
            new InteractionMapping("Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis1D.SecondaryHandTrigger", AxisType.SingleAxis, "RHandTrigger", DeviceInputType.Trigger),
            new InteractionMapping("Axis1D.SecondaryHandTrigger Press", AxisType.Digital, "RHandTrigger", DeviceInputType.TriggerPress),
            new InteractionMapping("Axis2D.SecondaryThumbstick", AxisType.DualAxis, "RThumbstick", DeviceInputType.ThumbStick),
            new InteractionMapping("Button.SecondaryThumbstick Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickTouch),
            new InteractionMapping("Button.SecondaryThumbstick Near Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Button.SecondaryThumbstick Press", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickPress),
            new InteractionMapping("Button.One Press", AxisType.Digital, "A", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Two Press", AxisType.Digital, "B", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.One Touch", AxisType.Digital, "A", DeviceInputType.ButtonTouch),
            new InteractionMapping("Button.Two Touch", AxisType.Digital, "B", DeviceInputType.ButtonTouch),
            new InteractionMapping("Axis2D.SecondaryThumbRest", AxisType.DualAxis, "RTouchpad", DeviceInputType.ThumbStick),
            new InteractionMapping("Touch.SecondaryThumbRest Touch", AxisType.Digital, "RThumbRest", DeviceInputType.ThumbTouch),
            new InteractionMapping("Touch.SecondaryThumbRest Near Touch", AxisType.Digital, "RThumbRest", DeviceInputType.ThumbNearTouch),
            new InteractionMapping("Grip Pose", AxisType.SixDof, DeviceInputType.SpatialGrip)
        };
    }
}