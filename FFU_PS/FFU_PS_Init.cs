using HarmonyLib;
using MGSC;
using System;
using System.IO;

namespace FFU_Phase_Shift {
    public static class ModMain {
        // Initial Variables
        public const string PathDump = "_Dump";
        public const string PathAssets = "Assets";
        public const string PathConfigs = "Configs";
        public const string PathLocale = "Localization";

        // Code Patching
        [Hook(ModHookType.BeforeBootstrap)]
        public static void BeforeBootstrap(IModContext context) {
            // Patching Start
            ModLog.Info("Patching Code...");
            var Mod = new Harmony("quasimorph.ffu.phase_shift");

            try { ModLog.Info("Patching: MGSC.MagnumDevelopmentSystem.CancelProject()");
                var refMethod = AccessTools.Method(typeof(MagnumDevelopmentSystem), "CancelProject");
                var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.CancelProject_ExploitFix(default, default, default, default));
                Mod.Patch(refMethod, new HarmonyMethod(prefixPatch));
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            try { ModLog.Info("Patching: MGSC.ItemInteraction.UseAutomap()");
                var refMethod = AccessTools.Method(typeof(ItemInteraction), "UseAutomap");
                var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.UseAutomap_UsageFix(default, default, default, default));
                Mod.Patch(refMethod, new HarmonyMethod(prefixPatch));
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            try { ModLog.Info("Patching: MGSC.InventoryScreen.RefreshItemsList()");
                /*var refMethod = AccessTools.Method(typeof(InventoryScreen), "RefreshItemsList");
                var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.RefreshItemsList_FixMissUI(default, default));
                Mod.Patch(refMethod, new HarmonyMethod(prefixPatch));*/
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            try { ModLog.Info("Patching: MGSC.NoPlayerInventoryView.RefreshItemsList()");
                /*var refMethod = AccessTools.Method(typeof(NoPlayerInventoryView), "RefreshItemsList");
                var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.RefreshItemsList_FixShipUI(default));
                Mod.Patch(refMethod, new HarmonyMethod(prefixPatch));*/
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            // Patching Complete
            ModLog.Info("Code Patched.");
        }

        // Data Modification
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfigsLoaded(IModContext context) {
            // Starting initialization
            ModLog.Info("Initializing...");
            ModLog.Info($"Content Path: {context.ModContentPath}");

            // Dump original assets
            string dumpFolder = Path.Combine(context.ModContentPath, PathDump);
            // ModTools.DumpConfig("config_globals", dumpFolder);
            // ModTools.DumpConfig("config_items", dumpFolder);
            // ModTools.DumpConfig("config_monsters", dumpFolder);
            // ModTools.DumpConfig("config_drops", dumpFolder);
            // ModTools.DumpConfig("config_wounds", dumpFolder);
            // ModTools.DumpConfig("config_mercenaries", dumpFolder);
            // ModTools.DumpConfig("config_spacesandbox", dumpFolder);
            // ModTools.DumpConfig("config_barter", dumpFolder);
            // ModTools.DumpConfig("config_magnum", dumpFolder);
            // ModTools.DumpDescriptors(dumpFolder);
            // ModTools.DumpDescriptor("rail_rifle", "rangeweapons", dumpFolder);

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, PathAssets);

            // Config files prerequisites
            string cfgFolder = Path.Combine(context.ModContentPath, PathConfigs);
            string locFolder = Path.Combine(context.ModContentPath, PathLocale);
            ModTools.Setup(context.ModContentPath, new ModConfigLoader());
            ModTools.Initialize();

            // Load all localization files
            if (Directory.Exists(locFolder)) {
                string[] localeFiles = Directory.GetFiles(locFolder, "*.json");
                foreach (string localeFile in localeFiles) {
                    string localeName = Path.GetFileName(localeFile);
                    ModLog.Info($"Localizing: {localeName.Replace(".json", string.Empty)}");
                    ModTools.LoadLocaleFile(localeName);
                }
            } else ModLog.Warning($"Main: {locFolder} is missing. Ignoring.");

            // Load all config files
            if (Directory.Exists(cfgFolder)) {
                string[] configFiles = Directory.GetFiles(cfgFolder, "*.csv");
                foreach (string configFile in configFiles) {
                    string configName = Path.GetFileName(configFile);
                    ModLog.Info($"Loading: {configName.Replace(".csv", string.Empty)}");
                    ModTools.LoadConfigFile(configName);
                }
            } else ModLog.Warning($"Main: {cfgFolder} is missing. Ignoring.");

            // Finish mod initialization
            ModLog.Info("Initialized.");
        }
    }
}