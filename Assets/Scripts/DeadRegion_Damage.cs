using System.Collections.Generic;
using mattatz.Triangulation2DSystem;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Устанавливается на объект мертвой зоны.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DeadRegion_Damage))]
    public class DeadRegion_Damage : HGMonoBehaviour
    {
        /// Минимальное кол-во маяков после которого урон снижается
        [HGShowInSettings] [MinValue(3)] public int MinPointCount;

        /// Размер урона на каждый лишний маяк, который идет в минус общему урону
        [HGShowInSettings] [MinValue(0)] public int DecreaseDamagePerPoint;

        [HGShowInBindings] public DamageOnTouch TargetDamage;

        protected int StartMinDamageCaused;
        protected int StartMaxDamageCaused;

        protected virtual void Awake()
        {
            if (TargetDamage == null)
                TargetDamage = GetComponent<DamageOnTouch>();
            if (TargetDamage != null)
            {
                StartMinDamageCaused = TargetDamage.MinDamageCaused;
                StartMaxDamageCaused = TargetDamage.MaxDamageCaused;
            }
        }

        /// <summary>
        /// Устанавливает мертвую зону по заданным маякам.
        /// </summary>
        public virtual bool Set(params Vector2[] points)
        {
            if (points == null) return false;
            if (points.Length < 3) return false;

            SetMesh(points);
            SetDamage(points);

            return SetCollider(points);
        }

        /// <summary>
        /// Устанавливает форму меша с помощью дополнения Triangulation2D.
        /// Источник: https://github.com/mattatz/unity-triangulation2D
        /// </summary>
        protected virtual void SetMesh(params Vector2[] points)
        {
            // construct Polygon2D 
            var polygon = Polygon2D.Contour(points);

            // construct Triangulation2D with Polygon2D and threshold angle (18f ~ 27f recommended)
            var triangulation = new Triangulation2D(polygon, 22.5f);

            // build a mesh from triangles in a Triangulation2D instance
            var mesh = triangulation.Build();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        /// <summary>
        /// Устанавливает форму коллайдера по маякам.
        /// </summary>
        protected virtual bool SetCollider(params Vector2[] points)
        {
            if (points.Length < 2) return false;

            var target = GetComponent<PolygonCollider2D>();
            if (target == null) return false;

            var newPoints = new List<Vector2>();
            for (var i = 0; i < points.Length; i++)
                newPoints.Add(points[i]);
            if (points.Length > 2)
                newPoints.Add(points[0]);

            target.SetPath(0, newPoints);

            return true;
        }

        /// <summary>
        /// Устанавливает урон от мертвой зоны в зависимости от кол-ва маяков.
        /// </summary>
        protected virtual void SetDamage(params Vector2[] points)
        {
            if (TargetDamage == null) return;

            var minDamageCaused = StartMinDamageCaused;
            var maxDamageCaused = StartMaxDamageCaused;

            if (points.Length > MinPointCount)
            {
                var decreaseDamageCaused = (points.Length - MinPointCount) * DecreaseDamagePerPoint;
                minDamageCaused -= decreaseDamageCaused;
                maxDamageCaused -= decreaseDamageCaused;
            }

            minDamageCaused = Mathf.Clamp(minDamageCaused, 0, maxDamageCaused);
            maxDamageCaused = Mathf.Clamp(maxDamageCaused, minDamageCaused, int.MaxValue);

            TargetDamage.MinDamageCaused = minDamageCaused;
            TargetDamage.MaxDamageCaused = maxDamageCaused;
        }
    }
}