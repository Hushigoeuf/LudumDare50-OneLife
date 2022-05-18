using System.Collections;
using DG.Tweening;
using Doozy.Engine.Soundy;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Hushigoeuf
{
    /// <summary>
    /// Простой менеджер музыки, который работает благодаря Doozy Engine.
    /// Работает только в рамках фоновой музыки для простых решений.
    /// Добавляется в каждой основной сцене.
    ///
    /// P.S. Требует качественной переделки, но со своей задачей справляется (собрался во время LD49).
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(HGDoozyMusicManager))]
    public class HGDoozyMusicManager : HGSingletonMonoBehaviour<HGDoozyMusicManager>
    {
        /// Минимальный уровень звука в микшере
        protected const float MIN_VOLUME = -80;

        /// Максимальный уровень звука в микшере
        protected const float MAX_VOLUME = 20;

        protected const float DURATION_PER_COROUTINE = 1;

        /// Длительность перехода между треками по умолчанию
        protected const float DEFAULT_TRANSITION_DURATION = 1;

        /// Длительность одного трека по умолчанию
        protected const float DEFAULT_MAX_MUSIC_DURATION = 300;

        /// Если звук работает, то здесь находится основной контроллер
        protected static SoundyController currentController;

        /// Если звук работает, то здесь хранится имя трека
        protected static string currentSoundName;

        /// Если звук работает, то здесь хранится пройденная длительность
        protected static float currentSoundDuration;

        /// Временный контроллер, который нужен для смены треков
        protected static SoundyController switchController;

        /// Целевое имя трека из библиотеки DoozyUI
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ValueDropdown(nameof(_editorSoundNames))]
        [EnableIf(nameof(_editorSoundExists))]
#endif
        [HGShowInSettings]
        [HGBorders]
        [SerializeField]
        protected string _targetSoundName;

        /// Сменить трек на новый принудительно, если они не совпадают
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnableIf(nameof(_editorSoundExists))]
#endif
        [HGShowInSettings]
        [HGBorders]
        [SerializeField]
        protected bool _targetSoundForced = true;

        /// Воспироизвести трек на старте сцены
        [HGShowInSettings] [HGBorders] [SerializeField]
        protected bool _playOnStart = true;

        /// Сменить трек на старте сцены
        [HGShowInSettings] [HGBorders] [SerializeField]
        protected bool _switchOnStart;

        /// Заглушить звук на старте сцены (звук на этой сцене будет приглушен)
        [HGShowInSettings] [HGBorders] [SerializeField]
        protected bool _fadeOnStart;

        /// Автоматически меняет трек если длительность одного трека превысила лимит
        [HGShowInSettings] [HGBorders] [SerializeField]
        protected bool _automaticSwitchEnabled;

        /// Длительность воспроизведения трека
#if ODIN_INSPECTOR
        [MinValue(0)]
#endif
        [HGShowInSettings]
        [HGBorders]
        [SerializeField]
        protected float _playTransitionDuration = DEFAULT_TRANSITION_DURATION;

        /// Длительность смены трека на новый
#if ODIN_INSPECTOR
        [MinValue(0)]
#endif
        [HGShowInSettings]
        [HGBorders]
        [SerializeField]
        protected float _switchTransitionDuration = DEFAULT_TRANSITION_DURATION;

        /// Длительность остановки трека
#if ODIN_INSPECTOR
        [MinValue(0)]
#endif
        [HGShowInSettings]
        [HGBorders]
        [SerializeField]
        protected float _stopTransitionDuration = DEFAULT_TRANSITION_DURATION;

        /// Целевые настройки менеджера
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged(nameof(EditorOnSettingsChanged))]
#endif
        [HGShowInBindings]
        [HGBorders]
        [HGRequired]
        [SerializeField]
        protected HGDoozySoundSettings _targetSettings;

        protected Sequence _volumeSequence;

        /// Меняется ли громкость музыки в данный момент
        public virtual bool VolumeChanging => _volumeSequence != null;

        #region MONO_METHODS

        protected override void Start()
        {
            base.Start();

            if (currentController == null)
            {
                if (_playOnStart) Play();
            }
            else if (_targetSoundForced && currentSoundName != _targetSoundName)
            {
                Switch();
            }
            else if (_switchOnStart && TrySwitch())
            {
                TrySwitch();
            }

            if (currentController != null) PlayUnmuteVolume(_playTransitionDuration);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StartCoroutine(DurationCoroutine());
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StopCoroutine(DurationCoroutine());
        }

        protected virtual IEnumerator DurationCoroutine()
        {
            while (DURATION_PER_COROUTINE > 0)
            {
                yield return new WaitForSeconds(DURATION_PER_COROUTINE);

                currentSoundDuration++;

                if (_automaticSwitchEnabled)
                {
                    currentSoundDuration = 0;
                    Switch();
                }
            }
        }

        #endregion

        #region BASIC_METHODS

        /// <summary>
        /// Возвращает статус существования трека в библиотеке Doozy.
        /// </summary>
        public virtual bool IsSoundExists(string soundName)
        {
            if (!string.IsNullOrEmpty(soundName))
                return _targetSettings.MusicDatabase.SoundNames.Contains(soundName);
            return false;
        }

        /// <summary>
        /// Изменяет громкость музыки.
        /// </summary>
        public virtual bool SetVolume(float value)
        {
            if (VolumeChanging) return false;
            _targetSettings.MusicDatabase.OutputAudioMixerGroup.audioMixer.SetFloat(
                _targetSettings.MusicVolumeParameter, value);
            return true;
        }

        /// <summary>
        /// Снижает громкость до минимума.
        /// </summary>
        public virtual bool SetMuteVolume() => SetVolume(MIN_VOLUME);

        /// <summary>
        /// Возвращает громкость в нормальное состояние.
        /// </summary>
        public virtual bool SetUnmuteVolume()
        {
            if (!_fadeOnStart)
                return SetVolume(_targetSettings.DefaultMusicVolume);
            return SetVolume(_targetSettings.DefaultMusicVolume -
                             (_targetSettings.DefaultMusicVolume - MIN_VOLUME) * _targetSettings.FadeRatio);
        }

        /// <summary>
        /// Последовательно изменяет громкость с заданной длительностью.
        /// </summary>
        public virtual bool PlayVolume(float value, float duration, TweenCallback callback = null)
        {
            if (VolumeChanging) return false;

            _volumeSequence = DOTween.Sequence();
            _volumeSequence.Append(_targetSettings.MusicDatabase.OutputAudioMixerGroup.audioMixer
                .DOSetFloat(_targetSettings.MusicVolumeParameter, value, duration));
            _volumeSequence.OnComplete(() =>
            {
                _volumeSequence = null;
                if (callback != null) callback.Invoke();
            });

            return true;
        }

        /// <summary>
        /// Последовательно снижает громкость до минимума с заданной длительностью.
        /// </summary>
        public virtual bool PlayMuteVolume(float duration, TweenCallback callback = null) =>
            PlayVolume(MIN_VOLUME, duration, callback);

        /// <summary>
        /// Последовательно возвращает громкость в нормальное состояние с заданной длительностью.
        /// </summary>
        public virtual bool PlayUnmuteVolume(float duration, TweenCallback callback = null)
        {
            if (!_fadeOnStart)
                return PlayVolume(_targetSettings.DefaultMusicVolume, duration, callback);
            return PlayVolume(_targetSettings.DefaultMusicVolume -
                              (_targetSettings.DefaultMusicVolume - MIN_VOLUME) * _targetSettings.FadeRatio,
                duration, callback);
        }

        /// <summary>
        /// Останавливает любые изменения громкости, которые происходят в данный момент.
        /// </summary>
        public virtual bool StopVolumeChanging()
        {
            if (!VolumeChanging) return false;

            _volumeSequence.Kill(false);
            _volumeSequence = null;

            return true;
        }

        #endregion

        #region PLAY_METHODS

        /// <summary>
        /// Воспроизводит трек с заданным именем.
        /// Можно указать длительность воспроизведения.
        /// </summary>
        public virtual bool Play(string soundName, float duration)
        {
            if (currentController != null) return false;
            if (!IsSoundExists(soundName)) return false;

            currentController = SoundyManager.Play(_targetSettings.MusicDatabase.DatabaseName, soundName);
            currentSoundName = soundName;
            currentSoundDuration = 0;

            if (currentController != null)
            {
                if (VolumeChanging) StopVolumeChanging();
                if (duration > 0)
                {
                    SetMuteVolume();
                    PlayUnmuteVolume(duration);
                }
                else
                {
                    SetUnmuteVolume();
                }
            }

            return currentController != null;
        }

        /// <summary>
        /// Воспроизводит трек, который указан в параметрах менеджера.
        /// Можно указать длительность воспроизведения.
        /// </summary>
        public virtual bool Play(float duration) => Play(_targetSoundName, duration);

        public virtual bool Play() => Play(_playTransitionDuration);

        #endregion

        #region SWITCH

        /// <summary>
        /// Меняет текущий музыкальный трек на другой.
        /// Можно задать длительность перехода между треками.
        /// </summary>
        public virtual bool Switch(string soundName, float duration)
        {
            if (currentController == null) return false;
            if (!IsSoundExists(soundName)) return false;
            if (VolumeChanging) return false;

            void _()
            {
                if (switchController != null)
                {
                    switchController.Kill();
                    switchController = null;
                }

                switchController = currentController;
                switchController.Mute();

                currentController = SoundyManager.Play(_targetSettings.MusicDatabase.DatabaseName, soundName);
                currentSoundName = soundName;
                currentSoundDuration = 0;
            }

            if (duration > 0)
                return PlayMuteVolume(duration / 2f, () =>
                {
                    _();

                    PlayUnmuteVolume(duration / 2f);
                });
            else
                _();

            return true;
        }

        public virtual bool Switch(float duration) => Switch(_targetSoundName, duration);

        public virtual bool Switch() => Switch(_switchTransitionDuration);

        /// <summary>
        /// Возвращает текущий музыкальный трек на предыдущий.
        /// Можно задать длительность перехода между треками.
        /// </summary>
        public virtual bool SwitchBack(float duration)
        {
            if (switchController == null) return false;
            if (VolumeChanging) return false;

            void _()
            {
                if (currentController != null)
                {
                    currentController.Kill();
                    currentController = null;
                }

                currentController = switchController;
                currentController.Unmute();
            }

            if (duration > 0)
                return PlayMuteVolume(duration / 2f, () =>
                {
                    _();

                    PlayUnmuteVolume(duration / 2f);
                });
            else
                _();

            return true;
        }

        public virtual bool SwitchBack() => SwitchBack(_switchTransitionDuration);

        /// <summary>
        /// Пытается сменить текущий музыкальный трек на другой, если прошло достаточно много времени.
        /// Можно задать длительность перехода между треками.
        /// </summary>
        public virtual bool TrySwitch(float duration)
        {
            if (currentSoundDuration >= _targetSettings.MaxMusicDuration)
                return Switch(duration);
            return false;
        }

        public virtual bool TrySwitch() => TrySwitch(_switchTransitionDuration);

        #endregion

        #region STOP_METHODS

        /// <summary>
        /// Остановить текущий музыкальный трек.
        /// Можно задать длительность "затухания" звука.
        /// </summary>
        public virtual bool Stop(float duration)
        {
            if (currentController == null) return false;

            void _()
            {
                currentController.Kill();
                currentController = null;
            }

            if (VolumeChanging) StopVolumeChanging();

            if (duration > 0)
            {
                return PlayMuteVolume(duration, _);
            }
            else
            {
                _();
                SetMuteVolume();
            }

            return true;
        }

        public virtual bool Stop() => Stop(_stopTransitionDuration);

        /// <summary>
        /// Пытается остановить текущий музыкальный трек, если прошло достаточно много времени.
        /// Можно задать длительность "затухания" звука.
        /// </summary>
        public virtual bool TryStop(float duration)
        {
            if (currentSoundDuration >= _targetSettings.MaxMusicDuration)
                return Stop(duration);
            return false;
        }

        public virtual bool TryStop() => TryStop(_stopTransitionDuration);

        #endregion

#if UNITY_EDITOR && ODIN_INSPECTOR
        protected ValueDropdownList<string> _editorSoundNames;
        protected bool _editorSoundExists;

        [OnInspectorInit]
        protected virtual void EditorInitSoundNames()
        {
            _editorSoundNames = new ValueDropdownList<string>();

            EditorOnSettingsChanged();
        }

        /// <summary>
        /// Заполняет список музыкальных треков из библиотеки DoozyUI для инспектора.
        /// </summary>
        protected virtual void EditorOnSettingsChanged()
        {
            _editorSoundNames.Clear();
            if (_targetSettings != null)
                foreach (var i in _targetSettings.MusicDatabase.SoundNames)
                    _editorSoundNames.Add(i);
            _editorSoundExists = _editorSoundNames.Count != 0;
            if (!_editorSoundExists) _targetSoundName = null;
        }
#endif
    }
}