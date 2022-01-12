using System;
using BepInEx.Configuration;
using UnityEngine;

namespace Trashy.UI
{
    internal static partial class GUILayoutEx
    {
        private static GUIContent s_tempContent = new GUIContent();
        private static int s_dropDownButtonId = -1;

        public static GUIContent TempContent(string text)
        {
            s_tempContent.text = text;
            s_tempContent.tooltip = null;
            s_tempContent.image = null;
            return s_tempContent;
        }

        public static void Separator()
        {
            var rect = GUILayoutUtility.GetRect(1, Screen.width, 1, 1);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            // GUILayout.Box("", GUI.skin.horizontalSlider, GUILayout.Height(1));
            // GUILayout.Label("", GUI.skin.horizontalSlider);
        }

        public static IDisposable HorizontalScope()
        {
            return new GUILayout.HorizontalScope();
        }

        public static IDisposable VerticalScope()
        {
            return new GUILayout.VerticalScope();
        }

        public static IDisposable ScrollView(ref Vector2 scrollPosition)
        {
            var scope = new GUILayout.ScrollViewScope(scrollPosition);
            scrollPosition = scope.scrollPosition;
            return scope;
        }

        public static bool DropDownButton(string text, ref int selectedIndex, string[] items,
            params GUILayoutOption[] layoutOptions)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);

            // I have no idea why this is -1 sometimes
            if (id == -1)
                return false;

            if (GUILayout.Button(text, layoutOptions))
                s_dropDownButtonId = id;

            if (s_dropDownButtonId == id && GUIUtility.hotControl == 0)
            {
                if (Popup(ref selectedIndex, items))
                {
                    s_dropDownButtonId = -1;
                    return true;
                }

                return false;
            }

            return false;
        }

        public static void NewConfigEntry(string name, Action draw)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(
                    name,
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 10, 0) },
                    GUILayout.Width(150)
                );
                GUILayout.BeginVertical();
                {
                    draw();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        public static int ConfigIntSlider(
            string name,
            int value,
            int minValue,
            int maxValue,
            int steps = 1,
            Func<int, string> formatLabel = null
        )
        {
            var newValue = value;
            NewConfigEntry(name, () =>
            {
                GUILayout.Label(
                    formatLabel != null ? formatLabel(value) : value.ToString(),
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                );
                newValue = steps * (Mathf.RoundToInt(
                    GUILayout.HorizontalSlider(value, minValue, maxValue)
                ) / steps);
            });

            return newValue;
        }

        public static void ConfigIntSlider(
            string name,
            ConfigEntry<int> configValue,
            int minValue,
            int maxValue,
            int steps = 1,
            Func<int, string> formatLabel = null
        )
        {
            var value = configValue.Value;

            NewConfigEntry(name, () =>
            {
                GUILayout.Label(
                    formatLabel != null ? formatLabel(value) : value.ToString(),
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                );
                configValue.Value = steps * (Mathf.RoundToInt(
                    GUILayout.HorizontalSlider(value, minValue, maxValue)
                ) / steps);
            });
        }

        public static float ConfigFloatSlider(
            string name,
            float value,
            float minValue,
            float maxValue,
            Func<float, string> formatLabel = null,
            int roundDigits = 1
        )
        {
            var newValue = value;

            NewConfigEntry(name, () =>
            {
                GUILayout.Label(
                    formatLabel != null ? formatLabel(value) : value.ToString(),
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                );
                newValue = (float)Math.Round(
                    (decimal)GUILayout.HorizontalSlider(value, minValue, maxValue),
                    roundDigits
                );
            });

            return newValue;
        }

        public static void ConfigFloatSlider(
            string name,
            ConfigEntry<float> configValue,
            float minValue,
            float maxValue,
            Func<float, string> formatLabel = null,
            int roundDigits = 1
        )
        {
            var value = configValue.Value;

            NewConfigEntry(name, () =>
            {
                GUILayout.Label(
                    formatLabel != null ? formatLabel(value) : value.ToString(),
                    new GUIStyle(GUI.skin.label) { padding = new RectOffset(100, 0, 0, 0) }
                );
                configValue.Value = (float)Math.Round(
                    (decimal)GUILayout.HorizontalSlider(value, minValue, maxValue),
                    roundDigits
                );
            });
        }
    }
}
