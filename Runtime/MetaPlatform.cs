// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.Utilities.Extensions;
using RealityToolkit.MetaPlatform.Plugins;
using System.Linq;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.XR.Management;

namespace RealityToolkit.MetaPlatform
{
    [System.Runtime.InteropServices.Guid("DB1ACC26-EC8D-4BC6-AFCA-C51351B2DA2E")]
    public class MetaPlatform : BasePlatform
    {
        private static readonly System.Version NoVersion = new System.Version();

        private bool IsXRLoaderActive => XRGeneralSettings.Instance.IsNotNull() &&
                    ((XRGeneralSettings.Instance.Manager.activeLoader != null && XRGeneralSettings.Instance.Manager.activeLoader.GetType() == typeof(OculusLoader)) ||
                    (XRGeneralSettings.Instance.Manager.activeLoaders != null && XRGeneralSettings.Instance.Manager.activeLoaders.Any(l => l.GetType() == typeof(OculusLoader))));

        /// <inheritdoc />
        public override bool IsAvailable =>
            !Application.isEditor && OculusApi.Version > NoVersion && OculusApi.Initialized;

        /// <inheritdoc />
        public override IPlatform[] PlatformOverrides { get; } =
        {
            new AndroidPlatform(),
            new WindowsStandalonePlatform()
        };

#if UNITY_EDITOR
        /// <inheritdoc />
        public override bool IsBuildTargetAvailable =>
            base.IsBuildTargetAvailable &&
            OculusApi.Version > NoVersion &&
            IsXRLoaderActive;

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.Android,
            UnityEditor.BuildTarget.StandaloneWindows64,
            UnityEditor.BuildTarget.StandaloneWindows
        };
#endif // UNITY_EDITOR
    }
}