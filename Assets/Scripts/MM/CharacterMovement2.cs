using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(CharacterMovement2))]
    public class CharacterMovement2 : CharacterMovement
    {
        protected Vector2 _movement;

        public virtual float MovementVectorMultiplier
        {
            get
            {
                _movement.x = Mathf.Abs(_horizontalMovement);
                _movement.y = Mathf.Abs(_verticalMovement);
                _movement = _movement.normalized;
                return (_movement.x + _movement.y) / 2f;
            }
        }
    }
}