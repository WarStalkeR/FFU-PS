#pragma warning disable CS0162

using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FFU_Phase_Shift {
    public static class ModMain {
        // Code Patching
        [Hook(ModHookType.BeforeBootstrap)]
        public static void BeforeBootstrap(IModContext context) {
            // Start loading mod
            ModLog.Info("Loading Mod...");

            // Load mod settings
            ModConfig.LoadModConfiguration(context.ModContentPath);

            // Initialize code patcher 
            var Mod = new Harmony("quasimorph.ffu.phase_shift");

            if (ModConfig.FixCancelOverflow)
                try { ModLog.Info("Patching: MGSC.MagnumDevelopmentSystem.CancelProject()");
                    var refMethod = AccessTools.Method(typeof(MagnumDevelopmentSystem), "CancelProject");
                    var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                        ModPatch.CancelProject_ExploitFix(default, default, default, default));
                    Mod.Patch(refMethod, prefix: new HarmonyMethod(prefixPatch));
                } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            if (ModConfig.FixAutomapStackUse)
                try { ModLog.Info("Patching: MGSC.ItemInteraction.UseAutomap()");
                    var refMethod = AccessTools.Method(typeof(ItemInteraction), "UseAutomap");
                    var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                        ModPatch.UseAutomap_UsageFix(default, default, default, default));
                    Mod.Patch(refMethod, prefix: new HarmonyMethod(prefixPatch));
                } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            if (ModConfig.BetterItemUnlocks)
                try { ModLog.Info("Patching: MGSC.ItemFactory.CreateComponent()");
                    var refMethod = AccessTools.Method(typeof(ItemFactory), "CreateComponent");
                    var postfixPatch = SymbolExtensions.GetMethodInfo(() =>
                        ModPatch.CreateComponent_BetterUnlock(default, default, default, default, default, default));
                    Mod.Patch(refMethod, postfix: new HarmonyMethod(postfixPatch));
                } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            if (ModConfig.IsExperimental)
                try { ModLog.Info("Patching: MGSC.InventoryScreen.RefreshItemsList()");
                    var refMethod = AccessTools.Method(typeof(InventoryScreen), "RefreshItemsList");
                    var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                        ModPatch.RefreshItemsList_FixMissUI(default, default));
                    Mod.Patch(refMethod, prefix: new HarmonyMethod(prefixPatch));
                } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            if (ModConfig.IsExperimental)
                try { ModLog.Info("Patching: MGSC.NoPlayerInventoryView.RefreshItemsList()");
                    var refMethod = AccessTools.Method(typeof(NoPlayerInventoryView), "RefreshItemsList");
                    var prefixPatch = SymbolExtensions.GetMethodInfo(() =>
                        ModPatch.RefreshItemsList_FixShipUI(default));
                    Mod.Patch(refMethod, prefix: new HarmonyMethod(prefixPatch));
                } catch (Exception ex) { ModLog.Error($"Patch Failed: {ex}"); }

            // Finish mod loading
            ModLog.Info("Mod Loaded.");
        }

        // Data Modification
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfigsLoaded(IModContext context) {
            // Starting initialization
            ModLog.Info("Initializing...");
            ModLog.Info($"Content Path: {context.ModContentPath}");

            // Dump original assets
            string dumpFolder = Path.Combine(context.ModContentPath, ModConfig.PathDump);
            if (ModConfig.DoAssetsDump) {

            }
            if (ModConfig.DoConfigsDump) {
                if (ModConfig.AllConfigsDump) {
                    List<TextAsset> configFiles = Resources.FindObjectsOfTypeAll<TextAsset>()
                        .Where(x => x.name.StartsWith("config_")).ToList();
                    foreach (TextAsset configFile in configFiles) 
                        ModTools.DumpText(configFile, dumpFolder);
                } else {
                    if (ModConfig.ToDumpConfigs.Count > 0)
                        foreach (string configEntry in ModConfig.ToDumpConfigs)
                            ModTools.DumpText(configEntry, dumpFolder);
                }
            }
            if (ModConfig.DoLocalesDump) {
                ModTools.DumpText("localization", dumpFolder);
            }

            // Load custom assets
            string astFolder = Path.Combine(context.ModContentPath, ModConfig.PathAssets);

            // Config files prerequisites
            string cfgFolder = Path.Combine(context.ModContentPath, ModConfig.PathConfigs);
            string locFolder = Path.Combine(context.ModContentPath, ModConfig.PathLocale);
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

            // Finish initialization
            ModLog.Info("Initialized.");
        }
    }
}