using MGSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MGSC.MagnumSelectItemToProduceWindow;
using System;

namespace FFU_Phase_Shift {
    public static class ModPatch {
        // Original function refunds bullets as stacks and not as units, thus causes inventory overflow, beside being exploit
        public static bool CancelProject_ExploitFix(SpaceTime spaceTime, MagnumProjects projects, MagnumCargo magnumCargo, MagnumProject project) {
            project.IsInDevelopment = false;
            project.UpcomingModificationsCount = 0;
            project.UpcomingModifications.Clear();
            if (project.ModificationsCount == 0) {
                projects.Values.Remove(project);
            }
            foreach (KeyValuePair<string, int> lastDevelopmentPrice in project.LastDevelopmentPrices) {
                for (int i = 0; i < lastDevelopmentPrice.Value; i++) {
                    BasePickupItem item = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(lastDevelopmentPrice.Key);
                    item.StackCount = 1; // Enforce stack size to 1, since each unit is returned separately anyway
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, item);
                }
            }
            project.LastDevelopmentPrices.Clear();
            return false; // Original function is completely replaced
        }

        // Original function uses whole stack of automaps, instead of only one item, which is quite negative
        public static bool UseAutomap_UsageFix(MapController mapController, MapObstacles mapObstacles, Creatures creatures, BasePickupItem item) {
            AutomapRecord automapRecord = item.Record<AutomapRecord>();
            if (automapRecord == null) 
                return false;
            AutomapDescriptor automapDescriptor = item.View<AutomapDescriptor>();
            if (automapDescriptor.CustomUseSound != null) 
                SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(automapDescriptor.CustomUseSound);
            if (automapRecord.ExploreLevel) mapController.ExploreLevelFull();
            if (automapRecord.DestroyJammers) {
                List<MapObstacle> listJammers = new List<MapObstacle>();
                foreach (MapObstacle obstacle in mapObstacles.Obstacles) 
                    if (obstacle.Jammer != null)
                        listJammers.Add(obstacle);
                foreach (MapObstacle itemJammer in listJammers) {
                    DamageHitInfo hitInfo = new DamageHitInfo {
                        finalDmg = itemJammer.ObstacleHealth.MaxHealth + itemJammer.ObstacleHealth.Armor,
                        wasMiss = false
                    };
                    itemJammer.Injure(hitInfo, out var shouldRemoveDestroyed, creatures.Player.pos);
                    if (shouldRemoveDestroyed) mapObstacles.Remove(itemJammer);
                }
            }
            if (automapRecord.HighlightQuasimorphsDuration > 0) {
                foreach (Creature monster in creatures.Monsters) {
                    if (monster.Record.CreatureClass == CreatureClass.Quasimorph || monster.Record.CreatureClass == CreatureClass.QuasimorphBaron) {
                        monster.EffectsController.RemoveAllEffects<Spotted>();
                        monster.EffectsController.Add(new Spotted(automapRecord.HighlightQuasimorphsDuration));
                    }
                }
            }
            item.StackCount--;
            if (item.StackCount <= 0) item.Storage?.Remove(item);
            return false; // Original function is completely replaced
        }

        // Spaceship inventory context interaction improvement with initially non-stackable item stacks
        public static bool OnContextCommandClick_UsageFix(NoPlayerContextMenu __instance, CommonButton button) {
            bool wasTriggered = false;
            ContextMenuCommand contextCommand = __instance._commandBinds[button];
            switch (contextCommand) {
                case ContextMenuCommand.UnlockDatadisk: {
                    DatadiskComponent datadiskComponent = __instance._item.Comp<DatadiskComponent>();
                    DatadiskRecord datadiskRecord = __instance._item.Record<DatadiskRecord>();
                    SpaceTime time = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<SpaceTime>();
                    Mercenaries mercenaries = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>();
                    MagnumCargo magnumCargo = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<MagnumCargo>();
                    MagnumProjects magnumProjects = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<MagnumProjects>();
                    Statistics statistics = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Statistics>();
                    switch (datadiskRecord.UnlockType) {
                        case DatadiskUnlockType.Mercenary:
                            if (mercenaries.UnlockedMercenaries.IndexOf(datadiskComponent.UnlockId) == -1) {
                                mercenaries.UnlockedMercenaries.Add(datadiskComponent.UnlockId);
                                MercenarySystem.CloneMercenary(time, magnumProjects, mercenaries, 
									datadiskComponent.UnlockId, false, __instance._difficulty, __instance._perkFactory);
                                SharedUi.NotificationPanel.AddUnlockDatadiskNotify(__instance._item);
                                statistics.IncreaseStatistic(StatisticType.ChipUnlock);
                                __instance._item.StackCount--;
                                if (__instance._item.StackCount <= 0)
                                    __instance._item.Storage.Remove(__instance._item);
                                if (ModConfig.SortUnlockedTech)
                                    mercenaries.UnlockedMercenaries = mercenaries.UnlockedMercenaries
                                        .OrderBy(merc => merc).ToList();
                                wasTriggered = true;
                            }
                            break;
                        case DatadiskUnlockType.MercenaryClass:
                            if (mercenaries.UnlockedClasses.IndexOf(datadiskComponent.UnlockId) == -1) {
                                mercenaries.UnlockedClasses.Add(datadiskComponent.UnlockId);
                                SharedUi.NotificationPanel.AddUnlockDatadiskNotify(__instance._item);
                                statistics.IncreaseStatistic(StatisticType.ChipUnlock);
                                __instance._item.StackCount--;
                                if (__instance._item.StackCount <= 0)
                                    __instance._item.Storage.Remove(__instance._item);
                                if (ModConfig.SortUnlockedTech)
                                    mercenaries.UnlockedClasses = mercenaries.UnlockedClasses
                                        .OrderBy(clas => clas).ToList();
                                wasTriggered = true;
                            }
                            break;
                        case DatadiskUnlockType.ProductionItem: {
                            WeaponRecord record = (Data.Items.GetRecord(datadiskComponent.UnlockId) as CompositeItemRecord).GetRecord<WeaponRecord>();
                            if (magnumCargo.UnlockedProductionItems.IndexOf(datadiskComponent.UnlockId) == -1) {
                                magnumCargo.UnlockedProductionItems.Add(datadiskComponent.UnlockId);
                                SharedUi.NotificationPanel.AddUnlockDatadiskNotify(__instance._item);
                                statistics.IncreaseStatistic(StatisticType.ChipUnlock);
                                __instance._item.StackCount--;
                                if (__instance._item.StackCount <= 0)
                                    __instance._item.Storage.Remove(__instance._item);
                                if (record != null && !string.IsNullOrEmpty(record.RequiredAmmo) &&
                                    magnumCargo.UnlockedProductionItems.IndexOf(record.DefaultAmmoId) == -1) {
                                    magnumCargo.UnlockedProductionItems.Add(record.DefaultAmmoId);
                                }
                                if (ModConfig.SortUnlockedTech)
                                    magnumCargo.UnlockedProductionItems = magnumCargo.UnlockedProductionItems
                                        .OrderBy(item => ModTools.GetItemOrder(item))
                                        .ThenBy(item => item).ToList();
                                wasTriggered = true;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            if (wasTriggered) {
                SingletonMonoBehaviour<SpaceUI>.Instance?.ArsenalScreen?.RefreshView();
                __instance.gameObject.SetActive(value: false);
                SingletonMonoBehaviour<UiCanvas>.Instance.DragController.Pause(0.1f);
                return false; // Halt the execution and return.
            }
            return true; // Continue with original code.
        }

        // Mission inventory context interaction improvement with initially non-stackable item stacks
        public static bool InteractWithCharacter_UsageFix(InventoryScreen __instance, BasePickupItem item, bool spendTurn, ref bool __result) {
            if (!TurnSystem.CanProcessPlayerTurn(__instance._turnController, __instance._turnMetadata, __instance._creatures)) {
                SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                __result = false;
                return false;
            }
            if (item.Is<TurretRecord>()) {
                TurretRecord turretRecord = item.Record<TurretRecord>();
                CellPosition pos = __instance._creatures.Player.pos;
                int x = pos.X;
                int y = pos.Y;
                CellPosition.ChangePositionByCmd(__instance._creatures.Player.LookDirection, ref x, ref y);
                pos = new CellPosition(x, y);
                MapCell cell = __instance._mapGrid.GetCell(pos);
                if (cell != null && cell.IsFloor && !cell.isObjBlockPass && __instance._creatures.GetCreature(x, y) == null) {
                    __instance._creatures.Player.RaisePerkAction(PerkLevelUpActionType.PlaceTurret);
                    Monster monster = CreatureSystem.SpawnMonster(__instance._creatures, __instance._turnController, turretRecord.TurretMonsterId, pos);
                    SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.DeployTurret);
                    monster.CreatureAlliance = CreatureAlliance.PlayerAlliance;
                    item.StackCount--;
                    if (item.StackCount <= 0)
                        item.Storage?.Remove(item);
                    int turretHealth = __instance._creatures.Player.Mercenary.GetTurretHealth();
                    monster.OverallDmgMult = __instance._creatures.Player.Mercenary.GetTurretDamageMult();
                    if (turretHealth != 0) 
                        monster.Health.Reinitialize(monster.Health.MaxValue + turretHealth);
                    __instance.DragControllerRefreshCallback();
                    __result = true;
                    return false;
                }
                SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                __result = false;
                return false;
            }
            return true; // Continue with original code.
        }

        // Rebalances post-mission rewards: bullets grant 10% ~ 60% of stack, everything else grants 1 unit ~ 20% of stack
        public static bool MissionFinishedByPlayer_Rebalance(Missions missions, Stations stations, MagnumCargo magnumCargo, MagnumSpaceship magnumSpaceship, SpaceTime spaceTime, PopulationDebugData populationDebugData, TravelMetadata travelMetadata, Factions factions, ItemsPrices itemsPrices, Difficulty difficulty, Mission mission) {
            if (!mission.IsStoryMission) {
                MissionSystem.ProcessMissionSuccessActions(stations, spaceTime, 
				populationDebugData, travelMetadata, factions, itemsPrices, difficulty, mission);
            }
            MissionSystem.RemoveMission(missions, mission.StationId);
            Faction fBeneficiary = factions.Get(mission.BeneficiaryFactionId);
            Faction fVictim = factions.Get(mission.VictimFactionId);
            float repPlayer = fVictim.PlayerReputation;
            fBeneficiary.PlayerReputation += mission.BeneficiaryReputationDelta;
            fVictim.PlayerReputation += mission.VictimReputationDelta;
            fBeneficiary.PlayerReputation = Mathf.Clamp(fBeneficiary.PlayerReputation, Data.Global.MinReputation, Data.Global.MaxReputation);
            fVictim.PlayerReputation = Mathf.Clamp(fVictim.PlayerReputation, Data.Global.MinReputation, Data.Global.MaxReputation);
            int repThreshold = Data.Global.WipeRelationsReputationThreshold;
            if (fVictim.PlayerReputation < repThreshold && repPlayer >= repThreshold) {
                FactionSystem.WipeTradeRelations(stations, fVictim);
            }
            if (mission.IsStoryMission) {
                foreach (BasePickupItem storyPickup in mission.RewardItems) {
                    if (storyPickup.MaxStack > 1) {
                        if (storyPickup.Is<AmmoRecord>())
                            storyPickup.StackCount = (short)UnityEngine.Random.Range
                            (storyPickup.MaxStack * 0.1f, storyPickup.MaxStack * 0.6f);
                        else if (!storyPickup.Is<TrashRecord>())
							storyPickup.StackCount = (short)UnityEngine.Random.Range
                            (1f, Mathf.Max(1f, storyPickup.MaxStack * 0.2f));
                    }
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, storyPickup);
                }
                return false;
            }
            fBeneficiary.PlayerTradePoints += mission.RemainsRewardPoints;
            foreach (BasePickupItem missionPickup in mission.RewardItems) {
                if (missionPickup.MaxStack > 1) {
                    if (missionPickup.Is<AmmoRecord>())
                        missionPickup.StackCount = (short)UnityEngine.Random.Range
                        (missionPickup.MaxStack * 0.1f, missionPickup.MaxStack * 0.6f);
                    else missionPickup.StackCount = (short)UnityEngine.Random.Range
                        (1f, Mathf.Max(1f, missionPickup.MaxStack * 0.2f));
                }
                MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, missionPickup);
            }
            if (!magnumSpaceship.HasPurgeBrigadeDepartment) 
				return false;
            Station station = stations.Get(mission.StationId);
            ProcMissionTemplate record = Data.ProcMissionTemplates.GetRecord(station.Record.MissionTemplateId);
            List<ContentDropRecord> dropRecords = new List<ContentDropRecord>();
            foreach (string itemTablesId in record.ItemTablesIds) 
				dropRecords.AddRange(Data.LocationItemDrop.Get(itemTablesId));
            List<ItemRecord> salvageItems = new List<ItemRecord>();
            salvageItems.AddRange(DropManager.DropWithLimit(DropManager.GetItemsByType<TrashRecord>(dropRecords), (int)magnumSpaceship.PurgeBrigadeResourcesBonus));
            List<ItemRecord> armorItems = DropManager.GetItemsByType<ArmorRecord>(dropRecords);
            List<ItemRecord> weaponItems = DropManager.GetItemsByType<WeaponRecord>(dropRecords);
            List<ItemRecord> bonusEquipment = armorItems.Concat(weaponItems).ToList();
            salvageItems.AddRange(DropManager.DropWithLimit(bonusEquipment, (int)magnumSpaceship.PurgeBrigadeArmorWeaponBonus));
            List<ItemRecord> foodItems = DropManager.GetItemsByType<FoodRecord>(dropRecords);
            List<ItemRecord> medicalItems = DropManager.GetItemsByType<MedkitRecord>(dropRecords);
            List<ItemRecord> bonusProvisions = foodItems.Concat(medicalItems).ToList();
            salvageItems.AddRange(DropManager.DropWithLimit(bonusProvisions, (int)magnumSpaceship.PurgeBrigadeFoodMedsBonus));
            List<ItemRecord> ammoItems = DropManager.GetItemsByType<AmmoRecord>(dropRecords);
            List<ItemRecord> grenadeItems = DropManager.GetItemsByType<GrenadeRecord>(dropRecords);
            List<ItemRecord> bonusSupplies = ammoItems.Concat(grenadeItems).ToList();
            salvageItems.AddRange(DropManager.DropWithLimit(bonusSupplies, (int)magnumSpaceship.PurgeBrigadeAmmoGrenadesBonus));
            foreach (ItemRecord salvageItem in salvageItems) {
                BasePickupItem salvagePickup = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(salvageItem.Id);
                if (salvagePickup != null) {
                    if (salvagePickup.MaxStack > 1) {
                        if (salvagePickup.Is<AmmoRecord>())
                            salvagePickup.StackCount = (short)UnityEngine.Random.Range
                            (salvagePickup.MaxStack * 0.1f, salvagePickup.MaxStack * 0.6f);
                        else salvagePickup.StackCount = (short)UnityEngine.Random.Range
                            (1f, Mathf.Max(1f, salvagePickup.MaxStack * 0.2f));
                    }
                    mission.RewardItems.Add(salvagePickup);
                    salvagePickup.ExaminedItem = false;
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, salvagePickup);
                }
            }
            return false; // Original function is completely replaced
        }

        // Data disks rework to prioritize unlocking unknown data first
        public static void CreateComponent_BetterUnlock(ItemFactory __instance, PickupItem item, List<PickupItemComponent> itemComponents,
            BasePickupItemRecord itemRecord, bool randomizeConditionAndCapacity, bool isPrimary) {
            if (SingletonMonoBehaviour<DungeonGameMode>.Instance == null &&
                SingletonMonoBehaviour<SpaceGameMode>.Instance == null)
                return;
            DatadiskRecord datadiskRecord = itemRecord as DatadiskRecord;
            if (datadiskRecord != null) {
                var refDatadiskComponent = __instance._componentsCache.Find(x => x is DatadiskComponent) as DatadiskComponent;
                if (refDatadiskComponent != null) {
                    List<string> lockedIds = new List<string>();
                    bool isInDungeon = SingletonMonoBehaviour<DungeonGameMode>.Instance != null;
                    Mercenaries refMercs = isInDungeon ?
                        SingletonMonoBehaviour<DungeonGameMode>.Instance.Get<Mercenaries>() :
                        SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>();
                    MagnumCargo refCargo = isInDungeon ?
                        SingletonMonoBehaviour<DungeonGameMode>.Instance.Get<MagnumCargo>() :
                        SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<MagnumCargo>();
                    switch (datadiskRecord.UnlockType) {
                        case DatadiskUnlockType.ProductionItem: {
                            foreach (var unlockId in datadiskRecord.UnlockIds)
                                if (!refCargo.UnlockedProductionItems.Contains(unlockId))
                                    lockedIds.Add(unlockId);
                            break;
                        }
                        case DatadiskUnlockType.MercenaryClass: {
                            foreach (var unlockId in datadiskRecord.UnlockIds)
                                if (!refMercs.UnlockedClasses.Contains(unlockId))
                                    lockedIds.Add(unlockId);
                            break;
                        }
                        case DatadiskUnlockType.Mercenary: {
                            foreach (var unlockId in datadiskRecord.UnlockIds)
                                if (!refMercs.UnlockedMercenaries.Contains(unlockId))
                                    lockedIds.Add(unlockId);
                            break;
                        }
                    }
                    if (lockedIds.Count > 0) refDatadiskComponent.SetUnlockId(lockedIds[UnityEngine.Random.Range(0, lockedIds.Count)]);
                }
            }
        }

		// Allow additional items in the vest slots, such as mines, scanners and other devices
		public static void CreateComponent_VestCompatible(ItemFactory __instance, PickupItem item, List<PickupItemComponent> itemComponents,
			BasePickupItemRecord itemRecord, bool randomizeConditionAndCapacity, bool isPrimary) {
            if ((itemRecord is MineRecord || itemRecord is AutomapRecord || itemRecord is ExpGainerRecord || 
				itemRecord is ResurrectKitRecord || itemRecord is TurretRecord) && !HasCompOfType<AllowPutInVestItemComp>()) {
                itemComponents.Add(new AllowPutInVestItemComp());
            }
            bool HasCompOfType<T>() where T : PickupItemComponent {
                if (item.Comp<T>() != null) return true;
                foreach (PickupItemComponent itemComponent in itemComponents) 
					if (itemComponent is T) return true;
                return false;
            }
        }

		// Enables interaction with additional items that can be placed into vest slots
        public static void InteractVestSlot_VestCompatible(Creatures creatures, RaidMetadata raidMetadata, 
			SelectGrenadeTargetController selectGrenade, int slotIndex) {
            Player player = creatures.Player;
            BasePickupItem itemByIndex = player.Inventory.VestStore.GetItemByIndex(slotIndex - 1);
            if (itemByIndex.Is<AutomapRecord>()) {
                if (raidMetadata.StationVisit) {
                    SingletonMonoBehaviour<SoundController>.Instance
						.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
					return;
                }
                SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.InteractWithCharacter(itemByIndex, true);
            } else if (itemByIndex.Is<MineRecord>()) {
                if (raidMetadata.StationVisit) {
                    SingletonMonoBehaviour<SoundController>.Instance
						.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                    return;
                }
                SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.InteractWithCharacter(itemByIndex, true);
            } else if (itemByIndex.Is<TurretRecord>()) {
                if (raidMetadata.StationVisit) {
                    SingletonMonoBehaviour<SoundController>.Instance
						.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                    return;
                }
                SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.InteractWithCharacter(itemByIndex, true);
            } else if (itemByIndex.Is<ExpGainerRecord>()) {
                SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.InteractWithCharacter(itemByIndex, true);
            } else if (itemByIndex.Is<ResurrectKitRecord>()) {
                SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.InteractWithCharacter(itemByIndex, true);
            }
        }

        // Implements support for rendering wide (1x2) items in the vest slots
		public static bool Refresh_VestCompatible(SlotsView __instance, ItemStorage storage, int slotsCount) {
            __instance._storage = storage;
            __instance._slotsCount = slotsCount;
            __instance.FreeSlots();
            if (__instance.Centrate) {
                float oddOffset = (__instance._storage.Width % 2 != 0) ? 12f : 0f;
                __instance.transform.localPosition = new Vector3((__instance.MaxSlotWidth - __instance._storage.Width) / 2 * 25f + __instance.centrateOffsetX + oddOffset, __instance.transform.localPosition.y, 0f);
            }
            CellPosition cPos = new CellPosition(0, 0);
            for (int i = 0; i < slotsCount; i++) {
                ItemSlot component = __instance.SlotsPool.Take().GetComponent<ItemSlot>();
                BasePickupItem itemByIndex = storage.GetItemByIndex(i);
                float widthOffset = 0f;
                __instance._slots.Add(component);
                component.transform.SetParent(__instance.SlotsParent, false);
                if (itemByIndex != null) {
                    cPos = itemByIndex.InventoryPos;
                    component.Initialize(itemByIndex, itemByIndex.Storage);
                    if (itemByIndex.InventoryWidthSize == 2) {
						component._forceSize = ItemSlotForceSize.DoubleSize;
						component.InitSlotSize();
                    } else {
                        component._forceSize = ItemSlotForceSize.SingleSize;
                        component.InitSlotSize();
                    }
                } else {
                    storage.CalculatePositionByIndex(i, ref cPos);
                    component.InitializeEmpty(storage);
                    component._forceSize = ItemSlotForceSize.SingleSize;
                    component.InitSlotSize();
                }
                component.CellPos = cPos;
                if (!__instance.NoPositionControl) {
                    component.transform.localPosition = new Vector3(cPos.X * 26f + __instance.OffsetX + widthOffset, cPos.Y * -26 + __instance.OffsetY, 0f);
                } else {
                    component.transform.SetAsLastSibling();
                }
                component.CachedLocalPosition = component.transform.localPosition;
                if (itemByIndex != null && itemByIndex.InventoryWidthSize == 2) {
                    i++;
                }
            }
            return false; // Original function is completely replaced
        }

        // Item production rework to allow precise production hours and smart bonus application
        public static bool StartMagnumItemProduction_SmartPrecision(MagnumCargo magnumCargo, MagnumProjects projects, 
            MagnumSpaceship magnumSpaceship, SpaceTime time, Difficulty difficulty, ItemProduceReceipt receipt, int count, int lineIndex) {
            MagnumProject withModifications = projects.GetWithModifications(receipt.OutputItem);
            float prodlineProduceSpeedBonus = magnumSpaceship.ProdlineProduceSpeedBonus;
            int durationInHours = (int)Mathf.Min(
                Mathf.Max((receipt.ProduceTimeInHours + prodlineProduceSpeedBonus) * (1f / difficulty.Preset.MagnumCraftingTime), 1f) * count,
                Mathf.Max(receipt.ProduceTimeInHours * (1f / difficulty.Preset.MagnumCraftingTime) * count + prodlineProduceSpeedBonus, 1f));
            if (!magnumCargo.ItemProduceOrders.ContainsKey(lineIndex)) 
                magnumCargo.ItemProduceOrders[lineIndex] = new List<ProduceOrder>();
            ProduceOrder produceOrder = new ProduceOrder {
                OrderId = (withModifications != null) ? withModifications.CustomRecord.Id : receipt.OutputItem,
                Count = count,
                DurationInHours = durationInHours,
                StartTime = time.Time
            };
            foreach (ItemQuantity requiredItem in receipt.RequiredItems)
                for (int i = 0; i < requiredItem.Count; i++)
                    produceOrder.RequiredItems.Add(requiredItem.ItemId);
            magnumCargo.ItemProduceOrders[lineIndex].Add(produceOrder);
            foreach (ItemQuantity requiredItem in receipt.RequiredItems)
                MagnumCargoSystem.RemoveSpecificCargo(magnumCargo, requiredItem.ItemId, (short)(count * requiredItem.Count));
            return false; // Original function is completely replaced
        }

        // Rework recipe listing initialization to respect unlock order and/or sorting
        public static bool InitPanels_SmartSorting(MagnumSelectItemToProduceWindow __instance) {
            foreach (ItemReceiptPanel receiptPanel in __instance._receiptPanels) {
                receiptPanel.OnStartProduction -= __instance.ReceiptPanelOnStartProduction;
                __instance._panelsPool.Put(receiptPanel.gameObject);
            }
            __instance._receiptPanels.Clear();
            foreach (string unlockedItem in __instance._magnumCargo.UnlockedProductionItems) {
                var refRecipe = Data.ProduceReceipts.Find(x => x.OutputItem == unlockedItem);
                var refRecord = Data.Items.GetRecord(unlockedItem) as CompositeItemRecord;
                if (refRecipe != null && refRecord != null) {
                    switch (__instance._receiptCategory) {
                        case ReceiptCategory.RangeWeapons: {
                            if (refRecord.GetRecord<WeaponRecord>() != null && !refRecord.GetRecord<WeaponRecord>().IsMelee) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
									__instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        case ReceiptCategory.MeleeWeapons: {
                            if (refRecord.GetRecord<WeaponRecord>() != null && refRecord.GetRecord<WeaponRecord>().IsMelee) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship,
                                    __instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        case ReceiptCategory.Ammo: {
                            if (refRecord.GetRecord<AmmoRecord>() != null) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
									__instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        case ReceiptCategory.Armors: {
                            if (refRecord.GetRecord<ArmorRecord>() != null ||
                                refRecord.GetRecord<LeggingsRecord>() != null ||
                                refRecord.GetRecord<HelmetRecord>() != null ||
                                refRecord.GetRecord<BootsRecord>() != null) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
									__instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        case ReceiptCategory.Medicine: {
                            if (refRecord.GetRecord<MedkitRecord>() != null) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
									__instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        case ReceiptCategory.Other: {
                            if (refRecord.GetRecord<WeaponRecord>() == null &&
                                refRecord.GetRecord<AmmoRecord>() == null && 
                                refRecord.GetRecord<MedkitRecord>() == null && 
                                refRecord.GetRecord<ArmorRecord>() == null &&
                                refRecord.GetRecord<LeggingsRecord>() == null &&
                                refRecord.GetRecord<HelmetRecord>() == null &&
                                refRecord.GetRecord<BootsRecord>() == null) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
									__instance._magnumProjects, refRecipe, __instance._difficulty);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        default: {
                            ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                            component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, 
								__instance._magnumProjects, refRecipe, __instance._difficulty);
                            component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                            component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                            component.transform.SetAsLastSibling();
                            __instance._receiptPanels.Add(component);
                            break;
                        }
                    }
                }
            }
            __instance._scrollRect.normalizedPosition = Vector2.one;
            return false; // Original function is completely replaced
        }

        // Original function isn't displaying the grid correctly, if its height is bigger than 4 rows
        public static void Show_FixShipUI(ArsenalScreen __instance, Mercenary mercenary, bool showShuttle, Action closeCallback) {
            if (__instance._merc == null) return;
            ModLog.Info("Triggered: MGSC.ArsenalScreen.Show");
            // DUMP
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
                ModLog.Info($"FixShipUI DEBUG. Object: {obj.name} - {obj.GetType()} - {obj.transform.GetTransformPath()}");
            // INIT
            GameObject refBackpackView = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(
                x => x.transform.name == "Viewport" && x.transform.parent.name == "BackpackGrid");
            RectTransform refBackpackRect = refBackpackView.transform.parent.GetComponent<RectTransform>();
            // BEFORE
            if (refBackpackRect != null) {
                ModLog.Info($"FixShipUI DEBUG: {refBackpackRect.name}, " +
                    $"X:{refBackpackRect.rect.x}, Y:{refBackpackRect.rect.y}, " +
                    $"W:{refBackpackRect.rect.width}, H:{refBackpackRect.rect.height}");
            }
            // CHANGE
            refBackpackRect.rect.Set(refBackpackRect.rect.x, refBackpackRect.rect.y, 
                refBackpackRect.rect.width, Mathf.Min(refBackpackRect.rect.height, 102));
            // AFTER
            if (refBackpackRect != null) {
                ModLog.Info($"FixShipUI DEBUG: {refBackpackRect.name}, " +
                    $"X:{refBackpackRect.rect.x}, Y:{refBackpackRect.rect.y}, " +
                    $"W:{refBackpackRect.rect.width}, H:{refBackpackRect.rect.height}");
            }
        }
    }
}

// Reference Output: ILSpy v9.0.0.7660 / C# 11.0 / 2022.4
/*
public static void CancelProject(SpaceTime spaceTime, MagnumProjects projects, MagnumCargo magnumCargo, MagnumProject project)
{
	project.IsInDevelopment = false;
	project.UpcomingModificationsCount = 0;
	project.UpcomingModifications.Clear();
	if (project.ModificationsCount == 0)
	{
		projects.Values.Remove(project);
	}
	foreach (KeyValuePair<string, int> lastDevelopmentPrice in project.LastDevelopmentPrices)
	{
		for (int i = 0; i < lastDevelopmentPrice.Value; i++)
		{
			BasePickupItem item = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(lastDevelopmentPrice.Key);
			MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, item);
		}
	}
	project.LastDevelopmentPrices.Clear();
}

public static void UseAutomap(MapController mapController, MapObstacles mapObstacles, Creatures creatures, BasePickupItem item)
{
	AutomapRecord automapRecord = item.Record<AutomapRecord>();
	if (automapRecord == null)
	{
		return;
	}
	AutomapDescriptor automapDescriptor = item.View<AutomapDescriptor>();
	if (automapDescriptor.CustomUseSound != null)
	{
		SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(automapDescriptor.CustomUseSound);
	}
	if (automapRecord.ExploreLevel)
	{
		mapController.ExploreLevelFull();
	}
	if (automapRecord.DestroyJammers)
	{
		List<MapObstacle> list = new List<MapObstacle>();
		foreach (MapObstacle obstacle in mapObstacles.Obstacles)
		{
			if (obstacle.Jammer != null)
			{
				list.Add(obstacle);
			}
		}
		foreach (MapObstacle item2 in list)
		{
			DamageHitInfo hitInfo = new DamageHitInfo
			{
				finalDmg = item2.ObstacleHealth.MaxHealth + item2.ObstacleHealth.Armor,
				wasMiss = false
			};
			item2.Injure(hitInfo, out var shouldRemoveDestroyed, creatures.Player.pos);
			if (shouldRemoveDestroyed)
			{
				mapObstacles.Remove(item2);
			}
		}
	}
	if (automapRecord.HighlightQuasimorphsDuration > 0)
	{
		foreach (Creature monster in creatures.Monsters)
		{
			if (monster.Record.CreatureClass == CreatureClass.Quasimorph || monster.Record.CreatureClass == CreatureClass.QuasimorphBaron)
			{
				monster.EffectsController.RemoveAllEffects<Spotted>();
				monster.EffectsController.Add(new Spotted(automapRecord.HighlightQuasimorphsDuration));
			}
		}
	}
	item.Storage?.Remove(item);
}

protected override void OnContextCommandClick(CommonButton button, int clickCount)
{
	ContextMenuCommand bind = _commandBinds[button];
	switch (bind)
	{
	case ContextMenuCommand.Use:
		if (_item.Is<ExpGainerRecord>())
		{
			_merc.OnPerkLevelUp += MercOnOnPerkLevelUp;
			if (!ItemInteraction.UseExpGainer(_item, _merc, _perkFactory, spendCount: true))
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
			}
			_merc.OnPerkLevelUp -= MercOnOnPerkLevelUp;
		}
		break;
	case ContextMenuCommand.Drop:
		PutItemInStorage(_storageForDrop, _item);
		break;
	case ContextMenuCommand.Repair:
		SingletonMonoBehaviour<UiCanvas>.Instance.DragController.StartRepairMode(_activeSlot);
		break;
	case ContextMenuCommand.Unequip:
	{
		ItemStorage itemStorage4 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		_merc.Inventory.Unequip(_item, itemStorage4);
		if (!itemStorage4.Empty)
		{
			PutItemInStorage(_storageForDrop, itemStorage4.First);
		}
		break;
	}
	case ContextMenuCommand.Take:
		if (_merc.Inventory.TakeOrEquip(_item, putIfSlotBusy: true))
		{
			if (_item.Is<WeaponRecord>())
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EquipWeapon);
			}
			else
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.TakeItem);
			}
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		break;
	case ContextMenuCommand.Equip:
	{
		ItemStorage itemStorage3 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		if (_merc.Inventory.Equip(_item, itemStorage3))
		{
			if (_item.Is<WeaponRecord>())
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EquipWeapon);
			}
			else
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.TakeItem);
			}
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		if (!itemStorage3.Empty)
		{
			_storageForDrop.TryPutItem(itemStorage3.First, CellPosition.Zero);
		}
		break;
	}
	case ContextMenuCommand.Reload:
	{
		List<ItemStorage> list = new List<ItemStorage>();
		if (_merc != null)
		{
			list.AddRange(_merc.Inventory.Storages);
		}
		list.Add(_storageForDrop);
		bool wasReloadedFromVest;
		if (ReloadWeapon.CanReload(null, _item, list))
		{
			ReloadWeapon.Reload(_item, list, out wasReloadedFromVest, ignoreOneClipReload: true);
			WeaponDescriptor weaponDescriptor = _item.View<WeaponDescriptor>();
			SingletonMonoBehaviour<SoundController>.Instance.PlaySound(base.transform.position, weaponDescriptor.RandomReloadSoundBank);
		}
		else if (ReloadLauncher.CanReload(null, _item, list))
		{
			ReloadLauncher.Reload(_item, list, out wasReloadedFromVest, ignoreOneClipReload: true);
			WeaponDescriptor weaponDescriptor2 = _item.View<WeaponDescriptor>();
			SingletonMonoBehaviour<SoundController>.Instance.PlaySound(base.transform.position, weaponDescriptor2.RandomReloadSoundBank);
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		break;
	}
	case ContextMenuCommand.UnlockDatadisk:
	{
		DatadiskComponent datadiskComponent = _item.Comp<DatadiskComponent>();
		DatadiskRecord datadiskRecord = _item.Record<DatadiskRecord>();
		SpaceTime time = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<SpaceTime>();
		Mercenaries mercenaries = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>();
		MagnumCargo magnumCargo = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<MagnumCargo>();
		MagnumProjects magnumProjects = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<MagnumProjects>();
		Statistics statistics = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Statistics>();
		switch (datadiskRecord.UnlockType)
		{
		case DatadiskUnlockType.Mercenary:
			if (mercenaries.UnlockedMercenaries.IndexOf(datadiskComponent.UnlockId) == -1)
			{
				mercenaries.UnlockedMercenaries.Add(datadiskComponent.UnlockId);
				MercenarySystem.CloneMercenary(time, magnumProjects, mercenaries, datadiskComponent.UnlockId, cloneInstant: false, _difficulty, _perkFactory);
				SharedUi.NotificationPanel.AddUnlockDatadiskNotify(_item);
				statistics.IncreaseStatistic(StatisticType.ChipUnlock);
				_item.Storage.Remove(_item);
			}
			break;
		case DatadiskUnlockType.MercenaryClass:
			if (mercenaries.UnlockedClasses.IndexOf(datadiskComponent.UnlockId) == -1)
			{
				mercenaries.UnlockedClasses.Add(datadiskComponent.UnlockId);
				SharedUi.NotificationPanel.AddUnlockDatadiskNotify(_item);
				statistics.IncreaseStatistic(StatisticType.ChipUnlock);
				_item.Storage.Remove(_item);
			}
			break;
		case DatadiskUnlockType.ProductionItem:
		{
			WeaponRecord record = (Data.Items.GetRecord(datadiskComponent.UnlockId) as CompositeItemRecord).GetRecord<WeaponRecord>();
			if (magnumCargo.UnlockedProductionItems.IndexOf(datadiskComponent.UnlockId) == -1)
			{
				magnumCargo.UnlockedProductionItems.Add(datadiskComponent.UnlockId);
				SharedUi.NotificationPanel.AddUnlockDatadiskNotify(_item);
				statistics.IncreaseStatistic(StatisticType.ChipUnlock);
				_item.Storage.Remove(_item);
				if (record != null && !string.IsNullOrEmpty(record.RequiredAmmo) && magnumCargo.UnlockedProductionItems.IndexOf(record.DefaultAmmoId) == -1)
				{
					magnumCargo.UnlockedProductionItems.Add(record.DefaultAmmoId);
				}
			}
			break;
		}
		}
		break;
	}
	case ContextMenuCommand.UnloadAmmo:
	{
		ItemStorage itemStorage2 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		if (ItemInteraction.UnloadWeapon(_item, _merc?.Inventory, itemStorage2))
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.AmmoReceived);
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		while (!itemStorage2.Empty)
		{
			PutItemInStorage(_storageForDrop, itemStorage2.First);
		}
		break;
	}
	case ContextMenuCommand.Disassemble:
	case ContextMenuCommand.DisassembleAll:
	case ContextMenuCommand.DisassembleX1:
	{
		int toDisassembleCount = ((bind == ContextMenuCommand.DisassembleX1) ? 1 : (-1));
		if (ItemInteraction.Disassemble(_item, _merc?.Inventory, out var itemsWithoutStorage, toDisassembleCount))
		{
			foreach (BasePickupItem item in itemsWithoutStorage)
			{
				PutItemInStorage(_storageForDrop, item);
			}
			ItemStorage itemStorage = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
			ItemInteraction.UnloadWeapon(_item, _merc?.Inventory, itemStorage);
			while (!itemStorage.Empty)
			{
				PutItemInStorage(_storageForDrop, itemStorage.First);
			}
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		break;
	}
	case ContextMenuCommand.SplitStacks:
		StartSplitStacks();
		break;
	case ContextMenuCommand.SplitStacksConfirm:
		FinishSplitStacks();
		break;
	case ContextMenuCommand.ApplySkull:
		SharedUi.ManageSkullWindow.Show(_item, delegate(BasePickupItem item)
		{
			SkullRecord skullRecord = item.Record<SkullRecord>();
			StoryTriggers storyTriggers = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<StoryTriggers>();
			Stations stations = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Stations>();
			Mercenaries mercenaries2 = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>();
			TravelMetadata travelMetadata = SingletonMonoBehaviour<SpaceGameMode>.Instance.TravelMetadata;
			SpaceTime spaceTime = SingletonMonoBehaviour<SpaceGameMode>.Instance.SpaceTime;
			Factions factions = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Factions>();
			Missions missions = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Missions>();
			item.Storage.Remove(item);
			mercenaries2.MercToUltimateSkulls.Add(_merc.ProfileId, skullRecord.Id);
			if (_merc != null)
			{
				PerkSystem.AddUltimatePerk(skullRecord.PerkId, skullRecord.Id, _merc, _perkFactory);
			}
			StorySystem.ApplySkull(storyTriggers, stations, missions, factions, spaceTime, travelMetadata, item);
			SingletonMonoBehaviour<SoundController>.Instance.PlayUISound(SingletonMonoBehaviour<SoundsStorage>.Instance.SkullApply);
			this.OnContextActionPerformed(bind, _item);
		});
		break;
	case ContextMenuCommand.RemoveSkull:
		SharedUi.ManageSkullWindow.Show(_item, delegate(BasePickupItem item)
		{
			item.Storage.Remove(item);
			SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>().MercToUltimateSkulls.Remove(_merc.ProfileId);
			_merc?.RemoveUltimatePerk();
			SingletonMonoBehaviour<SoundController>.Instance.PlayUISound(SingletonMonoBehaviour<SoundsStorage>.Instance.SkullCancel);
			this.OnContextActionPerformed(bind, _item);
		});
		break;
	}
	this.OnContextActionPerformed(bind, _item);
	if (bind != ContextMenuCommand.SplitStacks)
	{
		base.gameObject.SetActive(value: false);
	}
	SingletonMonoBehaviour<UiCanvas>.Instance.DragController.Pause(0.1f);
	static void MercOnOnPerkLevelUp(PerkRecord obj)
	{
		SharedUi.NotificationPanel.AddPerkLevelUpNotify(obj);
	}
}

public bool InteractWithCharacter(BasePickupItem item, bool spendTurn)
{
	if (!TurnSystem.CanProcessPlayerTurn(_turnController, _turnMetadata, _creatures))
	{
		SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		return false;
	}
	if (item.Is<MedkitRecord>())
	{
		MedkitRecord medkitRecord = item.Record<MedkitRecord>();
		if (item.Is<FoodRecord>())
		{
			PutFoodInCharacter(item, spendTurn: false, spendCount: false);
		}
		if (item.Is<ExpGainerRecord>() && ItemInteraction.UseExpGainer(item, _merc, _perkFactory, spendCount: false))
		{
			DragControllerRefreshCallback();
		}
		if (medkitRecord.HealSpecificWound)
		{
			HealWound(item, spendTurn);
		}
		else
		{
			PutMedkitInCharacter(item, spendTurn);
		}
		return true;
	}
	if (item.Is<TurretRecord>())
	{
		TurretRecord turretRecord = item.Record<TurretRecord>();
		CellPosition pos = _creatures.Player.pos;
		int x = pos.X;
		int y = pos.Y;
		CellPosition.ChangePositionByCmd(_creatures.Player.LookDirection, ref x, ref y);
		pos = new CellPosition(x, y);
		MapCell cell = _mapGrid.GetCell(pos);
		if (cell != null && cell.IsFloor && !cell.isObjBlockPass && _creatures.GetCreature(x, y) == null)
		{
			_creatures.Player.RaisePerkAction(PerkLevelUpActionType.PlaceTurret);
			Monster monster = CreatureSystem.SpawnMonster(_creatures, _turnController, turretRecord.TurretMonsterId, pos);
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.DeployTurret);
			monster.CreatureAlliance = CreatureAlliance.PlayerAlliance;
			item.Storage?.Remove(item);
			int turretHealth = _creatures.Player.Mercenary.GetTurretHealth();
			monster.OverallDmgMult = _creatures.Player.Mercenary.GetTurretDamageMult();
			if (turretHealth != 0)
			{
				monster.Health.Reinitialize(monster.Health.MaxValue + turretHealth);
			}
			if (spendTurn)
			{
				PlayerInteractionSystem.EndPlayerTurn(_creatures, PlayerEndTurnContext.InventoryInteraction);
			}
			DragControllerRefreshCallback();
			return true;
		}
		SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		return false;
	}
	if (item.Is<MineRecord>())
	{
		CellPosition pos2 = _creatures.Player.pos;
		int x2 = pos2.X;
		int y2 = pos2.Y;
		CellPosition.ChangePositionByCmd(_creatures.Player.LookDirection, ref x2, ref y2);
		pos2 = new CellPosition(x2, y2);
		MapCell cell2 = _mapGrid.GetCell(pos2);
		if (cell2 != null && cell2.IsFloor && !cell2.isObjBlockPass && _creatures.GetCreature(x2, y2) == null && _mapEntities.Get(x2, y2) == null)
		{
			MineItemDescriptor mineItemDescriptor = item.View<MineItemDescriptor>();
			SingletonMonoBehaviour<SoundController>.Instance.PlaySound(_creatures.Player.GetSoundContext(), mineItemDescriptor.deploySoundBank);
			MineEntity entity = SingletonMonoBehaviour<MapEntityFactory>.Instance.CreateMine(item, pos2, 1f, isPlayerOwner: true);
			MapEntitiesSystem.SpawnEntity(_turnController, _mapEntities, _creatures, entity);
			if (item.StackCount > 1)
			{
				item.StackCount--;
			}
			else
			{
				item.StackCount = 0;
				item.Storage?.Remove(item);
			}
			if (spendTurn)
			{
				PlayerInteractionSystem.EndPlayerTurn(_creatures, PlayerEndTurnContext.InventoryInteraction);
			}
			DragControllerRefreshCallback();
			return true;
		}
		SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		return false;
	}
	if (item.Is<FoodRecord>())
	{
		PutFoodInCharacter(item, spendTurn);
		return true;
	}
	if (item.Is<AutomapRecord>())
	{
		ItemInteraction.UseAutomap(_mapController, _mapObstacles, _creatures, item);
		MapSystem.RefreshFogAndMinimap(_mapController, _creatures, _itemsOnFloor, _mapObstacles);
		DragControllerRefreshCallback();
		if (spendTurn)
		{
			PlayerInteractionSystem.EndPlayerTurn(_creatures, PlayerEndTurnContext.InventoryInteraction);
		}
		return true;
	}
	if (item.Is<ResurrectKitRecord>())
	{
		if (!ItemInteraction.UseResurrectKit(_turnController, _creatures, _mapObstacles, item, _creatures.Player))
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
			return false;
		}
		Hide();
		if (spendTurn)
		{
			PlayerInteractionSystem.EndPlayerTurn(_creatures, PlayerEndTurnContext.InventoryInteraction);
		}
		return true;
	}
	if (item.Is<ExpGainerRecord>())
	{
		if (!ItemInteraction.UseExpGainer(item, _merc, _perkFactory, spendCount: true))
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
			return false;
		}
		DragControllerRefreshCallback();
		if (spendTurn)
		{
			PlayerInteractionSystem.EndPlayerTurn(_creatures, PlayerEndTurnContext.InventoryInteraction);
		}
		return true;
	}
	SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
	return false;
}

public static void MissionFinishedByPlayer(Missions missions, Stations stations, MagnumCargo magnumCargo, MagnumSpaceship magnumSpaceship, SpaceTime spaceTime, PopulationDebugData populationDebugData, TravelMetadata travelMetadata, Factions factions, ItemsPrices itemsPrices, Difficulty difficulty, Mission mission)
{
	if (!mission.IsStoryMission)
	{
		ProcessMissionSuccessActions(stations, spaceTime, populationDebugData, travelMetadata, factions, itemsPrices, difficulty, mission);
	}
	RemoveMission(missions, mission.StationId);
	Faction faction = factions.Get(mission.BeneficiaryFactionId);
	Faction faction2 = factions.Get(mission.VictimFactionId);
	float playerReputation = faction2.PlayerReputation;
	faction.PlayerReputation += mission.BeneficiaryReputationDelta;
	faction2.PlayerReputation += mission.VictimReputationDelta;
	faction.PlayerReputation = Mathf.Clamp(faction.PlayerReputation, Data.Global.MinReputation, Data.Global.MaxReputation);
	faction2.PlayerReputation = Mathf.Clamp(faction2.PlayerReputation, Data.Global.MinReputation, Data.Global.MaxReputation);
	int wipeRelationsReputationThreshold = Data.Global.WipeRelationsReputationThreshold;
	if (faction2.PlayerReputation < (float)wipeRelationsReputationThreshold && playerReputation >= (float)wipeRelationsReputationThreshold)
	{
		FactionSystem.WipeTradeRelations(stations, faction2);
	}
	if (mission.IsStoryMission)
	{
		foreach (BasePickupItem rewardItem in mission.RewardItems)
		{
			MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, rewardItem);
		}
		return;
	}
	faction.PlayerTradePoints += mission.RemainsRewardPoints;
	foreach (BasePickupItem rewardItem2 in mission.RewardItems)
	{
		MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, rewardItem2);
	}
	if (!magnumSpaceship.HasPurgeBrigadeDepartment)
	{
		return;
	}
	Station station = stations.Get(mission.StationId);
	ProcMissionTemplate record = Data.ProcMissionTemplates.GetRecord(station.Record.MissionTemplateId);
	List<ContentDropRecord> list = new List<ContentDropRecord>();
	foreach (string itemTablesId in record.ItemTablesIds)
	{
		list.AddRange(Data.LocationItemDrop.Get(itemTablesId));
	}
	List<ItemRecord> list2 = new List<ItemRecord>();
	list2.AddRange(DropManager.DropWithLimit(DropManager.GetItemsByType<TrashRecord>(list), (int)magnumSpaceship.PurgeBrigadeResourcesBonus));
	List<ItemRecord> itemsByType = DropManager.GetItemsByType<AmmoRecord>(list);
	List<ItemRecord> itemsByType2 = DropManager.GetItemsByType<WeaponRecord>(list);
	List<ItemRecord> dropCollection = itemsByType.Concat(itemsByType2).ToList();
	list2.AddRange(DropManager.DropWithLimit(dropCollection, (int)magnumSpaceship.PurgeBrigadeArmorWeaponBonus));
	List<ItemRecord> itemsByType3 = DropManager.GetItemsByType<FoodRecord>(list);
	List<ItemRecord> itemsByType4 = DropManager.GetItemsByType<MedkitRecord>(list);
	List<ItemRecord> dropCollection2 = itemsByType3.Concat(itemsByType4).ToList();
	list2.AddRange(DropManager.DropWithLimit(dropCollection2, (int)magnumSpaceship.PurgeBrigadeFoodMedsBonus));
	List<ItemRecord> itemsByType5 = DropManager.GetItemsByType<AmmoRecord>(list);
	List<ItemRecord> itemsByType6 = DropManager.GetItemsByType<GrenadeRecord>(list);
	List<ItemRecord> dropCollection3 = itemsByType5.Concat(itemsByType6).ToList();
	list2.AddRange(DropManager.DropWithLimit(dropCollection3, (int)magnumSpaceship.PurgeBrigadeAmmoGrenadesBonus));
	foreach (ItemRecord item in list2)
	{
		BasePickupItem basePickupItem = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(item.Id);
		if (basePickupItem != null)
		{
			mission.RewardItems.Add(basePickupItem);
			basePickupItem.ExaminedItem = false;
			MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, basePickupItem);
		}
	}
}

private void CreateComponent(PickupItem item, List<PickupItemComponent> itemComponents, BasePickupItemRecord itemRecord, bool randomizeConditionAndCapacity, bool isPrimary)
{
	itemComponents.Clear();
	if (itemRecord is BreakableItemRecord record)
	{
		BreakableItemComponent breakableItemComponent = new BreakableItemComponent(record);
		itemComponents.Add(breakableItemComponent);
		if (itemRecord is WeaponRecord)
		{
			breakableItemComponent.SetMaxDurability(Mathf.RoundToInt((float)breakableItemComponent.MaxDurability * WeaponDurabilityMult), restoreDurability: true);
		}
		if (randomizeConditionAndCapacity)
		{
			breakableItemComponent.RandomizeCondition(40);
		}
	}
	if (itemRecord is IAllowInVestRecord && !HasCompOfType<AllowPutInVestItemComp>())
	{
		itemComponents.Add(new AllowPutInVestItemComp());
	}
	if (itemRecord is IExpirableRecord && !HasCompOfType<ExpireComponent>())
	{
		ItemExpireRecord record2 = Data.ItemExpire.GetRecord(itemRecord.Id);
		DateTime time = _state.Get<SpaceTime>().Time;
		if (record2 != null)
		{
			itemComponents.Add(new ExpireComponent(time.AddHours(record2.ExpiresAfterHours), isStarted: true));
		}
		else
		{
			itemComponents.Add(new ExpireComponent(time, isStarted: false));
		}
	}
	if (itemRecord is AmmoRecord ammoRecord)
	{
		int num = UnityEngine.Random.Range(ammoRecord.MinAmmoAmount, ammoRecord.MaxAmmoAmount + 1);
		StackableItemComponent stackableItemComponent = new StackableItemComponent(ammoRecord.MaxStack);
		if (randomizeConditionAndCapacity)
		{
			stackableItemComponent.Count = (short)num;
		}
		itemComponents.Add(stackableItemComponent);
		return;
	}
	IStackableRecord stackableRecord = itemRecord as IStackableRecord;
	if (stackableRecord != null && isPrimary)
	{
		short num2 = stackableRecord.MaxStack;
		if (itemRecord is FoodRecord || itemRecord is MedkitRecord || itemRecord is RepairRecord)
		{
			num2 += ConsumablesStackBonus;
		}
		StackableItemComponent stackableItemComponent2 = new StackableItemComponent(num2);
		stackableItemComponent2.Count = 1;
		itemComponents.Add(stackableItemComponent2);
	}
	if (itemRecord is WeaponRecord weaponRecord)
	{
		WeaponComponent weaponComponent = new WeaponComponent(weaponRecord);
		weaponComponent.CurrentAmmoType = Data.Items.GetSimpleRecord<AmmoRecord>(weaponRecord.DefaultAmmoId);
		weaponComponent.CurrentFireMode = Data.Firemodes.GetRecord(weaponRecord.Firemodes[0]);
		if (!string.IsNullOrEmpty(weaponRecord.OverrideAmmo))
		{
			weaponComponent.OverridenAmmo = Data.Items.GetSimpleRecord<AmmoRecord>(weaponRecord.OverrideAmmo);
		}
		else
		{
			weaponComponent.OverridenAmmo = weaponComponent.CurrentAmmoType;
		}
		if (randomizeConditionAndCapacity)
		{
			weaponComponent.RandomizeCapacity();
		}
		_componentsCache.Add(weaponComponent);
		if (weaponRecord.WeaponClass == WeaponClass.GrenadeLauncher)
		{
			LauncherComponent launcherComponent = new LauncherComponent();
			_componentsCache.Add(launcherComponent);
			for (int i = 0; i < weaponComponent.CurrentAmmo; i++)
			{
				launcherComponent.LoadedGrenadesIds.Add(weaponRecord.DefaultGrenadeId);
			}
		}
	}
	if (itemRecord is DatadiskRecord datadiskRecord)
	{
		DatadiskComponent datadiskComponent = new DatadiskComponent();
		datadiskComponent.SetUnlockId(datadiskRecord.UnlockIds[UnityEngine.Random.Range(0, datadiskRecord.UnlockIds.Count)]);
		_componentsCache.Add(datadiskComponent);
	}
	if (itemRecord is SkullRecord)
	{
		SkullComponent item2 = new SkullComponent();
		_componentsCache.Add(item2);
	}
	bool HasCompOfType<T>() where T : PickupItemComponent
	{
		if (item.Comp<T>() != null)
		{
			return true;
		}
		foreach (PickupItemComponent itemComponent in itemComponents)
		{
			if (itemComponent is T)
			{
				return true;
			}
		}
		return false;
	}
}

public static void InteractVestSlot(Creatures creatures, RaidMetadata raidMetadata, SelectGrenadeTargetController selectGrenade, int slotIndex)
{
	Player player = creatures.Player;
	if (player.Inventory.VestStore.GetItemByIndex(slotIndex - 1) == null)
	{
		SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.ButtonClick);
		return;
	}
	BasePickupItem itemByIndex = player.Inventory.VestStore.GetItemByIndex(slotIndex - 1);
	if (itemByIndex.Is<MedkitRecord>())
	{
		MedkitRecord medkitRecord = itemByIndex.Record<MedkitRecord>();
		if (itemByIndex.Is<FoodRecord>())
		{
			if (player.EffectsController.HasAnyEffect<WoundEffectNoFood>())
			{
				SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
				return;
			}
			SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.PutFoodInCharacter(itemByIndex, false, false);
		}
		if (medkitRecord.CanFixWound && !medkitRecord.FixAllWounds)
		{
			LimitsTooltip.LimitType limitType;
			if (!CanUseInventory(creatures, out limitType))
			{
				SingletonMonoBehaviour<TooltipFactory>.Instance.ShowLimitsTooltip(limitType);
				return;
			}
			SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.FixWound(itemByIndex, true);
		}
		else if (medkitRecord.HealSpecificWound)
		{
			LimitsTooltip.LimitType limitType2;
			if (!CanUseInventory(creatures, out limitType2))
			{
				SingletonMonoBehaviour<TooltipFactory>.Instance.ShowLimitsTooltip(limitType2);
				return;
			}
			SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.HealWound(itemByIndex, true);
		}
		else
		{
			SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.PutMedkitInCharacter(itemByIndex, true);
		}
		player.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
		EndPlayerTurn(creatures, PlayerEndTurnContext.InventoryInteraction);
	}
	else if (itemByIndex.Is<FoodRecord>())
	{
		if (player.EffectsController.HasAnyEffect<WoundEffectNoFood>())
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
			return;
		}
		SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.PutFoodInCharacter(itemByIndex, true);
		player.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
		EndPlayerTurn(creatures, PlayerEndTurnContext.InventoryInteraction);
	}
	else if (itemByIndex.Is<GrenadeRecord>())
	{
		if (raidMetadata.StationVisit)
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		else if (!selectGrenade.IsActive)
		{
			selectGrenade.StartSelection(slotIndex);
		}
	}
	else
	{
		if (!itemByIndex.Is<AmmoRecord>())
		{
			return;
		}
		if (player.Inventory.CurrentWeapon != null && ReloadWeapon.CanReload(player.Inventory, player.Inventory.CurrentWeapon, itemByIndex))
		{
			player.StartReload(player.Inventory.CurrentWeapon, itemByIndex);
			player.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
			SingletonMonoBehaviour<DungeonUI>.Instance.Hud.RefreshVest();
			SingletonMonoBehaviour<DungeonUI>.Instance.Hud.RefreshWeapon();
			if (SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.IsViewActive)
			{
				SingletonMonoBehaviour<DungeonUI>.Instance.InventoryScreen.DragControllerRefreshCallback();
			}
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
	}
}

public void Refresh(ItemStorage storage, int slotsCount)
{
	_storage = storage;
	_slotsCount = slotsCount;
	FreeSlots();
	if (Centrate)
	{
		float num = ((_storage.Width % 2 != 0) ? 12f : 0f);
		base.transform.localPosition = new Vector3((float)((MaxSlotWidth - _storage.Width) / 2) * 25f + centrateOffsetX + num, base.transform.localPosition.y, 0f);
	}
	CellPosition pos = new CellPosition(0, 0);
	for (int i = 0; i < slotsCount; i++)
	{
		ItemSlot component = SlotsPool.Take().GetComponent<ItemSlot>();
		BasePickupItem itemByIndex = storage.GetItemByIndex(i);
		float num2 = 0f;
		_slots.Add(component);
		component.transform.SetParent(SlotsParent, false);
		if (itemByIndex != null)
		{
			if (itemByIndex.InventoryWidthSize == 2)
			{
				num2 = 13f;
			}
			pos = itemByIndex.InventoryPos;
			component.Initialize(itemByIndex, itemByIndex.Storage);
		}
		else
		{
			storage.CalculatePositionByIndex(i, ref pos);
			component.InitializeEmpty(storage);
		}
		component.CellPos = pos;
		if (!NoPositionControl)
		{
			component.transform.localPosition = new Vector3((float)pos.X * 26f + OffsetX + num2, (float)(pos.Y * -26) + OffsetY, 0f);
		}
		else
		{
			component.transform.SetAsLastSibling();
		}
		component.CachedLocalPosition = component.transform.localPosition;
		if (itemByIndex != null && itemByIndex.InventoryWidthSize == 2)
		{
			i++;
		}
	}
}

public static void StartMagnumItemProduction(MagnumCargo magnumCargo, MagnumProjects projects, MagnumSpaceship magnumSpaceship, SpaceTime time, Difficulty difficulty, ItemProduceReceipt receipt, int count, int lineIndex)
{
	MagnumProject withModifications = projects.GetWithModifications(receipt.OutputItem);
	float prodlineProduceSpeedBonus = magnumSpaceship.ProdlineProduceSpeedBonus;
	int durationInHours = (int)Mathf.Max((receipt.ProduceTimeInHours + prodlineProduceSpeedBonus) * (1f / difficulty.Preset.MagnumCraftingTime), 1f) * count;
	if (!magnumCargo.ItemProduceOrders.ContainsKey(lineIndex))
	{
		magnumCargo.ItemProduceOrders[lineIndex] = new List<ProduceOrder>();
	}
	ProduceOrder produceOrder = new ProduceOrder
	{
		OrderId = ((withModifications != null) ? withModifications.CustomRecord.Id : receipt.OutputItem),
		Count = count,
		DurationInHours = durationInHours,
		StartTime = time.Time
	};
	foreach (ItemQuantity requiredItem in receipt.RequiredItems)
	{
		for (int i = 0; i < requiredItem.Count; i++)
		{
			produceOrder.RequiredItems.Add(requiredItem.ItemId);
		}
	}
	magnumCargo.ItemProduceOrders[lineIndex].Add(produceOrder);
	foreach (ItemQuantity requiredItem2 in receipt.RequiredItems)
	{
		MagnumCargoSystem.RemoveSpecificCargo(magnumCargo, requiredItem2.ItemId, (short)(count * requiredItem2.Count));
	}
}

private void InitPanels()
{
	foreach (ItemReceiptPanel receiptPanel in _receiptPanels)
	{
		receiptPanel.OnStartProduction -= ReceiptPanelOnStartProduction;
		_panelsPool.Put(receiptPanel.gameObject);
	}
	_receiptPanels.Clear();
	foreach (ItemProduceReceipt produceReceipt in Data.ProduceReceipts)
	{
		if (_magnumCargo.UnlockedProductionItems.IndexOf(produceReceipt.OutputItem) == -1)
		{
			continue;
		}
		CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(produceReceipt.OutputItem) as CompositeItemRecord;
		switch (_receiptCategory)
		{
		case ReceiptCategory.RangeWeapons:
		{
			WeaponRecord record = compositeItemRecord.GetRecord<WeaponRecord>();
			if (record == null || record.IsMelee)
			{
				continue;
			}
			break;
		}
		case ReceiptCategory.MeleeWeapons:
		{
			WeaponRecord record2 = compositeItemRecord.GetRecord<WeaponRecord>();
			if (record2 == null || !record2.IsMelee)
			{
				continue;
			}
			break;
		}
		case ReceiptCategory.Ammo:
			if (compositeItemRecord.GetRecord<AmmoRecord>() == null)
			{
				continue;
			}
			break;
		case ReceiptCategory.Armors:
			if (compositeItemRecord.GetRecord<ArmorRecord>() == null && compositeItemRecord.GetRecord<LeggingsRecord>() == null && compositeItemRecord.GetRecord<HelmetRecord>() == null && compositeItemRecord.GetRecord<BootsRecord>() == null)
			{
				continue;
			}
			break;
		case ReceiptCategory.Medicine:
			if (compositeItemRecord.GetRecord<MedkitRecord>() == null)
			{
				continue;
			}
			break;
		case ReceiptCategory.Other:
			if (compositeItemRecord.GetRecord<WeaponRecord>() != null || compositeItemRecord.GetRecord<MedkitRecord>() != null || compositeItemRecord.GetRecord<AmmoRecord>() != null || compositeItemRecord.GetRecord<ArmorRecord>() != null || compositeItemRecord.GetRecord<HelmetRecord>() != null || compositeItemRecord.GetRecord<LeggingsRecord>() != null || compositeItemRecord.GetRecord<BootsRecord>() != null)
			{
				continue;
			}
			break;
		}
		ItemReceiptPanel component = _panelsPool.Take().GetComponent<ItemReceiptPanel>();
		component.Initialize(_magnumCargo, _magnumSpaceship, _magnumProjects, produceReceipt, _difficulty);
		component.OnStartProduction += ReceiptPanelOnStartProduction;
		component.transform.SetParent(_panelsRoot, worldPositionStays: false);
		component.transform.SetAsLastSibling();
		_receiptPanels.Add(component);
	}
	_scrollRect.normalizedPosition = Vector2.one;
}

public void Show(Mercenary mercenary, bool showShuttle, Action closeCallback)
{
	_merc = mercenary;
	_closeCallback = closeCallback;
	if (_merc == null)
	{
		SingletonMonoBehaviour<ItemFactory>.Instance.SetConsumablesStackBonus(0);
		SingletonMonoBehaviour<ItemFactory>.Instance.SetWeaponDurabilityMult(1f);
		_inventoryWindow.SetActive(value: false);
		_inventoryView.gameObject.SetActive(value: false);
		_shuttleCargoStorageView.gameObject.SetActive(value: false);
		_vestGrid.gameObject.SetActive(value: false);
	}
	else
	{
		SingletonMonoBehaviour<ItemFactory>.Instance.SetConsumablesStackBonus(mercenary.ConsumablesStackBonus);
		SingletonMonoBehaviour<ItemFactory>.Instance.SetWeaponDurabilityMult(mercenary.WeaponDurabilityMult);
		_inventoryWindow.SetActive(value: true);
		_vestGrid.gameObject.SetActive(value: true);
		mercenary.Inventory.InitializeItemsOnFloor(this);
		bool num = _magnumSpaceship.GetDepartment<ShuttleCargoDepartment>().IsActiveDepartment();
		_inventoryTabsView.AddTab(this, _inventoryView, TabType.Inventory);
		if (num && showShuttle)
		{
			_shuttleCargoStorageView.Initialize(_magnumSpaceship);
			_inventoryTabsView.AddTab(this, _shuttleCargoStorageView, TabType.CargoShuttle);
		}
		_inventoryTabsView.SelectAndShowFirstTab();
		_inventoryTabsView.gameObject.SetActive(_inventoryTabsView.TabsCount > 1);
	}
	foreach (ItemStorage item in _magnumCargo.ShipCargo)
	{
		ItemInteraction.FixStacksCount(item);
		ItemInteraction.FixWeaponDurability(item);
	}
	ItemInteraction.FixStacksCount(_magnumCargo.RecyclingStorage);
	ItemInteraction.FixWeaponDurability(_magnumCargo.RecyclingStorage);
	Show();
}
*/