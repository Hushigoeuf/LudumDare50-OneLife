using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Этот компонент должны содержать персонажи, которые спавнятся из портала.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(CharacterPortalTarget))]
    public class CharacterPortalTarget : CharacterAbility
    {
        /// <summary>
        /// Регистрируем объект в менеджере.
        /// </summary>
        protected override void OnEnable()
        {
            if (PortalManager.Instance != null)
                if (!PortalManager.Instance.Characters.Contains(gameObject))
                    PortalManager.Instance.Characters.Add(gameObject);
        }

        /// <summary>
        /// Снимаем регистрацию объекта из менеджера.
        /// </summary>
        protected override void OnDisable()
        {
            if (PortalManager.Instance != null)
                if (PortalManager.Instance.Characters.Contains(gameObject))
                    PortalManager.Instance.Characters.Remove(gameObject);
        }

        /// <summary>
        /// Вызывается каждый раз когда персонаж входит в зону портала.
        /// </summary>
        public virtual void PortalEnter(Portal target)
        {
            _controller.CollisionsOff();
        }

        /// <summary>
        /// Вызывается каждый раз когда персонаж выходит из зоны портала.
        /// </summary>
        public virtual void PortalExit(Portal target)
        {
            _controller.CollisionsOn();
        }
    }
}