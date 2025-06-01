using System;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public abstract class ArrowUsableCard : UsableCard
    {
        protected override void SelectStart()
        {
            BezierArrow.Instance.Show();
            BezierArrow.Instance.SetCurve(transform.ScreenPosition(), GameCursor.MouseScreenPosition);
        }

        protected override void SelectStay()
        {
            BezierArrow.Instance.SetCurve(transform.ScreenPosition(), GameCursor.MouseScreenPosition);
        }

        protected override void CancelSelect()
        {
            BezierArrow.Instance.Hide();
        }
        
        protected override void Use()
        {
            Discard();
        }
    }
}