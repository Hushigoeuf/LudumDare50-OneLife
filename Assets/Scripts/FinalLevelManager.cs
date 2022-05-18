using System.Collections;
using Cinemachine;
using DG.Tweening;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Менеджер для создания финальной сцены в последней уровне.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(FinalLevelManager))]
    public class FinalLevelManager : HGSingletonMonoBehaviour<FinalLevelManager>, HGEventListener<HGProgressEvent>
    {
        /// ID прогресса после которого последует финальная сцена
        [HGShowInSettings] [HGRequired] public string ProgressID;

        /// Задержка перед блокированием управления
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeBlocked;

        /// Задержка до начала работы камеры для этой сцены
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeCinemachineStarted;

        /// Задержка до завершения работы камеры
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeCinemachineFinished;

        /// Коэффициент деления скорости персонажа
        [HGShowInSettings] [MinValue(1)] public float CharacterSpeedDivider;

        /// На какое кол-во изменить лимит спавна для порталов в финальной сцене
        [HGShowInSettings] [MinValue(0)] public int SpawnNewRate;

        /// Задержка до начала появления анимации главного организма
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeBodyStarted;

        /// Длительность анимации появления тела организма
        [HGShowInSettings] [MinValue(0)] public float BodyDuration;

        /// Задержка до появления разломов на теле организма
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeCracks;

        /// Длительность анимации появления разломов
        [HGShowInSettings] [MinValue(0)] public float CracksDuration;

        /// Задержка между старым и новым разломами
        [HGShowInSettings] [MinValue(0)] public float DelayPerCracks;

        /// Задержка до завершения анимации главного организма
        [HGShowInSettings] [MinValue(0)] public float DelayBeforeBodyFinished;

        /// Коэффциент деления скорости покачивания у тентаклей организма
        [HGShowInSettings] [MinValue(1)] public float WiggleSpeedDivider;

        /// Имя финальной музыки, которые воспроизводится в этой сцене
        [HGShowInSettings] public string FinalSoundName;

        /// Длительность перехода между текущей и финальной музыкой
        [HGShowInSettings] [MinValue(0)] public float SwitchSoundDuration;

        [HGShowInBindings] public CinemachineVirtualCamera VirtualCamera;
        [HGShowInBindings] public SpriteRenderer Body1;
        [HGShowInBindings] public SpriteRenderer Body2;
        [HGShowInBindings] public SpriteRenderer Body3;
        [HGShowInBindings] public SpriteRenderer Cracks1;
        [HGShowInBindings] public SpriteRenderer Cracks2;
        [HGShowInBindings] public SpriteRenderer Cracks3;
        [HGShowInBindings] public Tentacle[] Tentacles;

        protected Character _character;
        protected float _startBody1Alpha;
        protected float _startBody2Alpha;
        protected float _startBody3Alpha;
        protected float _startCracks1Alpha;
        protected float _startCracks2Alpha;
        protected float _startCracks3Alpha;

        protected override void Awake()
        {
            base.Awake();

            VirtualCamera.HGSetActive(false);
            Body1.HGSetActive(false);
            Body2.HGSetActive(false);
            Body3.HGSetActive(false);
            Cracks1.HGSetActive(false);
            Cracks2.HGSetActive(false);
            Cracks3.HGSetActive(false);
        }

        protected override void Start()
        {
            base.Start();

            _startBody1Alpha = Body1.color.a;
            _startBody2Alpha = Body2.color.a;
            _startBody3Alpha = Body3.color.a;
            _startCracks1Alpha = Cracks1.color.a;
            _startCracks2Alpha = Cracks2.color.a;
            _startCracks3Alpha = Cracks3.color.a;

            Body1.DOFade(0, 0);
            Body2.DOFade(0, 0);
            Body3.DOFade(0, 0);
            Cracks1.DOFade(0, 0);
            Cracks2.DOFade(0, 0);
            Cracks3.DOFade(0, 0);
        }

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

        protected virtual IEnumerator ExecuteCoroutine()
        {
            Execute(0);

            yield return null;

            Execute(1);

            if (DelayBeforeBlocked > 0)
                yield return new WaitForSeconds(DelayBeforeBlocked);

            Execute(2);

            if (DelayBeforeCinemachineStarted > 0)
                yield return new WaitForSeconds(DelayBeforeCinemachineStarted);

            Execute(3);

            if (DelayBeforeCinemachineFinished > 0)
                yield return new WaitForSeconds(DelayBeforeCinemachineFinished);

            Execute(4);
        }

        /// <summary>
        /// Выполняет действия на основе заданного индекса.
        /// </summary>
        protected virtual void Execute(int step)
        {
            if (_character == null) return;

            switch (step)
            {
                case 0:

                    // Отключаем получение урона для основного персонажа
                    var health = _character.GetComponent<Health>();
                    if (health != null) health.DamageDisabled();

                    // Воспроизводим финальную музыку
                    if (!string.IsNullOrEmpty(FinalSoundName))
                        if (HGDoozyMusicManager.Instance != null)
                            HGDoozyMusicManager.Instance.Switch(FinalSoundName, SwitchSoundDuration);

                    break;

                case 1:

                    // Меняем слой основного персонажа на вражеский
                    _character.gameObject.layer = 13; // Enemies

                    // Отключаем основное и дополнительное оружие у персонажа
                    var handleWeapon = _character.FindAbility<CharacterHandleWeapon>();
                    if (handleWeapon != null) handleWeapon.ChangeWeapon(null, null);
                    handleWeapon = _character.FindAbility<CharacterHandleSecondaryWeapon>();
                    if (handleWeapon != null) handleWeapon.ChangeWeapon(null, null);

                    // Отключаем воспроизведение звуков
                    if (HGDoozySFXManager.Instance != null)
                        HGDoozySFXManager.Instance.enabled = false;

                    break;

                case 2:

                    // Включаем AI и отключаем управление для персонажа
                    _character.CharacterType = Character.CharacterTypes.AI;
                    _character.SetPlayerID("FinalPlayer");

                    // Уменьшаем скорость движения персонажа
                    var movement = _character.FindAbility<CharacterMovement>();
                    if (movement != null)
                    {
                        movement.WalkSpeed /= CharacterSpeedDivider;
                        movement.ResetSpeed();
                    }

                    // Скрываем интерфейс
                    HGUIRequestEvent.Trigger(HGUIRequestTypes.HideHUDView);

                    break;

                case 3:

                    // Включаем виртуальную камеру, которая воспроизводит финальную сцену
                    VirtualCamera.HGSetActive(true);

                    // Отключаем старое отслеживание камеры
                    MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

                    // Меняем лимит спавна для порталов
                    PortalManager.Instance.MaxCount = SpawnNewRate;

                    break;

                case 4:

                    // Воспроизводим анимацию организма с помощью DOTween
                    var seq = DOTween.Sequence();
                    seq.AppendInterval(DelayBeforeBodyStarted);
                    seq.AppendCallback(() => { Body1.HGSetActive(true); });
                    seq.AppendCallback(() => { Body2.HGSetActive(true); });
                    seq.Append(Body1.DOFade(_startBody1Alpha, BodyDuration));
                    seq.Join(Body2.DOFade(_startBody2Alpha, BodyDuration));
                    // Воспроизводим анимацию появления разломов
                    seq.AppendInterval(DelayBeforeCracks);
                    seq.AppendCallback(() => { Cracks1.HGSetActive(true); });
                    seq.Append(Cracks1.DOFade(_startCracks1Alpha, CracksDuration));
                    seq.AppendInterval(DelayPerCracks);
                    seq.AppendCallback(() => { Cracks2.HGSetActive(true); });
                    seq.Append(Cracks2.DOFade(_startCracks2Alpha, CracksDuration));
                    seq.AppendInterval(DelayPerCracks);
                    // Меняем скорость покачивания тентаклей у организма
                    seq.AppendCallback(() =>
                    {
                        var seq1 = DOTween.Sequence();
                        for (var i = 0; i < Tentacles.Length; i++)
                        {
                            var ii = i;
                            seq1.AppendCallback(() => { Tentacles[ii].WiggleSpeed /= WiggleSpeedDivider; });
                            seq1.AppendInterval(CracksDuration / Tentacles.Length);
                        }
                    });
                    seq.AppendCallback(() => { Cracks3.HGSetActive(true); });
                    seq.AppendCallback(() => { Body3.HGSetActive(true); });
                    seq.Append(Cracks3.DOFade(_startCracks3Alpha, CracksDuration));
                    seq.Join(Body2.DOFade(0, CracksDuration));
                    seq.Join(Body3.DOFade(_startBody3Alpha, CracksDuration));
                    seq.AppendCallback(() => { Body2.HGSetActive(false); });
                    seq.AppendInterval(DelayBeforeBodyFinished);
                    seq.OnComplete(() => { Execute(5); });

                    break;

                case 5:

                    // Заканчиваем и отправляем запрос для завершения уровня
                    HGGameEvent.Trigger(HGGameEventTypes.FinishLevelRequest);

                    break;
            }
        }

        /// <summary>
        /// Проверяет статус прогресса для перехода к финальной сцене.
        /// </summary>
        public virtual void OnHGEvent(HGProgressEvent e)
        {
            if (_character != null) return;
            if (e.ProgressID != ProgressID) return;
            if (e.EventType != HGProgressEventTypes.ValueCompleted) return;

            if (LevelManager.Instance.Players.Count == 0) return;
            _character = LevelManager.Instance.Players[0];
            if (_character == null) return;

            StartCoroutine(ExecuteCoroutine());
        }
    }
}