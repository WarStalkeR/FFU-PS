﻿using HarmonyLib;
using MGSC;
using System;
using System.IO;

namespace FFU_Phase_Shift {
    public static class ModMain {
        // Code Patching
        [Hook(ModHookType.BeforeBootstrap)]
        public static void BeforeBootstrap(IModContext context) {
            // Patching Start
            ModLog.Info("Patching Code...");
            var Mod = new Harmony("quasimorph.ffu.phase_shift");

            try { ModLog.Info("Patching: MGSC.MagnumDevelopmentSystem.CancelProject()");
                var refCancelProject = AccessTools.Method(typeof(MagnumDevelopmentSystem), "CancelProject");
                var prefixCancelProject = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.CancelProject_ExploitFix(default, default, default, default));
                Mod.Patch(refCancelProject, new HarmonyMethod(prefixCancelProject));
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            try { ModLog.Info("Patching: MGSC.ItemInteraction.UseAutomap()");
                var refUseAutomap = AccessTools.Method(typeof(ItemInteraction), "UseAutomap");
                var prefixUseAutomap = SymbolExtensions.GetMethodInfo(() =>
                    ModPatch.UseAutomap_UsageFix(default, default, default, default));
                Mod.Patch(refUseAutomap, new HarmonyMethod(prefixUseAutomap));
            } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            // try { ModLog.Info("Patching: MGSC.InventoryScreen.RefreshItemsList()");
            //     var refRefreshItemsList = AccessTools.Method(typeof(InventoryScreen), "RefreshItemsList");
            //     var prefixRefreshItemsList = SymbolExtensions.GetMethodInfo(() =>
            //         ModPatch.RefreshItemsList_FixUI(default, default));
            //     Mod.Patch(refRefreshItemsList, new HarmonyMethod(prefixRefreshItemsList));
            // } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

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
            // ModTools.DumpDescriptors(dumpFolder);
            // ModTools.DumpDescriptor("rail_rifle", "rangeweapons", dumpFolder);

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, "Assets");

            // Config files prerequisites
            string cfgFolder = Path.Combine(context.ModContentPath, "Configs");
            ModTools.Setup(cfgFolder, new ModConfigLoader());
            ModTools.Initialize();
            // ModTools.Verbose();

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