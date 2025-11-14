using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace AffenCode
{
    [CustomEditor(typeof(SafeArea))]
    public class SafeAreaEditor : Editor
    {
        private SafeArea _safeArea;
        private SerializedProperty _rectTransform;
        private SerializedProperty _canvasScaler;
        private SerializedProperty _strategy;
        private SerializedProperty _padding;
        private SerializedProperty _leftMargin;
        private SerializedProperty _rightMargin;
        private SerializedProperty _topMargin;
        private SerializedProperty _bottomMargin;
        private SerializedProperty _leftSide;
        private SerializedProperty _rightSide;
        private SerializedProperty _topSide;
        private SerializedProperty _bottomSide;
        private SerializedProperty _animateChanges;
        private SerializedProperty _animationDuration;
        private SerializedProperty _animationCurve;
        private SerializedProperty _postProcesses;

        // Device simulation
        private bool _showDeviceSimulation = false;
        private DevicePreset _selectedDevice = DevicePreset.iPhone14;
        private bool _showPreviewSettings = true;
        private bool _autoUpdatePreview = true;

        // Preview settings
        private Color _safeAreaColor = new Color(0f, 1f, 0f, 0.2f);
        private Color _unsafeAreaColor = new Color(1f, 0f, 0f, 0.1f);
        private bool _showSafeAreaOutline = true;
        private bool _showDeviceFrame = false;

        // Foldouts
        private bool _showPositioning = true;
        private bool _showMargins = false;
        private bool _showIndividualSides = false;
        private bool _showAnimation = false;
        private bool _showPostProcessing = false;

        private enum DevicePreset
        {
            iPhoneSE, iPhone14, iPhone14Pro, iPhone14ProMax,
            iPad, iPadPro, SamsungGalaxyS23, Pixel7,
            Custom
        }

        private static readonly Dictionary<DevicePreset, Rect> DeviceSafeAreas = new Dictionary<DevicePreset, Rect>
        {
            { DevicePreset.iPhoneSE, new Rect(0, 0, 375, 667) }, // No notch
            { DevicePreset.iPhone14, new Rect(0, 59, 390, 668) }, // Notch
            { DevicePreset.iPhone14Pro, new Rect(0, 59, 393, 699) }, // Dynamic Island
            { DevicePreset.iPhone14ProMax, new Rect(0, 59, 430, 874) }, // Dynamic Island
            { DevicePreset.iPad, new Rect(0, 0, 768, 1024) }, // No safe area
            { DevicePreset.iPadPro, new Rect(0, 0, 1024, 1366) }, // No safe area
            { DevicePreset.SamsungGalaxyS23, new Rect(0, 0, 360, 780) }, // Punch hole
            { DevicePreset.Pixel7, new Rect(0, 0, 412, 915) } // No notch
        };

        private void OnEnable()
        {
            _safeArea = (SafeArea)target;

            _rectTransform = serializedObject.FindProperty("RectTransform");
            _canvasScaler = serializedObject.FindProperty("CanvasScaler");
            _strategy = serializedObject.FindProperty("Strategy");
            _padding = serializedObject.FindProperty("Padding");
            _leftMargin = serializedObject.FindProperty("LeftMargin");
            _rightMargin = serializedObject.FindProperty("RightMargin");
            _topMargin = serializedObject.FindProperty("TopMargin");
            _bottomMargin = serializedObject.FindProperty("BottomMargin");
            _leftSide = serializedObject.FindProperty("LeftSide");
            _rightSide = serializedObject.FindProperty("RightSide");
            _topSide = serializedObject.FindProperty("TopSide");
            _bottomSide = serializedObject.FindProperty("BottomSide");
            _animateChanges = serializedObject.FindProperty("AnimateChanges");
            _animationDuration = serializedObject.FindProperty("AnimationDuration");
            _animationCurve = serializedObject.FindProperty("AnimationCurve");
            _postProcesses = serializedObject.FindProperty("PostProcesses");

            // Load preview settings
            _safeAreaColor = EditorPrefs.GetString("SafeAreaEditor_SafeAreaColor", JsonUtility.ToJson(_safeAreaColor));
            _safeAreaColor = JsonUtility.FromJson<Color>(_safeAreaColor);
            _unsafeAreaColor = EditorPrefs.GetString("SafeAreaEditor_UnsafeAreaColor", JsonUtility.ToJson(_unsafeAreaColor));
            _unsafeAreaColor = JsonUtility.FromJson<Color>(_unsafeAreaColor);
            _showSafeAreaOutline = EditorPrefs.GetBool("SafeAreaEditor_ShowOutline", true);
            _showDeviceFrame = EditorPrefs.GetBool("SafeAreaEditor_ShowDeviceFrame", false);
            _autoUpdatePreview = EditorPrefs.GetBool("SafeAreaEditor_AutoUpdate", true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            DrawCoreSettings();
            DrawPositioningSection();
            DrawAnimationSection();
            DrawPostProcessingSection();
            DrawPreviewSection();

            serializedObject.ApplyModifiedProperties();

            // Auto-update preview
            if (_autoUpdatePreview && GUI.changed)
            {
                UpdatePreview();
                SceneView.RepaintAll();
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("🎯 Safe Area", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    _safeArea.ForceRefresh();
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Help", GUILayout.Width(40)))
                {
                    Application.OpenURL("https://docs.unity3d.com/Manual/class-CanvasScaler.html");
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawCoreSettings()
        {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_rectTransform);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_canvasScaler);

                if (_safeArea.CanvasScaler == null)
                {
                    if (GUILayout.Button("Auto Detect", GUILayout.Width(80)))
                    {
                        _safeArea.CanvasScaler = _safeArea.GetComponentInParent<CanvasScaler>();
                        serializedObject.Update();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_safeArea.CanvasScaler == null)
                {
                    EditorGUILayout.HelpBox("CanvasScaler is required for proper safe area calculations.", MessageType.Warning);
                }
                else
                {
                    var scaler = _safeArea.CanvasScaler;
                    EditorGUILayout.LabelField($"Canvas Mode: {scaler.screenMatchMode}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Reference Resolution: {scaler.referenceResolution.x}×{scaler.referenceResolution.y}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawPositioningSection()
        {
            _showPositioning = EditorGUILayout.Foldout(_showPositioning, "Positioning Strategy", true);

            if (_showPositioning)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_strategy);
                    EditorGUILayout.PropertyField(_padding);

                    var strategy = (SafeArea.PositioningStrategy)_strategy.enumValueIndex;

                    switch (strategy)
                    {
                        case SafeArea.PositioningStrategy.CustomMargins:
                            DrawCustomMarginsSettings();
                            break;

                        case SafeArea.PositioningStrategy.IndividualSides:
                            DrawIndividualSidesSettings();
                            break;

                        default:
                            // Show current safe area info for Stretch and Fit
                            if (_safeArea.CanvasSafeArea != Rect.zero)
                            {
                                EditorGUILayout.LabelField("Current Safe Area:", EditorStyles.miniBoldLabel);
                                EditorGUILayout.LabelField($"Position: ({_safeArea.CanvasSafeArea.x:F1}, {_safeArea.CanvasSafeArea.y:F1})", EditorStyles.miniLabel);
                                EditorGUILayout.LabelField($"Size: {_safeArea.CanvasSafeArea.width:F1} × {_safeArea.CanvasSafeArea.height:F1}", EditorStyles.miniLabel);
                            }
                            break;
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawCustomMarginsSettings()
        {
            _showMargins = EditorGUILayout.Foldout(_showMargins, "Custom Margins", true);

            if (_showMargins)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_leftMargin);
                    EditorGUILayout.PropertyField(_rightMargin);
                    EditorGUILayout.PropertyField(_topMargin);
                    EditorGUILayout.PropertyField(_bottomMargin);

                    // Visual margin editor
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Visual Margin Editor:", EditorStyles.miniBoldLabel);

                    var rect = EditorGUILayout.GetControlRect(false, 100);
                    DrawMarginVisualizer(rect);
                }
            }
        }

        private void DrawIndividualSidesSettings()
        {
            _showIndividualSides = EditorGUILayout.Foldout(_showIndividualSides, "Individual Sides", true);

            if (_showIndividualSides)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawSideProperty("Left Side", _leftSide);
                    DrawSideProperty("Right Side", _rightSide);
                    DrawSideProperty("Top Side", _topSide);
                    DrawSideProperty("Bottom Side", _bottomSide);
                }
            }
        }

        private void DrawSideProperty(string label, SerializedProperty sideProperty)
        {
            var enabled = sideProperty.FindPropertyRelative("enabled");
            var margin = sideProperty.FindPropertyRelative("margin");

            using (new EditorGUILayout.HorizontalScope())
            {
                enabled.boolValue = EditorGUILayout.ToggleLeft(label, enabled.boolValue, GUILayout.Width(80));
                GUI.enabled = enabled.boolValue;
                EditorGUILayout.PropertyField(margin, GUIContent.none);
                GUI.enabled = true;
            }
        }

        private void DrawMarginVisualizer(Rect rect)
        {
            // Draw a visual representation of margins
            var canvasSize = _safeArea.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>().rect.size ?? new Vector2(400, 300);

            // Background
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));

            // Safe area (green)
            var safeRect = new Rect(
                rect.x + (_leftMargin.floatValue / canvasSize.x) * rect.width,
                rect.y + (_bottomMargin.floatValue / canvasSize.y) * rect.height,
                rect.width - ((_leftMargin.floatValue + _rightMargin.floatValue) / canvasSize.x) * rect.width,
                rect.height - ((_topMargin.floatValue + _bottomMargin.floatValue) / canvasSize.y) * rect.height
            );
            EditorGUI.DrawRect(safeRect, new Color(0f, 1f, 0f, 0.5f));

            // Border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.white);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Color.white);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.white);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), Color.white);
        }

        private void DrawAnimationSection()
        {
            _showAnimation = EditorGUILayout.Foldout(_showAnimation, "Animation", true);

            if (_showAnimation)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_animateChanges);

                    if (_animateChanges.boolValue)
                    {
                        EditorGUILayout.PropertyField(_animationDuration);
                        EditorGUILayout.PropertyField(_animationCurve);
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawPostProcessingSection()
        {
            _showPostProcessing = EditorGUILayout.Foldout(_showPostProcessing, "Post Processing", true);

            if (_showPostProcessing)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_postProcesses, true);

                    if (GUILayout.Button("Add Common Post-Processors"))
                    {
                        ShowPostProcessorMenu();
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawPreviewSection()
        {
            _showPreviewSettings = EditorGUILayout.Foldout(_showPreviewSettings, "Preview & Device Simulation", true);

            if (_showPreviewSettings)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    // Device simulation
                    _showDeviceSimulation = EditorGUILayout.ToggleLeft("Enable Device Simulation", _showDeviceSimulation);

                    if (_showDeviceSimulation)
                    {
                        _selectedDevice = (DevicePreset)EditorGUILayout.EnumPopup("Device", _selectedDevice);

                        if (_selectedDevice != DevicePreset.Custom)
                        {
                            var safeArea = DeviceSafeAreas[_selectedDevice];
                            EditorGUILayout.LabelField($"Resolution: {safeArea.width}×{safeArea.height}", EditorStyles.miniLabel);
                        }

                        if (GUILayout.Button("Apply Device Safe Area"))
                        {
                            ApplyDeviceSimulation();
                        }
                    }

                    EditorGUILayout.Space();

                    // Preview settings
                    EditorGUILayout.LabelField("Preview Colors:", EditorStyles.miniBoldLabel);

                    _safeAreaColor = EditorGUILayout.ColorField("Safe Area", _safeAreaColor);
                    _unsafeAreaColor = EditorGUILayout.ColorField("Unsafe Area", _unsafeAreaColor);
                    _showSafeAreaOutline = EditorGUILayout.Toggle("Show Outline", _showSafeAreaOutline);
                    _showDeviceFrame = EditorGUILayout.Toggle("Show Device Frame", _showDeviceFrame);

                    EditorGUILayout.Space();

                    _autoUpdatePreview = EditorGUILayout.Toggle("Auto Update Preview", _autoUpdatePreview);

                    if (!_autoUpdatePreview)
                    {
                        if (GUILayout.Button("Update Preview"))
                        {
                            UpdatePreview();
                            SceneView.RepaintAll();
                        }
                    }

                    // Save preview settings
                    if (GUI.changed)
                    {
                        EditorPrefs.SetString("SafeAreaEditor_SafeAreaColor", JsonUtility.ToJson(_safeAreaColor));
                        EditorPrefs.SetString("SafeAreaEditor_UnsafeAreaColor", JsonUtility.ToJson(_unsafeAreaColor));
                        EditorPrefs.SetBool("SafeAreaEditor_ShowOutline", _showSafeAreaOutline);
                        EditorPrefs.SetBool("SafeAreaEditor_ShowDeviceFrame", _showDeviceFrame);
                        EditorPrefs.SetBool("SafeAreaEditor_AutoUpdate", _autoUpdatePreview);
                    }
                }
            }
        }

        private void ShowPostProcessorMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Max Width Post-Processor"), false, () =>
            {
                var postProcessor = _safeArea.gameObject.AddComponent<MaxWidthSafeAreaPostProcess>();
                // Refresh the post-processors array
                serializedObject.Update();
            });

            menu.AddItem(new GUIContent("Aspect Ratio Post-Processor"), false, () =>
            {
                var postProcessor = _safeArea.gameObject.AddComponent<AspectRatioSafeAreaPostProcess>();
                serializedObject.Update();
            });

            menu.AddItem(new GUIContent("Padding Post-Processor"), false, () =>
            {
                var postProcessor = _safeArea.gameObject.AddComponent<PaddingSafeAreaPostProcess>();
                serializedObject.Update();
            });

            menu.ShowAsContext();
        }

        private void ApplyDeviceSimulation()
        {
            if (_selectedDevice == DevicePreset.Custom)
                return;

            var safeArea = DeviceSafeAreas[_selectedDevice];
            // This would apply the device simulation to the preview
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_safeArea != null)
            {
                _safeArea.ShowPreviewInEditor = true;
                _safeArea.PreviewColor = _safeAreaColor;
                _safeArea.ForceRefresh();
            }
        }

        // Scene view visualization
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawSafeAreaGizmo(SafeArea safeArea, GizmoType gizmoType)
        {
            if (!safeArea.ShowPreviewInEditor || !safeArea.enabled)
                return;

            var canvas = safeArea.GetComponentInParent<Canvas>();
            if (!canvas)
                return;

            var canvasRect = canvas.GetComponent<RectTransform>();
            var canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            // Draw safe area rectangle
            var safeAreaRect = safeArea.CanvasSafeArea;
            var worldSafeAreaMin = canvasCorners[0] + new Vector3(safeAreaRect.x, safeAreaRect.y, 0);
            var worldSafeAreaMax = canvasCorners[0] + new Vector3(safeAreaRect.xMax, safeAreaRect.yMax, 0);

            // Draw safe area fill
            Gizmos.color = safeArea.PreviewColor;
            Gizmos.DrawCube((worldSafeAreaMin + worldSafeAreaMax) * 0.5f, worldSafeAreaMax - worldSafeAreaMin);

            // Draw outline
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldSafeAreaMin, new Vector3(worldSafeAreaMax.x, worldSafeAreaMin.y, worldSafeAreaMin.z));
            Gizmos.DrawLine(new Vector3(worldSafeAreaMax.x, worldSafeAreaMin.y, worldSafeAreaMin.z), worldSafeAreaMax);
            Gizmos.DrawLine(worldSafeAreaMax, new Vector3(worldSafeAreaMin.x, worldSafeAreaMax.y, worldSafeAreaMax.z));
            Gizmos.DrawLine(new Vector3(worldSafeAreaMin.x, worldSafeAreaMax.y, worldSafeAreaMax.z), worldSafeAreaMin);
        }
    }
}
