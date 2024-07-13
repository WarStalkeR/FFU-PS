using MGSC;
using System.Collections.Generic;

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

        // Original function isn't displaying the grid correctly, if its height is bigger than 4 rows
        public static bool RefreshItemsList_FixMissUI(InventoryScreen __instance, bool refreshFloor) {
            ModLog.Info("Triggered: MGSC.InventoryScreen.RefreshItemsList()");
            Inventory inventory = __instance._creatures.Player.Inventory;
            __instance._backpackGridView.Initialize(inventory.BackpackStore);
            __instance._backpackGridView.InitFakeGrid(6, 4);
            __instance._backpackScrollbar.gameObject.SetActive(inventory.BackpackStore.Height > 4);
            __instance._backpackSlot.Initialize(inventory.BackpackSlot.First, inventory.BackpackSlot);
            SingletonMonoBehaviour<DungeonUI>.Instance.Hud.RefreshWeapon();
            SingletonMonoBehaviour<DungeonUI>.Instance.Hud.RefreshVest();
            SingletonMonoBehaviour<TooltipFactory>.Instance.HideTooltip();
            __instance.RefreshHelmetAndArmor();
            __instance.RefreshVest();
            __instance.RefreshWeapon();
            if (__instance._lastInteractObstacle != null) {
                __instance._lastInteractObstacle.RefreshVisual();
            }
            if (!__instance.IsViewActive) {
                return false;
            }
            ItemOnFloor orCreate = __instance._itemsOnFloor.GetOrCreate(__instance._creatures.Player.pos);
            if (refreshFloor) {
                if (orCreate.Storage.Empty && __instance._tabsView.HasTab(orCreate.Storage)) {
                    __instance._tabsView.RemoveTab(orCreate.Storage);
                    if (__instance._tabsView.TabsCount > 0) {
                        __instance._tabsView.SelectAndShowFirstTab();
                    }
                } else if (!orCreate.Storage.Empty && !__instance._tabsView.HasTab(orCreate.Storage)) {
                    ItemTab itemTab = __instance._tabsView.AddTab(__instance._itemsOnFloorView, orCreate.Storage);
                    if (__instance._tabsView.TabsCount == 1) {
                        itemTab.Select(val: true);
                    }
                }
            }
            __instance._tabsView.RefreshAllTabs();
            return false; // Original function is completely replaced
        }

        // Original function isn't displaying the grid correctly, if its height is bigger than 4 rows
        public static bool RefreshItemsList_FixShipUI(NoPlayerInventoryView __instance) {
            ModLog.Info("Triggered: MGSC.NoPlayerInventoryView.RefreshItemsList()");
            Inventory inventory = __instance._merc.Inventory;
            __instance._backpackGrid.Initialize(inventory.BackpackStore);
            __instance._vestGrid.Initialize(inventory.VestStore);
            __instance._backpackGrid.InitFakeGrid(6, 4); // 6, 5
            __instance._vestGrid.InitFakeGrid(8, 1); // 6, 1
            __instance._backpackScrollbar.gameObject.SetActive(inventory.BackpackStore.Height > 4);
            __instance._armorSlot.Initialize(inventory.ArmorSlot.First, inventory.ArmorSlot);
            __instance._helmetSlot.Initialize(inventory.HelmetSlot.First, inventory.HelmetSlot);
            __instance._leggingsSlot.Initialize(inventory.LeggingsSlot.First, inventory.LeggingsSlot);
            __instance._bootsSlot.Initialize(inventory.BootsSlot.First, inventory.BootsSlot);
            __instance._backpackSlot.Initialize(inventory.BackpackSlot.First, inventory.BackpackSlot);
            __instance._vestSlot.Initialize(inventory.VestSlot.First, inventory.VestSlot);
            __instance.RefreshWeapon();
            SingletonMonoBehaviour<TooltipFactory>.Instance.HideTooltip();
            return false; // Original function is completely replaced
        }
    }
}
