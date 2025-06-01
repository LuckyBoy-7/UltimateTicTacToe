using System;
using System.Collections;
using AceAttorney.Scripts.Card.Data;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace AceAttorney.Scripts.Card.Logic
{
    public abstract class UsableCard : Card
    {
        private UsableCardData Data => cardData as UsableCardData;

        public override bool CanInteract => !played && canInteract;

        public virtual bool CanUse => !played && belongToCharacterCard == CardManager.Instance.currentInControlCharacter && !CardManager.Instance.InCounterState &&
                                      !CardManager.Instance.InReverseState && !CardManager.Instance.duringUnsafeAnim;

        public bool played = false;

        public Vector3 targetPosition; // 生成时移动到的目标位置(可以是起始点, 也可以是拖拽时候的鼠标位置)
        public Vector3 startPosition; // 手牌的初始位置
        public const float MoveToTargetLocalPositionLerpK = 0.1f;

        public float popupDistance;

        public Vector2Int popupDir;


        public CharacterCard belongToCharacterCard;
        public Action<UsableCard> OnRemovedFromCardDeck;

        public StateMachine stateMachine;
        public const int StDealing = 0;
        public const int StNormal = 1;
        public const int StShowing = 2;
        public const int StSelected = 3;
        public const int StPlayed = 4;
        public const int StDiscard = 5;

        public bool blockUse;
        private bool thrown;

        protected Vector2 lastMouseWorldPositionBeforePlayed;

        protected override void Awake()
        {
            base.Awake();

            stateMachine = new StateMachine(this, 6);
            stateMachine.SetCallbacks(StDealing, "StDealing", DealingBegin, null, DealingUpdate);
            stateMachine.SetCallbacks(StNormal, "StNormal", null, null, NormalUpdate);
            stateMachine.SetCallbacks(StShowing, "StShowing", null, null, ShowingUpdate);
            stateMachine.SetCallbacks(StSelected, "StSelected", SelectedBegin, SelectedEnd, SelectedUpdate);
            // stateMachine.SetCallbacks(StSelected, "StSelected", SelectedBegin, SelectedEnd, null, SelectedCoroutine);
            stateMachine.SetCallbacks(StPlayed, "StPlayed", PlayedBegin);
            stateMachine.SetCallbacks(StDiscard, "StDiscard", DiscardBegin, null, DiscardUpdate);
        }


        private void PlayedBegin()
        {
            StartCoroutine(PlayedFlow());
        }

        private IEnumerator PlayedFlow()
        {
            BeforePlayed();
            EventManager.Instance.OnBeforeCardPlayed?.Invoke(this);
            yield return new WaitUntil(() => !blockUse || thrown);
            CancelSelect();

            if (!thrown && !UseValid())
            {
                played = false;
                stateMachine.State = StNormal;
                yield break;
            }

            EventManager.Instance.OnCardPlayed?.Invoke(this);
            OnRemovedFromCardDeck?.Invoke(this);
            if (thrown)
            {
                Removed();
            }
            else
            {
                AfterPlayed();
                Use();
            }
        }

        protected virtual void BeforePlayed()
        {
            played = true;
        }

        protected virtual void AfterPlayed()
        {
            UI.HideSelectedHint();
        }

        public virtual void Thrown()
        {
            thrown = true;
        }

        private void SelectedEnd()
        {
            cardUI.canvasGroup.alpha = 1;
            CardManager.Instance.currentSelectedCard = null;
        }

        private void SelectedBegin()
        {
            cardUI.canvasGroup.alpha = 0.6f;
            CardManager.Instance.currentSelectedCard = this;
            SelectStart();
            ShowAtTop();
        }

        private int SelectedUpdate()
        {
            SelectStay();
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) // "对应点击后拖拽出来再次点击使用"和"拖拽出来使用的场景"的场景
            {
                if (Input.GetMouseButtonUp(0) && CheckCancelUse())
                {
                    CancelUse();
                    return StNormal;
                }

                if (UseValid())
                {
                    return StPlayed;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                CancelSelect();
                return StNormal;
            }

            return StSelected;
        }

        private int ShowingUpdate()
        {
            UI.transform.localPosition = Vector3.Lerp(UI.transform.localPosition, Vector3.zero, MoveToTargetLocalPositionLerpK);
            if (!CursorInBounds())
                return StNormal;
            if (Input.GetMouseButtonDown(0) && CardManager.Instance.currentSelectedCard == null && CanUse)
                return StSelected;
            return StShowing;
        }

        private int NormalUpdate()
        {
            if (CanUse)
            {
                UI.ShowSelectedHint();
                UI.SetSelectedHintColor(Color.white);
            }
            else
                UI.HideSelectedHint();

            transform.position = Vector3.Lerp(transform.position, startPosition, MoveToTargetLocalPositionLerpK);
            UI.transform.localPosition = Vector3.Lerp(UI.transform.localPosition, -(Vector2)popupDir * popupDistance, MoveToTargetLocalPositionLerpK);
            if (CursorInBounds() && this.Dist(startPosition) < 5)
                return StShowing;
            return StNormal;
        }

        private void DealingBegin()
        {
            transform.position = belongToCharacterCard.transform.position;
            UI.ScaleFromTo(0.6f, 1, 0.2f);

            belongToCharacterCard.UI.Pulse();
        }

        private int DealingUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, MoveToTargetLocalPositionLerpK);
            UI.transform.localPosition = Vector3.Lerp(UI.transform.localPosition, -(Vector2)popupDir * popupDistance, MoveToTargetLocalPositionLerpK);
            float leftDist = Vector2.Distance(transform.position, targetPosition);
            if (leftDist < 5)
                return StNormal;
            return StDealing;
        }


        protected virtual void PlacedOnBoard()
        {
            transform.SetParent(CardManager.Instance.worldCanvas);
            PlacedToGridCenter();
        }


        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            stateMachine.Update();

            if (!played)
                lastMouseWorldPositionBeforePlayed = GameCursor.MouseWorldPosition;
        }

        private void DiscardBegin()
        {
            OnRemovedFromCardDeck?.Invoke(this);
            targetPosition = belongToCharacterCard.transform.position;
        }

        private int DiscardUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, MoveToTargetLocalPositionLerpK);
            float leftDist = Vector2.Distance(transform.position, targetPosition);
            if (leftDist < 5)
            {
                Removed();
                belongToCharacterCard.UI.Pulse();
            }

            return StDiscard;
        }

        public bool TryDiscard()
        {
            if (Data.retained)
                return false;
            Discard();
            return true;
        }

        public virtual void Discard()
        {
            stateMachine.State = StDiscard;
        }

        public virtual void Removed()
        {
            Destroy(gameObject);
        }

        protected virtual bool CheckCancelUse() => false;

        protected virtual void CancelUse()
        {
        }

        protected abstract bool UseValid();

        protected abstract void Use();
        protected abstract void SelectStart();
        protected abstract void SelectStay();
        protected abstract void CancelSelect();
    }
}