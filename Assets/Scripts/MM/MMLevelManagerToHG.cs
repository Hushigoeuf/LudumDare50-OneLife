using System.Collections;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Переопределяет LevelManager из TopDownEngine под собственные решения.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(MMLevelManagerToHG))]
    public class MMLevelManagerToHG : LevelManager
    {
        protected override IEnumerator PlayerDeadCo()
        {
            yield return new WaitForSeconds(DelayBeforeDeathScreen);

            GUIManager.Instance.SetDeathScreen(true);

            HGGameEvent.Trigger(HGGameEventTypes.GameOverRequest);
        }
    }
}