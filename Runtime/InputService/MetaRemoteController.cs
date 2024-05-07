// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Controllers;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Interfaces.Modules;
using RealityToolkit.MetaPlatform.Plugins;

namespace RealityToolkit.MetaPlatform.InputService
{
    [System.Runtime.InteropServices.Guid("071048C6-31F3-460C-863F-5D3121F47654")]
    public class MetaRemoteController : BaseMetaController
    {
        /// <inheritdoc />
        public MetaRemoteController() { }

        /// <inheritdoc />
        public MetaRemoteController(IControllerServiceModule controllerServiceModule, TrackingState trackingState, Handedness controllerHandedness, ControllerProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerServiceModule, trackingState, controllerHandedness, controllerMappingProfile, controllerType, nodeType)
        {
        }

        /// <inheritdoc />
        public override InteractionMapping[] DefaultInteractions => new[]
        {
            new InteractionMapping("Button.DpadUp", AxisType.Digital, "DpadUp", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.DpadDown", AxisType.Digital, "DpadDown", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.DpadLeft", AxisType.Digital, "DpadLeft", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.DpadRight", AxisType.Digital, "DpadRight", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.One", AxisType.Digital, "One", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Two", AxisType.Digital, "Two", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Start", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new InteractionMapping("Button.Back", AxisType.Digital, "Back", DeviceInputType.ButtonPress),
        };
    }
}