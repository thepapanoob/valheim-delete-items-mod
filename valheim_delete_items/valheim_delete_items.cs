using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace valheim_delete_items
{
    public class valheim_delete_items
    {
        public static void Main(string[] args)
        {
            new Thread(() =>
            {
                Harmony harmony = new Harmony("com.valheim.delete_items_from_inventory");

                bool allFound = false;
                do
                {
                    allFound = AccessTools.Method(typeof(InventoryGrid), "OnLeftClick") != null;
                    Thread.Sleep(1000);
                } while (allFound == false);

                harmony.PatchAll();
            }).Start();
        }
    }

    [HarmonyPatch]
    public class InventoryGridStubs
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(InventoryGrid), "GetButtonPos")]
        static public Vector2i GetButtonPos(InventoryGrid instance, GameObject go)
        {
            throw new NotImplementedException("It's a stub");
        }
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(InventoryGrid), "OnLeftClick")]
    static class InventoryGrid_OnLeftClickPatch
    {
        static AccessTools.FieldRef<InventoryGrid, Inventory> m_inventoryRef =
           AccessTools.FieldRefAccess<InventoryGrid, Inventory>("m_inventory");

        static bool Prefix(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            GameObject gameObject = clickHandler.gameObject;
            Vector2i buttonPos = InventoryGridStubs.GetButtonPos(__instance, gameObject);
            ItemDrop.ItemData itemAt = m_inventoryRef(__instance).GetItemAt(buttonPos.x, buttonPos.y);

            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                return true;

            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                return true;

            m_inventoryRef(__instance).RemoveItem(itemAt);
            return false;
        }
    }
}
