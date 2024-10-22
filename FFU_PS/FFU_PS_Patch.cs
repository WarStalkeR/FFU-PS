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

protected override void OnContextCommandClick(CommonButton button)
{
	ContextMenuCommand bind = _commandBinds[button];
	switch (bind)
	{
	case ContextMenuCommand.Drop:
		PutItemInStorage(_storageForDrop, _item);
		break;
	case ContextMenuCommand.Repair:
		SingletonMonoBehaviour<UiCanvas>.Instance.DragController.StartRepairMode(_activeSlot);
		break;
	case ContextMenuCommand.Unequip:
	{
		ItemStorage itemStorage2 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		_merc.Inventory.Unequip(_item, itemStorage2);
		if (!itemStorage2.Empty)
		{
			PutItemInStorage(_storageForDrop, itemStorage2.First);
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
		ItemStorage itemStorage4 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		if (_merc.Inventory.Equip(_item, itemStorage4))
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
		if (!itemStorage4.Empty)
		{
			_storageForDrop.TryPutItem(itemStorage4.First, CellPosition.Zero);
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
				MercenarySystem.CloneMercenary(time, magnumProjects, mercenaries, datadiskComponent.UnlockId, cloneInstant: false);
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
		ItemStorage itemStorage3 = new ItemStorage(ItemStorageSource.ShipCargo, 2, 1);
		if (ItemInteraction.UnloadWeapon(_item, _merc?.Inventory, itemStorage3))
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.AmmoReceived);
		}
		else
		{
			SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
		}
		while (!itemStorage3.Empty)
		{
			PutItemInStorage(_storageForDrop, itemStorage3.First);
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
			_merc?.AddUltimatePerk(skullRecord.PerkId, skullRecord.Id);
			StorySystem.ApplySkull(storyTriggers, stations, missions, factions, spaceTime, travelMetadata, item);
			this.OnContextActionPerformed(bind, _item);
		});
		break;
	case ContextMenuCommand.RemoveSkull:
		SharedUi.ManageSkullWindow.Show(_item, delegate(BasePickupItem item)
		{
			item.Storage.Remove(item);
			SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<Mercenaries>().MercToUltimateSkulls.Remove(_merc.ProfileId);
			_merc?.RemoveUltimatePerk();
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
			_creatures.Player.Mercenary.RaisePerkAction(PerkLevelUpActionType.PlaceTurret);
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
	SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
	return false;
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

public static void StartItemProduction(MagnumCargo magnumCargo, MagnumProjects projects, MagnumSpaceship magnumSpaceship, SpaceTime time, ItemProduceReceipt receipt, int count, int lineIndex)
{
	MagnumProject withModifications = projects.GetWithModifications(receipt.OutputItem);
	float prodlineProduceSpeedBonus = magnumSpaceship.ProdlineProduceSpeedBonus;
	int durationInHours = (int)Mathf.Max(receipt.ProduceTimeInHours + prodlineProduceSpeedBonus, 1f) * count;
	if (!magnumCargo.ItemProduceOrders.ContainsKey(lineIndex))
	{
		magnumCargo.ItemProduceOrders[lineIndex] = new List<ItemProduceOrder>();
	}
	ItemProduceOrder itemProduceOrder = new ItemProduceOrder
	{
		OrderId = ((withModifications != null) ? withModifications.CustomRecord.Id : receipt.OutputItem),
		Count = count,
		DurationInHours = durationInHours,
		StartTime = time.Time
	};
	itemProduceOrder.RequiredItems.AddRange(receipt.RequiredItems);
	magnumCargo.ItemProduceOrders[lineIndex].Add(itemProduceOrder);
	foreach (string requiredItem in receipt.RequiredItems)
	{
		MagnumCargoSystem.RemoveSpecificCargo(magnumCargo, requiredItem, (short)count);
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
		case ReceiptCategory.Weapons:
			if (compositeItemRecord.GetRecord<WeaponRecord>() == null)
			{
				continue;
			}
			break;
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
		component.Initialize(_magnumCargo, _magnumSpaceship, _magnumProjects, produceReceipt);
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