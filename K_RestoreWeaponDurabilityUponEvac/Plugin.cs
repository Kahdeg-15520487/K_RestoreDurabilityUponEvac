using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MGSC;
using System.Linq;
using UnityEngine;

namespace K_RestoreWeaponDurabilityUponEvac
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin ThisPlugin;
        public static ManualLogSource Log;

        private void Awake()
        {
            ThisPlugin = this;
            // Plugin startup logic
            Log = this.Logger;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(DungeonGameMode), "FinishGame")]
    public static class Patch_RestoreWeaponDurability
    {
        public static void Prefix(DungeonFinishedData dungeonFinishedData, DungeonGameMode __instance)
        {
            if (dungeonFinishedData.Reason == GameFinishedReason.FinishReached && string.IsNullOrEmpty(dungeonFinishedData.To.LocationUniqueId))
            {
                float weaponDurabilityMult = SingletonMonoBehaviour<ItemFactory>.Instance.WeaponDurabilityMult;
                __instance.Creatures.Player.Inventory.Slots.SelectMany(slot => slot.Items).ToList().ForEach(item =>
                {
                    BreakableItemComponent breakableItemComponent = item.Comp<BreakableItemComponent>();
                    if (item.Is<WeaponRecord>())
                    {
                        WeaponRecord weaponRecord = item.Record<WeaponRecord>();
                        breakableItemComponent.SetMaxDurability(Mathf.RoundToInt((float)weaponRecord.MaxDurability * weaponDurabilityMult), restoreDurability: false);
                        breakableItemComponent.Restore(breakableItemComponent.MaxDurability, 0, true);
                    }
                    else
                    {
                        breakableItemComponent.Restore(breakableItemComponent.MaxDurability, 0, true);
                    }
                });
            }
        }
    }
}