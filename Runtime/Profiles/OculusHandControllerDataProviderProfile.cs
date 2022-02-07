﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;

namespace XRTK.Oculus.Profiles
{
    /// <summary>
    /// Configuration profile for Oculus hand controllers.
    /// </summary>
    public class OculusHandControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [SerializeField]
        [Tooltip("The minimum hand tracking confidence expected.")]
        private int minConfidenceRequired = 0;

        /// <summary>
        /// The minimum hand tracking confidence expected.
        /// </summary>
        public int MinConfidenceRequired => minConfidenceRequired;
    }
}