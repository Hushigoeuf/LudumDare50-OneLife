using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Менеджер для управления всеми порталами на уровне.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(PortalManager))]
    [RequireComponent(typeof(MMMultipleObjectPooler))]
    public class PortalManager : HGSingletonMonoBehaviour<PortalManager>
    {
        /// Максимальное кол-во объектов, которые могут появится одновременно
        [HGShowInSettings] [HGBorders] [MinValue(0)]
        public int MaxCount;

        /// Имя звука, который воспроизводится в момент спавна
        [HGShowInSettings] [HGBorders] public string SFXSoundNameOnSpawn;

        /// Список всех объектов, которые появились на сцене
        [NonSerialized] public List<GameObject> Characters = new List<GameObject>();

        public virtual bool Spawn(Transform spawnPoint)
        {
            if (Characters.Count >= MaxCount) return false;

            var pooler = GetComponent<MMMultipleObjectPooler>();
            if (pooler == null) return false;

            var obj = pooler.GetPooledGameObject();
            if (obj == null) return false;

            obj.GetComponent<Character>().RespawnAt(spawnPoint,
                obj.GetComponent<CharacterOrientation2D>().InitialFacingDirection);

            if (!string.IsNullOrEmpty(SFXSoundNameOnSpawn))
                HGDoozySFXEvent.Trigger(HGDoozySFXEventTypes.PlayOnceRequest, SFXSoundNameOnSpawn);

            return true;
        }
    }
}