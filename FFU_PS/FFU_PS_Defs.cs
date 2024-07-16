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

        // Initial Variables
        public static bool allAssetsLoad = true;
        public static bool allConfigsLoad = true;
        public static bool allLocalesLoad = true;
        public static bool doAssetsDump = false;
        public static bool doConfigsDump = false;
        public static bool doLocalesDump = false;
        public static bool allAssetsDump = false;
        public static bool allConfigsDump = false;
        public static List<string> toLoadConfigs = new List<string>();
        public static List<string> toLoadLocales = new List<string>();
        public static List<string> toDumpConfigs = new List<string>();
        public static List<Dictionary<string, string>> toLoadAssets = new List<Dictionary<string, string>>();
        public static List<Dictionary<string, string>> toDumpAssets = new List<Dictionary<string, string>>();

        public static void CreateModConfig(string savePath) {
            ModLog.Warning("Mod configuration file doesn't exists or obsolete!");
            IniFile modConfig = new IniFile();

            // Mod settings template variables
            modConfig["InitConfig"]["modVersion"] = ModVersion;
            modConfig["LoaderConfig"]["toLoadAssets"] = "ALL";
            modConfig["LoaderConfig"]["toLoadConfigs"] = "ALL";
            modConfig["LoaderConfig"]["toLoadLocales"] = "ALL";
            modConfig["DumpConfig"]["doAssetsDump"] = false;
            modConfig["DumpConfig"]["doConfigsDump"] = false;
            modConfig["DumpConfig"]["doLocalesDump"] = false;
            modConfig["DumpConfig"]["toDumpAssets"] = "rail_rifle*rangeweapons, nanoinjection*medkits, doom_armor*armors";
            modConfig["DumpConfig"]["toDumpConfigs"] = "config_globals, config_items, config_wounds, config_mercenaries, config_magnum";

            // Get the correct path and save mod settings
            string settingsPath = Path.Combine(savePath, "mod_settings.ini");
            ModLog.Warning($"Creating template mod configuration file at {settingsPath}");
            modConfig.Save(settingsPath);
        }

        public static void LoadModConfiguration(string savePath) {
            string settingsPath = Path.Combine(savePath, "mod_settings.ini");
            ModLog.Info("Loading Mod Configuration...");
            IniFile modConfig = new IniFile();
            modConfig.Load(settingsPath);

            // Check if settings file is valid and create new if not
            if (string.IsNullOrEmpty(modConfig["InitConfig"]["modVersion"].Value) ||
                modConfig["InitConfig"]["modVersion"].ToString() != ModVersion) {
                CreateModConfig(savePath);
                return;
            }

            // Load mod settings variables from settings file
            allAssetsLoad = modConfig["LoaderConfig"]["toLoadAssets"].GetString() == "ALL";
            allConfigsLoad = modConfig["LoaderConfig"]["toLoadConfigs"].GetString() == "ALL";
            allLocalesLoad = modConfig["LoaderConfig"]["toLoadLocales"].GetString() == "ALL";
            doAssetsDump = modConfig["DumpConfig"]["doAssetsDump"].ToBool(doAssetsDump);
            doConfigsDump = modConfig["DumpConfig"]["doConfigsDump"].ToBool(doConfigsDump);
            doLocalesDump = modConfig["DumpConfig"]["doLocalesDump"].ToBool(doLocalesDump);
            allAssetsDump = modConfig["DumpConfig"]["toDumpAssets"].GetString() == "ALL";
            allConfigsDump = modConfig["DumpConfig"]["toDumpConfigs"].GetString() == "ALL";
        }
    }
}
