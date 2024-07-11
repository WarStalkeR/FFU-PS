using HarmonyLib;
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
    }
}
