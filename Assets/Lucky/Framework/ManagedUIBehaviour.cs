using UnityEngine;

namespace Lucky.Framework
{
    [RequireComponent(typeof(RectTransform))]
    public class ManagedUIBehaviour : ManagedBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
    }
}