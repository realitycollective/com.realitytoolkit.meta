// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Services;
using RealityCollective.Utilities.Extensions;
using RealityToolkit.Definitions.Devices;
using RealityToolkit.Input.Definitions;
using RealityToolkit.Input.Hands;
using RealityToolkit.Input.Hands.Utilities;
using RealityToolkit.MetaPlatform.InputService.Extensions;
using RealityToolkit.MetaPlatform.Plugins;
using RealityToolkit.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.MetaPlatform.InputService.Utilities
{
    /// <summary>
    /// Converts Meta hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class MetaHandDataConverter
    {
        /// <summary>
        /// Destructor.
        /// </summary>
        ~MetaHandDataConverter()
        {
            if (!conversionProxyRootTransform.IsNull())
            {
                conversionProxyTransforms.Clear();
                conversionProxyRootTransform.Destroy();
            }
        }

        private Transform conversionProxyRootTransform;
        private readonly Dictionary<OculusApi.BoneId, Transform> conversionProxyTransforms = new Dictionary<OculusApi.BoneId, Transform>();
        private readonly Pose[] jointPoses = new Pose[HandData.JointCount];

        private OculusApi.Skeleton handSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState handState = new OculusApi.HandState();
        private OculusApi.Mesh handMesh = new OculusApi.Mesh();

        private Transform rigTransform = null;

        private Transform RigTransform
        {
            get
            {
                if (rigTransform == null)
                {
                    rigTransform = ServiceManager.Instance.TryGetService<IPlayerService>(out var playerService)
                        ? playerService.PlayerRig.RigTransform
                        : Camera.main.transform.parent;
                }

                return rigTransform;
            }
        }

        /// <summary>
        /// Reads hand APIs for the current frame and converts it to agnostic <see cref="HandData"/>.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to get <see cref="HandData"/> for.</param>
        /// <param name="minTrackingConfidence">The minimum <see cref="OculusApi.TrackingConfidence"/> required to consider hands tracked.</param>
        /// <param name="handData">The output <see cref="HandData"/>.</param>
        /// <returns>True, if data conversion was a success.</returns>
        public bool TryGetHandData(Handedness handedness, OculusApi.TrackingConfidence minTrackingConfidence, out HandData handData)
        {
            // Here we check whether the hand is being tracked at all by the Oculus system.
            if (!(OculusApi.GetHandState(OculusApi.Step.Render, handedness.ToHand(), ref handState) &&
                OculusApi.GetSkeleton(handedness.ToSkeletonType(), out handSkeleton)))
            {
                handData = default;
                return false;
            }

            // The hand is being tracked, next we verify it meets our confidence requirements to consider
            // it tracked.
            handData = new HandData
            {
                TrackingState = (handState.HandConfidence >= minTrackingConfidence && (handState.Status & OculusApi.HandStatus.HandTracked) != 0) ? TrackingState.Tracked : TrackingState.NotTracked,
                UpdatedAt = DateTimeOffset.UtcNow.Ticks
            };

            // If the hand is tracked per requirements, we get updated joint data
            // and other data needed for updating the hand controller's state.
            if (handData.TrackingState == TrackingState.Tracked)
            {
                handData.RootPose = GetHandRootPose(handedness);
                handData.Joints = GetJointPoses(handedness);
                handData.PointerPose = GetPointerPose(handedness);
            }

            // Even if the hand is being tracked by the system but the confidence did not
            // meet our requirements, we return true. This allows the hand controller and visualizers
            // to react to tracking loss and keep the hand up for a given time before destroying the controller.
            return true;
        }

        /// <summary>
        /// Gets updated joint poses for all <see cref="HandJoint"/>s.
        /// </summary>
        /// <param name="handedness">Handedness of the hand to read joint poses for.</param>
        /// <returns>Joint poses in <see cref="HandJoint"/> order.</returns>
        private Pose[] GetJointPoses(Handedness handedness)
        {
            jointPoses[(int)HandJoint.Wrist] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);

            jointPoses[(int)HandJoint.ThumbMetacarpal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            jointPoses[(int)HandJoint.ThumbProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
            jointPoses[(int)HandJoint.ThumbDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
            jointPoses[(int)HandJoint.ThumbTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);

            jointPoses[(int)HandJoint.IndexProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
            jointPoses[(int)HandJoint.IndexIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
            jointPoses[(int)HandJoint.IndexDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
            jointPoses[(int)HandJoint.IndexTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);

            jointPoses[(int)HandJoint.MiddleProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
            jointPoses[(int)HandJoint.MiddleIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
            jointPoses[(int)HandJoint.MiddleDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
            jointPoses[(int)HandJoint.MiddleTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);

            jointPoses[(int)HandJoint.RingProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
            jointPoses[(int)HandJoint.RingIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
            jointPoses[(int)HandJoint.RingDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
            jointPoses[(int)HandJoint.RingTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);

            jointPoses[(int)HandJoint.LittleMetacarpal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            jointPoses[(int)HandJoint.LittleProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
            jointPoses[(int)HandJoint.LittleIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
            jointPoses[(int)HandJoint.LittleDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
            jointPoses[(int)HandJoint.LittleTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);

            // Estimated: These joint poses are not provided by the Oculus
            // hand tracking implementation. But with the data we now have, we can
            // estimate their poses fairly well.
            jointPoses[(int)HandJoint.Palm] = HandUtilities.GetEstimatedPalmPose(jointPoses);
            jointPoses[(int)HandJoint.IndexMetacarpal] = HandUtilities.GetEstimatedIndexMetacarpalPose(jointPoses);
            jointPoses[(int)HandJoint.MiddleMetacarpal] = HandUtilities.GetEstimatedMiddleMetacarpalPose(jointPoses);
            jointPoses[(int)HandJoint.RingMetacarpal] = HandUtilities.GetEstimatedRingMetacarpalPose(jointPoses);

            return jointPoses;
        }

        /// <summary>
        /// Gets a single joint's pose relative to the hand root pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <param name="bone">Bone data retrieved from Oculus API with pose information.</param>
        /// <returns>Converted joint pose in hand space.</returns>
        private Pose GetJointPose(Handedness handedness, OculusApi.Bone bone)
        {
            // The Pinky/Thumb 1+ bones depend on the Pinky/Thumb 0 bone
            // to be available, which the Reality Toolkit hand tracking does not use. We still have to compute them to
            // be able to resolve pose relation dependencies.
            if (bone.Id == OculusApi.BoneId.Hand_Thumb1)
            {
                GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb0]);
            }
            else if (bone.Id == OculusApi.BoneId.Hand_Pinky1)
            {
                GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            }

            var boneProxyTransform = GetProxyTransform(handedness, bone.Id);
            var parentProxyTransform = GetProxyTransform(handedness, (OculusApi.BoneId)bone.ParentBoneIndex);

            boneProxyTransform.parent = parentProxyTransform;

            if (bone.ParentBoneIndex == (int)OculusApi.BoneId.Invalid)
            {
                var rootPose = FixRotation(handedness, new Pose(conversionProxyRootTransform.localPosition, conversionProxyRootTransform.localRotation));
                boneProxyTransform.localPosition = rootPose.position;
                boneProxyTransform.localRotation = rootPose.rotation;
            }
            else
            {
                boneProxyTransform.localPosition = bone.Pose.Position;
                boneProxyTransform.localRotation = handState.BoneRotations[(int)bone.Id].ToQuaternionFlippedXY();
            }

            return FixRotation(handedness, new Pose(
                conversionProxyRootTransform.InverseTransformPoint(boneProxyTransform.position),
                Quaternion.Inverse(conversionProxyRootTransform.rotation) * boneProxyTransform.rotation));
        }

        /// <summary>
        /// WARNING: THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK.
        /// This fix is a hack to fix broken and inconsistent rotations for hands.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <param name="jointPose">The joint pose to apply the fix to.</param>
        /// <returns>Joint pose with fixed rotation.</returns>
        private Pose FixRotation(Handedness handedness, Pose jointPose)
        {
            if (handedness == Handedness.Left)
            {
                // Rotate bone 180 degrees on X to flip up.
                jointPose.rotation *= Quaternion.Euler(180f, 0f, 0f);

                // Rotate bone 90 degrees on Y to align X with right.
                jointPose.rotation *= Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                // Rotate bone 90 degrees on Y to align X with left.
                jointPose.rotation *= Quaternion.Euler(0f, -90f, 0f);
            }

            return jointPose;
        }

        /// <summary>
        /// The oculus APIs return joint poses relative to their parent joint unlike
        /// other platforms where joint poses are relative to the hand root. To convert
        /// the joint-->parent-joint relation to joint-->hand-root relations proxy <see cref="Transform"/>s
        /// are used. The proxies are parented to their respective parent <see cref="Transform"/>.
        /// That way we can make use of Unity APIs to translate coordinate spaces.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the proxy <see cref="Transform"/> belongs to.</param>
        /// <param name="boneId">The Meta bone ID to lookup the proxy <see cref="Transform"/> for.</param>
        /// <returns>The proxy <see cref="Transform"/>.</returns>
        private Transform GetProxyTransform(Handedness handedness, OculusApi.BoneId boneId)
        {
            if (conversionProxyRootTransform.IsNull())
            {
                conversionProxyRootTransform = new GameObject("Meta Hand Conversion Proxy").transform;
                conversionProxyRootTransform.transform.SetParent(RigTransform, false);
                conversionProxyRootTransform.gameObject.SetActive(false);
            }

            // Depending on the handedness we are currently working on, we need to
            // rotate the conversion root. Same dilemma as with FixRotation above.
            conversionProxyRootTransform.localRotation = Quaternion.Euler(0f, handedness == Handedness.Right ? 180f : 0f, 0f);

            if (boneId == OculusApi.BoneId.Invalid)
            {
                return conversionProxyRootTransform;
            }

            if (conversionProxyTransforms.ContainsKey(boneId))
            {
                return conversionProxyTransforms[boneId];
            }

            var transform = new GameObject($"Meta Hand {boneId} Proxy").transform;
            conversionProxyTransforms.Add(boneId, transform);

            return transform;
        }

        /// <summary>
        /// Gets the hand's root pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand to get the pose for.</param>
        /// <returns>The hands <see cref="HandData.RootPose"/> value.</returns>
        private Pose GetHandRootPose(Handedness handedness)
        {
            var rigRotation = RigTransform.rotation;
            var rootPosition = RigTransform.InverseTransformPoint(RigTransform.position + rigRotation * handState.RootPose.Position);
            var rootRotation = Quaternion.Inverse(rigRotation) * rigRotation * handState.RootPose.Orientation.ToQuaternionFlippedXY();

            return FixRotation(handedness, new Pose(rootPosition, rootRotation));
        }

        /// <summary>
        /// Gets the hand's local pointer pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <returns>The hands <see cref="HandData.PointerPose"/> value.</returns>
        private Pose GetPointerPose(Handedness handedness)
        {
            var rootPose = GetHandRootPose(handedness);
            var rigRotation = RigTransform.rotation;
            var platformRootPosition = handState.RootPose.Position;
            var platformPointerPosition = rootPose.position + handState.PointerPose.Position - platformRootPosition;
            var platformPointerRotation = Quaternion.Inverse(rigRotation) * rigRotation * handState.PointerPose.Orientation.ToQuaternionFlippedXY();

            return new Pose(platformPointerPosition, platformPointerRotation);
        }
    }
}