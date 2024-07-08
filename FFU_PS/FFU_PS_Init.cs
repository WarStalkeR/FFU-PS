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

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, "Assets");
            foreach (var descriptor in Data.Descriptors) {
                ModLog.Info($"TEST: {descriptor.Key}");
            }

            // Config files prerequisites
            ModTools mTools = new ModTools();
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