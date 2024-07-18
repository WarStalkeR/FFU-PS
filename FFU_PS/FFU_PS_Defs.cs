using System;
using System.Collections.Generic;
using System.IO;

namespace FFU_Phase_Shift {
    public static class ModConfig {
        // Constant Variables
        public const string ModVersion = "0.1.5.0";
        public const string PathDump = "_Dump";
        public const string PathAssets = "Assets";
        public const string PathConfigs = "Configs";
        public const string PathLocale = "Localization";
        public const bool IsExperimental = false;

        // Initial Variables
        public static bool FixCancelOverflow = true;
        public static bool FixAutomapStackUse = true;
        public static bool BetterItemUnlocks = true;
        public static bool AllAssetsLoad = true;
        public static bool AllConfigsLoad = true;
        public static bool AllLocalesLoad = true;
        public static bool DoAssetsDump = false;
        public static bool DoConfigsDump = false;
        public static bool DoLocalesDump = false;
        public static bool AllAssetsDump = false;
        public static bool AllConfigsDump = false;
        public static List<string> ToLoadConfigs = new List<string>();
        public static List<string> ToLoadLocales = new List<string>();
        public static List<string> ToDumpConfigs = new List<string>();
        public static Dictionary<string, string> ToLoadAssets = new Dictionary<string, string>();
        public static Dictionary<string, string> ToDumpAssets = new Dictionary<string, string>();

        public static void CreateModConfig(string savePath) {
            ModLog.Warning("Mod settings file doesn't exists or obsolete!");
            IniFile modConfig = new IniFile();

            // Mod settings template variables
            modConfig["InitConfig"]["ModVersion"] = ModVersion;
            modConfig["PatchConfig"]["FixCancelOverflow"] = true;
            modConfig["PatchConfig"]["FixAutomapStackUse"] = true;
            modConfig["PatchConfig"]["BetterItemUnlocks"] = true;
            modConfig["LoaderConfig"]["ToLoadAssets"] = "ALL";
            modConfig["LoaderConfig"]["ToLoadConfigs"] = "ALL";
            modConfig["LoaderConfig"]["ToLoadLocales"] = "ALL";
            modConfig["DumpConfig"]["DoAssetsDump"] = false;
            modConfig["DumpConfig"]["DoConfigsDump"] = false;
            modConfig["DumpConfig"]["DoLocalesDump"] = false;
            modConfig["DumpConfig"]["ToDumpAssets"] = "rail_rifle*rangeweapons, nanoinjection*medkits, doom_armor*armors";
            modConfig["DumpConfig"]["ToDumpConfigs"] = "config_globals, config_items, config_wounds,config_mercenaries, config_magnum";

            // Get the correct path and save mod settings
            string settingsPath = Path.Combine(savePath, "mod_settings.ini");
            ModLog.Warning($"Creating template mod settings file at {settingsPath}");
            modConfig.Save(settingsPath);
        }

        public static void LoadModConfiguration(string savePath) {
            string settingsPath = Path.Combine(savePath, "mod_settings.ini");
            ModLog.Info("Loading Settings...");
            IniFile modConfig = new IniFile();

            if (File.Exists(settingsPath)) modConfig.Load(settingsPath);

            // Check if settings file is valid and create new if not
            if (string.IsNullOrEmpty(modConfig["InitConfig"]["modVersion"].GetString()) ||
                modConfig["InitConfig"]["modVersion"].GetString() != ModVersion) {
                CreateModConfig(savePath);
                return;
            }

            // Load patch settings variables
            FixCancelOverflow = modConfig["PatchConfig"]["FixCancelOverflow"].ToBool(FixCancelOverflow);
            FixAutomapStackUse = modConfig["PatchConfig"]["FixAutomapStackUse"].ToBool(FixAutomapStackUse);
            BetterItemUnlocks = modConfig["PatchConfig"]["BetterItemUnlocks"].ToBool(BetterItemUnlocks);

            // Load loader settings variables
            AllAssetsLoad = modConfig["LoaderConfig"]["ToLoadAssets"].GetString() == "ALL";
            AllConfigsLoad = modConfig["LoaderConfig"]["ToLoadConfigs"].GetString() == "ALL";
            AllLocalesLoad = modConfig["LoaderConfig"]["ToLoadLocales"].GetString() == "ALL";

            // Load dump settings variables
            DoAssetsDump = modConfig["DumpConfig"]["DoAssetsDump"].ToBool(DoAssetsDump);
            DoConfigsDump = modConfig["DumpConfig"]["DoConfigsDump"].ToBool(DoConfigsDump);
            DoLocalesDump = modConfig["DumpConfig"]["DoLocalesDump"].ToBool(DoLocalesDump);
            AllAssetsDump = modConfig["DumpConfig"]["ToDumpAssets"].GetString() == "ALL";
            AllConfigsDump = modConfig["DumpConfig"]["ToDumpConfigs"].GetString() == "ALL";

            // Conditional settings lists parsing
            if (!AllAssetsLoad) {
                try {
                    string[] items = modConfig["LoaderConfig"]["ToLoadAssets"]
                        .GetString().Replace(" ","").Split(',');
                    foreach (string item in items) {
                        string[] pair = item.Split('*');
                        if (pair.Length == 2) ToLoadAssets.Add(pair[0], pair[1]);
                    }
                } catch (Exception ex) { ModLog.Error($"ToLoadAssets Parsing Failed: {ex}"); }
            }
            if (!AllConfigsLoad) {
                try {
                    string[] items = modConfig["LoaderConfig"]["ToLoadConfigs"]
                        .GetString().Replace(" ", "").Split(',');
                    foreach (string item in items) ToLoadConfigs.Add(item);
                } catch (Exception ex) { ModLog.Error($"ToLoadConfigs Parsing Failed: {ex}"); }
            }
            if (!AllLocalesLoad) {
                try {
                    string[] items = modConfig["LoaderConfig"]["ToLoadLocales"]
                        .GetString().Replace(" ", "").Split(',');
                    foreach (string item in items) ToLoadLocales.Add(item);
                } catch (Exception ex) { ModLog.Error($"ToLoadLocales Parsing Failed: {ex}"); }
            }
            if (DoAssetsDump && !AllAssetsDump) {
                try {
                    string[] items = modConfig["DumpConfig"]["ToDumpAssets"]
                        .GetString().Replace(" ", "").Split(',');
                    foreach (string item in items) {
                        string[] pair = item.Split('*');
                        if (pair.Length == 2) ToDumpAssets.Add(pair[0], pair[1]);
                    }
                } catch (Exception ex) { ModLog.Error($"ToDumpAssets Parsing Failed: {ex}"); }
            }
            if (DoConfigsDump && !AllConfigsDump) {
                try {
                    string[] items = modConfig["DumpConfig"]["ToDumpConfigs"]
                        .GetString().Replace(" ", "").Split(',');
                    foreach (string item in items) ToDumpConfigs.Add(item);
                } catch (Exception ex) { ModLog.Error($"ToDumpConfigs Parsing Failed: {ex}"); }
            }
        }
    }
}
