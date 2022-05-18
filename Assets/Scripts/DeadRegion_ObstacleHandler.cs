using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Обрабатывает столкновения маяков с препятствиями.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DeadRegion_ObstacleHandler))]
    public class DeadRegion_ObstacleHandler : HGMonoBehaviour
    {
        public DeadRegion_Point Target;
        public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }

        protected virtual void Colliding(GameObject collider)
        {
            if (!isActiveAndEnabled) return;
            if (!MMLayers.LayerInLayerMask(collider.layer, ObstacleLayerMask)) return;
            if (Time.time == 0f) return;
            
            Target.Free();
        }
    }
}