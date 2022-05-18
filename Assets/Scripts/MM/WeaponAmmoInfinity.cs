using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Класс-заглушка для снятия ограничений стрельбы из TopDownEngine.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(WeaponAmmoInfinity))]
    public class WeaponAmmoInfinity : WeaponAmmo
    {
        protected override void Start()
        {
        }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void LoadOnStart()
        {
        }

        protected override void RefreshCurrentAmmoAvailable()
        {
        }

        public override bool EnoughAmmoToFire() => true;

        protected override void ConsumeAmmo()
        {
        }

        public override void FillWeaponWithAmmo()
        {
        }

        public override void EmptyMagazine()
        {
        }
    }
}