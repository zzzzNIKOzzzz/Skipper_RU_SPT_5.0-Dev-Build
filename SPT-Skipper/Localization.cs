using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terkoiz.Skipper
{
    public static class Localization
    {
        private static string _currentLang = "En";

        private static readonly Dictionary<string, Dictionary<string, string>> _table =
            new Dictionary<string, Dictionary<string, string>>
            {
                ["En"] = new Dictionary<string, string>
                {
                    ["Section_Main"] = "Main",

                    ["Name_Enabled"] = "1. Enabled",
                    ["Desc_Enabled"] = "Global mod toggle.",

                    ["Name_AlwaysDisplay"] = "2. Always display Skip button",
                    ["Desc_AlwaysDisplay"] = "Always visible.",

                    ["Name_DisplayHotkey"] = "3. Display hotkey",
                    ["Desc_DisplayHotkey"] = "Holding this key will show the Skip buttons.",
                },
                ["Ru"] = new Dictionary<string, string>
                {
                    ["Section_Main"] = "Основное",

                    ["Name_Enabled"] = "1. Включено",
                    ["Desc_Enabled"] = "Глобальный переключатель мода.",

                    ["Name_AlwaysDisplay"] = "2. Всегда показывать кнопку пропуска",
                    ["Desc_AlwaysDisplay"] = "Кнопка видна всегда, без удержания клавиши.",

                    ["Name_DisplayHotkey"] = "3. Клавиша показа",
                    ["Desc_DisplayHotkey"] = "Удержание этой клавиши показывает кнопки пропуска.",
                }
            };

        public static string T(string key)
        {
            if (_table.TryGetValue(_currentLang, out var lang) && lang.TryGetValue(key, out var value))
                return value;

            if (_table["En"].TryGetValue(key, out var fallback))
                return fallback;

            return key;
        }

        public static void SetLanguage(string lang)
        {
            var normalized = NormalizeLanguage(lang);
            if (!string.IsNullOrEmpty(normalized))
            {
                _currentLang = normalized;
            }
        }

        public static string CurrentLanguage => _currentLang;

        public static void DetectLanguage()
        {
            var detected = TryGetSptLanguage();
            if (string.IsNullOrEmpty(detected))
                detected = TryGetUnityLanguage();

            var normalized = NormalizeLanguage(detected);

            if (!string.IsNullOrEmpty(normalized))
            {
                _currentLang = normalized;
                return;
            }

            _currentLang = "En";
        }

        private static string TryGetSptLanguage()
        {
            try
            {
                Type helperType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("EFT.LocalizedHelper", false)
                         ?? asm.GetType("LocalizedHelper", false);
                    if (t != null) { helperType = t; break; }
                }

                if (helperType == null) return null;

                const System.Reflection.BindingFlags flags =
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static;

                var prop = helperType.GetProperty("CurrentLanguage", flags)
                        ?? helperType.GetProperty("Language", flags);
                if (prop != null)
                {
                    var v = prop.GetValue(null);
                    if (v is string s && !string.IsNullOrEmpty(s))
                        return s;
                }

                var field = helperType.GetField("CurrentLanguage", flags)
                         ?? helperType.GetField("_currentLanguage", flags)
                         ?? helperType.GetField("Language", flags);
                if (field != null)
                {
                    var v = field.GetValue(null);
                    if (v is string s && !string.IsNullOrEmpty(s))
                        return s;

                    if (v != null) return v.ToString();
                }
            }
            catch { }

            return null;
        }

        private static string TryGetUnityLanguage()
        {
            try
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Russian: return "ru";
                    case SystemLanguage.English: return "en";
                }
            }
            catch { }
            return null;
        }

        private static string NormalizeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return null;

            var main = lang.Split('-', '_')[0].Trim().ToLowerInvariant();

            switch (main)
            {
                case "ru": return "Ru";
                case "en": return "En";
                default: return null;
            }
        }
    }
}