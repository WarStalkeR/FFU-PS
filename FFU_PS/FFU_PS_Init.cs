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
            ModTools mTools = new ModTools();

            // Dump original assets
            string dumpFolder = Path.Combine(context.ModContentPath, "_Dump");
            // mTools.DumpConfig("config_globals", dumpFolder);
            // mTools.DumpConfig("config_items", dumpFolder);
            // mTools.DumpConfig("config_monsters", dumpFolder);
            // mTools.DumpConfig("config_drops", dumpFolder);
            // mTools.DumpConfig("config_wounds", dumpFolder);
            // mTools.DumpConfig("config_mercenaries", dumpFolder);
            // mTools.DumpConfig("config_spacesandbox", dumpFolder);
            // mTools.DumpConfig("config_barter", dumpFolder);
            // mTools.DumpConfig("config_magnum", dumpFolder);
            // mTools.DumpDescriptors();
            mTools.DumpDescriptor();

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, "Assets");

            // Config files prerequisites
            string cfgFolder = Path.Combine(context.ModContentPath, "Configs");
            mTools.Setup(cfgFolder, new ModConfigLoader());
            mTools.Initialize();

            // Load all config files
            if (Directory.Exists(cfgFolder)) {
                string[] configFiles = Directory.GetFiles(cfgFolder, "*.csv");
                foreach (string configFile in configFiles) {
                    string configName = Path.GetFileName(configFile);
                    ModLog.Info($"Loading: {configName}");
                    mTools.LoadConfigFile(configName);
                }
            } else ModLog.Warning($"Main: {cfgFolder} is missing. Ignoring.");

            // Finish mod initialization
            ModLog.Info("Initialized.");
        }
    }
}