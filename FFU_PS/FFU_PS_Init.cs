using MGSC;
using System.IO;

namespace FFU_Phase_Shift {
    public static class ModMain {
        // Data Modification
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfigsLoaded(IModContext context) {
            // Starting initialization
            ModLog.Info("Initializing...");
            ModLog.Info($"Content Path: {context.ModContentPath}");

            // Dump original assets
            string dumpFolder = Path.Combine(context.ModContentPath, "_Dump");
            // ModTools.DumpConfig("config_globals", dumpFolder);
            // ModTools.DumpConfig("config_items", dumpFolder);
            // ModTools.DumpConfig("config_monsters", dumpFolder);
            // ModTools.DumpConfig("config_drops", dumpFolder);
            // ModTools.DumpConfig("config_wounds", dumpFolder);
            // ModTools.DumpConfig("config_mercenaries", dumpFolder);
            // ModTools.DumpConfig("config_spacesandbox", dumpFolder);
            // ModTools.DumpConfig("config_barter", dumpFolder);
            // ModTools.DumpConfig("config_magnum", dumpFolder);
            // ModTools.DumpDescriptors();
            // ModTools.DumpDescriptor();

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, "Assets");

            // Config files prerequisites
            string cfgFolder = Path.Combine(context.ModContentPath, "Configs");
            ModTools.Setup(cfgFolder, new ModConfigLoader());
            ModTools.Initialize();

            // Load all config files
            if (Directory.Exists(cfgFolder)) {
                string[] configFiles = Directory.GetFiles(cfgFolder, "*.csv");
                foreach (string configFile in configFiles) {
                    string configName = Path.GetFileName(configFile);
                    ModLog.Info($"Loading: {configName}");
                    ModTools.LoadConfigFile(configName);
                }
            } else ModLog.Warning($"Main: {cfgFolder} is missing. Ignoring.");

            // Finish mod initialization
            ModLog.Info("Initialized.");
        }
    }
}