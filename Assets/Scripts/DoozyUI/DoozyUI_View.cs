using Doozy.Engine.UI;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Hushigoeuf
{
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DoozyUI_View))]
    [RequireComponent(typeof(UIView))]
    public class DoozyUI_View : HGSimpleUIView
    {
        public override bool IsHidden => Target.IsHidden;
        public override bool IsHiding => Target.IsHiding;
        public override bool IsShowing => Target.IsShowing;
        public override bool IsVisible => Target.IsVisible;

        protected UIView _target;

        public virtual UIView Target
        {
            get
            {
                if (_target == null)
                    _target = GetComponent<UIView>();
                return _target;
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);

            ResetListeners();

            Target.ShowBehavior.OnFinished.Event.AddListener(ShowOnFinished);
            Target.Show(instant);
            Target.HGSetActive(true);
        }

        protected override void ShowOnFinished()
        {
            Target.ShowBehavior.OnFinished.Event.RemoveListener(ShowOnFinished);

            base.ShowOnFinished();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);

            ResetListeners();

            Target.HideBehavior.OnFinished.Event.AddListener(HideOnFinished);
            Target.Hide(instant);
            Target.HGSetActive(true);
        }

        protected override void HideOnFinished()
        {
            Target.HideBehavior.OnFinished.Event.RemoveListener(ShowOnFinished);

            base.HideOnFinished();
        }

        public override void Reset(bool visible)
        {
            base.Reset(visible);

            Target.ShowBehavior.OnFinished.Event.RemoveListener(ShowOnFinished);
            Target.HideBehavior.OnFinished.Event.RemoveListener(HideOnFinished);

            if (visible) Target.InstantShow();
            else Target.InstantHide();
        }

        protected virtual void ResetListeners()
        {
            Target.ShowBehavior.OnFinished.Event.RemoveListener(ShowOnFinished);
            Target.HideBehavior.OnFinished.Event.RemoveListener(HideOnFinished);
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnInspectorInit]
        private void EditorOnInitViewID()
        {
            if (string.IsNullOrEmpty(ViewName))
                ViewName = GetComponent<UIView>().ViewName;
        }
#endif
    }
}