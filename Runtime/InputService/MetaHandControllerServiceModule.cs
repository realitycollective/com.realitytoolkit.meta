// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Hands;
using RealityToolkit.Input.Interfaces;
using RealityToolkit.MetaPlatform.InputService.Profiles;
using RealityToolkit.MetaPlatform.InputService.Utilities;
using RealityToolkit.MetaPlatform.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.MetaPlatform.InputService
{
    [RuntimePlatform(typeof(MetaPlatform))]
    [System.Runtime.InteropServices.Guid("EA666456-BAEF-4412-A829-A4C7132E98C3")]
    public class MetaHandControllerServiceModule : BaseHandControllerServiceModule, IMetaHandControllerServiceModule
    {
        /// <inheritdoc />
        public MetaHandControllerServiceModule(string name, uint priority, MetaHandControllerServiceModuleProfile profile, IInputService parentService)
            : base(name, priority, profile, parentService)
        {
            if (!ServiceManager.Instance.TryGetServiceProfile<IInputService, InputServiceProfile>(out var inputServiceProfile))
            {
                throw new ArgumentException($"Unable to get a valid {nameof(InputServiceProfile)}!");
            }

            MinConfidenceRequired = (OculusApi.TrackingConfidence)profile.MinConfidenceRequired;
            handDataConverter = new MetaHandDataConverter();

            var isGrippingThreshold = profile.GripThreshold != inputServiceProfile.HandControllerSettings.GripThreshold
                ? profile.GripThreshold
                : inputServiceProfile.HandControllerSettings.GripThreshold;

            postProcessor = new HandDataPostProcessor(TrackedPoses, isGrippingThreshold)
            {
                PlatformProvidesPointerPose = true
            };
        }

        private readonly MetaHandDataConverter handDataConverter;
        private readonly HandDataPostProcessor postProcessor;
        private readonly Dictionary<Handedness, HandController> activeControllers = new Dictionary<Handedness, HandController>();

        /// <summary>
        /// The minimum required tracking confidence for hands to be registered.
        /// </summary>
        public OculusApi.TrackingConfidence MinConfidenceRequired { get; set; }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (handDataConverter.TryGetHandData(Handedness.Left, MinConfidenceRequired, out var leftHandData))
            {
                var controller = GetOrAddController(Handedness.Left);
                leftHandData = postProcessor.PostProcess(Handedness.Left, leftHandData);
                controller?.UpdateController(leftHandData);
            }
            else
            {
                RemoveController(Handedness.Left);
            }

            if (handDataConverter.TryGetHandData(Handedness.Right, MinConfidenceRequired, out var rightHandData))
            {
                var controller = GetOrAddController(Handedness.Right);
                rightHandData = postProcessor.PostProcess(Handedness.Right, rightHandData);
                controller?.UpdateController(rightHandData);
            }
            else
            {
                RemoveController(Handedness.Right);
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
        }

        private bool TryGetController(Handedness handedness, out HandController controller)
        {
            if (activeControllers.ContainsKey(handedness))
            {
                var existingController = activeControllers[handedness];
                Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
                controller = existingController;
                return true;
            }

            controller = null;
            return false;
        }

        private HandController GetOrAddController(Handedness handedness)
        {
            // If a device is already registered with the handedness, just return it.
            if (TryGetController(handedness, out var existingController))
            {
                return existingController;
            }

            HandController detectedController;
            try
            {
                detectedController = new HandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(HandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(HandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();
            AddController(detectedController);
            activeControllers.Add(handedness, detectedController);
            InputService?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
        {
            if (TryGetController(handedness, out var controller))
            {
                InputService?.RaiseSourceLost(controller.InputSource, controller);

                if (removeFromRegistry)
                {
                    RemoveController(controller);
                    activeControllers.Remove(handedness);
                }
            }
        }
    }
}