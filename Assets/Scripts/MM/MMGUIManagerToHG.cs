using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Переопределяет GUIManager из TopDownEngine под собственные решения.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(MMGUIManagerToHG))]
    public class MMGUIManagerToHG : GUIManager
    {
        public override void SetPauseScreen(bool state)
        {
            base.SetPauseScreen(state);

            if (state)
                HGUIRequestEvent.Trigger(HGUIRequestTypes.ShowPauseView);
            else
                HGUIRequestEvent.Trigger(HGUIRequestTypes.HidePauseView);
        }

        public override void SetDeathScreen(bool state)
        {
            base.SetDeathScreen(state);

            if (state)
                HGUIRequestEvent.Trigger(HGUIRequestTypes.ShowGameOverView);
            else
                HGUIRequestEvent.Trigger(HGUIRequestTypes.HideGameOverView);
        }
    }
}