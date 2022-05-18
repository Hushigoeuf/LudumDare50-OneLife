using MoreMountains.Feedbacks;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Отправляет событие-запрос для воспроизведения звуку из Doozy Soundy.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")]
    [FeedbackPath(HGEditor.BASE + "/Doozy Soundy (SFX)")]
    public class MMFeedbackDoozySoundSFX : MMFeedback
    {
        public static bool FeedbackTypeAuthorized = true;

#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.SoundsColor;
#endif

        [Header(nameof(MMFeedbackDoozySoundSFX))]
        public HGDoozySFXEventTypes SoundEventType;

        public string SoundName;
        public float SoundDuration1;
        public float SoundDuration2;
        public string SoundControllerID;

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || !FeedbackTypeAuthorized) return;

            switch (SoundEventType)
            {
                case HGDoozySFXEventTypes.PlayOnceRequest:

                    if (!string.IsNullOrEmpty(SoundName))
                        HGDoozySFXEvent.Trigger(SoundEventType, SoundName);

                    break;

                case HGDoozySFXEventTypes.PlayRequest:

                    if (!string.IsNullOrEmpty(SoundName))
                        HGDoozySFXEvent.Trigger(SoundEventType, SoundName, SoundDuration1, 0, null, SoundControllerID);

                    break;

                case HGDoozySFXEventTypes.StopRequest:

                    if (!string.IsNullOrEmpty(SoundControllerID))
                        HGDoozySFXEvent.Trigger(SoundEventType, null, SoundDuration1, 0, null, SoundControllerID);

                    break;

                case HGDoozySFXEventTypes.PlayIntervalRequest:

                    if (!string.IsNullOrEmpty(SoundName))
                        HGDoozySFXEvent.Trigger(SoundEventType, SoundName, SoundDuration1, SoundDuration2);

                    break;
            }
        }
    }
}