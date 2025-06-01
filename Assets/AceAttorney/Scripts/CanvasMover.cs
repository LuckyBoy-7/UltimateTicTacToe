using Lucky.Framework;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts
{
    public class CanvasMover : ManagedBehaviour
    {
        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            if (Input.GetMouseButton(1) && !CardManager.Instance.counterCharacter)
                transform.position += (Vector3)GameCursor.MouseWorldPositionDelta;
        }
    }
}