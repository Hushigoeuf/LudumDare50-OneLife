using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Создает точку спавна для создания врагов.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(Portal))]
    public class Portal : HGMonoBehaviour
    {
        protected static int FreeEnemiesLayer = 23;

        /// Задержка перед началом спавна.
        [HGBorders] public float SpawnTimeDelay;

        /// Время между появлением врага в рандомизированном виде. 
        [HGBorders] public Vector2 SpawnTimeout;

        /// Локальные точки спавна врагов.
        public Transform[] SpawnPoints;

        protected int _currentSpawnPoint;

        protected virtual void OnEnable()
        {
            _currentSpawnPoint = 0;

            if (SpawnTimeDelay > 0) StartCoroutine(DelayCoroutine());
            else StartCoroutine(TimeoutCoroutine());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        protected virtual void OnTriggerEnter2D(Collider2D target)
        {
            Enter(target.gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D target)
        {
            Enter(target.gameObject);
        }

        /// <summary>
        /// Проверяет, входит ли объект в зону спавна.
        /// </summary>
        protected virtual void Enter(GameObject targetObject)
        {
            var target = targetObject.gameObject.MMGetComponentNoAlloc<CharacterPortalTarget>();
            if (target == null) return;

            target.PortalEnter(this);
        }

        protected virtual void OnTriggerExit2D(Collider2D target)
        {
            Exit(target.gameObject);
        }

        protected virtual void OnCollisionExit2D(Collision2D target)
        {
            Exit(target.gameObject);
        }

        /// <summary>
        /// Проверяет, выходит ли объект из зоны спавна.
        /// </summary>
        protected virtual void Exit(GameObject targetObject)
        {
            var target = targetObject.gameObject.MMGetComponentNoAlloc<CharacterPortalTarget>();
            if (target == null) return;

            target.PortalExit(this);
        }

        protected virtual IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(SpawnTimeDelay);

            StartCoroutine(TimeoutCoroutine());
        }

        protected virtual IEnumerator TimeoutCoroutine()
        {
            var time = Random.Range(SpawnTimeout.x, SpawnTimeout.y);

            while (time > 0)
            {
                yield return new WaitForSeconds(time);

                Spawn();

                time = Random.Range(SpawnTimeout.x, SpawnTimeout.y);
            }
        }

        /// <summary>
        /// Создает копию врага в локальной точке спавна.
        /// Для этого использует PortalManager для проверки лимита и непосредственного спавна.
        /// </summary>
        protected virtual void Spawn()
        {
            var spawnPoint = transform;
            if (SpawnPoints.Length != 0)
                spawnPoint = SpawnPoints[_currentSpawnPoint];
            if (PortalManager.Instance.Spawn(spawnPoint))
            {
                _currentSpawnPoint++;
                if (_currentSpawnPoint >= SpawnPoints.Length)
                    _currentSpawnPoint = 0;
            }
        }
    }
}