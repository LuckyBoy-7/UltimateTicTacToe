using UnityEngine;

namespace Lucky.Kits.Interactive
{
    public class Interactable : InteractableBase
    {
        #region Collider

        [SerializeField] private Collider2D _collider;

        public Collider2D Collider
        {
            get
            {
                if (_collider == null)
                    _collider = GetComponent<Collider2D>();
                return _collider;
            }
        }

        #endregion

        #region Renderer

        [SerializeField] private SpriteRenderer _renderer;

        public SpriteRenderer Renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<SpriteRenderer>();
                return _renderer;
            }
        }

        #endregion

        public override bool CursorInBounds() => Collider.OverlapPoint(GameCursor.MouseWorldPosition);

        protected void OnEnable()
        {
            GameCursor.Instance?.RegisterInteractable<Interactable>(this);
        }

        protected void OnDisable()
        {
            GameCursor.Instance?.UnregisterInteractable<Interactable>(this);
        }
    }
}