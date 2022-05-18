using System.Collections.Generic;
using DG.Tweening;
using Doozy.Engine.Soundy;
using UnityEngine;

namespace Hushigoeuf
{
    public enum HGDoozySFXEventTypes
    {
        PlayOnceRequest,
        PlayRequest,
        StopRequest,
        PlayIntervalRequest,
    }

    public struct HGDoozySFXEvent
    {
        public HGDoozySFXEventTypes EventType;
        public string TargetSoundName;
        public float Duration1;
        public float Duration2;
        public SoundyController TargetController;
        public string ControllerID;

        public HGDoozySFXEvent(HGDoozySFXEventTypes eventType, string targetSoundName, float duration1 = 0,
            float duration2 = 0, SoundyController targetController = null, string controllerID = null)
        {
            EventType = eventType;
            TargetSoundName = targetSoundName;
            Duration1 = duration1;
            Duration2 = duration2;
            TargetController = targetController;
            ControllerID = controllerID;
        }

        public void Trigger()
        {
            HGEventManager.TriggerEvent(this);
        }

        private static HGDoozySFXEvent e;

        public static void Trigger(HGDoozySFXEventTypes eventType, string targetSoundName, float duration1 = 0,
            float duration2 = 0, SoundyController targetController = null, string controllerID = null)
        {
            e.EventType = eventType;
            e.TargetSoundName = targetSoundName;
            e.Duration1 = duration1;
            e.Duration2 = duration2;
            e.TargetController = targetController;
            e.ControllerID = controllerID;

            e.Trigger();
        }
    }

    /// <summary>
    /// Простой менеджер SFX, который работает благодаря Doozy Engine.
    /// Так же принимает события чтобы воспроизводить SFX извне.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(HGDoozySFXManager))]
    public class HGDoozySFXManager : HGSingletonMonoBehaviour<HGDoozySFXManager>,
        HGEventListener<HGDoozySFXEvent>
    {
        /// <summary>
        /// Целевой файл с глобальными настройками.
        /// </summary>
        [HGShowInBindings] [HGBorders] [HGRequired]
        public HGDoozySoundSettings TargetSettings;

        public readonly Dictionary<string, SoundyController> Controllers = new Dictionary<string, SoundyController>();

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

        /// <summary>
        /// Возвращает информацию о корректности и существовании SFX в базе.
        /// </summary>
        public bool IsSoundExists(string soundName)
        {
            if (!string.IsNullOrEmpty(soundName))
                return TargetSettings.SFXDatabase.SoundNames.Contains(soundName);
            return false;
        }

        /// <summary>
        /// Воспроизводит одноразовый SFX.
        /// </summary>
        public void PlayOnce(string soundName)
        {
            if (!enabled) return;
            if (!IsSoundExists(soundName)) return;

            SoundyManager.Play(TargetSettings.SFXDatabase.DatabaseName, soundName);
        }

        /// <summary>
        /// Начинает воспроизводить SFX с эффектом "подъема".
        /// </summary>
        public virtual SoundyController Play(string soundName, float duration, string controllerID = null)
        {
            if (!enabled) return null;
            if (!IsSoundExists(soundName)) return null;
            if (!string.IsNullOrEmpty(controllerID))
                if (Controllers.ContainsKey(controllerID))
                    return Controllers[controllerID];

            var controller = SoundyManager.Play(TargetSettings.SFXDatabase.DatabaseName, soundName);
            if (controller == null) return null;

            if (duration > 0)
            {
                var vol = controller.AudioSource.volume;
                controller.AudioSource.volume = 0f;
                controller.AudioSource.DOFade(vol, duration);
            }

            if (!string.IsNullOrEmpty(controllerID))
                Controllers.Add(controllerID, controller);

            return controller;
        }

        /// <summary>
        /// Останавливает заданный (ранее воспроизводимый) SFX с эффектом затухания.
        /// </summary>
        public virtual void Stop(SoundyController controller, float duration)
        {
            if (controller == null) return;

            if (duration > 0)
                if (!controller.IsMuted && !controller.IsPaused)
                {
                    var vol = controller.AudioSource.volume;
                    controller.AudioSource.DOFade(0, duration).OnComplete(() =>
                    {
                        controller.AudioSource.volume = vol;
                        controller.Kill();
                    });
                    return;
                }

            {
                string controllerID = null;
                foreach (var cID in Controllers.Keys)
                {
                    if (Controllers[cID] != controller) continue;
                    controllerID = cID;
                    break;
                }

                if (!string.IsNullOrEmpty(controllerID))
                    Controllers.Remove(controllerID);
            }

            controller.Kill();
        }

        public virtual void Stop(string controllerID, float duration)
        {
            if (string.IsNullOrEmpty(controllerID)) return;
            if (!Controllers.ContainsKey(controllerID)) return;
            Stop(Controllers[controllerID], duration);
        }

        /// <summary>
        /// Воспроизводит SFX согласно заданным продолжительностью и переходом.
        /// </summary>
        public void PlayInterval(string soundName, float duration, float transitionDuration)
        {
            if (!enabled) return;
            if (!IsSoundExists(soundName)) return;

            var target = SoundyManager.Play(TargetSettings.SFXDatabase.DatabaseName, soundName);
            if (target == null) return;

            var startVolume = target.AudioSource.volume;

            var sequence = DOTween.Sequence();

            if (transitionDuration > 0)
            {
                target.AudioSource.volume = 0f;
                sequence.Append(target.AudioSource.DOFade(startVolume, transitionDuration));
            }

            if (duration > 0) sequence.AppendInterval(duration);

            sequence.OnComplete(() =>
            {
                if (transitionDuration > 0)
                {
                    target.AudioSource.DOFade(0, transitionDuration).OnComplete(() =>
                    {
                        target.AudioSource.volume = startVolume;
                        target.Kill();
                    });
                    return;
                }

                target.Kill();
            });
        }

        /// <summary>
        /// Прослушивает события извне, чтобы воспроизводить SFX "на расстоянии".
        /// </summary>
        public void OnHGEvent(HGDoozySFXEvent e)
        {
            switch (e.EventType)
            {
                case HGDoozySFXEventTypes.PlayOnceRequest:
                    PlayOnce(e.TargetSoundName);
                    break;

                case HGDoozySFXEventTypes.PlayRequest:
                    Play(e.TargetSoundName, e.Duration1, e.ControllerID);
                    break;

                case HGDoozySFXEventTypes.StopRequest:
                    if (e.TargetController != null)
                        Stop(e.TargetController, e.Duration1);
                    else if (!string.IsNullOrEmpty(e.ControllerID))
                        Stop(e.ControllerID, e.Duration1);
                    break;

                case HGDoozySFXEventTypes.PlayIntervalRequest:
                    PlayInterval(e.TargetSoundName, e.Duration1, e.Duration2);
                    break;
            }
        }
    }
}