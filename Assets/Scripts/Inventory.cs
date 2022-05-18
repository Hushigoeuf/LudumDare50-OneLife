using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Дополняет оригинальный Inventory установкой кастомного кол-ва у предметов на старте.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(Inventory))]
    public class Inventory : MoreMountains.InventoryEngine.Inventory
    {
        [Header(nameof(Inventory))] public int[] StartQuantity;

        protected virtual void Start()
        {
            for (var i = 0; i < Content.Length; i++)
            {
                if (i >= StartQuantity.Length) continue;
                Content[i].Quantity = StartQuantity[i];
            }
        }
    }
}