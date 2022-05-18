using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Дополняет оригинальный InputManager обработкой движения мыши,
    /// чтобы персонаж следовал за курсором при зажатии заданной кнопки.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(InputManager))]
    public class InputManager : MoreMountains.TopDownEngine.InputManager
    {
        protected Character _currentCharacter;
        protected Camera _currentCamera;

        protected virtual Character CurrentCharacter
        {
            get
            {
                if (_currentCharacter == null)
                    foreach (var ch in FindObjectsOfType<Character>(true))
                    {
                        if (ch.PlayerID != PlayerID) continue;
                        _currentCharacter = ch;
                        break;
                    }

                return _currentCharacter;
            }
        }

        protected virtual Camera CurrentCamera
        {
            get
            {
                if (_currentCamera == null)
                    _currentCamera = Camera.main;
                return _currentCamera;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsMobile && InputDetectionActive) SetMovementFromMousePosition();
        }

        /// <summary>
        /// Устанавливает направление движения исходя из позиции курсора.
        /// </summary>
        protected virtual void SetMovementFromMousePosition()
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                var fromPosition = CurrentCharacter.transform.position;
                var toPosition = CurrentCamera.ScreenToWorldPoint(Input.mousePosition);
                var result = MMMaths.Vector2ToVector3((fromPosition - toPosition) * -1, 0);

                _primaryMovement.x = result.x;
                _primaryMovement.y = result.y;

                _primaryMovement = ApplyCameraRotation(_primaryMovement);
            }
        }
    }
}