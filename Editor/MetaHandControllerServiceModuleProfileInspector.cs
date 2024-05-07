// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Editor.Utilities;
using RealityToolkit.Editor.Profiles.Input.Controllers;
using RealityToolkit.MetaPlatform.InputService.Profiles;
using RealityToolkit.MetaPlatform.Plugins;
using UnityEditor;
using UnityEngine;

namespace RealityToolkit.MetaPlatform.Editor
{
    /// <summary>
    /// Default inspector for <see cref="MetaHandControllerServiceModuleProfile"/>.
    /// </summary>
    [CustomEditor(typeof(MetaHandControllerServiceModuleProfile))]
    public class MetaHandControllerServiceModuleProfileInspector : BaseHandControllerServiceModuleProfileInspector
    {
        private SerializedProperty minConfidenceRequired;

        private bool showMetaHandTrackingSettings = true;
        private GUIContent confidenceContent;
        private static readonly GUIContent MetaHandSettingsFoldoutHeader = new GUIContent("Meta Hand Tracking Settings");

        protected override void OnEnable()
        {
            base.OnEnable();

            minConfidenceRequired = serializedObject.FindProperty(nameof(minConfidenceRequired));
            confidenceContent = new GUIContent(minConfidenceRequired.displayName, minConfidenceRequired.tooltip);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showMetaHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showMetaHandTrackingSettings, MetaHandSettingsFoldoutHeader, true);
            if (showMetaHandTrackingSettings)
            {
                EditorGUI.indentLevel++;
                minConfidenceRequired.intValue = (int)(OculusApi.TrackingConfidence)EditorGUILayout.EnumPopup(confidenceContent, (OculusApi.TrackingConfidence)minConfidenceRequired.intValue);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}