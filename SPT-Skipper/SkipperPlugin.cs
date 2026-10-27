using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Configuration;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace Terkoiz.Skipper
{
    [BepInPlugin("com.terkoiz.skipper", "Terkoiz.Skipper", "1.1.5")]
    public class SkipperPlugin : BasePlugin
    {
        internal const string SkipButtonName = "SkipButton";

        internal static ManualLogSource Logger { get; private set; }

        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<bool> AlwaysDisplay;
        internal static ConfigEntry<KeyboardShortcut> DisplayHotkey;

        internal static GameObject LastSeenObjectivesBlock;

        internal static readonly List<GameObject> TrackedButtons = new List<GameObject>();
        internal static string CurrentQuestId = "";

        public override void Load()
        {
            Logger = Log;
            InitConfiguration();
            Logger.LogInfo("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
            Logger.LogInfo("______________________Skipper от zzzNIKOzzz загружается_______________________");
            Logger.LogInfo("[Skipper] загрузка патча:");

            try
            {
                var harmony = new Harmony("com.terkoiz.skipper");
                harmony.PatchAll(typeof(QuestObjectiveViewPatch));
                Logger.LogInfo("[Skipper] патч применён");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Skipper] Ошибка патча: {ex}");
            }

            try
            {
                AddComponent<SkipperBehaviour>();
                Logger.LogInfo("[Skipper] Вызов компонента <SkipperBehaviour>");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Skipper] Ошибка добавления компонента: {ex}");
            }

            Logger.LogInfo("[Skipper] Загрузка завершена");
            Logger.LogInfo("[Skipper] Перевод выполнил zzzNIKOzzz");
            Logger.LogInfo("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        }

        private void InitConfiguration()
        {
            Localization.DetectLanguage();

            // Используем переведенные строки в качестве ключей конфига.
            // BepInEx сам подхватит их и красиво отобразит в меню F12.
            string section = Localization.T("Section_Main");

            ModEnabled = Config.Bind(
                section,
                Localization.T("Name_Enabled"),
                true,
                Localization.T("Desc_Enabled"));

            AlwaysDisplay = Config.Bind(
                section,
                Localization.T("Name_AlwaysDisplay"),
                false,
                Localization.T("Desc_AlwaysDisplay"));

            DisplayHotkey = Config.Bind(
                section,
                Localization.T("Name_DisplayHotkey"),
                new KeyboardShortcut(KeyCode.LeftControl),
                Localization.T("Desc_DisplayHotkey"));
        }
    }

    public class SkipperBehaviour : MonoBehaviour
    {
        private static bool _loggedFirstUpdate = false;

        public SkipperBehaviour(IntPtr ptr) : base(ptr)
        {
        }

        public void Awake()
        {
            SkipperPlugin.Logger.LogInfo("[Skipper] SkipperBehaviour.Awake");
        }

        public void OnEnable()
        {
            SkipperPlugin.Logger.LogInfo("[Skipper] SkipperBehaviour.OnEnable");
        }

        public void Update()
        {
            if (!_loggedFirstUpdate)
            {
                _loggedFirstUpdate = true;
                SkipperPlugin.Logger.LogInfo("[Skipper] Update running (first tick)");
            }

            if (!SkipperPlugin.ModEnabled.Value)
                return;

            if (SkipperPlugin.AlwaysDisplay.Value)
                return;

            if (SkipperPlugin.DisplayHotkey.Value.IsDown())
                ChangeButtonVisibility(true);

            if (SkipperPlugin.DisplayHotkey.Value.IsUp())
                ChangeButtonVisibility(false);
        }

        private static void ChangeButtonVisibility(bool visible)
        {
            int matched = 0;

            if (SkipperPlugin.LastSeenObjectivesBlock != null)
            {
                var buttons = SkipperPlugin.LastSeenObjectivesBlock
                    .GetComponentsInChildren<DefaultUIButton>(true);

                foreach (var b in buttons)
                {
                    if (b == null) continue;
                    if (b.name != SkipperPlugin.SkipButtonName) continue;

                    b.gameObject.SetActive(visible);
                    matched++;
                }
            }

            SkipperPlugin.TrackedButtons.RemoveAll(b => b == null);
            foreach (var b in SkipperPlugin.TrackedButtons)
            {
                if (b == null) continue;
                b.SetActive(visible);
                matched++;
            }

            SkipperPlugin.Logger.LogInfo(
                $"[Skipper] ChangeButtonVisibility({visible}) matched {matched}");
        }
    }
}