using Doozy.Engine.Progress;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Использует Progressor из DoozyUI для визуализации прогресса.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DoozyUI_ProgressHandler))]
    [RequireComponent(typeof(Progressor))]
    public class DoozyUI_ProgressHandler : HGProgressHandler
    {
        protected Progressor _target;

        protected virtual Progressor Target
        {
            get
            {
                if (_target == null)
                    _target = GetComponent<Progressor>();
                return _target;
            }
        }

        protected override void SetValue(float value)
        {
            Target.SetValue(value);
        }

        protected override void SetLimit(float min, float max)
        {
            Target.SetMin(min);
            Target.SetMax(max);
        }
    }
}