using Trashy.UI;
using UnityEngine;

namespace Trashy
{
    public class ConfigTabTrashy : ConfigWindowTab
    {
        private readonly CanvasGroup _hotkeysCanvasGroup;

        public ConfigTabTrashy()
        {
            WindowID = 4;
            _hotkeysCanvasGroup = FindObjectOfType<ConfigTab_Hotkeys>().GetComponent<CanvasGroup>();
        }

        private new void Awake()
        {
            // Overwrite ConfigWindowTab.Awake since our gameobject does not have the Components this is looking for
        }

        public override void Initialize(int selectedTab)
        {
            setIsSelected(selectedTab);
            CanvasGroup.alpha = isSelected ? 1 : 0;
            UIManager.GetWindow<GeneralConfigWindow>().IsOpen = isSelected;
        }

        public override void SetPlatformLayout(RuntimePlatform platform)
        {
        }

        public override void ClosedConfig()
        {
        }

        private void Update()
        {
            if (!isSelected)
                return;

            if (_hotkeysCanvasGroup.alpha > 0)
                _hotkeysCanvasGroup.alpha = 0;
        }
    }
}
