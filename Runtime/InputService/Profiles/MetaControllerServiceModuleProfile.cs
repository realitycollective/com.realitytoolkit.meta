﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.Definitions.Controllers;
using RealityCollective.Definitions.Utilities;

namespace RealityToolkit.MetaPlatform.InputService.Profiles
{
    /// <summary>
    /// Configuration profile for Meta controllers.
    /// </summary>
    public class MetaControllerServiceModuleProfile : BaseMixedRealityControllerServiceModuleProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(MetaRemoteController)),
                new ControllerDefinition(typeof(MetaTouchController), Handedness.Left),
                new ControllerDefinition(typeof(MetaTouchController), Handedness.Right)
            };
        }
    }
}