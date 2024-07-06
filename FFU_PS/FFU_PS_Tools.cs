using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FFU_Phase_Shift {
    public class ModTools {
        private string _cntPath = null;
        private ModConfigLoader _cfgLoader = null;

        public ModTools() {
        }

        public void Setup(string contentPath = null, ModConfigLoader configLoader = null) {
            if (contentPath != null) _cntPath = contentPath;
            if (configLoader != null) _cfgLoader = configLoader;
        }

        public void Initialize() {
            if (_cfgLoader == null) { ModLog.Error($"Initialize: config loader is not referenced!"); return; }
            _cfgLoader.OnDescriptorsLoaded += delegate (string header, DescriptorsCollection descriptors) {
                Data.Descriptors.Add(header, descriptors);
            };
            _cfgLoader.AddParser(new VertTableParser<GlobalSettings>(Data.Global, "global"));
            _cfgLoader.AddParser(new TableParser<CreatureRecord>("monsters", delegate (CreatureRecord r, string header, DescriptorsCollection descs) {
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
                }
                Data.MissionDifficulty.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionParametersRecord>("procmissionparameters", delegate (ProcMissionParametersRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissionParameters.FindIndex(x => x.ProcMissionType == r.ProcMissionType);
                if (index != -1) {
                    Data.ProcMissionParameters[index] = r;
                }
                Data.ProcMissionParameters.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<PrizeByRatingRecord>("prizes_by_rating", delegate (PrizeByRatingRecord r, string header, DescriptorsCollection descs) {
                int index = Data.PrizesByRatings.FindIndex(x => x.ProcMissionType == r.ProcMissionType && x.DifficultyRating == r.DifficultyRating);
                if (index != -1) {
                    Data.PrizesByRatings[index] = r;
                }
                Data.PrizesByRatings.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<DamageTypeRecord>("damagetypes", delegate (DamageTypeRecord r, string header, DescriptorsCollection descs) {
                if (Data.DamageTypes.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<DamageTypeRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, DamageTypeRecord>)refInfo.GetValue(Data.DamageTypes);
                    records[r.Id] = r;
                }
                Data.DamageTypes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<EquipmentVariantRecord>("monster_equipment", delegate (EquipmentVariantRecord r, string header, DescriptorsCollection descs) {
                if (Data.MonsterEquipments.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<EquipmentVariantRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, EquipmentVariantRecord>)refInfo.GetValue(Data.MonsterEquipments);
                    records[r.Id] = r;
                }
                Data.MonsterEquipments.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<AiPresetRecord>("ai-presets", delegate (AiPresetRecord r, string header, DescriptorsCollection descs) {
                if (Data.AiPresets.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<AiPresetRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, AiPresetRecord>)refInfo.GetValue(Data.AiPresets);
                    records[r.Id] = r;
                }
                Data.AiPresets.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StatusEffectsRecord>("statuseffects", delegate (StatusEffectsRecord r, string header, DescriptorsCollection descs) {
                if (Data.StatusEffects.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StatusEffectsRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StatusEffectsRecord>)refInfo.GetValue(Data.StatusEffects);
                    records[r.Id] = r;
                }
                Data.StatusEffects.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FactionRecord>("factions", delegate (FactionRecord r, string header, DescriptorsCollection descs) {
                if (Data.Factions.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<FactionRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, FactionRecord>)refInfo.GetValue(Data.Factions);
                    records[r.Id] = r;
                }
                Data.Factions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryClassRecord>("mercenary_classes", delegate (MercenaryClassRecord r, string header, DescriptorsCollection descs) {
                if (Data.MercenaryClasses.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MercenaryClassRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MercenaryClassRecord>)refInfo.GetValue(Data.MercenaryClasses);
                    records[r.Id] = r;
                }
                Data.MercenaryClasses.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MercenaryProfileRecord>("mercenary_profiles", delegate (MercenaryProfileRecord r, string header, DescriptorsCollection descs) {
                if (Data.MercenaryProfiles.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MercenaryProfileRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MercenaryProfileRecord>)refInfo.GetValue(Data.MercenaryProfiles);
                    records[r.Id] = r;
                }
                Data.MercenaryProfiles.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<PerkRecord>("perks", delegate (PerkRecord r, string header, DescriptorsCollection descs) {
                if (Data.Perks.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<PerkRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, PerkRecord>)refInfo.GetValue(Data.Perks);
                    records[r.Id] = r;
                }
                Data.Perks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AllianceRecord>("alliances", delegate (AllianceRecord r, string header, DescriptorsCollection descs) {
                if (Data.Alliances.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<AllianceRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, AllianceRecord>)refInfo.GetValue(Data.Alliances);
                    records[r.Id] = r;
                }
                Data.Alliances.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionTemplate>("procmissiontemplates", delegate (ProcMissionTemplate r, string header, DescriptorsCollection descs) {
                if (Data.ProcMissionTemplates.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<ProcMissionTemplate>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, ProcMissionTemplate>)refInfo.GetValue(Data.ProcMissionTemplates);
                    records[r.Id] = r;
                }
                Data.ProcMissionTemplates.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationRecord>("stations", delegate (StationRecord r, string header, DescriptorsCollection descs) {
                if (Data.Stations.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StationRecord>)refInfo.GetValue(Data.Stations);
                    records[r.Id] = r;
                }
                Data.Stations.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<StoryMissionRecord>("storymissions", delegate (StoryMissionRecord r, string header, DescriptorsCollection descs) {
                if (Data.StoryMissions.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StoryMissionRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StoryMissionRecord>)refInfo.GetValue(Data.StoryMissions);
                    records[r.Id] = r;
                }
                Data.StoryMissions.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SpaceObjectRecord>("spaceobjects", delegate (SpaceObjectRecord r, string header, DescriptorsCollection descs) {
                if (Data.SpaceObjects.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<SpaceObjectRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, SpaceObjectRecord>)refInfo.GetValue(Data.SpaceObjects);
                    records[r.Id] = r;
                }
                Data.SpaceObjects.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<TileTransformationRecord>("tilesettransformation", delegate (TileTransformationRecord r, string header, DescriptorsCollection descs) {
                if (Data.TileTransformation.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<TileTransformationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, TileTransformationRecord>)refInfo.GetValue(Data.TileTransformation);
                    records[r.Id] = r;
                }
                Data.TileTransformation.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<FireModeRecord>("firemodes", delegate (FireModeRecord r, string header, DescriptorsCollection descs) {
                if (Data.Firemodes.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<FireModeRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, FireModeRecord>)refInfo.GetValue(Data.Firemodes);
                    records[r.Id] = r;
                }
                Data.Firemodes.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ItemExpireRecord>("itemexpire", delegate (ItemExpireRecord r, string header, DescriptorsCollection descs) {
                if (Data.ItemExpire.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<ItemExpireRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, ItemExpireRecord>)refInfo.GetValue(Data.ItemExpire);
                    records[r.Id] = r;
                }
                Data.ItemExpire.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<WoundSlotRecord>("woundslots", delegate (WoundSlotRecord r, string header, DescriptorsCollection descs) {
                if (Data.WoundSlots.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<WoundSlotRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, WoundSlotRecord>)refInfo.GetValue(Data.WoundSlots);
                    records[r.Id] = r;
                }
                Data.WoundSlots.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WoundRecord>("woundtypes", delegate (WoundRecord r, string header, DescriptorsCollection descs) {
                if (Data.Wounds.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<WoundRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, WoundRecord>)refInfo.GetValue(Data.Wounds);
                    records[r.Id] = r;
                }
                Data.Wounds.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ItemTransformationRecord>("itemtransformation", delegate (ItemTransformationRecord r, string header, DescriptorsCollection descs) {
                if (Data.ItemTransformation.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<ItemTransformationRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, ItemTransformationRecord>)refInfo.GetValue(Data.ItemTransformation);
                    records[r.Id] = r;
                }
                Data.ItemTransformation.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<WorkbenchReceiptRecord>("workbenchreceipts", delegate (WorkbenchReceiptRecord r, string header, DescriptorsCollection descs) {
                int index = Data.WorkbenchReceipts.FindIndex(x => x.OutputItem == r.OutputItem);
                if (index != -1) {
                    Data.WorkbenchReceipts[index] = r;
                }
                Data.WorkbenchReceipts.Add(r);
                r.GenerateId();
            }));
            _cfgLoader.AddParser(new TableParser<ItemProduceReceipt>("itemreceipts", delegate (ItemProduceReceipt r, string header, DescriptorsCollection descs) {
                int index = Data.ProduceReceipts.FindIndex(x => x.OutputItem == r.OutputItem);
                if (index != -1) {
                    Data.ProduceReceipts[index] = r;
                }
                Data.ProduceReceipts.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<BarterReceipt>("barter_receipts", delegate (BarterReceipt r, string header, DescriptorsCollection descs) {
                if (Data.BarterReceipts.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BarterReceipt>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BarterReceipt>)refInfo.GetValue(Data.BarterReceipts);
                    records[r.Id] = r;
                }
                Data.BarterReceipts.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<StationBarterRecord>("station_barter", delegate (StationBarterRecord r, string header, DescriptorsCollection descs) {
                if (Data.StationBarter.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<StationBarterRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, StationBarterRecord>)refInfo.GetValue(Data.StationBarter);
                    records[r.Id] = r;
                }
                Data.StationBarter.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<QmorphosRecord>("quazimorphosis", delegate (QmorphosRecord r, string header, DescriptorsCollection descs) {
                if (Data.Qmorphos.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<QmorphosRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, QmorphosRecord>)refInfo.GetValue(Data.Qmorphos);
                    records[r.Id] = r;
                }
                Data.Qmorphos.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<ProcMissionObjectiveRecord>("missionobjectives", delegate (ProcMissionObjectiveRecord r, string header, DescriptorsCollection descs) {
                int index = Data.ProcMissionObjectives.FindIndex(x => x.BeneficiaryFactionType == r.BeneficiaryFactionType && x.VictimFactionType == r.VictimFactionType);
                if (index != -1) {
                    Data.ProcMissionObjectives[index] = r;
                }
                Data.ProcMissionObjectives.Add(r);
            }));
            _cfgLoader.AddParser(new TableParser<AmmoRecord>("ammo", delegate (AmmoRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("meleeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.IsMelee = true;
                r.CanThrow = r.ThrowRange != 0;
                r.Range = 1;
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<WeaponRecord>("rangeweapons", delegate (WeaponRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.DefineClassTraits();
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MedkitRecord>("medkits", delegate (MedkitRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<FoodRecord>("food", delegate (FoodRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BackpackRecord>("backpacks", delegate (BackpackRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<VestRecord>("vests", delegate (VestRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<HelmetRecord>("helmets", delegate (HelmetRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ArmorRecord>("armors", delegate (ArmorRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<LeggingsRecord>("leggings", delegate (LeggingsRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<BootsRecord>("boots", delegate (BootsRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<RepairRecord>("repairs", delegate (RepairRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<SkullRecord>("skulls", delegate (SkullRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<QuasiArtifactRecord>("quasiartifacts", delegate (QuasiArtifactRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<GrenadeRecord>("grenades", delegate (GrenadeRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MineRecord>("mines", delegate (MineRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<AutomapRecord>("automaps", delegate (AutomapRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<ResurrectKitRecord>("resurrectkits", delegate (ResurrectKitRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TrashRecord>("trash", delegate (TrashRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<DatadiskRecord>("datadisks", delegate (DatadiskRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<TurretRecord>("turrets", delegate (TurretRecord r, string header, DescriptorsCollection descs) {
                if (Data.Items.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<BasePickupItemRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, BasePickupItemRecord>)refInfo.GetValue(Data.Items);
                    records[r.Id] = r;
                }
                Data.Items.AddRecord(r.Id, r);
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
                if (Data.MagnumPerks.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MagnumPerkRecord>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MagnumPerkRecord>)refInfo.GetValue(Data.MagnumPerks);
                    records[r.Id] = r;
                }
                Data.MagnumPerks.AddRecord(r.Id, r);
                r.ContentDescriptor = descs.GetDescriptor(r.Id);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumProjectParameter>("magnum_projects_params", delegate (MagnumProjectParameter r, string header, DescriptorsCollection descs) {
                if (Data.MagnumProjectParameters.GetRecord(r.Id) != null) {
                    FieldInfo refInfo = typeof(ConfigRecordCollection<MagnumProjectParameter>).GetField("_records", BindingFlags.NonPublic | BindingFlags.Instance);
                    var records = (Dictionary<string, MagnumProjectParameter>)refInfo.GetValue(Data.MagnumProjectParameters);
                    records[r.Id] = r;
                }
                Data.MagnumProjectParameters.AddRecord(r.Id, r);
            }));
            _cfgLoader.AddParser(new TableParser<MagnumProjectPrice>("magnum_projects_prices", delegate (MagnumProjectPrice r, string header, DescriptorsCollection descs) {
                int index = Data.MagnumProjectPrices.FindIndex(x => x.ProjectType == r.ProjectType);
                if (index != -1) {
                    Data.MagnumProjectPrices[index] = r;
                }
                Data.MagnumProjectPrices.Add(r);
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
            _cfgLoader.ProcessConfigFile(cfgPath);
        }
    }

    public class ModConfigLoader {
        private readonly List<IConfigParser> _parsers = new List<IConfigParser>();

        public event Action<string, DescriptorsCollection> OnDescriptorsLoaded = delegate {};

        public void AddParser(IConfigParser parser) {
            _parsers.Add(parser);
        }

        public void ProcessConfigFile(string configPath) {
            string configFile = File.ReadAllText(configPath);
            string[] configEntries = configFile.Split(new char[] { '\n' }, StringSplitOptions.None);
            bool parserFound = false;
            string parserType = string.Empty;
            DescriptorsCollection dCollection = null;
            IConfigParser cfgParser = null;
            foreach (string cfgEntry in configEntries) {
                if (!string.IsNullOrEmpty(cfgEntry) && (cfgEntry.Length < 2 || cfgEntry[0] != '/' || cfgEntry[1] != '/')) {
                    if (parserFound) {
                        parserFound = false;
                        cfgParser.ParseHeaders(this.SplitLine(cfgEntry));
                    } else if (cfgEntry.Contains("#end")) {
                        cfgParser = null;
                        dCollection = null;
                    } else if (cfgEntry[0] == '#') {
                        parserType = cfgEntry.Trim(new char[] { '\t', '\r', '\n', '#' });
                        foreach (IConfigParser refParser in this._parsers) {
                            if (refParser.ValidTableKey(parserType)) {
                                parserFound = true;
                                cfgParser = refParser;
                            }
                        }
                        dCollection = Resources.Load<DescriptorsCollection>("DescriptorsCollections/" + parserType + "_descriptors");
                        if (dCollection != null) {
                            this.OnDescriptorsLoaded(parserType, dCollection);
                        }
                    } else if (cfgParser != null) {
                        cfgParser.ParseLine(this.SplitLine(cfgEntry), parserType, dCollection);
                    }
                }
            }
        }

        private string[] SplitLine(string s) {
            return Regex.Replace(s, "\\n|\\r", string.Empty).Split('\t');
        }
    }
}
