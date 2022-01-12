using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Gui;
using Lean.Transition;
using Lean.Transition.Method;
using Trashy.Twitch;
using UnityEngine;
using UnityEngine.UI;

namespace Trashy.UI
{
    public class UIManager : MonoBehaviour
    {
        private static readonly List<Window> s_windows = new List<Window>();
        private GameObject _configSelectorIcon;
        private GameObject _configSelectorButton;
        private GameObject _configSelectorTransition;
        private LeanPlayer _configSelectorLeanPlayer;
        private ConfigWindowController _configWindowController;
        private ConfigTabTrashy _configTab;

        public static GUISkin Skin;

        public static void AddWindow(Window window)
        {
            s_windows.Add(window);
        }

        public static T GetWindow<T>()
        {
            return s_windows.OfType<T>().FirstOrDefault();
        }

        public UIManager()
        {
            AddWindow(new MessageWindow());
        }

        private void Start()
        {
            if (TrashyPlugin.Bundle != null)
                Skin = TrashyPlugin.Bundle.LoadAsset<GUISkin>("Assets/TrashySkin.guiskin");

            AddWindow(new GeneralConfigWindow(
                GetComponent<SpriteManager>(),
                GetComponent<PubSubService>(),
                GetComponent<ChatService>()
            ));
            AddWindow(new TriggersWindow(GetComponent<ItemSpawner>(), GetComponent<SpriteManager>()));
            StartCoroutine(WaitForConfigWindowController());
        }

        private void OnGUI()
        {
            if (Skin != null)
                GUI.skin = Skin;

            foreach (var window in s_windows)
            {
                if (window.IsOpen)
                    window.OnDraw();
            }
        }

        private void OnDestroy()
        {
            _configWindowController.ConfigWindowSwitch.States.Remove(_configSelectorLeanPlayer);
            Destroy(_configSelectorIcon);
            Destroy(_configSelectorButton);
            Destroy(_configSelectorTransition);

            _configWindowController.WindowTabs().Remove(_configTab);
            foreach (var window in s_windows)
                window.IsOpen = false;

            s_windows.Clear();
        }

        private IEnumerator WaitForConfigWindowController()
        {
            while ((_configWindowController = FindObjectOfType<ConfigWindowController>()) == null)
                yield return new WaitForSeconds(1);

            while (_configWindowController.WindowTabs() == null)
                yield return new WaitForSeconds(1);

            _configTab = gameObject.AddComponent<ConfigTabTrashy>();
            _configWindowController.WindowTabs().Add(_configTab);
            InitializeConfigSelector();
        }

        private void InitializeConfigSelector()
        {
            var iconTemplate = GameObject.Find("Right Icon: Hotkey Config");
            var buttonTemplate = GameObject.Find("Right Button (LeanButton)");
            var transitionTemplate = GameObject.Find("[Right Transitions]");
            var configTabSelector = GameObject.Find("ConfigTabSelector");

            _configSelectorIcon = Instantiate(iconTemplate, configTabSelector.transform, true);
            _configSelectorIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(293.76f, -6.3997f);
            _configSelectorIcon.name = "Trashy: Icon";
            _configSelectorIcon.GetComponent<Image>().sprite = GetComponent<SpriteManager>().Icon;

            _configSelectorButton = Instantiate(buttonTemplate, configTabSelector.transform, true);
            _configSelectorButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(293.76f, -6.3997f);
            _configSelectorButton.name = "Trashy: Button";

            // Destroy the cloned button because it contains persistent event callbacks that cant be removed
            DestroyImmediate(_configSelectorButton.GetComponent<LeanButton>());

            _configSelectorButton.AddComponent<LeanButton>().OnDown.AddListener(() =>
            {
                _configWindowController.ConfigWindowSwitch.Switch(4);
                _configWindowController.TabSelected(4);
            });

            _configSelectorTransition = Instantiate(transitionTemplate, configTabSelector.transform, true);
            _configSelectorTransition.name = "Trashy: Transition";
            _configSelectorTransition.GetComponent<LeanRectTransformAnchoredPositionX>().Data.Position = 300;

            _configSelectorLeanPlayer = new LeanPlayer();
            _configSelectorLeanPlayer.Roots.Add(_configSelectorTransition.transform);
            _configWindowController.ConfigWindowSwitch.States.Add(_configSelectorLeanPlayer);
        }
    }
}
