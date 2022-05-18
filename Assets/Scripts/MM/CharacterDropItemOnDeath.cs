using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Создает копию заданного объекта на месте смерти персонажа.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(CharacterDropItemOnDeath))]
    public class CharacterDropItemOnDeath : CharacterAbility
    {
        [Header(nameof(CharacterDropItemOnDeath))]
        public GameObject DropItem;

        protected override void OnDeath()
        {
            base.OnDeath();

            var item = Instantiate(DropItem);
            if (item == null) return;

            item.transform.position = MMMaths.Vector3ToVector2(_character.CharacterModel.transform.position);
            item.gameObject.SetActive(true);
        }
    }
}