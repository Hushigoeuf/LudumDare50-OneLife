using Sirenix.OdinInspector;
using UnityEngine;

namespace Hushigoeuf
{
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(TentacleGroup))]
    public class TentacleGroup : HGMonoBehaviour
    {
        [HGShowInBindings] [HGListDrawerSettings]
        public Tentacle[] Targets = new Tentacle[0];

#if UNITY_EDITOR
        [Button(nameof(EditorLoadTargetsInChildren))]
        private void EditorLoadTargetsInChildren()
        {
            Targets = GetComponentsInChildren<Tentacle>();
        }
#endif
    }
}