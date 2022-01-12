using System;
using UnityEngine;

namespace Trashy.UI
{
    internal static partial class GUILayoutEx
    {
        private static PopupWindow s_popupWindow;
        private static GUIStyle s_popupStyle;

        public static bool Popup(ref int selectedIndex, string[] items)
        {
            if (s_popupWindow == null)
            {
                s_popupStyle = new GUIStyle(GUI.skin.window);
                s_popupStyle.normal = GUI.skin.box.normal;
                s_popupStyle.padding.top = 5;
                s_popupWindow = new PopupWindow();
                UIManager.AddWindow(s_popupWindow);
            }

            if (!s_popupWindow.IsOpen)
            {
                s_popupWindow.Items = items;
                s_popupWindow.Result = null;
                s_popupWindow.IsOpen = true;
            }
            else
            {
                if (s_popupWindow.Result.HasValue)
                {
                    selectedIndex = s_popupWindow.Result.Value;
                    s_popupWindow.IsOpen = false;
                    return true;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    rect = GUIUtility.GUIToScreenRect(rect);
                    s_popupWindow.Position = rect.position + new Vector2(0, rect.size.y);
                    var size = new Vector2(rect.width, GUI.skin.button.margin.bottom * items.Length + 5);
                    foreach (var item in items)
                    {
                        var itemSize = GUI.skin.button.CalcSize(TempContent(item));
                        size.x = Math.Max(size.x, itemSize.x);
                        size.y += itemSize.y;
                    }

                    s_popupWindow.Size = size;
                    s_popupWindow.Items = items;
                }
            }

            return false;
        }

        private class PopupWindow : Window
        {
            public Vector2 Position { get; set; }
            public Vector2 Size { get; set; }
            public string[] Items { get; set; }
            public int? Result { get; set; }

            public override void OnDraw()
            {
                GUI.ModalWindow(Id, new Rect(Position, Size), windowId =>
                {
                    for (var i = 0; i < Items.Length; ++i)
                    {
                        if (GUILayout.Button(Items[i]))
                            Result = i;
                    }
                }, GUIContent.none, s_popupStyle);
            }
        }
    }
}
