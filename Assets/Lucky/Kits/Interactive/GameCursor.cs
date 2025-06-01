using System;
using System.Collections.Generic;
using Lucky.Framework;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using UnityEngine;


namespace Lucky.Kits.Interactive
{
    /// <summary>
    /// 注意先把相机大小放大100倍
    /// </summary>
    public class GameCursor : ManagedBehaviour
    {
        public static GameCursor Instance;
        // private HashSet<InteractableBase> _interactables = new();
        private DefaultDict<Type, HashSet<InteractableBase>> _interactables = new(() => new());
        public static Vector2 MouseWorldPositionDelta;
        public static Vector2 MouseWorldPosition => Camera.main.ScreenToWorldPoint(MouseScreenPosition);
        public static bool UseCustomMouseScreenPosition = false;
        public static Vector2 CustomMouseScreenPosition;
        public static Vector2 MouseScreenPosition => UseCustomMouseScreenPosition ? CustomMouseScreenPosition : Input.mousePosition.WithZ(0);
        private Vector2 PreviousMouseWorldPosition { get; set; }
        private InteractableBase PreviousInteractable { get; set; }
        private InteractableBase CurrentInteractable { get; set; }
        public InteractableBase MouseButtonDownInteractable { get; set; } // 点击时对应的第一个对象
        private float MouseButtonDownTimestamp { get; set; } = -1;
        private float RealtimeSinceMouseButtonDown => Time.realtimeSinceStartup - MouseButtonDownTimestamp;
        [Header("Click")] [SerializeField] private float clickTimeThreshold = 0.2f;

        private bool hasStartDrag;


        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            UpdateCurrentInteractable();
            // 如果是刚开始的话，就把previous改成当前位置
            if (PreviousMouseWorldPosition == default)
                PreviousMouseWorldPosition = MouseWorldPosition;

            if (CurrentInteractable != PreviousInteractable && PreviousInteractable != null)
                PreviousInteractable.CursorExit();
            if (CurrentInteractable != PreviousInteractable && CurrentInteractable != null)
                CurrentInteractable.CursorEnter();
            if (CurrentInteractable != null)
                CurrentInteractable.CursorHover();
            if (Input.GetMouseButtonDown(0))
            {
                MouseButtonDownTimestamp = Time.realtimeSinceStartup;
                MouseButtonDownInteractable = CurrentInteractable;
                if (CurrentInteractable)
                    CurrentInteractable.CursorPress();
            }

            MouseWorldPositionDelta = MouseWorldPosition - PreviousMouseWorldPosition;
            if (Input.GetMouseButton(0))
            {
                if (MouseWorldPositionDelta != Vector2.zero)
                    hasStartDrag = true;
                if (MouseButtonDownInteractable != null)
                    MouseButtonDownInteractable.CursorDrag(hasStartDrag, MouseWorldPositionDelta);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseButtonDownInteractable != null)
                {
                    if (RealtimeSinceMouseButtonDown <= clickTimeThreshold)
                        MouseButtonDownInteractable.CursorClick();
                    MouseButtonDownInteractable.CursorRelease();
                }

                MouseButtonDownInteractable = null;
                hasStartDrag = false;
            }

            PreviousMouseWorldPosition = MouseWorldPosition;
        }

        private void UpdateCurrentInteractable()
        {
            InteractableBase topInteractable = GetTopHitInteractable();
            // foreach (var curInteractable in)
            // {
            //     if (curInteractable.canInteract)
            //     {
            //         topInteractable = curInteractable;
            //         break;
            //     }
            // }

            // 更新
            PreviousInteractable = CurrentInteractable;
            CurrentInteractable = topInteractable;
        }

        public InteractableBase GetTopHitInteractableAt(Vector3 screenPosition, Func<InteractableBase, bool> filter = null)
        {
            return GetTopHitInteractableAt<InteractableScreenUI>(screenPosition, filter)
                   ?? GetTopHitInteractableAt<InteractableWorldUI>(screenPosition, filter)
                   ?? GetTopHitInteractableAt<Interactable>(screenPosition, filter);
        }

        public InteractableBase GetTopHitInteractableAt<T>(Vector3 screenPosition, Func<InteractableBase, bool> filter = null) where T : InteractableBase
        {
            UseCustomMouseScreenPosition = true;
            CustomMouseScreenPosition = screenPosition;
            InteractableBase topInteractable = null;

            foreach (var interactable in _interactables[typeof(T)])
            {
                if (
                    interactable.CursorInBounds()
                    // && interactable.canInteract  , 这里我们认为不能交互但是还是参与碰撞的, 不然能被穿透点击还挺怪的
                    && (filter == null || filter(interactable))
                    && (topInteractable == null || interactable.CompareSortingOrder(topInteractable) == 1)
                )
                    topInteractable = interactable;
            }

            UseCustomMouseScreenPosition = false;
            return topInteractable;
        }

        public InteractableBase GetTopHitInteractable() => GetTopHitInteractableAt(MouseScreenPosition);

        public void RegisterInteractable<T>(InteractableBase interactable) => _interactables[typeof(T)].Add(interactable);
        public void UnregisterInteractable<T>(InteractableBase interactable) => _interactables[typeof(T)].Remove(interactable);
    }
}