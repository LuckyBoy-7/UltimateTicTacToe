using UnityEngine;

namespace Lucky.Kits.Interactive
{
    public class ScaleInteractableUI : InteractableWorldUI
    {
        public Transform rendererTransform;
        private Vector3 _origScale;
        private const float OnCursorEnterScaleMultiplier = 1.05f;
        private const float OnCursorPressScaleMultiplier = 0.95f;
        private bool _mouseButtonKeepHolding;

        protected void Awake()
        {
            _origScale = rendererTransform.localScale;
        }

        protected override void OnCursorHover()
        {
            if (_mouseButtonKeepHolding)
            {
                rendererTransform.localScale = _origScale * OnCursorPressScaleMultiplier;
                return;
            }

            rendererTransform.localScale = _origScale * OnCursorEnterScaleMultiplier;
        }

        protected override void OnCursorExit()
        {
            rendererTransform.localScale = _origScale;
        }

        protected override void OnCursorPress()
        {
            _mouseButtonKeepHolding = true;
        }

        protected override void OnCursorRelease()
        {
            _mouseButtonKeepHolding = false;
        }
    }
}