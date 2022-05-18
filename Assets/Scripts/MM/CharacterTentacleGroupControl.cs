using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Помогает управлять тентаклями, основываясь на показателях персонажа из TopDownEngine.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(CharacterTentacleGroupControl))]
    public class CharacterTentacleGroupControl : CharacterAbility
    {
        [Header(nameof(CharacterTentacleGroupControl))]
        public TentacleGroup TentacleGroup;

        [Range(0, 1)] public float MinIntensivity;

        private CharacterMovement2 _characterMovement2;
        private float _currentIntensivity;

        protected override void Initialization()
        {
            base.Initialization();

            _characterMovement2 = (CharacterMovement2) _characterMovement;

            SetIntensivity(MinIntensivity);
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();

            var intensivity = Mathf.Clamp01(MinIntensivity + _characterMovement2.MovementVectorMultiplier);
            if (intensivity != _currentIntensivity)
                SetIntensivity(intensivity);
        }

        private void SetIntensivity(float value)
        {
            value = Mathf.Clamp01(value);
            foreach (var t in TentacleGroup.Targets)
                t.WiggleIntensivity = value;
            _currentIntensivity = value;
        }
    }
}