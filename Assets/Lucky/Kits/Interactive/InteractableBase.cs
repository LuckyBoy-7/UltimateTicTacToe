using System;
using Lucky.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lucky.Kits.Interactive
{
    public abstract class InteractableBase : ManagedBehaviour
    {
        protected const int OneSortingLayerCapacity = 10000;

        [FoldoutGroup("Interactable"), ShowInInspector]
        private int baseSortingOrder = 0;

        [FoldoutGroup("Interactable"), ShowInInspector]
        public int SortingOrderAdjustment { get; set; }
        private int SortingOrder => baseSortingOrder + SortingOrderAdjustment;

        public int CompareSortingOrder(InteractableBase other) => (int)Mathf.Sign(SortingOrder - other.SortingOrder);


        public abstract bool CursorInBounds();

        #region Delegates

        // 这些是可以给外部用的给
        public event Action<InteractableBase> OnCursorEnterEvent; // 鼠标进入时
        public event Action<InteractableBase> OnCursorExitEvent; // 鼠标离开时
        public event Action<InteractableBase> OnCursorPressEvent; // 鼠标按下时
        public event Action<InteractableBase> OnCursorReleaseEvent; // 鼠标释放时
        public event Action<InteractableBase> OnCursorClickEvent; // 鼠标点击时
        public event Action<bool, Vector2> OnCursorDragEvent; // 鼠标拖拽时
        public event Action<InteractableBase> OnCursorHoverEvent; // 鼠标悬停时（只要在范围内就触发）

        #endregion

        private bool debug = false;

        [FoldoutGroup("Interactable")] public bool canInteract = true;

        public virtual bool CanInteract => canInteract;


        #region Methods Called By GameCursor

        public void CursorEnter()
        {
            if (debug)
                print("Enter");

            if (!CanInteract)
                return;

            OnCursorEnterEvent?.Invoke(this);
            OnCursorEnter();
        }

        public void CursorExit()
        {
            if (debug)
                print("Exit");
            if (!CanInteract)
                return;
            OnCursorExitEvent?.Invoke(this);
            OnCursorExit();
        }

        public void CursorPress()
        {
            if (debug)
                print("Press");
            if (!CanInteract)
                return;
            OnCursorPressEvent?.Invoke(this);
            OnCursorPress();
        }

        public void CursorRelease()
        {
            if (debug)
                print("Release");
            if (!CanInteract)
                return;
            OnCursorReleaseEvent?.Invoke(this);
            OnCursorRelease();
        }

        public void CursorClick()
        {
            if (debug)
                print("Click");
            if (!CanInteract)
                return;
            OnCursorClickEvent?.Invoke(this);
            OnCursorClick();
        }

        public void CursorDrag(bool hasStart, Vector2 delta)
        {
            if (debug)
                print("Drag");
            if (!CanInteract)
                return;
            OnCursorDragEvent?.Invoke(hasStart, delta);
            OnCursorDrag(hasStart, delta);
        }

        public void CursorHover()
        {
            if (debug)
                print("Hover");
            if (!CanInteract)
                return;
            OnCursorHoverEvent?.Invoke(this);
            OnCursorHover();
        }

        #endregion

        #region Methods Overridden By Children

        // 这些是可以给子类用的用的给
        protected virtual void OnCursorEnter()
        {
        }

        protected virtual void OnCursorExit()
        {
        }

        protected virtual void OnCursorEnterBounds()
        {
        }

        protected virtual void OnCursorExitBounds()
        {
        }

        protected virtual void OnCursorPress()
        {
        }

        protected virtual void OnCursorRelease()
        {
        }

        protected virtual void OnCursorClick()
        {
        }

        /// <summary>
        /// OnCursor分为两种, 一种是从鼠标按下开始算, 一种是要拖动才开始算
        /// 由于第二种可以由第一种稍微判断一下来实现, 且碰到的需求相对少, 且我也不太知道取什么名所以就只写第一种的实现
        /// </summary>
        /// <param name="delta"></param>
        protected virtual void OnCursorDrag(bool hasStart, Vector2 delta)
        {
        }

        protected virtual void OnCursorHover()
        {
        }

        #endregion
    }
}