#if UNITY_EDITOR
using UnityEngine;

namespace ShiftConstructor.DevTools
{
    /// <summary>
    /// Draws the play mode tint and border over the Game view. The editor creates this
    /// component when Play Mode starts (see <c>ShiftConstructor.EditorTools.PlayModeOverlay</c>);
    /// it is never saved into a scene and is compiled out of player builds.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class PlayModeOverlayHud : MonoBehaviour
    {
        private bool _drawTint;
        private Color _tintColor;
        private bool _drawBorder;
        private Color _borderColor;
        private float _borderWidth;

        public void Apply(bool drawTint, Color tintColor, bool drawBorder, Color borderColor, float borderWidth)
        {
            _drawTint = drawTint;
            _tintColor = tintColor;
            _drawBorder = drawBorder;
            _borderColor = borderColor;
            _borderWidth = borderWidth;
        }

        /// <summary>
        /// Fills <paramref name="area"/> with the tint and draws an inset border around it.
        /// Shared with the preferences preview so both always look identical.
        /// </summary>
        public static void DrawOverlay(
            Rect area,
            bool drawTint,
            Color tintColor,
            bool drawBorder,
            Color borderColor,
            float borderWidth)
        {
            Color previous = GUI.color;

            if (drawTint && tintColor.a > 0f)
            {
                GUI.color = tintColor;
                GUI.DrawTexture(area, Texture2D.whiteTexture);
            }

            float width = Mathf.Clamp(borderWidth, 0f, Mathf.Min(area.width, area.height) * 0.5f);

            if (drawBorder && width > 0f && borderColor.a > 0f)
            {
                GUI.color = borderColor;
                GUI.DrawTexture(new Rect(area.x, area.y, area.width, width), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(area.x, area.yMax - width, area.width, width), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(area.x, area.y, width, area.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(area.xMax - width, area.y, width, area.height), Texture2D.whiteTexture);
            }

            GUI.color = previous;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            GUI.depth = -1000;
            DrawOverlay(
                new Rect(0f, 0f, Screen.width, Screen.height),
                _drawTint,
                _tintColor,
                _drawBorder,
                _borderColor,
                _borderWidth);
        }
    }
}
#endif
