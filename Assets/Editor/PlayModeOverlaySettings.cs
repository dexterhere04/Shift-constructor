using System;
using UnityEditor;
using UnityEngine;

namespace ShiftConstructor.EditorTools
{
    /// <summary>
    /// User-level preferences for the play mode overlay. They live in <see cref="EditorPrefs"/>
    /// so trying out a colour never dirties the project or gets committed by accident.
    /// </summary>
    internal static class PlayModeOverlaySettings
    {
        private const string KeyPrefix = "ShiftConstructor.PlayModeOverlay.";

        private static readonly Color DefaultTintColor = new Color(1f, 0.45f, 0.05f, 0.1f);
        private static readonly Color DefaultBorderColor = new Color(1f, 0.45f, 0.05f, 0.9f);
        private const float DefaultBorderWidth = 3f;

        internal static event Action Changed;

        internal static bool Enabled
        {
            get => EditorPrefs.GetBool(KeyPrefix + "Enabled", true);
            set => SetBool(KeyPrefix + "Enabled", value);
        }

        internal static bool ApplyToSceneView
        {
            get => EditorPrefs.GetBool(KeyPrefix + "SceneView", true);
            set => SetBool(KeyPrefix + "SceneView", value);
        }

        internal static bool ApplyToGameView
        {
            get => EditorPrefs.GetBool(KeyPrefix + "GameView", true);
            set => SetBool(KeyPrefix + "GameView", value);
        }

        internal static bool ShowTint
        {
            get => EditorPrefs.GetBool(KeyPrefix + "ShowTint", true);
            set => SetBool(KeyPrefix + "ShowTint", value);
        }

        internal static Color TintColor
        {
            get => GetColor(KeyPrefix + "TintColor", DefaultTintColor);
            set => SetColor(KeyPrefix + "TintColor", value);
        }

        internal static bool ShowBorder
        {
            get => EditorPrefs.GetBool(KeyPrefix + "ShowBorder", true);
            set => SetBool(KeyPrefix + "ShowBorder", value);
        }

        internal static Color BorderColor
        {
            get => GetColor(KeyPrefix + "BorderColor", DefaultBorderColor);
            set => SetColor(KeyPrefix + "BorderColor", value);
        }

        internal static float BorderWidth
        {
            get => EditorPrefs.GetFloat(KeyPrefix + "BorderWidth", DefaultBorderWidth);
            set => SetFloat(KeyPrefix + "BorderWidth", value);
        }

        internal static void Reset()
        {
            EditorPrefs.DeleteKey(KeyPrefix + "Enabled");
            EditorPrefs.DeleteKey(KeyPrefix + "SceneView");
            EditorPrefs.DeleteKey(KeyPrefix + "GameView");
            EditorPrefs.DeleteKey(KeyPrefix + "ShowTint");
            EditorPrefs.DeleteKey(KeyPrefix + "TintColor");
            EditorPrefs.DeleteKey(KeyPrefix + "ShowBorder");
            EditorPrefs.DeleteKey(KeyPrefix + "BorderColor");
            EditorPrefs.DeleteKey(KeyPrefix + "BorderWidth");
            RaiseChanged();
        }

        [SettingsProvider]
        private static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Preferences/Play Mode Overlay", SettingsScope.User)
            {
                label = "Play Mode Overlay",
                keywords = new[] { "play", "playmode", "overlay", "tint", "colour", "color", "border" },
                guiHandler = DrawPreferences,
            };
        }

        private static void DrawPreferences(string searchContext)
        {
            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Play Mode Overlay", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    "Tints the Scene and Game views while Play Mode is active, so it is obvious that you are not editing.",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space(8f);

            bool enabled = Enabled;
            bool sceneView = ApplyToSceneView;
            bool gameView = ApplyToGameView;
            bool showTint = ShowTint;
            Color tintColor = TintColor;
            bool showBorder = ShowBorder;
            Color borderColor = BorderColor;
            float borderWidth = BorderWidth;

            EditorGUI.BeginChangeCheck();

            enabled = EditorGUILayout.Toggle("Enabled", enabled);

            using (new EditorGUI.DisabledScope(!enabled))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Where", EditorStyles.boldLabel);
                sceneView = EditorGUILayout.Toggle("Scene View", sceneView);
                gameView = EditorGUILayout.Toggle("Game View", gameView);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

                showTint = EditorGUILayout.Toggle("Fill", showTint);
                using (new EditorGUI.DisabledScope(!showTint))
                {
                    tintColor = EditorGUILayout.ColorField("Fill Colour", tintColor);
                }

                showBorder = EditorGUILayout.Toggle("Border", showBorder);
                using (new EditorGUI.DisabledScope(!showBorder))
                {
                    borderColor = EditorGUILayout.ColorField("Border Colour", borderColor);
                    borderWidth = EditorGUILayout.Slider("Border Width", borderWidth, 1f, 16f);
                }

                EditorGUILayout.Space(6f);
                DrawPreview(showTint, tintColor, showBorder, borderColor, borderWidth);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Enabled = enabled;
                ApplyToSceneView = sceneView;
                ApplyToGameView = gameView;
                ShowTint = showTint;
                TintColor = tintColor;
                ShowBorder = showBorder;
                BorderColor = borderColor;
                BorderWidth = borderWidth;
            }

            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Reset to defaults"))
            {
                Reset();
            }
        }

        private static void DrawPreview(bool showTint, Color tintColor, bool showBorder, Color borderColor, float borderWidth)
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            Rect rect = GUILayoutUtility.GetRect(0f, 56f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));
            DevTools.PlayModeOverlayHud.DrawOverlay(
                rect, showTint, tintColor, showBorder, borderColor, borderWidth);
        }

        private static void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool(key, value);
            RaiseChanged();
        }

        private static void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat(key, value);
            RaiseChanged();
        }

        private static Color GetColor(string key, Color fallback)
        {
            string stored = EditorPrefs.GetString(key, string.Empty);
            return ColorUtility.TryParseHtmlString(stored, out Color color) ? color : fallback;
        }

        private static void SetColor(string key, Color value)
        {
            EditorPrefs.SetString(key, "#" + ColorUtility.ToHtmlStringRGBA(value));
            RaiseChanged();
        }

        private static void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
