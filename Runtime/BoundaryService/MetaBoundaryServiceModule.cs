// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Modules;
using RealityToolkit.Boundary;
using RealityToolkit.Boundary.Definitions;
using RealityToolkit.MetaPlatform.Plugins;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RealityToolkit.MetaPlatform.BoundaryService
{
    [RuntimePlatform(typeof(MetaPlatform))]
    [Guid("8EF0CAB5-A37C-4912-AD5E-1E57E92A314D")]
    public class MetaBoundaryServiceModule : BaseServiceModule, IMetaBoundaryServiceModule
    {
        /// <inheritdoc />
        public MetaBoundaryServiceModule(string name, uint priority, BaseProfile profile, IBoundaryService parentService)
            : base(name, priority, profile, parentService)
        {
            boundarySystem = parentService;
        }

        private Vector3[] cachedPoints = new Vector3[0];
        private static readonly OculusApi.OVRNativeBuffer cachedGeometryNativeBuffer = new OculusApi.OVRNativeBuffer(0);
        private static readonly int cachedVector3fSize = Marshal.SizeOf(typeof(OculusApi.Vector3f));
        private static float[] cachedGeometryManagedBuffer = new float[0];
        private readonly IBoundaryService boundarySystem;

        /// <inheritdoc />
        public BoundaryVisibility Visibility => OculusApi.GetBoundaryVisible() ? BoundaryVisibility.Visible : BoundaryVisibility.Hidden;

        /// <inheritdoc />
        public bool IsPlatformConfigured => OculusApi.GetBoundaryConfigured();

        /// <inheritdoc />
        public override void Start()
        {
            boundarySystem.SetupBoundary(this);
        }

        /// <inheritdoc />
        public bool TryGetBoundaryGeometry(ref List<Vector3> geometry)
        {
            geometry.Clear();
            var oculusGeometry = GetGeometry(OculusApi.BoundaryType.PlayArea);

            if (oculusGeometry == null || oculusGeometry.Length == 0)
            {
                return false;
            }

            geometry.AddRange(oculusGeometry);
            return true;
        }

        /// <summary>
        /// Returns an array of 3d points (in clockwise order) that define the specified boundary type.
        /// All points are returned in local tracking space shared by tracked nodes and accessible through OVRPlayerRig's trackingSpace anchor.
        /// </summary>
        private Vector3[] GetGeometry(OculusApi.BoundaryType boundaryType)
        {
            int pointsCount = 0;

            if (OculusApi.GetBoundaryGeometry(boundaryType, IntPtr.Zero, ref pointsCount))
            {
                //Assume if the number of points returned in the boundary is the same, it is the same boundary.
                if (cachedPoints.Length == pointsCount)
                {
                    return cachedPoints;
                }

                if (pointsCount > 0)
                {
                    int requiredNativeBufferCapacity = pointsCount * cachedVector3fSize;

                    if (cachedGeometryNativeBuffer.GetCapacity() < requiredNativeBufferCapacity)
                    {
                        cachedGeometryNativeBuffer.Reset(requiredNativeBufferCapacity);
                    }

                    int requiredManagedBufferCapacity = pointsCount * 3;

                    if (cachedGeometryManagedBuffer.Length < requiredManagedBufferCapacity)
                    {
                        cachedGeometryManagedBuffer = new float[requiredManagedBufferCapacity];
                    }

                    if (OculusApi.GetBoundaryGeometry(boundaryType, cachedGeometryNativeBuffer.GetPointer(), ref pointsCount))
                    {
                        Marshal.Copy(cachedGeometryNativeBuffer.GetPointer(), cachedGeometryManagedBuffer, 0, requiredManagedBufferCapacity);

                        cachedPoints = new Vector3[pointsCount];

                        for (int i = 0; i < pointsCount; i++)
                        {
                            cachedPoints[i] = new OculusApi.Vector3f
                            {
                                x = cachedGeometryManagedBuffer[3 * i + 0],
                                y = cachedGeometryManagedBuffer[3 * i + 1],
                                z = cachedGeometryManagedBuffer[3 * i + 2],
                            };
                        }
                    }
                }
            }

            return cachedPoints;
        }

        /// <summary>
        /// Returns a vector that indicates the spatial dimensions of the specified boundary type. (x = width, y = height, z = depth)
        /// </summary>
        /// <remarks>
        /// Reserved for Future use.
        /// </remarks>
        private Vector3 GetDimensions(OculusApi.BoundaryType boundaryType)
        {
            return OculusApi.GetBoundaryDimensions(boundaryType).ToVector3FlippedZ();
        }
    }
}
