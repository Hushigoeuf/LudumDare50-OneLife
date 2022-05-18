using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Переопределяет GameManager из TopDownEngine под собственные решения.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(MMGameManagerToHG))]
    public class MMGameManagerToHG : GameManager, HGEventListener<HGGameEvent>
    {
        [Header(nameof(MMGameManagerToHG))] public bool PauseOnFinished = true;
        public bool PauseOnGameOver;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.HGEventStartListening();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.HGEventStopListening();
        }

        public override void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            base.Pause(pauseMethod);

            if (Paused) HGGameEvent.Trigger(HGGameEventTypes.PauseRequest);
        }

        public override void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            base.UnPause(pauseMethod);

            if (!Paused) HGGameEvent.Trigger(HGGameEventTypes.UnPauseRequest);
        }

        public void OnHGEvent(HGGameEvent e)
        {
            switch (e.EventType)
            {
                case HGGameEventTypes.LevelFinished:
                    if (PauseOnFinished)
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    break;
                case HGGameEventTypes.GameOver:
                    if (PauseOnGameOver)
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    break;
            }
        }
    }
}