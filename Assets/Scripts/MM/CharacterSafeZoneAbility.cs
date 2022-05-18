using System;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Определяет способность персонажа использовать безопасные зоны.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(CharacterSafeZoneAbility))]
    public sealed class CharacterSafeZoneAbility : CharacterAbility
    {
        [NonSerialized] public bool SafeZoneActivated;
    }
}