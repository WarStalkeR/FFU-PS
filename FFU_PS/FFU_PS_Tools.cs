﻿using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FFU_Phase_Shift {
    public class ModTools {
        private bool _verbose = false;
        private string _cntPath = null;
        private ModConfigLoader _cfgLoader = null;

        public ModTools() {
        }

        public void Setup(string contentPath = null, ModConfigLoader configLoader = null, bool logVerbose = false) {
            if (contentPath != null) _cntPath = contentPath;
            if (configLoader != null) _cfgLoader = configLoader;
            if (logVerbose) _verbose = true;
        }

        public void Initialize() {
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
                    FieldInfo refInfo = typeof(ConfigRecordCollection<CreatureRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, CreatureRecord>)refInfo.GetValue(Data.Creatures);
                    records[r.Id] = r;
                } else Data.Creatures.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<GameDifficultyRecord>("game_difficulty", delegate (GameDifficultyRecord r, string header, DescriptorsCollection descs) {
                int index = Data.GameDifficulty.FindIndex(x => x.CompletedMissionNumber == r.CompletedMissionNumber);
                if (index != -1) {
                    Data.GameDifficulty[index] = r;
                } else Data.GameDifficulty.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<MissionDifficultyRecord>("mission_difficulty", delegate (MissionDifficultyRecord r, string header, DescriptorsCollection descs) {
                int index = Data.MissionDifficulty.FindIndex(x => x.DifficultyRating == r.DifficultyRating);
                if (index != -1) {
                    Data.MissionDifficulty[index] = r;
                } else Data.MissionDifficulty.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionParametersRecord>("procmissionparameters", delegate (ProcMissionParametersRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissionParameters.FindIndex(x => x.ProcMissionType == r.ProcMissionType);
                if (index != -1) {
                    Data.ProcMissionParameters[index] = r;
                } else Data.ProcMissionParameters.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<PrizeByRatingRecord>("prizes_by_rating", delegate (PrizeByRatingRecord r, string header, DescriptorsCollection descs) {
                int index = Data.PrizesByRatings.FindIndex(x => x.ProcMissionType == r.ProcMissionType && x.DifficultyRating == r.DifficultyRating);
                if (index != -1) {
                    Data.PrizesByRatings[index] = r;
                } else Data.PrizesByRatings.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<DamageTypeRecord>("damagetypes", delegate (DamageTypeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, damagetypes: {r.Id}");
                if (Data.DamageTypes.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<DamageTypeRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, DamageTypeRecord>)refInfo.GetValue(Data.DamageTypes);
                    records[r.Id] = r;
                } else Data.DamageTypes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<EquipmentVariantRecord>("monster_equipment", delegate (EquipmentVariantRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, monster_equipment: {r.Id}");
                if (Data.MonsterEquipments.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<EquipmentVariantRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, EquipmentVariantRecord>)refInfo.GetValue(Data.MonsterEquipments);
                    records[r.Id] = r;
                } else Data.MonsterEquipments.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<AiPresetRecord>("ai-presets", delegate (AiPresetRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, ai-presets: {r.Id}");
                if (Data.AiPresets.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<AiPresetRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, AiPresetRecord>)refInfo.GetValue(Data.AiPresets);
                    records[r.Id] = r;
                } else Data.AiPresets.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StatusEffectsRecord>("statuseffects", delegate (StatusEffectsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, statuseffects: {r.Id}");
                if (Data.StatusEffects.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StatusEffectsRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StatusEffectsRecord>)refInfo.GetValue(Data.StatusEffects);
                    records[r.Id] = r;
                } else Data.StatusEffects.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FactionRecord>("factions", delegate (FactionRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, factions: {r.Id}");
                if (Data.Factions.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<FactionRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, FactionRecord>)refInfo.GetValue(Data.Factions);
                    records[r.Id] = r;
                } else Data.Factions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryClassRecord>("mercenary_classes", delegate (MercenaryClassRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mercenary_classes: {r.Id}");
                if (Data.MercenaryClasses.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MercenaryClassRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MercenaryClassRecord>)refInfo.GetValue(Data.MercenaryClasses);
                    records[r.Id] = r;
                } else Data.MercenaryClasses.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryProfileRecord>("mercenary_profiles", delegate (MercenaryProfileRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mercenary_profiles: {r.Id}");
                if (Data.MercenaryProfiles.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MercenaryProfileRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MercenaryProfileRecord>)refInfo.GetValue(Data.MercenaryProfiles);
                    records[r.Id] = r;
                } else Data.MercenaryProfiles.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<PerkRecord>("perks", delegate (PerkRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, perks: {r.Id}");
                if (Data.Perks.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<PerkRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, PerkRecord>)refInfo.GetValue(Data.Perks);
                    records[r.Id] = r;
                } else Data.Perks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AllianceRecord>("alliances", delegate (AllianceRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, alliances: {r.Id}");
                if (Data.Alliances.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<AllianceRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, AllianceRecord>)refInfo.GetValue(Data.Alliances);
                    records[r.Id] = r;
                } else Data.Alliances.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionTemplate>("procmissiontemplates", delegate (ProcMissionTemplate r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, procmissiontemplates: {r.Id}");
                if (Data.ProcMissionTemplates.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<ProcMissionTemplate>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, ProcMissionTemplate>)refInfo.GetValue(Data.ProcMissionTemplates);
                    records[r.Id] = r;
                } else Data.ProcMissionTemplates.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationRecord>("stations", delegate (StationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, stations: {r.Id}");
                if (Data.Stations.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StationRecord>)refInfo.GetValue(Data.Stations);
                    records[r.Id] = r;
                } else Data.Stations.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<StoryMissionRecord>("storymissions", delegate (StoryMissionRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, storymissions: {r.Id}");
                if (Data.StoryMissions.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StoryMissionRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StoryMissionRecord>)refInfo.GetValue(Data.StoryMissions);
                    records[r.Id] = r;
                } else Data.StoryMissions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SpaceObjectRecord>("spaceobjects", delegate (SpaceObjectRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, spaceobjects: {r.Id}");
                if (Data.SpaceObjects.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<SpaceObjectRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, SpaceObjectRecord>)refInfo.GetValue(Data.SpaceObjects);
                    records[r.Id] = r;
                } else Data.SpaceObjects.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<TileTransformationRecord>("tilesettransformation", delegate (TileTransformationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, tilesettransformation: {r.Id}");
                if (Data.TileTransformation.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<TileTransformationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, TileTransformationRecord>)refInfo.GetValue(Data.TileTransformation);
                    records[r.Id] = r;
                } else Data.TileTransformation.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<FireModeRecord>("firemodes", delegate (FireModeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, firemodes: {r.Id}");
                if (Data.Firemodes.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<FireModeRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, FireModeRecord>)refInfo.GetValue(Data.Firemodes);
                    records[r.Id] = r;
                } else Data.Firemodes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ItemExpireRecord>("itemexpire", delegate (ItemExpireRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, itemexpire: {r.Id}");
                if (r.Id.StartsWith("--")) {
                    string refId = r.Id.Replace("--", string.Empty);
                    if (Data.ItemExpire.GetRecord(refId) != null) Data.ItemExpire.RemoveRecord(refId);
                } else {
                    if (Data.ItemExpire.GetRecord(r.Id) != null) {
                        FieldInfo refInfo = typeof(ConfigRecordCollection<ItemExpireRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                        var records = (Dictionary<string, ItemExpireRecord>)refInfo.GetValue(Data.ItemExpire);
                        records[r.Id] = r;
                    } else Data.ItemExpire.AddRecord(r.Id, r);
                }
            }));
            _cfgLoader.AddParser(new TableParser<WoundSlotRecord>("woundslots", delegate (WoundSlotRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, woundslots: {r.Id}");
                if (Data.WoundSlots.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<WoundSlotRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, WoundSlotRecord>)refInfo.GetValue(Data.WoundSlots);
                    records[r.Id] = r;
                } else Data.WoundSlots.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WoundRecord>("woundtypes", delegate (WoundRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, woundtypes: {r.Id}");
                if (Data.Wounds.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<WoundRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, WoundRecord>)refInfo.GetValue(Data.Wounds);
                    records[r.Id] = r;
                } else Data.Wounds.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ItemTransformationRecord>("itemtransformation", delegate (ItemTransformationRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, itemtransformation: {r.Id}");
                if (Data.ItemTransformation.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<ItemTransformationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, ItemTransformationRecord>)refInfo.GetValue(Data.ItemTransformation);
                    records[r.Id] = r;
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
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BarterReceipt>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BarterReceipt>)refInfo.GetValue(Data.BarterReceipts);
                    records[r.Id] = r;
                } else Data.BarterReceipts.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationBarterRecord>("station_barter", delegate (StationBarterRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, station_barter: {r.Id}");
                if (Data.StationBarter.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StationBarterRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StationBarterRecord>)refInfo.GetValue(Data.StationBarter);
                    records[r.Id] = r;
                } else Data.StationBarter.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<QmorphosRecord>("quazimorphosis", delegate (QmorphosRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, quazimorphosis: {r.Id}");
                if (Data.Qmorphos.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<QmorphosRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, QmorphosRecord>)refInfo.GetValue(Data.Qmorphos);
                    records[r.Id] = r;
                } else Data.Qmorphos.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionObjectiveRecord>("missionobjectives", delegate (ProcMissionObjectiveRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissionObjectives.FindIndex(x => x.BeneficiaryFactionType == r.BeneficiaryFactionType && x.VictimFactionType == r.VictimFactionType);
                if (index != -1) {
                    Data.ProcMissionObjectives[index] = r;
                } else Data.ProcMissionObjectives.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<AmmoRecord>("ammo", delegate (AmmoRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, ammo: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<AmmoRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is AmmoRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("meleeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, meleeweapons: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<WeaponRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is WeaponRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.IsMelee = true;
                r.CanThrow = r.ThrowRange != 0;
                r.Range = 1;
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("rangeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, rangeweapons: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<WeaponRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is WeaponRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MedkitRecord>("medkits", delegate (MedkitRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, medkits: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<MedkitRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is MedkitRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FoodRecord>("food", delegate (FoodRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, food: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<FoodRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is FoodRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BackpackRecord>("backpacks", delegate (BackpackRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, backpacks: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<BackpackRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is BackpackRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<VestRecord>("vests", delegate (VestRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, vests: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<VestRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is VestRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<HelmetRecord>("helmets", delegate (HelmetRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, helmets: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<HelmetRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is HelmetRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ArmorRecord>("armors", delegate (ArmorRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, armors: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<ArmorRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is ArmorRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<LeggingsRecord>("leggings", delegate (LeggingsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, leggings: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<LeggingsRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is LeggingsRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BootsRecord>("boots", delegate (BootsRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, boots: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<BootsRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is BootsRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<RepairRecord>("repairs", delegate (RepairRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, repairs: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<RepairRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is RepairRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SkullRecord>("skulls", delegate (SkullRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, skulls: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<SkullRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is SkullRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<QuasiArtifactRecord>("quasiartifacts", delegate (QuasiArtifactRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, quasiartifacts: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<QuasiArtifactRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is QuasiArtifactRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<GrenadeRecord>("grenades", delegate (GrenadeRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, grenades: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<GrenadeRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is GrenadeRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MineRecord>("mines", delegate (MineRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, mines: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<MineRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is MineRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AutomapRecord>("automaps", delegate (AutomapRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, automaps: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<AutomapRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is AutomapRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ResurrectKitRecord>("resurrectkits", delegate (ResurrectKitRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, resurrectkits: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<ResurrectKitRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is ResurrectKitRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TrashRecord>("trash", delegate (TrashRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, trash: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<TrashRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is TrashRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<DatadiskRecord>("datadisks", delegate (DatadiskRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, datadisks: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<DatadiskRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is DatadiskRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TurretRecord>("turrets", delegate (TurretRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, turrets: {r.Id}");
                bool isReference = r.Id.StartsWith("*");
                if (isReference) r.TrimId();
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    var multiRecord = records[r.Id] as CompositeItemRecord;
                    if (multiRecord != null) {
                        if (multiRecord.GetRecord<TurretRecord>() != null) {
                            int index = multiRecord.Records.FindIndex(rec => rec is TurretRecord);
                            if (index != -1) multiRecord.Records[index] = r;
                        } else {
                            if (isReference) multiRecord.Records.Insert(0, r);
                            else multiRecord.Records.Add(r);
                        }
                    } else records[r.Id] = r;
                } else Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("itemdrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                // Has Built-in Overwrite
                Data.LocationItemDrop.AddRecord(header, r);
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("monsterdrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                // Has Built-in Overwrite
                Data.LocationMonsterDrop.AddRecord(header, r);
            }));
            _cfgLoader.AddParser(new TableParser<ContentDropRecord>("factiondrop_", TableKeyComparisonMode.Contains, delegate (ContentDropRecord r, string header, DescriptorsCollection descs) {
                // Overwrite Implementation Complicated
                Data.FactionDrop.AddRecord(header, r);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumPerkRecord>("magnum_perks", TableKeyComparisonMode.Equals, delegate (MagnumPerkRecord r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, magnum_perks: {r.Id}");
                if (Data.MagnumPerks.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MagnumPerkRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MagnumPerkRecord>)refInfo.GetValue(Data.MagnumPerks);
                    records[r.Id] = r;
                } else Data.MagnumPerks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumProjectParameter>("magnum_projects_params", delegate (MagnumProjectParameter r, string header, DescriptorsCollection descs) {
                if (_verbose) ModLog.Info($"TRACE, magnum_projects_params: {r.Id}");
                if (Data.MagnumProjectParameters.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MagnumProjectParameter>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MagnumProjectParameter>)refInfo.GetValue(Data.MagnumProjectParameters);
                    records[r.Id] = r;
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
        }

        public void LoadConfigFile(string configName) {
            // Ensure that everything is in place
            if (_cntPath == null) { ModLog.Error($"LoadConfigFile: content path is undefined!"); return; }
            if (_cfgLoader == null) { ModLog.Error($"LoadConfigFile: config loader is not referenced!"); return; }
            if (configName == null) { ModLog.Error($"LoadConfigFile: config name is undefined!"); return; }
            string cfgPath = Path.Combine(_cntPath, configName);
            if (!File.Exists(cfgPath)) { ModLog.Error($"LoadConfigFile: config file doesn't exist!"); return; }

            // Read and parse config file into data
            _cfgLoader.ProcessConfigFile(cfgPath, _verbose);
        }
    }

    public class ModConfigLoader {
        private readonly List<IConfigParser> _parsers = new List<IConfigParser>();

        public event Action<string, DescriptorsCollection> OnDescriptorsLoaded = delegate {};

        public void AddParser(IConfigParser parser) {
            _parsers.Add(parser);
        }

        /// <remarks>
        /// Loads custom configuration files and overwrites existing entries, if possible.
        /// 
        /// <br/><br/>Configuration file is a TAB-bases CSV file that follows extremely strict writing pattern:
        /// <br/><c>#region_name</c> - defines start of the config region and its type (i.e. <c>#ammo</c>).
        /// <br/><c>[Key][Value]</c> - defines parameter columns (i.e. <c>ammoId</c>, <c>maxStack</c>). Can have more columns.
        /// <br/><c>[123][Data_45]</c> - row(s) with data that follow defined pattern by parameter columns.
        /// <br/><c>#end</c> - defines end of the config region. Add blank line after it, if there are more configs regions.
        /// 
        /// <br/><br/>Since config file is pretty much TAB-based CSV, just use CSV editor to handle it.
        /// <br/>Such as LibreCalc. Just make sure that you save it as TAB-based CSV as well.
        /// 
        /// <br/><br/>For possible parameters and config regions please refer to original config files.
        /// </remarks>
        public void ProcessConfigFile(string configPath, bool verboseLogging = false) {
            string configFile = File.ReadAllText(configPath);
            string[] configEntries = configFile.Split(new char[] { '\n' }, StringSplitOptions.None);
            bool headerParsed = false;
            string currentKey = string.Empty;
            DescriptorsCollection colDescriptors = null;
            IConfigParser currParser = null;
            foreach (string cfgEntry in configEntries) {
                if (!string.IsNullOrEmpty(cfgEntry) && (cfgEntry.Length < 2 || cfgEntry[0] != '/' || cfgEntry[1] != '/')) {
                    if (headerParsed) {
                        headerParsed = false;
                        currParser.ParseHeaders(SplitLine(cfgEntry));
                    } else if (cfgEntry.Contains("#end")) {
                        currParser = null;
                        colDescriptors = null;
                    } else if (cfgEntry[0] == '#') {
                        currentKey = cfgEntry.Trim(new char[] { '\t', '\r', '\n', '#' });
                        foreach (IConfigParser refParser in _parsers) {
                            if (refParser.ValidTableKey(currentKey)) {
                                headerParsed = true;
                                currParser = refParser;
                            }
                        }
                        colDescriptors = Resources.Load<DescriptorsCollection>("DescriptorsCollections/" + currentKey + "_descriptors");
                        if (colDescriptors != null) {
                            OnDescriptorsLoaded(currentKey, colDescriptors);
                        }
                    } else if (currParser != null) {
                        try {
                            currParser.ParseLine(SplitLine(cfgEntry), currentKey, colDescriptors);
                        } catch (Exception ex) { 
                            ModLog.Warning($"ERROR: {cfgEntry.Trim(new char[] { '\t', '\r', '\n' })}");
                            if (verboseLogging) ModLog.Error(ex.ToString());
                        }
                    }
                }
            }
        }

        private string[] SplitLine(string s) {
            return Regex.Replace(s, "\\n|\\r", string.Empty).Split('\t');
        }
    }
}
