using MGSC;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FFU_Phase_Shift {
    public static class ModTools {
        private static bool _verbose = false;
        private static string _cntPath = null;
        private static ModConfigLoader _cfgLoader = null;
        private static Localization _locInstance = null;

        public static void Setup(string contentPath = null, ModConfigLoader configLoader = null) {
            // Loading passed variables
            if (contentPath != null) _cntPath = contentPath;
            if (configLoader != null) _cfgLoader = configLoader;

            // Loading instance variables
            _locInstance = Singleton<Localization>.Instance;
        }

        public static void Verbose() {
            _verbose = !_verbose;
        }

        public static void Initialize() {
            if (_cfgLoader == null) { ModLog.Error($"Initialize: config loader is not referenced!"); return; }
            _cfgLoader.OnDescriptorsLoaded += delegate (string header, DescriptorsCollection descriptors) {
                if (!Data.Descriptors.ContainsKey(header)) {
                    Data.Descriptors.Add(header, descriptors);
                }
            };
            _cfgLoader.AddParser(new VertTableParser<GlobalSettings>(Data.Global, "global"));
            _cfgLoader.AddParser(new TableParser<CreatureRecord>("monsters", delegate (CreatureRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, monsters: {r.Id}");
                if (Data.Creatures.GetRecord(r.Id) != null) {
                    Data.Creatures._records[r.Id] = r;
                } else Data.Creatures.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new MultiColVertTableParser<DifficultyPreset>(Data.DifficultyPresets, "difficulty_presets"));
            _cfgLoader.AddParser(new TableParser<DifficultySettingsRecord>("difficulty_settings", delegate (DifficultySettingsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, difficulty_settings: {r.Id}");
                if (Data.DifficultySettings.GetRecord(r.Id) != null) {
                    Data.DifficultySettings._records[r.Id] = r;
                } else Data.DifficultySettings.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProgressionDifficultyRecord>("progression_difficulty", delegate (ProgressionDifficultyRecord r, string header, DescriptorsCollection descs){
                int index = Data.ProgressionDifficulty.FindIndex(x => x.CompletedMissionNumber == r.CompletedMissionNumber);
                if (index != -1) {
                    Data.ProgressionDifficulty[index] = r;
                } else Data.ProgressionDifficulty.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<MissionDifficultyRecord>("mission_difficulty", delegate (MissionDifficultyRecord r, string header, DescriptorsCollection descs) {
                int index = Data.MissionDifficulty.FindIndex(x => x.DifficultyRating == r.DifficultyRating);
                if (index != -1) {
                    Data.MissionDifficulty[index] = r;
                } else Data.MissionDifficulty.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionRecord>("procmissions", delegate (ProcMissionRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissions.FindIndex(x => x.ProcMissionType == r.ProcMissionType);
                if (index != -1) {
                    Data.ProcMissions[index] = r;
                } else Data.ProcMissions.Add(r);
                r.ContentDescriptor = descs.GetDescriptor(r.ProcMissionType.ToString()) as ProcMissionDescriptor;
            }));
            _cfgLoader.AddParser(new TableParser<PrizeByRatingRecord>("prizes_by_rating", delegate (PrizeByRatingRecord r, string header, DescriptorsCollection descs) {
                int index = Data.PrizesByRatings.FindIndex(x => x.ProcMissionType == r.ProcMissionType && x.DifficultyRating == r.DifficultyRating);
                if (index != -1) {
                    Data.PrizesByRatings[index] = r;
                } else Data.PrizesByRatings.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<StartingItemsRecord>("starting_items", delegate (StartingItemsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, starting_items: {r.Id}");
                int index = Data.StartingItems.FindIndex(x => x.Id == r.Id);
                if (index != -1) {
                    Data.StartingItems[index] = r;
                } else Data.StartingItems.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<DamageTypeRecord>("damagetypes", delegate (DamageTypeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, damagetypes: {r.Id}");
                if (Data.DamageTypes.GetRecord(r.Id) != null) {
                    Data.DamageTypes._records[r.Id] = r;
                } else Data.DamageTypes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<EquipmentVariantRecord>("monster_equipment", delegate (EquipmentVariantRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, monster_equipment: {r.Id}");
                if (Data.MonsterEquipments.GetRecord(r.Id) != null) {
                    Data.MonsterEquipments._records[r.Id] = r;
                } else Data.MonsterEquipments.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<AiPresetRecord>("ai-presets", delegate (AiPresetRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, ai-presets: {r.Id}");
                if (Data.AiPresets.GetRecord(r.Id) != null) {
                    Data.AiPresets._records[r.Id] = r;
                } else Data.AiPresets.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StatusEffectsRecord>("statuseffects", delegate (StatusEffectsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, statuseffects: {r.Id}");
                if (Data.StatusEffects.GetRecord(r.Id) != null) {
                    Data.StatusEffects._records[r.Id] = r;
                } else Data.StatusEffects.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FactionRecord>("factions", delegate (FactionRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, factions: {r.Id}");
                if (Data.Factions.GetRecord(r.Id) != null) {
                    Data.Factions._records[r.Id] = r;
                } else Data.Factions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryClassRecord>("mercenary_classes", delegate (MercenaryClassRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mercenary_classes: {r.Id}");
                if (Data.MercenaryClasses.GetRecord(r.Id) != null) {
                    Data.MercenaryClasses._records[r.Id] = r;
                } else Data.MercenaryClasses.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryProfileRecord>("mercenary_profiles", delegate (MercenaryProfileRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mercenary_profiles: {r.Id}");
                if (Data.MercenaryProfiles.GetRecord(r.Id) != null) {
                    Data.MercenaryProfiles._records[r.Id] = r;
                } else Data.MercenaryProfiles.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<PerkRecord>("perks", delegate (PerkRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, perks: {r.Id}");
                if (Data.Perks.GetRecord(r.Id) != null) {
                    Data.Perks._records[r.Id] = r;
                } else Data.Perks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AllianceRecord>("alliances", delegate (AllianceRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, alliances: {r.Id}");
                if (Data.Alliances.GetRecord(r.Id) != null) {
                    Data.Alliances._records[r.Id] = r;
                } else Data.Alliances.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionTemplate>("procmissiontemplates", delegate (ProcMissionTemplate r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, procmissiontemplates: {r.Id}");
                if (Data.ProcMissionTemplates.GetRecord(r.Id) != null) {
                    Data.ProcMissionTemplates._records[r.Id] = r;
                } else Data.ProcMissionTemplates.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationRecord>("stations", delegate (StationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, stations: {r.Id}");
                if (Data.Stations.GetRecord(r.Id) != null) {
                    Data.Stations._records[r.Id] = r;
                } else Data.Stations.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<StoryMissionRecord>("storymissions", delegate (StoryMissionRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, storymissions: {r.Id}");
                if (Data.StoryMissions.GetRecord(r.Id) != null) {
                    Data.StoryMissions._records[r.Id] = r;
                } else Data.StoryMissions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SpaceObjectRecord>("spaceobjects", delegate (SpaceObjectRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, spaceobjects: {r.Id}");
                if (Data.SpaceObjects.GetRecord(r.Id) != null) {
                    Data.SpaceObjects._records[r.Id] = r;
                } else Data.SpaceObjects.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TileTransformationRecord>("tilesettransformation", delegate (TileTransformationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, tilesettransformation: {r.Id}");
                if (Data.TileTransformation.GetRecord(r.Id) != null) {
                    Data.TileTransformation._records[r.Id] = r;
                } else Data.TileTransformation.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<FireModeRecord>("firemodes", delegate (FireModeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, firemodes: {r.Id}");
                if (Data.Firemodes.GetRecord(r.Id) != null) {
                    Data.Firemodes._records[r.Id] = r;
                } else Data.Firemodes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ItemExpireRecord>("itemexpire", delegate (ItemExpireRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, itemexpire: {r.Id}");
                if (r.Id.StartsWith("^")) {
                    CleanRecordIdentifier(r);
                    if (Data.ItemExpire.GetRecord(r.Id) != null) 
                        Data.ItemExpire.RemoveRecord(r.Id);
                } else {
                    if (Data.ItemExpire.GetRecord(r.Id) != null) {
                        Data.ItemExpire._records[r.Id] = r;
                    } else Data.ItemExpire.AddRecord(r.Id, r);
                }
            }));
            _cfgLoader.AddParser(new TableParser<WoundSlotRecord>("woundslots", delegate (WoundSlotRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, woundslots: {r.Id}");
                if (Data.WoundSlots.GetRecord(r.Id) != null) {
                    Data.WoundSlots._records[r.Id] = r;
                } else Data.WoundSlots.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WoundRecord>("woundtypes", delegate (WoundRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, woundtypes: {r.Id}");
                if (Data.Wounds.GetRecord(r.Id) != null) {
                    Data.Wounds._records[r.Id] = r;
                } else Data.Wounds.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ItemTransformationRecord>("itemtransformation", delegate (ItemTransformationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, itemtransformation: {r.Id}");
                if (Data.ItemTransformation.GetRecord(r.Id) != null) {
                    Data.ItemTransformation._records[r.Id] = r;
                } else Data.ItemTransformation.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<WorkbenchReceiptRecord>("workbenchreceipts", delegate (WorkbenchReceiptRecord r, string header, DescriptorsCollection descs) {
                int index = Data.WorkbenchReceipts.FindIndex(x => x.OutputItem == r.OutputItem);
                if (index != -1) {
                    Data.WorkbenchReceipts[index] = r;
                } else Data.WorkbenchReceipts.Add(r);
                r.GenerateId();
            }));
            _cfgLoader.AddParser(new TableParser<ItemProduceReceipt>("itemreceipts", delegate (ItemProduceReceipt r, string header, DescriptorsCollection descs) {
                int index = Data.ProduceReceipts.FindIndex(x => x.OutputItem == r.OutputItem);
                if (index != -1) {
                    Data.ProduceReceipts[index] = r;
                } else Data.ProduceReceipts.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<BarterReceipt>("barter_receipts", delegate (BarterReceipt r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, barter_receipts: {r.Id}");
                if (Data.BarterReceipts.GetRecord(r.Id) != null) {
                    Data.BarterReceipts._records[r.Id] = r;
                } else Data.BarterReceipts.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationBarterRecord>("station_barter", delegate (StationBarterRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, station_barter: {r.Id}");
                if (Data.StationBarter.GetRecord(r.Id) != null) {
                    Data.StationBarter._records[r.Id] = r;
                } else Data.StationBarter.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<QmorphosRecord>("quazimorphosis", delegate (QmorphosRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, quazimorphosis: {r.Id}");
                if (Data.Qmorphos.GetRecord(r.Id) != null) {
                    Data.Qmorphos._records[r.Id] = r;
                } else Data.Qmorphos.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionObjectiveRecord>("missionobjectives", delegate (ProcMissionObjectiveRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissionObjectives.FindIndex(x => x.BeneficiaryFactionType == r.BeneficiaryFactionType && x.VictimFactionType == r.VictimFactionType);
                if (index != -1) {
                    Data.ProcMissionObjectives[index] = r;
                } else Data.ProcMissionObjectives.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<PopulationReceipt>("population_receipts", delegate (PopulationReceipt r, string header, DescriptorsCollection descs) {
                int index = Data.PopulationReceipts.FindIndex(x => x.BartherReceiptId == r.BartherReceiptId);
                if (index != -1) {
                    Data.PopulationReceipts[index] = r;
                } else Data.PopulationReceipts.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<AmmoRecord>("ammo", delegate (AmmoRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, ammo: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<AmmoRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is AmmoRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("meleeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, meleeweapons: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<WeaponRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is WeaponRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.IsMelee = true;
                r.CanThrow = r.ThrowRange != 0;
                r.Range = 1;
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("rangeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, rangeweapons: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<WeaponRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is WeaponRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MedkitRecord>("medkits", delegate (MedkitRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, medkits: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<MedkitRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is MedkitRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FoodRecord>("food", delegate (FoodRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, food: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<FoodRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is FoodRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BackpackRecord>("backpacks", delegate (BackpackRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, backpacks: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<BackpackRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is BackpackRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<VestRecord>("vests", delegate (VestRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, vests: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<VestRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is VestRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<HelmetRecord>("helmets", delegate (HelmetRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, helmets: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<HelmetRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is HelmetRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ArmorRecord>("armors", delegate (ArmorRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, armors: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<ArmorRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is ArmorRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<LeggingsRecord>("leggings", delegate (LeggingsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, leggings: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<LeggingsRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is LeggingsRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BootsRecord>("boots", delegate (BootsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, boots: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<BootsRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is BootsRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<RepairRecord>("repairs", delegate (RepairRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, repairs: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<RepairRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is RepairRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SkullRecord>("skulls", delegate (SkullRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, skulls: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<SkullRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is SkullRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<QuasiArtifactRecord>("quasiartifacts", delegate (QuasiArtifactRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, quasiartifacts: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<QuasiArtifactRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is QuasiArtifactRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<GrenadeRecord>("grenades", delegate (GrenadeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, grenades: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<GrenadeRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is GrenadeRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MineRecord>("mines", delegate (MineRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mines: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<MineRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is MineRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AutomapRecord>("automaps", delegate (AutomapRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, automaps: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<AutomapRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is AutomapRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ExpGainerRecord>("expgainers", delegate (ExpGainerRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, expgainers: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<ExpGainerRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is ExpGainerRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ResurrectKitRecord>("resurrectkits", delegate (ResurrectKitRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, resurrectkits: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<ResurrectKitRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is ResurrectKitRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TrashRecord>("trash", delegate (TrashRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, trash: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<TrashRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is TrashRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                if (descs.GetDescriptor(r.Id) != null) {
                    r.ContentDescriptor = descs.GetDescriptor(r.Id);
                } else {
                    if (Data.Items.GetRecord(r.Id) != null) {
                        var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                        if (multiRecord.GetRecord<AutomapRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<AutomapRecord>().ContentDescriptor;
                        else if (multiRecord.GetRecord<SkullRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<SkullRecord>().ContentDescriptor;
                        else if (multiRecord.GetRecord<QuasiArtifactRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<QuasiArtifactRecord>().ContentDescriptor;
                        else if (multiRecord.GetRecord<TurretRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<TurretRecord>().ContentDescriptor;
                        else if (multiRecord.GetRecord<DatadiskRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<DatadiskRecord>().ContentDescriptor;
                        else if (multiRecord.GetRecord<ResurrectKitRecord>() != null)
                            r.ContentDescriptor = multiRecord.GetRecord<ResurrectKitRecord>().ContentDescriptor;
                        else ModLog.Warning($"WARNING! Item '{r.Id}' has no fallback descriptor! Expect errors.");
                    }
                }
            }));
            _cfgLoader.AddParser(new TableParser<DatadiskRecord>("datadisks", delegate (DatadiskRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, datadisks: {r.Id}");
                bool isToDel = r.Id.StartsWith("^");
                bool isToAdd = r.Id.StartsWith("+");
                bool isToSub = r.Id.StartsWith("-");
                bool isOnTop = r.Id.StartsWith("*");
                bool isOperation = isToDel || isToAdd || isToSub;
                CleanRecordIdentifier(r);
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<DatadiskRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is DatadiskRecord);
                            if (index != -1) {
                                var refRecord = multiRecord.Records[index] as DatadiskRecord;
                                if (!isOperation) {
                                    multiRecord.Records[index] = r;
                                } else if (isToDel) {
                                    // NOT IMPLEMENTED YET //
                                } else if (isToAdd) {
                                    foreach (var unlockId in r.UnlockIds)
                                    if (!refRecord.UnlockIds.Contains(unlockId))
                                    refRecord.UnlockIds.Add(unlockId);
                                } else if (isToSub) {
                                    foreach (var unlockId in r.UnlockIds)
                                    if (refRecord.UnlockIds.Contains(unlockId))
                                    refRecord.UnlockIds.Remove(unlockId);
                                }
                            }
                        } else if (!isOperation) {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else if (!isOperation) Data.Items._records[r.Id] = r;
                } else if (!isOperation) Data.Items.AddRecord(r.Id, r);
                if (!isOperation) r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TurretRecord>("turrets", delegate (TurretRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, turrets: {r.Id}");
                bool isOnTop = r.Id.StartsWith("*");
                if (isOnTop) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    var multiRecord = Data.Items._records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<TurretRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is TurretRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isOnTop) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else Data.Items._records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("itemdrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                bool removeRecord = r.ContentIds[0].StartsWith("^");
                if (removeRecord) r.ContentIds[0] = CleanRecord(r.ContentIds[0]);
                string key = header.Replace(Data.LocationItemDrop._headerPrefix, string.Empty);
                if (Data.LocationItemDrop._recordsByLocations.ContainsKey(key)) {
                    int idx = Data.LocationItemDrop._recordsByLocations[key].FindIndex(
                        x => x.ContentIds.SequenceEqual(r.ContentIds));
                    if (idx >= 0) {
                        if (removeRecord) Data.LocationItemDrop._recordsByLocations[key].RemoveAt(idx);
                        else Data.LocationItemDrop._recordsByLocations[key][idx] = r;
                    } else Data.LocationItemDrop._recordsByLocations[key].Add(r);
                } else Data.LocationItemDrop._recordsByLocations[key] = new List<ContentDropRecord> { r };
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("monsterdrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                bool removeRecord = r.ContentIds[0].StartsWith("^");
                if (removeRecord) r.ContentIds[0] = CleanRecord(r.ContentIds[0]);
                string key = header.Replace(Data.LocationMonsterDrop._headerPrefix, string.Empty);
                if (Data.LocationMonsterDrop._recordsByLocations.ContainsKey(key)) {
                    int idx = Data.LocationMonsterDrop._recordsByLocations[key].FindIndex(
                        x => x.ContentIds.SequenceEqual(r.ContentIds));
                    if (idx >= 0) {
                        if (removeRecord) Data.LocationMonsterDrop._recordsByLocations[key].RemoveAt(idx);
                        else Data.LocationMonsterDrop._recordsByLocations[key][idx] = r;
                    } else Data.LocationMonsterDrop._recordsByLocations[key].Add(r);
                } else Data.LocationMonsterDrop._recordsByLocations[key] = new List<ContentDropRecord> { r };
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("factiondrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                try { // Overwrite Not Available
                    Data.FactionDrop.AddRecord(header, r);
                } catch { ModLog.Warning("Failed processing FactionDrop record!"); }
            }));
            _cfgLoader.AddParser(new TableParser<MagnumPerkRecord>("magnum_perks", TableKeyComparisonMode.Equals, delegate (MagnumPerkRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, magnum_perks: {r.Id}");
                if (Data.MagnumPerks.GetRecord(r.Id) != null) {
                    Data.MagnumPerks._records[r.Id] = r;
                } else Data.MagnumPerks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumProjectParameter>("magnum_projects_params", delegate (MagnumProjectParameter r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, magnum_projects_params: {r.Id}");
                if (Data.MagnumProjectParameters.GetRecord(r.Id) != null) {
                    Data.MagnumProjectParameters._records[r.Id] = r;
                } else Data.MagnumProjectParameters.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumProjectPrice>("magnum_projects_prices", delegate (MagnumProjectPrice r, string header, DescriptorsCollection descs) {
                int index = Data.MagnumProjectPrices.FindIndex(x => x.ProjectType == r.ProjectType);
                if (index != -1) {
                    Data.MagnumProjectPrices[index] = r;
                } else Data.MagnumProjectPrices.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumDefaultParameterRecord>("magnum_default_params", delegate (MagnumDefaultParameterRecord r, string header, DescriptorsCollection descs) {
                if (Data.MagnumDefaultValues.ContainsKey(r.Parameter))
                    Data.MagnumDefaultValues[r.Parameter] = r.Value;
                else Data.MagnumDefaultValues.Add(r.Parameter, r.Value);
            }));
            _cfgLoader.AddParser(new TableParser<TechLevelRecord>("techlevels", delegate (TechLevelRecord r, string s, DescriptorsCollection descs) {
                if (Data.TechLevels.ContainsKey(r.TechLevel))
                    Data.TechLevels[r.TechLevel] = r;
                else Data.TechLevels.Add(r.TechLevel, r);
            }));
        }

        public static int GetItemOrder(string itemId) {
            CompositeItemRecord refItem = Data.Items.GetRecord(itemId) as CompositeItemRecord;
            if (refItem != null) {
                if (refItem.GetRecord<WeaponRecord>() != null && !refItem.GetRecord<WeaponRecord>().IsMelee)
                    return 100 + (int)refItem.GetRecord<WeaponRecord>().WeaponClass;
                else if (refItem.GetRecord<WeaponRecord>() != null && refItem.GetRecord<WeaponRecord>().IsMelee)
                    return 200 + (int)refItem.GetRecord<WeaponRecord>().WeaponClass;
                else if (refItem.GetRecord<AmmoRecord>() != null)
                    return 300 + (int)refItem.GetRecord<AmmoRecord>().BallisticType;
                else if (refItem.GetRecord<GrenadeRecord>() != null)
                    return 400;
                else if (refItem.GetRecord<MineRecord>() != null)
                    return 500;
                else if (refItem.GetRecord<TurretRecord>() != null)
                    return 600;
                else if (refItem.GetRecord<MedkitRecord>() != null)
                    return 700;
                else if (refItem.GetRecord<FoodRecord>() != null)
                    return 800;
                else if (refItem.GetRecord<BackpackRecord>() != null)
                    return 900;
                else if (refItem.GetRecord<VestRecord>() != null)
                    return 1000;
                else if (refItem.GetRecord<HelmetRecord>() != null)
                    return 1100;
                else if (refItem.GetRecord<ArmorRecord>() != null)
                    return 1200;
                else if (refItem.GetRecord<LeggingsRecord>() != null)
                    return 1300;
                else if (refItem.GetRecord<BootsRecord>() != null)
                    return 1400;
                else if (refItem.GetRecord<RepairRecord>() != null && refItem.GetRecord<TrashRecord>() == null)
                    return 2000;
                else if (refItem.GetRecord<AutomapRecord>() != null)
                    return 2100;
                else if (refItem.GetRecord<ResurrectKitRecord>() != null)
                    return 2200;
                else if (refItem.GetRecord<ExpGainerRecord>() != null)
                    return 2300;
                else if (refItem.GetRecord<DatadiskRecord>() != null)
                    return 3000;
                else if (refItem.GetRecord<SkullRecord>() != null)
                    return 3100;
                else if (refItem.GetRecord<QuasiArtifactRecord>() != null)
                    return 3200;
                else if (refItem.GetRecord<TrashRecord>() != null)
                    return 10000;
                else return 100000;
            } else return 1000000;
        }

        public static void SafeWrite(string rawContent, string savePath, string fileName, bool logAction = false) {
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            File.WriteAllText(Path.Combine(savePath, fileName), rawContent);
            if (logAction) ModLog.Info($"Writing: saved raw data to file {fileName}");
        }

        public static void LoadConfigFile(string configName) {
            // Ensure that everything is in place
            if (_cntPath == null) { ModLog.Warning($"LoadConfigFile: content path is undefined!"); return; }
            if (_cfgLoader == null) { ModLog.Warning($"LoadConfigFile: config loader is not referenced!"); return; }
            if (configName == null) { ModLog.Warning($"LoadConfigFile: config name is undefined!"); return; }
            string cfgPath = Path.Combine(_cntPath, ModConfig.PathConfigs, configName);
            if (!File.Exists(cfgPath)) { ModLog.Warning($"LoadConfigFile: config file doesn't exist!"); return; }

            // Read and parse config file into data
            _cfgLoader.ProcessConfigFile(cfgPath, _verbose);
        }

        /// <remarks>
        /// Loads custom localization files. Allows precise overwrite of existing localization entries.
        /// 
        /// <br/><br/><u>Localization File JSON Writing Pattern</u>
        /// <code>{
        ///   "translation.id.1": {
        ///     "language.id.a": "localized_string_1",
        ///     "language.id.b": "localized_string_2"
        ///   },
        ///   "translation.id.2": {
        ///     "language.id.a": "localized_string_3",
        ///     "language.id.b": "localized_string_4"
        ///   }
        /// }</code>
        /// 
        /// <br/><br/><u>Localization Settings Information</u>
        /// <br/><c>translation.id</c> - corresponds to the identifier of localized value in-game.
        /// <br/><c>language.id</c> - the language of localized value. Refer to the language list below.
        /// <br/><c>localized_string</c> - localized string itself that will be rendered in-game.
        /// 
        /// <br/><br/><u>Allowed Localization Languages</u>
        /// <br/><c>english</c>, <c>russian</c>, <c>portuguese</c>, <c>german</c>, <c>french</c>,
        /// <c>spanish</c>, <br/><c>polish</c>, <c>turkish</c>, <c>korean</c>, <c>japanese</c>, 
        /// <c>chinese_s</c>.
        /// 
        /// <br/><br/>If <b>english</b> is set, it will be used as default for other undefined localization entries.
        /// 
        /// <br/><br/>For in-depth analysis of custom localization algorithm please inspect code itself.
        /// </remarks>
        public static void LoadLocaleFile(string localeName) {
            // Ensure that everything is in place
            if (_cntPath == null) { ModLog.Warning($"LoadLocaleFile: content path is undefined!"); return; }
            if (_locInstance == null) { ModLog.Warning($"LoadLocaleFile: locale instance is not referenced!"); return; }
            if (localeName == null) { ModLog.Warning($"LoadLocaleFile: locale name is undefined!"); return; }
            string localePath = Path.Combine(_cntPath, ModConfig.PathLocale, localeName);
            if (!File.Exists(localePath)) { ModLog.Warning($"LoadLocaleFile: locale file doesn't exist!"); return; }

            JSONNode localeFile = JSON.Parse(File.ReadAllText(localePath));
            foreach (var locNode in localeFile.AsObject) {
                // Parse default language
                if (locNode.Value["english"] != null)
                    AddLocaleToDB(Localization.Lang.EnglishUS, locNode.Key, locNode.Value["english"].Value, true);
                bool hasDefault = locNode.Value["english"] != null;
                string localeDefault = locNode.Value["english"]?.Value;

                // Parse other languages
                if (locNode.Value["russian"] != null) 
                    AddLocaleToDB(Localization.Lang.Russian, locNode.Key, locNode.Value["russian"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Russian, locNode.Key, localeDefault);
                if (locNode.Value["portuguese"] != null) 
                    AddLocaleToDB(Localization.Lang.BrazilianPortugal, locNode.Key, locNode.Value["portuguese"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.BrazilianPortugal, locNode.Key, localeDefault);
                if (locNode.Value["german"] != null) 
                    AddLocaleToDB(Localization.Lang.German, locNode.Key, locNode.Value["german"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.German, locNode.Key, localeDefault);
                if (locNode.Value["french"] != null) 
                    AddLocaleToDB(Localization.Lang.French, locNode.Key, locNode.Value["french"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.French, locNode.Key, localeDefault);
                if (locNode.Value["spanish"] != null) 
                    AddLocaleToDB(Localization.Lang.Spanish, locNode.Key, locNode.Value["spanish"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Spanish, locNode.Key, localeDefault);
                if (locNode.Value["polish"] != null) 
                    AddLocaleToDB(Localization.Lang.Polish, locNode.Key, locNode.Value["polish"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Polish, locNode.Key, localeDefault);
                if (locNode.Value["turkish"] != null) 
                    AddLocaleToDB(Localization.Lang.Turkish, locNode.Key, locNode.Value["turkish"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Turkish, locNode.Key, localeDefault);
                if (locNode.Value["korean"] != null) 
                    AddLocaleToDB(Localization.Lang.Korean, locNode.Key, locNode.Value["korean"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Korean, locNode.Key, localeDefault);
                if (locNode.Value["japanese"] != null) 
                    AddLocaleToDB(Localization.Lang.Japanese, locNode.Key, locNode.Value["japanese"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.Japanese, locNode.Key, localeDefault);
                if (locNode.Value["chinese_s"] != null) 
                    AddLocaleToDB(Localization.Lang.ChineseSimp, locNode.Key, locNode.Value["chinese_s"].Value, true);
                else if (hasDefault) AddLocaleToDB(Localization.Lang.ChineseSimp, locNode.Key, localeDefault);
            }
        }

        public static void AddLocaleToDB(Localization.Lang locLang, string locId, string locText, bool overwrite = false) {
            if (_locInstance == null) { ModLog.Warning($"AddLocaleToDB: locale instance is not referenced!"); return; }
            bool hasLocale = _locInstance.db[locLang].ContainsKey(locId);
            if (hasLocale && overwrite) _locInstance.db[locLang][locId] = locText;
            else if (!hasLocale) _locInstance.db[locLang].Add(locId, locText);
        }

        public static void DumpText(string assetName, string savePath, string assetExt = "csv") {
            TextAsset refAsset = Resources.Load(assetName) as TextAsset;
            if (refAsset == null) { ModLog.Warning($"DumpText: '{assetName}' text asset doesn't exist!"); return; };
            DumpText(refAsset, savePath, assetExt);
        }

        public static void DumpText(TextAsset refAsset, string savePath, string assetExt = "csv") {
            if (refAsset == null) { ModLog.Warning($"DumpText: text asset doesn't exist!"); return; };
            SafeWrite(refAsset.text, savePath, $"{refAsset.name}.{assetExt}");
            ModLog.Info($"Dumping: TextAsset '{refAsset.name}' saved as {refAsset.name}.{assetExt}");
        }

        public static void DumpDescriptors(string savePath) {
            // if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            foreach (KeyValuePair<string, DescriptorsCollection> collection in Data.Descriptors) {
                ModLog.Info($">>>>>>>>>>>>>>>>>>>> {collection.Key} <<<<<<<<<<<<<<<<<<<<");
                DumpDescriptors(collection, savePath);
            }
        }

        public static void DumpDescriptors(KeyValuePair<string, DescriptorsCollection> colRef, string savePath) {
            string colPath = Path.Combine(savePath, colRef.Key);
            switch (colRef.Key) {
                case "meleeweapons":
                case "rangeweapons": {
                    // if (!Directory.Exists(colPath)) Directory.CreateDirectory(colPath);
                    foreach (WeaponDescriptor descriptor in colRef.Value._descriptors) {
                        int idx = Array.IndexOf(colRef.Value._descriptors, descriptor);
                        DumpDescriptor(colRef.Value._ids[idx], descriptor, colPath);
                    }
                    break;
                }
                case "firemodes": { break; }
                case "ammo": { break; }
                case "medkits": { break; }
                case "food": { break; }
                case "backpacks": { break; }
                case "vests": { break; }
                case "armors": { break; }
                case "helmets": { break; }
                case "leggings": { break; }
                case "boots": { break; }
                case "repairs": { break; }
                case "automaps": { break; }
                case "skulls": { break; }
                case "quasiartifacts": { break; }
                case "grenades": { break; }
                case "mines": { break; }
                case "turrets": { break; }
                case "trash": { break; }
                case "datadisks": { break; }
                case "resurrectkits": { break; }
                case "monsters": { break; }
                case "statuseffects": { break; }
                case "damagetypes": { break; }
                case "woundtypes": { break; }
                case "woundslots": { break; }
                case "mercenary_profiles": { break; }
                case "mercenary_classes": { break; }
                case "perks": { break; }
                case "factions": { break; }
                case "stations": { break; }
                case "storymissions": { break; }
                case "magnum_perks": { break; }
            }
        }

        public static void DumpDescriptor(string itemId, string collectionId, string savePath) {
            string colPath = Path.Combine(savePath, collectionId);
            switch (collectionId) {
                case "meleeweapons": 
                case "rangeweapons": {
                    var refDesc = Data.Descriptors[collectionId]?.GetDescriptor(itemId);
                    DumpDescriptor(itemId, refDesc as WeaponDescriptor, colPath);
                    break;
                }
                case "firemodes": { break; }
                case "ammo": { break; }
                case "medkits": { break; }
                case "food": { break; }
                case "backpacks": { break; }
                case "vests": { break; }
                case "armors": { break; }
                case "helmets": { break; }
                case "leggings": { break; }
                case "boots": { break; }
                case "repairs": { break; }
                case "automaps": { break; }
                case "skulls": { break; }
                case "quasiartifacts": { break; }
                case "grenades": { break; }
                case "mines": { break; }
                case "turrets": { break; }
                case "trash": { break; }
                case "datadisks": { break; }
                case "resurrectkits": { break; }
                case "monsters": { break; }
                case "statuseffects": { break; }
                case "damagetypes": { break; }
                case "woundtypes": { break; }
                case "woundslots": { break; }
                case "mercenary_profiles": { break; }
                case "mercenary_classes": { break; }
                case "perks": { break; }
                case "factions": { break; }
                case "stations": { break; }
                case "storymissions": { break; }
                case "magnum_perks": { break; }
            }
        }

        public static void DumpDescriptor(string descId, WeaponDescriptor descRef, string savePath) {
            string descPath = Path.Combine(savePath, descId);
            string descData = $"Descriptor: {descId}";
            try {
                if (descRef == null) { ModLog.Warning($"DumpDescriptor: descriptor for '{descId}' is not referenced!"); return; }
                descData += $"\n  _overridenRenderId: {descRef._overridenRenderId}";
                descData += $"\n  _icon: {descRef._icon.name}";
                descData += $"\n  _smallIcon: {descRef._smallIcon.name}";
                descData += $"\n  _shadow: {descRef._shadow.name}";
                descData += $"\n  _attackSoundBanks:";
                if (descRef._attackSoundBanks.Length > 0)
                foreach (var soundBank in descRef._attackSoundBanks) {
                    int idx = Array.IndexOf(descRef._attackSoundBanks, soundBank);
                    descData += $"\n    sound_bank_{idx}: {soundBank?.name}";
                    DumpDescriptorData("\n      ", ref descData, soundBank);
                }
                descData += $"\n  _reloadSoundBanks:";
                if (descRef._reloadSoundBanks.Length > 0)
                foreach (var soundBank in descRef._reloadSoundBanks) {
                    int idx = Array.IndexOf(descRef._reloadSoundBanks, soundBank);
                    descData += $"\n    sound_bank_{idx}: {soundBank?.name}";
                    DumpDescriptorData("\n      ", ref descData, soundBank);
                }
                descData += $"\n  _dryShotSoundBanks:";
                if (descRef._dryShotSoundBanks.Length > 0)
                foreach (var soundBank in descRef._dryShotSoundBanks) {
                    int idx = Array.IndexOf(descRef._dryShotSoundBanks, soundBank);
                    descData += $"\n    sound_bank_{idx}: {soundBank?.name}";
                    DumpDescriptorData("\n      ", ref descData, soundBank);
                }
                descData += $"\n  _failedAttackSoundBanks:";
                if (descRef._failedAttackSoundBanks.Length > 0)
                foreach (var soundBank in descRef._failedAttackSoundBanks) {
                    int idx = Array.IndexOf(descRef._failedAttackSoundBanks, soundBank);
                    descData += $"\n    sound_bank_{idx}: {soundBank?.name}";
                    DumpDescriptorData("\n      ", ref descData, soundBank);
                }
                descData += $"\n  _muzzles:";
                if (descRef._muzzles.Length > 0)
                foreach (var muzzle in descRef._muzzles) {
                    int idx = Array.IndexOf(descRef._muzzles, muzzle);
                    descData += $"\n    muzzle_{idx}: {muzzle.name}";
                    descData += $"\n      _muzzleIntensityCurve:";
                    DumpDescriptorData($"\n        ", ref descData, muzzle._muzzleIntensityCurve?.keys);
                    descData += $"\n      _additionalLight: {muzzle._additionalLight?.name}";
                    descData += $"\n      _additLightIntencityMult: {muzzle._additLightIntencityMult}";
                    descData += $"\n      _muzzleLight: {muzzle._muzzleLight?.name}";
                }
                descData += $"\n  _grip: {descRef._grip}";
                descData += $"\n  _visualReachCellDuration: {descRef._visualReachCellDuration}";
                descData += $"\n  _entityFlySprites:";
                if (descRef._entityFlySprites.Length > 0)
                foreach (var sprite in descRef._entityFlySprites) {
                    int idx = Array.IndexOf(descRef._entityFlySprites, sprite);
                    descData += $"\n    {idx}: {sprite?.name}";
                }
                descData += $"\n  _overrideBullet: {descRef._overrideBullet?.name}";
                descData += $"\n    _bulletSpeed: {descRef._overrideBullet?._bulletSpeed}";
                descData += $"\n    _makeBloodDecals: {descRef._overrideBullet?._makeBloodDecals}";
                descData += $"\n    _putShotDecalsOnWalls: {descRef._overrideBullet?._putShotDecalsOnWalls}";
                descData += $"\n    _putBulletShellsOnFloor: {descRef._overrideBullet?._putBulletShellsOnFloor}";
                descData += $"\n    _rotateBulletInShotDir: {descRef._overrideBullet?._rotateBulletInShotDir}";
                descData += $"\n    _shakeDuration: {descRef._overrideBullet?._shakeDuration}";
                descData += $"\n    _shakeStrength: {descRef._overrideBullet?._shakeStrength}";
                descData += $"\n    _facadeDecals:";
                if (descRef._overrideBullet && 
                    descRef._overrideBullet._facadeDecals != null && 
                    descRef._overrideBullet._facadeDecals.Length > 0)
                foreach (var sprite in descRef._overrideBullet._facadeDecals) {
                    int idx = Array.IndexOf(descRef._overrideBullet._facadeDecals, sprite);
                    descData += $"\n      {idx}: {sprite?.name}";
                }
                descData += $"\n    _gibsController_MORE_DATA: {descRef._overrideBullet?._gibsController?.name}";
                descData += $"\n    _putDecals: {descRef._overrideBullet?._putDecals}";
                descData += $"\n  _hasHFGOverlay: {descRef._hasHFGOverlay}";
                ModLog.Info(descData);
                // if (!Directory.Exists(descPath)) Directory.CreateDirectory(descPath);
                // ModTools.DumpAsset(descriptor._icon);
                // ModTools.DumpAsset(descriptor._smallIcon);
                // ModTools.DumpAsset(descriptor._shadow);
                ModLog.Info($"DUMPED: {descId}");
            } catch (Exception ex) {
                ModLog.Error($"DUMP FAIL AT '{descId}' ITEM: {ex}");
            }
        }

        public static void DumpDescriptorData(string infoPrefix, ref string descData, SoundBank soundBank) {
            try {
                descData += $"{infoPrefix}_clips: ";
                if (soundBank._clips.Length > 0)
                foreach (var audio in soundBank._clips)
                    descData += $"{audio.name}{(soundBank._clips.Last() != audio ? "|" : "")}";
                descData += $"{infoPrefix}_customMixerGroup: {soundBank._customMixerGroup?.name}";
                descData += $"{infoPrefix}_volume: {soundBank._volume}";
                descData += $"{infoPrefix}_priority: {soundBank._priority}";
                descData += $"{infoPrefix}_pitch: {soundBank._pitch}";
                descData += $"{infoPrefix}_stereoPan: {soundBank._stereoPan}";
                descData += $"{infoPrefix}_spatialBlend: {soundBank._spatialBlend}";
                descData += $"{infoPrefix}_minDistanceInCells: {soundBank._minDistanceInCells}";
                descData += $"{infoPrefix}_maxDistanceInCells: {soundBank._maxDistanceInCells}";
                descData += $"{infoPrefix}_rolloffMode: {soundBank._rolloffMode}";
                descData += $"{infoPrefix}_customRollOffCurve: ";
                DumpDescriptorData($"{infoPrefix}  ", ref descData, soundBank._customRollOffCurve?.keys);
            } catch (Exception ex) {
                ModLog.Error($"DUMP FAIL AT 'SoundBank' TYPE: {ex}");
            }
        }

        public static void DumpDescriptorData(string infoPrefix, ref string descData, Keyframe[] keyFrames) {
            try {
                if (keyFrames.Length > 0)
                foreach (var keyFrame in keyFrames) {
                    descData += $"{infoPrefix}m_Time: {keyFrame.time}";
                    descData += $"{infoPrefix}m_Value: {keyFrame.value}";
                    descData += $"{infoPrefix}m_InTangent: {keyFrame.inTangent}";
                    descData += $"{infoPrefix}m_OutTangent: {keyFrame.outTangent}";
                    descData += $"{infoPrefix}m_WeightedMode: {keyFrame.weightedMode}";
                    descData += $"{infoPrefix}m_InWeight: {keyFrame.inWeight}";
                    descData += $"{infoPrefix}m_OutWeight: {keyFrame.outWeight}";
                }
            } catch (Exception ex) {
                ModLog.Error($"DUMP FAIL AT 'Keyframe[]' TYPE: {ex}");
            }
        }

        public static void CleanRecordIdentifier(ConfigTableRecord refRecord) {
            refRecord.Id = CleanRecord(refRecord.Id);
        }

        public static string CleanRecord(string original) {
            return original.Trim(new char[] { '*', '-', '+', '^' });
        }
    }

    public class ModConfigLoader {
        private readonly List<IConfigParser> _parsers = new List<IConfigParser>();

        public event Action<string, DescriptorsCollection> OnDescriptorsLoaded = delegate { };

        public void AddParser(IConfigParser parser) {
            _parsers.Add(parser);
        }

        /// <remarks>
        /// Loads custom configuration files and overwrites existing entries, if possible.
        /// 
        /// <br/><br/><u>Configuration File TAB CSV Writing Pattern</u>
        /// <br/><c>#region_name</c> - defines start of the config region and its type (i.e. <c>#ammo</c>).
        /// <br/><c>|Key| |Value|</c> - defines parameter columns (i.e. <c>ammoId</c>, <c>maxStack</c>). Can have more columns.
        /// <br/><c>|entry_id| |20 50 C1|</c> - row(s) with data that follow defined pattern by parameter columns.
        /// <br/><c>#end</c> - defines end of the config region. Add blank line after it, if there are more configs regions.
        /// 
        /// <br/><br/><u>Identifier Prefix Information</u>
        /// <br/><c>*entry_id</c> - will put entry as topmost in composite records only. Reads all values.
        /// <br/><c>+entry_id</c> - will append data to records and values that allow it. Reads some values.
        /// <br/><c>-entry_id</c> - will remove data from records and values that allow it. Reads some values.
        /// <br/><c>^entry_id</c> - will delete record from the collection. Disregards values, can't be empty.
        /// 
        /// <br/><br/>Since config file is pretty much TAB-based CSV, just use CSV editor to handle it.
        /// <br/>Such as LibreCalc. Just make sure that you save it as TAB-based CSV as well.
        /// 
        /// <br/><br/>For possible parameters and config regions please refer to original config files.
        /// </remarks>
        public void ProcessConfigFile(string configPath, bool verboseLogging = false) {
            string configFile = File.ReadAllText(configPath);
            string[] configEntries = configFile.Split(new char[] {'\n'}, StringSplitOptions.None);
            bool headerParsed = false;
            string currentKey = string.Empty;
            DescriptorsCollection desCollection = null;
            IConfigParser currParser = null;
            foreach (string cfgEntry in configEntries) {
                if (!string.IsNullOrEmpty(cfgEntry) && (cfgEntry.Length < 2 || cfgEntry[0] != '/' || cfgEntry[1] != '/')) {
                    if (headerParsed) {
                        headerParsed = false;
                        currParser.ParseHeaders(SplitLine(cfgEntry));
                    } else if (cfgEntry.Contains("#end")) {
                        currParser = null;
                        desCollection = null;
                    } else if (cfgEntry[0] == '#') {
                        currentKey = cfgEntry.Trim(new char[] {'\t', '\r', '\n', '#'});
                        foreach (IConfigParser refParser in _parsers) {
                            if (refParser.ValidTableKey(currentKey)) {
                                headerParsed = true;
                                currParser = refParser;
                            }
                        }
                        desCollection = Resources.Load<DescriptorsCollection>("DescriptorsCollections/" + currentKey + "_descriptors");
                        if (desCollection != null) {
                            OnDescriptorsLoaded(currentKey, desCollection);
                        }
                    } else if (currParser != null) {
                        try {
                            if (cfgEntry.Contains(" \t") || cfgEntry.Contains("\t ")) 
                                ModLog.Warning($"WARNING! Bad Spacing: {cfgEntry.Trim(new char[] { '\t', '\r', '\n' })}");
                            currParser.ParseLine(SplitLine(cfgEntry), currentKey, desCollection);
                        } catch (Exception ex) {
                            ModLog.Error($"ERROR! Parsing Failed: {cfgEntry.Trim(new char[] {'\t', '\r', '\n'})}");
                            if (verboseLogging) ModLog.Error(ex.ToString());
                        }
                    }
                }
            }
        }

        private string[] SplitLine(string str) {
            return Regex.Replace(str, "\\n|\\r", string.Empty).Split('\t');
        }
    }
}

// Reference Output: ILSpy v9.0.0.7660 / C# 11.0 / 2022.4
/*
public class Data
{
	public static Dictionary<string, DescriptorsCollection> Descriptors { get; private set; }

	public static GlobalSettings Global { get; private set; }

	public static ConfigRecordCollection<DifficultySettingsRecord> DifficultySettings { get; private set; }

	public static Dictionary<string, DifficultyPreset> DifficultyPresets { get; private set; }

	public static Tilesets Tilesets { get; private set; }

	public static BinaryPresetsMap PresetsMap { get; private set; }

	public static FactionDropCollection FactionDrop { get; private set; }

	public static ItemsCollection Items { get; private set; }

	public static LocationBasedDropCollection LocationItemDrop { get; private set; }

	public static LocationBasedDropCollection LocationMonsterDrop { get; private set; }

	public static List<ProcMissionRecord> ProcMissions { get; private set; }

	public static List<PrizeByRatingRecord> PrizesByRatings { get; private set; }

	public static List<StartingItemsRecord> StartingItems { get; private set; }

	public static List<WorkbenchReceiptRecord> WorkbenchReceipts { get; private set; }

	public static List<ItemProduceReceipt> ProduceReceipts { get; private set; }

	public static List<ProcMissionObjectiveRecord> ProcMissionObjectives { get; private set; }

	public static List<ProgressionDifficultyRecord> ProgressionDifficulty { get; private set; }

	public static List<MissionDifficultyRecord> MissionDifficulty { get; private set; }

	public static List<PopulationReceipt> PopulationReceipts { get; private set; }

	public static ConfigRecordCollection<CreatureRecord> Creatures { get; private set; }

	public static ConfigRecordCollection<AiPresetRecord> AiPresets { get; private set; }

	public static ConfigRecordCollection<WoundRecord> Wounds { get; private set; }

	public static ConfigRecordCollection<QmorphosRecord> Qmorphos { get; private set; }

	public static ConfigRecordCollection<ItemTransformationRecord> ItemTransformation { get; private set; }

	public static ConfigRecordCollection<BarterReceipt> BarterReceipts { get; private set; }

	public static ConfigRecordCollection<EquipmentVariantRecord> MonsterEquipments { get; private set; }

	public static ConfigRecordCollection<FireModeRecord> Firemodes { get; private set; }

	public static ConfigRecordCollection<PerkRecord> Perks { get; private set; }

	public static ConfigRecordCollection<MercenaryClassRecord> MercenaryClasses { get; private set; }

	public static ConfigRecordCollection<MercenaryProfileRecord> MercenaryProfiles { get; private set; }

	public static ConfigRecordCollection<WoundSlotRecord> WoundSlots { get; private set; }

	public static ConfigRecordCollection<StoryMissionRecord> StoryMissions { get; private set; }

	public static ConfigRecordCollection<SpaceObjectRecord> SpaceObjects { get; private set; }

	public static ConfigRecordCollection<TileTransformationRecord> TileTransformation { get; private set; }

	public static ConfigRecordCollection<FactionRecord> Factions { get; private set; }

	public static ConfigRecordCollection<AllianceRecord> Alliances { get; private set; }

	public static ConfigRecordCollection<StationRecord> Stations { get; private set; }

	public static ConfigRecordCollection<StationBarterRecord> StationBarter { get; private set; }

	public static ConfigRecordCollection<ProcMissionTemplate> ProcMissionTemplates { get; private set; }

	public static ConfigRecordCollection<ItemExpireRecord> ItemExpire { get; private set; }

	public static ConfigRecordCollection<StatusEffectsRecord> StatusEffects { get; private set; }

	public static ConfigRecordCollection<DamageTypeRecord> DamageTypes { get; private set; }

	public static ConfigRecordCollection<MagnumPerkRecord> MagnumPerks { get; private set; }

	public static ConfigRecordCollection<MagnumProjectParameter> MagnumProjectParameters { get; private set; }

	public static Dictionary<int, TechLevelRecord> TechLevels { get; private set; }

	public static List<MagnumProjectPrice> MagnumProjectPrices { get; private set; }

	public static Dictionary<SpaceshipParameter, float> MagnumDefaultValues { get; private set; }

	public static void Load()
	{
		Tilesets = new Tilesets(Resources.Load<DescriptorsCollection>("DescriptorsCollections/tilesets_descriptors"));
		PresetsMap = Resources.Load<BinaryPresetsMap>("PresetsMap");
		Descriptors = new Dictionary<string, DescriptorsCollection>();
		Global = new GlobalSettings();
		DifficultySettings = new ConfigRecordCollection<DifficultySettingsRecord>();
		DifficultyPresets = new Dictionary<string, DifficultyPreset>();
		ProgressionDifficulty = new List<ProgressionDifficultyRecord>();
		MissionDifficulty = new List<MissionDifficultyRecord>();
		Creatures = new ConfigRecordCollection<CreatureRecord>();
		AiPresets = new ConfigRecordCollection<AiPresetRecord>();
		Items = new ItemsCollection();
		Wounds = new ConfigRecordCollection<WoundRecord>();
		Qmorphos = new ConfigRecordCollection<QmorphosRecord>();
		LocationItemDrop = new LocationBasedDropCollection("itemdrop_");
		LocationMonsterDrop = new LocationBasedDropCollection("monsterdrop_");
		FactionDrop = new FactionDropCollection();
		ItemTransformation = new ConfigRecordCollection<ItemTransformationRecord>();
		WorkbenchReceipts = new List<WorkbenchReceiptRecord>();
		ProduceReceipts = new List<ItemProduceReceipt>();
		BarterReceipts = new ConfigRecordCollection<BarterReceipt>();
		MonsterEquipments = new ConfigRecordCollection<EquipmentVariantRecord>();
		Firemodes = new ConfigRecordCollection<FireModeRecord>();
		Perks = new ConfigRecordCollection<PerkRecord>();
		MercenaryClasses = new ConfigRecordCollection<MercenaryClassRecord>();
		MercenaryProfiles = new ConfigRecordCollection<MercenaryProfileRecord>();
		WoundSlots = new ConfigRecordCollection<WoundSlotRecord>();
		StoryMissions = new ConfigRecordCollection<StoryMissionRecord>();
		SpaceObjects = new ConfigRecordCollection<SpaceObjectRecord>();
		TileTransformation = new ConfigRecordCollection<TileTransformationRecord>();
		Factions = new ConfigRecordCollection<FactionRecord>();
		Alliances = new ConfigRecordCollection<AllianceRecord>();
		Stations = new ConfigRecordCollection<StationRecord>();
		StationBarter = new ConfigRecordCollection<StationBarterRecord>();
		ProcMissions = new List<ProcMissionRecord>();
		ProcMissionObjectives = new List<ProcMissionObjectiveRecord>();
		ProcMissionTemplates = new ConfigRecordCollection<ProcMissionTemplate>();
		PopulationReceipts = new List<PopulationReceipt>();
		ItemExpire = new ConfigRecordCollection<ItemExpireRecord>();
		StatusEffects = new ConfigRecordCollection<StatusEffectsRecord>();
		DamageTypes = new ConfigRecordCollection<DamageTypeRecord>();
		PrizesByRatings = new List<PrizeByRatingRecord>();
		StartingItems = new List<StartingItemsRecord>();
		MagnumPerks = new ConfigRecordCollection<MagnumPerkRecord>();
		MagnumProjectParameters = new ConfigRecordCollection<MagnumProjectParameter>();
		TechLevels = new Dictionary<int, TechLevelRecord>();
		MagnumProjectPrices = new List<MagnumProjectPrice>();
		MagnumDefaultValues = new Dictionary<SpaceshipParameter, float>();
		ConfigLoader configLoader = new ConfigLoader();
		configLoader.OnDescriptorsLoaded += delegate(string header, DescriptorsCollection descriptors)
		{
			Descriptors.Add(header, descriptors);
		};
		configLoader.AddParser(new VertTableParser<GlobalSettings>(Global, "global"));
		configLoader.AddParser(new TableParser<CreatureRecord>("monsters", delegate(CreatureRecord r, string header, DescriptorsCollection descs)
		{
			Creatures.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new MultiColVertTableParser<DifficultyPreset>(DifficultyPresets, "difficulty_presets"));
		configLoader.AddParser(new TableParser<DifficultySettingsRecord>("difficulty_settings", delegate(DifficultySettingsRecord r, string header, DescriptorsCollection descs)
		{
			DifficultySettings.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<ProgressionDifficultyRecord>("progression_difficulty", delegate(ProgressionDifficultyRecord r, string header, DescriptorsCollection descs)
		{
			ProgressionDifficulty.Add(r);
		}));
		configLoader.AddParser(new TableParser<MissionDifficultyRecord>("mission_difficulty", delegate(MissionDifficultyRecord r, string header, DescriptorsCollection descs)
		{
			MissionDifficulty.Add(r);
		}));
		configLoader.AddParser(new TableParser<ProcMissionRecord>("procmissions", delegate(ProcMissionRecord r, string header, DescriptorsCollection descs)
		{
			ProcMissions.Add(r);
			r.ContentDescriptor = descs.GetDescriptor(r.ProcMissionType.ToString()) as ProcMissionDescriptor;
		}));
		configLoader.AddParser(new TableParser<PrizeByRatingRecord>("prizes_by_rating", delegate(PrizeByRatingRecord r, string header, DescriptorsCollection descs)
		{
			PrizesByRatings.Add(r);
		}));
		configLoader.AddParser(new TableParser<StartingItemsRecord>("starting_items", delegate(StartingItemsRecord r, string header, DescriptorsCollection descs)
		{
			StartingItems.Add(r);
		}));
		configLoader.AddParser(new TableParser<DamageTypeRecord>("damagetypes", delegate(DamageTypeRecord r, string header, DescriptorsCollection descs)
		{
			DamageTypes.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<EquipmentVariantRecord>("monster_equipment", delegate(EquipmentVariantRecord r, string header, DescriptorsCollection descs)
		{
			MonsterEquipments.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<AiPresetRecord>("ai-presets", delegate(AiPresetRecord r, string header, DescriptorsCollection descs)
		{
			AiPresets.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<StatusEffectsRecord>("statuseffects", delegate(StatusEffectsRecord r, string header, DescriptorsCollection descs)
		{
			StatusEffects.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<FactionRecord>("factions", delegate(FactionRecord r, string header, DescriptorsCollection descs)
		{
			Factions.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<MercenaryClassRecord>("mercenary_classes", delegate(MercenaryClassRecord r, string header, DescriptorsCollection descs)
		{
			MercenaryClasses.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<MercenaryProfileRecord>("mercenary_profiles", delegate(MercenaryProfileRecord r, string header, DescriptorsCollection descs)
		{
			MercenaryProfiles.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<PerkRecord>("perks", delegate(PerkRecord r, string header, DescriptorsCollection descs)
		{
			Perks.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<AllianceRecord>("alliances", delegate(AllianceRecord r, string header, DescriptorsCollection descs)
		{
			Alliances.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ProcMissionTemplate>("procmissiontemplates", delegate(ProcMissionTemplate r, string header, DescriptorsCollection descs)
		{
			ProcMissionTemplates.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<StationRecord>("stations", delegate(StationRecord r, string header, DescriptorsCollection descs)
		{
			Stations.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<StoryMissionRecord>("storymissions", delegate(StoryMissionRecord r, string header, DescriptorsCollection descs)
		{
			StoryMissions.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<SpaceObjectRecord>("spaceobjects", delegate(SpaceObjectRecord r, string header, DescriptorsCollection descs)
		{
			SpaceObjects.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<TileTransformationRecord>("tilesettransformation", delegate(TileTransformationRecord r, string header, DescriptorsCollection descs)
		{
			TileTransformation.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<FireModeRecord>("firemodes", delegate(FireModeRecord r, string header, DescriptorsCollection descs)
		{
			Firemodes.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ItemExpireRecord>("itemexpire", delegate(ItemExpireRecord r, string header, DescriptorsCollection descs)
		{
			ItemExpire.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<WoundSlotRecord>("woundslots", delegate(WoundSlotRecord r, string header, DescriptorsCollection descs)
		{
			WoundSlots.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<WoundRecord>("woundtypes", delegate(WoundRecord r, string header, DescriptorsCollection descs)
		{
			Wounds.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<ItemTransformationRecord>("itemtransformation", delegate(ItemTransformationRecord r, string header, DescriptorsCollection descs)
		{
			ItemTransformation.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<WorkbenchReceiptRecord>("workbenchreceipts", delegate(WorkbenchReceiptRecord r, string header, DescriptorsCollection descs)
		{
			WorkbenchReceipts.Add(r);
			r.GenerateId();
		}));
		configLoader.AddParser(new TableParser<ItemProduceReceipt>("itemreceipts", delegate(ItemProduceReceipt r, string header, DescriptorsCollection descs)
		{
			ProduceReceipts.Add(r);
		}));
		configLoader.AddParser(new TableParser<BarterReceipt>("barter_receipts", delegate(BarterReceipt r, string header, DescriptorsCollection descs)
		{
			BarterReceipts.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<StationBarterRecord>("station_barter", delegate(StationBarterRecord r, string header, DescriptorsCollection descs)
		{
			StationBarter.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<QmorphosRecord>("quazimorphosis", delegate(QmorphosRecord r, string header, DescriptorsCollection descs)
		{
			Qmorphos.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<ProcMissionObjectiveRecord>("missionobjectives", delegate(ProcMissionObjectiveRecord r, string header, DescriptorsCollection descs)
		{
			ProcMissionObjectives.Add(r);
		}));
		configLoader.AddParser(new TableParser<PopulationReceipt>("population_receipts", delegate(PopulationReceipt r, string header, DescriptorsCollection descs)
		{
			PopulationReceipts.Add(r);
		}));
		configLoader.AddParser(new TableParser<AmmoRecord>("ammo", delegate(AmmoRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<WeaponRecord>("meleeweapons", delegate(WeaponRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.IsMelee = true;
			r.CanThrow = r.ThrowRange != 0;
			r.Range = 1;
			r.DefineClassTraits();
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<WeaponRecord>("rangeweapons", delegate(WeaponRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.DefineClassTraits();
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<MedkitRecord>("medkits", delegate(MedkitRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<FoodRecord>("food", delegate(FoodRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<BackpackRecord>("backpacks", delegate(BackpackRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<VestRecord>("vests", delegate(VestRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<HelmetRecord>("helmets", delegate(HelmetRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ArmorRecord>("armors", delegate(ArmorRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<LeggingsRecord>("leggings", delegate(LeggingsRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<BootsRecord>("boots", delegate(BootsRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<RepairRecord>("repairs", delegate(RepairRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<SkullRecord>("skulls", delegate(SkullRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<QuasiArtifactRecord>("quasiartifacts", delegate(QuasiArtifactRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<GrenadeRecord>("grenades", delegate(GrenadeRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<MineRecord>("mines", delegate(MineRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<AutomapRecord>("automaps", delegate(AutomapRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ResurrectKitRecord>("resurrectkits", delegate(ResurrectKitRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ExpGainerRecord>("expgainers", delegate(ExpGainerRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<TrashRecord>("trash", delegate(TrashRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<DatadiskRecord>("datadisks", delegate(DatadiskRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<TurretRecord>("turrets", delegate(TurretRecord r, string header, DescriptorsCollection descs)
		{
			Items.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<ContentDropRecord>("itemdrop_", TableKeyComparisonMode.Contains, delegate(ContentDropRecord r, string header, DescriptorsCollection descs)
		{
			LocationItemDrop.AddRecord(header, r);
		}));
		configLoader.AddParser(new TableParser<ContentDropRecord>("monsterdrop_", TableKeyComparisonMode.Contains, delegate(ContentDropRecord r, string header, DescriptorsCollection descs)
		{
			LocationMonsterDrop.AddRecord(header, r);
		}));
		configLoader.AddParser(new TableParser<ContentDropRecord>("factiondrop_", TableKeyComparisonMode.Contains, delegate(ContentDropRecord r, string header, DescriptorsCollection descs)
		{
			FactionDrop.AddRecord(header, r);
		}));
		configLoader.AddParser(new TableParser<MagnumPerkRecord>("magnum_perks", TableKeyComparisonMode.Equals, delegate(MagnumPerkRecord r, string header, DescriptorsCollection descs)
		{
			MagnumPerks.AddRecord(r.Id, r);
			r.ContentDescriptor = descs.GetDescriptor(r.Id);
		}));
		configLoader.AddParser(new TableParser<MagnumProjectParameter>("magnum_projects_params", delegate(MagnumProjectParameter r, string header, DescriptorsCollection descs)
		{
			MagnumProjectParameters.AddRecord(r.Id, r);
		}));
		configLoader.AddParser(new TableParser<MagnumProjectPrice>("magnum_projects_prices", delegate(MagnumProjectPrice r, string header, DescriptorsCollection descs)
		{
			MagnumProjectPrices.Add(r);
		}));
		configLoader.AddParser(new TableParser<MagnumDefaultParameterRecord>("magnum_default_params", delegate(MagnumDefaultParameterRecord r, string header, DescriptorsCollection descs)
		{
			MagnumDefaultValues.Add(r.Parameter, r.Value);
		}));
		configLoader.AddParser(new TableParser<TechLevelRecord>("techlevels", delegate(TechLevelRecord r, string s, DescriptorsCollection descs)
		{
			TechLevels.TryAdd(r.TechLevel, r);
		}));
		configLoader.Load();
		MagnumPerkRecord.CacheChilds(MagnumPerks.Records);
	}
}

public class ConfigLoader
{
	private readonly List<IConfigParser> _parsers = new List<IConfigParser>();

	public event Action<string, DescriptorsCollection> OnDescriptorsLoaded = delegate
	{
	};

	public void AddParser(IConfigParser parser)
	{
		_parsers.Add(parser);
	}

	public void Load()
	{
		LoadSpecificFile("config_globals");
		LoadSpecificFile("config_difficulty");
		LoadSpecificFile("config_items");
		LoadSpecificFile("config_monsters");
		LoadSpecificFile("config_drops");
		LoadSpecificFile("config_wounds");
		LoadSpecificFile("config_mercenaries");
		LoadSpecificFile("config_spacesandbox");
		LoadSpecificFile("config_barter");
		LoadSpecificFile("config_magnum");
	}

	public void LoadSpecificFile(string path)
	{
		TextAsset obj = Resources.Load(path) as TextAsset;
		if (obj == null)
		{
			throw new NotImplementedException("Failed open " + path + " in Resources folder.");
		}
		string[] array = obj.text.Split('\n');
		bool flag = false;
		string text = string.Empty;
		DescriptorsCollection descriptorsCollection = null;
		IConfigParser configParser = null;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (string.IsNullOrEmpty(text2) || (text2.Length >= 2 && text2[0] == '/' && text2[1] == '/'))
			{
				continue;
			}
			if (flag)
			{
				flag = false;
				configParser.ParseHeaders(SplitLine(text2));
			}
			else if (text2.Contains("#end"))
			{
				configParser = null;
				descriptorsCollection = null;
			}
			else if (text2[0] == '#')
			{
				text = text2.Trim('\t', '\r', '\n', '#');
				foreach (IConfigParser parser in _parsers)
				{
					if (parser.ValidTableKey(text))
					{
						flag = true;
						configParser = parser;
					}
				}
				descriptorsCollection = Resources.Load<DescriptorsCollection>("DescriptorsCollections/" + text + "_descriptors");
				if (descriptorsCollection != null)
				{
					this.OnDescriptorsLoaded(text, descriptorsCollection);
				}
			}
			else
			{
				configParser?.ParseLine(SplitLine(text2), text, descriptorsCollection);
			}
		}
	}

	private string[] SplitLine(string s)
	{
		return Regex.Replace(s, "\\n|\\r", string.Empty).Split('\t');
	}
}
*/