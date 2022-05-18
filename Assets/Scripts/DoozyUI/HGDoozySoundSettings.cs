using Doozy.Engine.Soundy;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Hushigoeuf
{
    /// <summary>
    /// Глобальный настройки для музыки и SFX, которые работают благодаря Doozy Engine.
    /// Для работы так же требуется создать AudioMixer с параметрами громкости.
    /// </summary>
    [CreateAssetMenu(menuName = HGEditor.PATH_ASSET_CURRENT + nameof(HGDoozySoundSettings))]
    public class HGDoozySoundSettings : HGScriptableObject
    {
        /// Параметр громкости музыки из AudioMixer.
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged(nameof(EditorOnDatabaseChanged))]
#endif
        [HGShowInSettings]
        [HGBorders]
        [HGRequired]
        public string MusicVolumeParameter;

        /// Параметр громкости SFX из AudioMixer.
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged(nameof(EditorOnDatabaseChanged))]
#endif
        [HGShowInSettings]
        [HGBorders]
        [HGRequired]
        public string SFXVolumeParameter;

        /// Максимальная длительность трека музыки (после меняется на другой если это возможно).
#if ODIN_INSPECTOR
        [MinValue(0)]
#endif
        [HGShowInSettings]
        [HGBorders]
        public float MaxMusicDuration = 300;

        /// Коэффициент затухания, который можно применять к музыке.
        [HGShowInSettings] [HGBorders] [Range(0, 1)]
        public float FadeRatio = .5f;

        /// Целевая база музыки из Doozy Settings.
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged(nameof(EditorOnDatabaseChanged))]
#endif
        [HGShowInBindings]
        [HGBorders]
        [HGRequired]
        public SoundDatabase MusicDatabase;

        /// Целевая база SFX из Doozy Settings.
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged(nameof(EditorOnDatabaseChanged))]
#endif
        [HGShowInBindings]
        [HGBorders]
        [HGRequired]
        public SoundDatabase SFXDatabase;

        /// Громкость музыки по умолчанию (берется автоматически).
        [HGShowInDebug] [HGBorders] [HGReadOnly]
        public float DefaultMusicVolume;

        /// Громкость SFX по умолчанию (берется автоматически).
        [HGShowInDebug] [HGBorders] [HGReadOnly]
        public float DefaultSFXVolume;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnInspectorInit]
        protected virtual void EditorInitDatabases()
        {
            EditorOnDatabaseChanged();
        }

        protected virtual void EditorOnDatabaseChanged()
        {
            if (MusicDatabase != null)
            {
                DefaultMusicVolume = 0;
                if (!string.IsNullOrEmpty(MusicVolumeParameter))
                    MusicDatabase.OutputAudioMixerGroup.audioMixer.GetFloat(MusicVolumeParameter,
                        out DefaultMusicVolume);
            }

            if (SFXDatabase != null)
            {
                DefaultSFXVolume = 0;
                if (!string.IsNullOrEmpty(SFXVolumeParameter))
                    SFXDatabase.OutputAudioMixerGroup.audioMixer.GetFloat(SFXVolumeParameter,
                        out DefaultSFXVolume);
            }
        }
#endif
    }
}