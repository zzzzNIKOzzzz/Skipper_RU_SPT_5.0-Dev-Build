using System;
using BepInEx.Configuration;

namespace ConfigurationManager
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationManagerAttributes : Attribute
    {
        public Action<ConfigEntryBase> CustomDrawer { get; set; }
        public bool? Browsable { get; set; }
        public string Category { get; set; }
        public string DispName { get; set; }
        public int? Order { get; set; }
        public bool? ShowRangeAsPercent { get; set; }
        public bool? IsAdvanced { get; set; }
    }
}