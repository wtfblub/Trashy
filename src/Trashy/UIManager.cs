using System.Collections;
using Lean.Gui;
using Lean.Transition;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.UI;

namespace Trashy
{
    public class UIManager : MonoBehaviour
    {
        private GameObject _configSelectorIcon;
        private GameObject _configSelectorButton;
        private GameObject _configSelectorTransition;
        private LeanPlayer _configSelectorLeanPlayer;
        private ConfigWindowController _configWindowController;
        private ConfigTabTrashy _configTab;

        public static GUISkin Skin;

        private void Start()
        {
            if (TrashyPlugin.Bundle != null)
                Skin = TrashyPlugin.Bundle.LoadAsset<GUISkin>("Assets/TrashySkin.guiskin");

            StartCoroutine(WaitForConfigWindowController());
        }

        private void OnDestroy()
        {
            _configWindowController.ConfigWindowSwitch.States.Remove(_configSelectorLeanPlayer);
            Destroy(_configSelectorIcon);
            Destroy(_configSelectorButton);
            Destroy(_configSelectorTransition);

            _configWindowController.WindowTabs().Remove(_configTab);
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
            _configSelectorIcon.transform.position = new Vector3(-64.5f, 53.7468f, 100);
            _configSelectorIcon.name = "Trashy: Icon";
            _configSelectorIcon.GetComponent<Image>().sprite = GetComponent<SpriteManager>().Icon;

            _configSelectorButton = Instantiate(buttonTemplate, configTabSelector.transform, true);
            _configSelectorButton.transform.position = new Vector3(-64.5f, 53.7468f, 100);
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
