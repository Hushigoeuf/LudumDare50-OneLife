using System;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Устанавливает на маяк и обрабатывает взаимодействие с ним.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DeadRegion_Point))]
    public class DeadRegion_Point : ButtonActivated
    {
        /// Определяет необходимость поворачиваться в сторону родителя
        [Header(nameof(DeadRegion_Point))] public bool RotationToWeapon;

        /// Уничтожать ли маяк после создания мертвой зоны
        public bool DestroyOnWeaponUsed = true;

        [NonSerialized] public DeadRegion_Weapon CurrentWeapon;
        [NonSerialized] public int Index;
        [NonSerialized] public Transform TargetRotation;
        [NonSerialized] public DeadRegion_Trail Trail;

        private bool _initShowPrompt;

        protected virtual void Awake()
        {
            _initShowPrompt = ShowPromptWhenColliding;
        }

        public override void Initialization()
        {
            base.Initialization();

            ShowPromptWhenColliding = _initShowPrompt;
        }

        protected virtual void OnDisable()
        {
            CurrentWeapon = null;
            Index = -1;
            TargetRotation = null;
            if (Trail != null)
                Trail.HGSetActive(false);
            Trail = null;

            ShowPromptWhenColliding = false;
            if (_buttonPrompt != null)
                Destroy(_buttonPrompt.gameObject);
        }

        protected override void Update()
        {
            base.Update();

            if (RotationToWeapon)
                if (TargetRotation != null)
                {
                    var direction = TargetRotation.position - transform.position;
                    var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
                }
        }

        /// <summary>
        /// Добавляет маяк в существующую цепочку.
        /// </summary>
        protected override void ActivateZone()
        {
            base.ActivateZone();

            var character = _characterButtonActivation.GetComponent<Character>();
            if (character == null) return;

            var handle = character.FindAbility<CharacterHandleSecondaryWeapon>();
            if (handle == null) return;

            var weapon = handle.CurrentWeapon as DeadRegion_Weapon;
            if (weapon == null) return;

            if (CurrentWeapon == null)
            {
                if (weapon.OpenPoint(this) == -1) Destroy();
            }
            else
            {
                weapon.ClosePoint(this);
            }
        }

        public override void ShowPrompt()
        {
            if (CurrentWeapon != null)
                if (!CurrentWeapon.CheckCloseConditions(this))
                    return;

            base.ShowPrompt();
        }

        /// <summary>
        /// Проверяет налиичие персонажа в зоне маяка.
        /// </summary>
        public virtual bool TestForWeaponOverlap()
        {
            if (CurrentWeapon == null) return false;

            return _collidingObjects.Contains(CurrentWeapon.Owner.gameObject);
        }

        /// <summary>
        /// Освобождает маяк от всей существующей цепочки и уничтожает его.
        /// </summary>
        public virtual void Free()
        {
            if (CurrentWeapon != null)
                CurrentWeapon.Free(this);
        }

        /// <summary>
        /// Уничтожает маяк со всеми связанными с ним объектами.
        /// </summary>
        public virtual void Destroy()
        {
            if (!DestroyOnWeaponUsed)
            {
                CurrentWeapon = null;
                Index = -1;
                TargetRotation = null;
                if (Trail != null)
                    Trail.HGSetActive(false);
                Trail = null;

                return;
            }

            var t = GetComponent<MMPoolableObject>();
            if (t != null) t.Destroy();
            else Destroy(gameObject);
        }
    }
}