using ShiftConstructor.DevTools;
using UnityEditor;
using UnityEngine;

namespace ShiftConstructor.EditorTools
{
    /// <summary>
    /// Draws the play mode overlay in the Scene view and keeps a runtime HUD alive in the
    /// Game view while Play Mode is running. Settings live in <see cref="PlayModeOverlaySettings"/>.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayModeOverlay
    {
        private static PlayModeOverlayHud _hud;

        static PlayModeOverlay()
        {
            SceneView.duringSceneGui += OnSceneViewGui;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            PlayModeOverlaySettings.Changed += SyncHud;
            EditorApplication.delayCall += SyncHud;
        }

        [MenuItem("Tools/Play Mode Overlay/Enable", priority = 100)]
        private static void ToggleEnabled()
        {
            PlayModeOverlaySettings.Enabled = !PlayModeOverlaySettings.Enabled;
        }

        [MenuItem("Tools/Play Mode Overlay/Enable", true)]
        private static bool ToggleEnabledValidate()
        {
            Menu.SetChecked("Tools/Play Mode Overlay/Enable", PlayModeOverlaySettings.Enabled);
            return true;
        }

        [MenuItem("Tools/Play Mode Overlay/Settings", priority = 101)]
        private static void OpenSettings()
        {
            SettingsService.OpenUserPreferences("Preferences/Play Mode Overlay");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    SyncHud();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    HideHud();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    // Play Mode tore down every runtime object, including the HUD.
                    _hud = null;
                    SceneView.RepaintAll();
                    break;
            }
        }

        private static void OnSceneViewGui(SceneView sceneView)
        {
            if (!EditorApplication.isPlaying
                || !PlayModeOverlaySettings.Enabled
                || !PlayModeOverlaySettings.ApplyToSceneView)
            {
                return;
            }

            Camera camera = sceneView.camera;
            if (camera == null)
            {
                return;
            }

            // Handles.BeginGUI() coordinates start below the Scene view toolbar, so use the
            // camera viewport rather than sceneView.position, which includes the toolbar.
            float scale = EditorGUIUtility.pixelsPerPoint;
            Rect area = new Rect(0f, 0f, camera.pixelWidth / scale, camera.pixelHeight / scale);

            Handles.BeginGUI();
            PlayModeOverlayHud.DrawOverlay(
                area,
                PlayModeOverlaySettings.ShowTint,
                PlayModeOverlaySettings.TintColor,
                PlayModeOverlaySettings.ShowBorder,
                PlayModeOverlaySettings.BorderColor,
                PlayModeOverlaySettings.BorderWidth);
            Handles.EndGUI();
        }

        private static void SyncHud()
        {
            if (!EditorApplication.isPlaying)
            {
                // Play Mode ended; the runtime HUD no longer exists.
                _hud = null;
                SceneView.RepaintAll();
                return;
            }

            if (!PlayModeOverlaySettings.Enabled || !PlayModeOverlaySettings.ApplyToGameView)
            {
                HideHud();
                SceneView.RepaintAll();
                return;
            }

            PlayModeOverlayHud hud = EnsureHud();
            if (hud != null)
            {
                hud.gameObject.SetActive(true);
                hud.Apply(
                    PlayModeOverlaySettings.ShowTint,
                    PlayModeOverlaySettings.TintColor,
                    PlayModeOverlaySettings.ShowBorder,
                    PlayModeOverlaySettings.BorderColor,
                    PlayModeOverlaySettings.BorderWidth);
            }

            SceneView.RepaintAll();
        }

        private static PlayModeOverlayHud EnsureHud()
        {
            if (_hud != null)
            {
                return _hud;
            }

            // HideAndDontSave objects are invisible to FindAnyObjectByType, and a domain reload
            // during Play Mode loses the cached reference, so look for a live HUD before creating one.
            foreach (PlayModeOverlayHud candidate in Resources.FindObjectsOfTypeAll<PlayModeOverlayHud>())
            {
                if (candidate != null)
                {
                    _hud = candidate;
                    return _hud;
                }
            }

            GameObject host = new GameObject("~PlayModeOverlay")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            _hud = host.AddComponent<PlayModeOverlayHud>();
            return _hud;
        }

        private static void HideHud()
        {
            if (_hud != null && _hud.gameObject != null)
            {
                _hud.gameObject.SetActive(false);
            }
        }
    }
}
