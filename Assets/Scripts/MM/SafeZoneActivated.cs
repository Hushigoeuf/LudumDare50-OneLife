using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(SafeZoneActivated))]
    public sealed class SafeZoneActivated : ButtonActivated
    {
        protected override void ActivateZone()
        {
            base.ActivateZone();

            for (var i = 0; i < _collidingObjects.Count; i++)
            {
                var target = _collidingObjects[i].GetComponent<CharacterSafeZoneAbility>();
                if (target != null) target.SafeZoneActivated = true;
            }
        }

        public override void TriggerExitAction(GameObject collider)
        {
            base.TriggerExitAction(collider);

            var target = collider.GetComponent<CharacterSafeZoneAbility>();
            if (target != null) target.SafeZoneActivated = false;
        }
    }
}