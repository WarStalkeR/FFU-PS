﻿#pragma warning disable CS0162

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

            // Config files prerequisites
            string dumpFolder = Path.Combine(context.ModContentPath, ModConfig.PathDump);
            string assetFolder = Path.Combine(context.ModContentPath, ModConfig.PathAssets);
            string configFolder = Path.Combine(context.ModContentPath, ModConfig.PathConfigs);
            string localeFolder = Path.Combine(context.ModContentPath, ModConfig.PathLocale);
            ModTools.Setup(context.ModContentPath, new ModConfigLoader());
            ModTools.Initialize();

            // Dump original asset files
            if (ModConfig.DoAssetsDump) {
                // Not Implemented Yet
            }

            // Dump original locale files
            if (ModConfig.DoLocalesDump) {
                ModTools.DumpText("localization", dumpFolder);
            }

            // Dump original config files
            if (ModConfig.DoConfigsDump) {
                if (ModConfig.AllConfigsDump) {
                    List<TextAsset> configFiles = Resources.FindObjectsOfTypeAll<TextAsset>()
                        .Where(x => x.name.StartsWith("config_")).ToList();
                    foreach (TextAsset configFile in configFiles) 
                        ModTools.DumpText(configFile, dumpFolder);
                } else if (ModConfig.ToDumpConfigs.Count > 0) {
                    foreach (string configEntry in ModConfig.ToDumpConfigs)
                        ModTools.DumpText(configEntry, dumpFolder);
                }
            }

            // Load new asset files
            if (Directory.Exists(assetFolder)) {
                if (ModConfig.AllAssetsLoad) {
                    // Not Implemented Yet
                } else if (ModConfig.ToLoadAssets.Count > 0) {
                    // Not Implemented Yet
                }
            }

            // Load new locale files
            if (Directory.Exists(localeFolder)) {
                if (ModConfig.AllLocalesLoad) {
                    string[] localeFiles = Directory.GetFiles(localeFolder, "*.json");
                    foreach (string localeFile in localeFiles) {
                        string localeName = Path.GetFileName(localeFile);
                        ModLog.Info($"Localizing: {localeName.Replace(".json", string.Empty)}");
                        ModTools.LoadLocaleFile(localeName);
                    }
                } else if (ModConfig.ToLoadLocales.Count > 0) {
                    foreach (string localeItem in ModConfig.ToLoadLocales) {
                        ModLog.Info($"Localizing: {localeItem}");
                        ModTools.LoadLocaleFile(localeItem + ".json");
                    }
                }
            } else ModLog.Warning($"Main: {localeFolder} is missing. Ignoring.");

            // Load new config files
            if (Directory.Exists(configFolder)) {
                if (ModConfig.AllConfigsLoad) {
                    string[] configFiles = Directory.GetFiles(configFolder, "*.csv");
                    foreach (string configFile in configFiles) {
                        string configName = Path.GetFileName(configFile);
                        ModLog.Info($"Loading: {configName.Replace(".csv", string.Empty)}");
                        ModTools.LoadConfigFile(configName);
                    }
                } else if (ModConfig.ToLoadConfigs.Count > 0) {
                    foreach (string configItem in ModConfig.ToLoadConfigs) {
                        ModLog.Info($"Loading: {configItem}");
                        ModTools.LoadConfigFile(configItem + ".csv");
                    }
                }
            } else ModLog.Warning($"Main: {configFolder} is missing. Ignoring.");

            // Finish initialization
            ModLog.Info("Initialized.");
        }
    }
}