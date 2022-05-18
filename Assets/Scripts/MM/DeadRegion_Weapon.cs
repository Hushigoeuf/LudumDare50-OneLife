using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Оружие для мертвой зоны. Вместо снарядов устанавливает маяки,
    /// которые будут связаны между собой и в итоге образуют форму мертвой зоны.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DeadRegion_Weapon))]
    public class DeadRegion_Weapon : ProjectileWeapon
    {
        /// Минимальное кол-во маяков для формирования мертвой зоны
        [Header(nameof(DeadRegion_Weapon))] public int MinPointsToCreateRegion = 3;

        /// Максимальное кол-во маяков для формирования мертвой зоны
        public int MaxPointsToCreateRegion = 12;

        /// Размер физического поля для проверки нахождения объекта в зоне препятствий
        public float OverlapBoxSize = 1;

        /// Целевой слой коллайдеров, которые характеризуются как препятствия
        public LayerMask OverlopBoxLayer = LayerManager.ObstaclesLayerMask;

        public MMObjectPooler TrailPooler;
        public MMObjectPooler TrapPooler;

        protected List<DeadRegion_Point> _points = new List<DeadRegion_Point>();
        protected DeadRegion_Damage _damage;

        protected override void OnDisable()
        {
            base.OnDisable();

            for (var i = 0; i < _points.Count; i++)
                _points[i].Destroy();
            _points.Clear();
        }

        public override void WeaponUse()
        {
            if (!CheckOpenConditions()) return;
            for (var i = 0; i < _points.Count; i++)
                if (_points[i].TestForWeaponOverlap())
                    return;

            if (Physics2D.OverlapBox(transform.position,
                    new Vector2(1, 1), 0, OverlopBoxLayer) != null) return;

            base.WeaponUse();
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex,
            int totalProjectiles, bool triggerObjectActivation = true)
        {
            var result =
                base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

            var point = result.GetComponent<DeadRegion_Point>();
            point.AutoActivation = true;

            return result;
        }

        /// <summary>
        /// Проверяет требования для активации маяка.
        /// </summary>
        public virtual bool CheckOpenConditions(DeadRegion_Point target = null)
        {
            if (_points.Count >= MaxPointsToCreateRegion) return false;

            if (target != null)
                if (_points.Contains(target))
                    return false;

            return true;
        }

        /// <summary>
        /// Проверяет требования для закрытия маяков и создания мертвой зоны.
        /// </summary>
        public virtual bool CheckCloseConditions(DeadRegion_Point target = null)
        {
            if (_points.Count < MinPointsToCreateRegion) return false;

            if (target != null)
                if (target != _points[0])
                    return false;

            return true;
        }

        /// <summary>
        /// Активирует маяк и связывает его с другими.
        /// </summary>
        public virtual int OpenPoint(DeadRegion_Point target)
        {
            if (!CheckOpenConditions(target)) return -1;

            var trail = TrailPooler.GetPooledGameObject().GetComponent<DeadRegion_Trail>();
            trail.FirstTarget = Owner.CharacterModel.transform;
            trail.LastTarget = target.transform;
            trail.gameObject.SetActive(true);

            if (_points.Count != 0)
            {
                _points[_points.Count - 1].TargetRotation = target.transform;
                _points[_points.Count - 1].Trail.FirstTarget = target.transform;
            }

            target.TargetRotation = Owner.CharacterModel.transform;
            target.AutoActivation = false;

            _points.Add(target);

            target.CurrentWeapon = this;
            target.Index = _points.Count - 1;
            target.Trail = trail;

            return _points.Count - 1;
        }

        /// <summary>
        /// Создает мертвую зону если требования соблюдены.
        /// </summary>
        public virtual bool ClosePoint(DeadRegion_Point target)
        {
            if (!CheckCloseConditions(target)) return false;

            {
                if (_damage == null)
                    _damage = TrapPooler.GetPooledGameObject()?.GetComponent<DeadRegion_Damage>();
                if (_damage == null) return false;

                var points = new Vector2[_points.Count];
                for (var i = 0; i < _points.Count; i++)
                    points[i] = _points[i].transform.position;

                _damage.Set(points);
                _damage.gameObject.SetActive(true);
            }

            {
                _points[_points.Count - 1].Trail.FirstTarget = target.transform;

                for (var i = 0; i < _points.Count; i++)
                    _points[i].Destroy();
                _points.Clear();
            }

            _delayBetweenUsesCounter = TimeBetweenUses;
            WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);

            return true;
        }

        /// <summary>
        /// Разрывает связь с маяком и уничтожает его.
        /// </summary>
        public virtual void Free(DeadRegion_Point target)
        {
            if (!_points.Contains(target)) return;

            var targetIndex = -1;
            for (var i = 0; i < _points.Count; i++)
            {
                if (_points[i] != target) continue;
                targetIndex = i;
                break;
            }

            if (_points.Count > 1)
            {
                if (targetIndex == _points.Count - 1)
                {
                    _points[targetIndex - 1].Trail.FirstTarget = Owner.CharacterModel.transform;
                    _points[targetIndex - 1].Trail.ResetSegments();
                }
                else if (targetIndex > 0)
                {
                    _points[targetIndex - 1].Trail.FirstTarget = _points[targetIndex + 1].Trail.LastTarget;
                    _points[targetIndex - 1].Trail.ResetSegments();
                }
            }

            _points[targetIndex].Destroy();
            _points.RemoveAt(targetIndex);
        }
    }
}