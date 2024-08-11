using MGSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MGSC.MagnumSelectItemToProduceWindow;
using UnityEngine.UI;
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
                                MercenarySystem.CloneMercenary(time, magnumProjects, mercenaries, datadiskComponent.UnlockId, cloneInstant: false);
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
                    __instance._creatures.Player.Mercenary.RaisePerkAction(PerkLevelUpActionType.PlaceTurret);
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

        // Item production rework to allow precise production hours and smart bonus application
        public static bool StartItemProduction_SmartPrecision(MagnumCargo magnumCargo, MagnumProjects projects, 
            MagnumSpaceship magnumSpaceship, SpaceTime time, ItemProduceReceipt receipt, int count, int lineIndex) {
            MagnumProject withModifications = projects.GetWithModifications(receipt.OutputItem);
            float prodlineProduceSpeedBonus = magnumSpaceship.ProdlineProduceSpeedBonus;
            int durationInHours = (int)Mathf.Min(
                Mathf.Max(receipt.ProduceTimeInHours + prodlineProduceSpeedBonus, 1f) * count,
                Mathf.Max(receipt.ProduceTimeInHours * count + prodlineProduceSpeedBonus, 1f));
            if (!magnumCargo.ItemProduceOrders.ContainsKey(lineIndex)) 
                magnumCargo.ItemProduceOrders[lineIndex] = new List<ItemProduceOrder>();
            ItemProduceOrder itemProduceOrder = new ItemProduceOrder {
                OrderId = ((withModifications != null) ? withModifications.CustomRecord.Id : receipt.OutputItem),
                Count = count,
                DurationInHours = durationInHours,
                StartTime = time.Time
            };
            itemProduceOrder.RequiredItems.AddRange(receipt.RequiredItems);
            magnumCargo.ItemProduceOrders[lineIndex].Add(itemProduceOrder);
            foreach (string requiredItem in receipt.RequiredItems) 
                MagnumCargoSystem.RemoveSpecificCargo(magnumCargo, requiredItem, (short)count);
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
                        case ReceiptCategory.Weapons: {
                            if (refRecord.GetRecord<WeaponRecord>() != null) {
                                ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
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
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
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
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
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
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
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
                                component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
                                component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                                component.transform.SetParent(__instance._panelsRoot, worldPositionStays: false);
                                component.transform.SetAsLastSibling();
                                __instance._receiptPanels.Add(component);
                            }
                            break;
                        }
                        default: {
                            ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                            component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, refRecipe);
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
