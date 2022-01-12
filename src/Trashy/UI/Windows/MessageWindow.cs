using System.Collections.Generic;
using UnityEngine;

namespace Trashy.UI
{
    public class MessageWindow : Window
    {
        private const int WindowWidth = 400;
        private const int WindowHeight = 165;

        private readonly Queue<MessageItem> _messages = new Queue<MessageItem>();

        public void Show(string title, string message, string url = null)
        {
            _messages.Enqueue(new MessageItem(title, message, url));
            if (!IsOpen)
                IsOpen = true;
        }

        public override void OnDraw()
        {
            var messageItem = _messages.Peek();
            GUI.Window(
                Id,
                new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight),
                windowId =>
                {
                    GUILayout.Space(20);
                    using (GUILayoutEx.VerticalScope())
                    {
                        GUILayout.Label(messageItem.Message);
                        GUILayout.FlexibleSpace();

                        using (GUILayoutEx.HorizontalScope())
                        {
                            if (!string.IsNullOrWhiteSpace(messageItem.Url))
                            {
                                if (GUILayout.Button("Open in browser", GUILayout.Width(125)))
                                    Application.OpenURL(messageItem.Url);
                            }

                            if (GUILayout.Button("OK", GUILayout.Width(125)))
                            {
                                _messages.Dequeue();
                                if (_messages.Count == 0)
                                    IsOpen = false;
                            }
                        }
                    }
                },
                messageItem.Title
            );
        }

        private struct MessageItem
        {
            public string Title;
            public string Message;
            public string Url;

            public MessageItem(string title, string message, string url)
            {
                Title = title;
                Message = message;
                Url = url;
            }
        }
    }
}
