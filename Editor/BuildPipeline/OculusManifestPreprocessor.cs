/************************************************************************************

Copyright   :   Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus SDK License Version 3.4.1 (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using XRTK.Editor.Utilities;

namespace XRTK.Oculus.Editor.Build
{
    /// <summary>
    /// https://developer.oculus.com/documentation/native/android/mobile-native-manifest/
    /// </summary>
    public class OculusManifestPreprocessor
    {
        private const string TEMPLATE_MANIFEST_FILE_NAME = "AndroidManifest.OVRSubmission.xml";
        private const string OUTPUT_MANIFEST_FILE_NAME = "AndroidManifest.xml";
        private const string DOF_MODE_MARKER = "<!-- Request the headset DoF mode -->";
        private const string HAND_TRACKING_MODE_MARKER = "<!-- Request the headset hand tracking mode -->";

        private static string ManifestFolder => $"{Application.dataPath}/Plugins/Android";

        private static string DestinationPath => $"{ManifestFolder}/{OUTPUT_MANIFEST_FILE_NAME}";

        /// <summary>
        /// Generates an Android Manifest that is valid for Oculus Store submissions and enables
        /// hand tracking on the Oculus Quest.
        /// </summary>
        [MenuItem("Mixed Reality Toolkit/Tools/Oculus/Create Oculus Quest compatible AndroidManifest.xml", false, 100000)]
        public static void GenerateManifestForSubmission()
        {
            var assetPath = PathFinderUtility.ResolvePath<IPathFinder>(typeof(OculusPathFinder));
            var editorDir = $"{Path.GetFullPath(assetPath)}/Editor";
            var srcFile = $"{editorDir}/BuildPipeline/{TEMPLATE_MANIFEST_FILE_NAME}";
            
            if (!File.Exists(srcFile))
            {
                Debug.LogError("Cannot find Android manifest template for submission. Please reimport the XRTK.Oculus package.");
                return;
            }

            if (!Directory.Exists(ManifestFolder))
            {
                Directory.CreateDirectory(ManifestFolder);
            }

            if (File.Exists(DestinationPath))
            {
                Debug.LogWarning($"Cannot create Oculus store-compatible manifest due to conflicting file: \"{DestinationPath}\". Please remove it and try again.");
                return;
            }

            var manifestText = File.ReadAllText(srcFile);
            var dofTextIndex = manifestText.IndexOf(DOF_MODE_MARKER, StringComparison.Ordinal);

            if (dofTextIndex != -1)
            {
                //Forces Quest configuration. Needs flip for Go/Gear viewer
                const string headTrackingFeatureText = "<uses-feature android:name=\"android.hardware.vr.headtracking\" android:version=\"1\" android:required=\"true\" />";
                manifestText = manifestText.Insert(dofTextIndex, headTrackingFeatureText);
            }
            else
            {
                Debug.LogWarning("Manifest error: unable to locate headset DoF mode");
            }

            var handTrackingTextIndex = manifestText.IndexOf(HAND_TRACKING_MODE_MARKER, StringComparison.Ordinal);

            if (handTrackingTextIndex != -1)
            {
                bool handTrackingEntryNeeded = true; // (targetHandTrackingSupport != OVRProjectConfig.HandTrackingSupport.ControllersOnly);
                bool handTrackingRequired = false; // (targetHandTrackingSupport == OVRProjectConfig.HandTrackingSupport.HandsOnly);

                // TODO add back in?
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (handTrackingEntryNeeded)
                {
                    string handTrackingFeatureText = $"<uses-feature android:name=\"oculus.software.handtracking\" android:required=\"{(handTrackingRequired ? "true" : "false")}\" />";
                    const string handTrackingPermissionText = "<uses-permission android:name=\"com.oculus.permission.HAND_TRACKING\" />";

                    manifestText = manifestText.Insert(handTrackingTextIndex, handTrackingPermissionText);
                    manifestText = manifestText.Insert(handTrackingTextIndex, handTrackingFeatureText);
                }
            }
            else
            {
                Debug.LogWarning("Manifest error: unable to locate headset hand tracking mode");
            }

            File.WriteAllText(DestinationPath, manifestText);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Removes any existing Android Manifest if it exists.
        /// </summary>
        [MenuItem("Mixed Reality Toolkit/Tools/Oculus/Remove AndroidManifest.xml", false, 100001)]
        public static void RemoveAndroidManifest()
        {
            AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest.xml");
            AssetDatabase.Refresh();
        }
    }
}
