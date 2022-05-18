using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// В дополнении к родительскому классу способен управлять позицией заданных Transform,
    /// и распределять их по всей длине тентакля.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(TentacleExtended))]
    public class TentacleExtended : Tentacle
    {
        [HGShowInBindings] [HGListDrawerSettings]
        public Transform[] BodyParts = new Transform[0];

        protected override void UpdateSegment(int i)
        {
            if (i == 0)
            {
                base.UpdateSegment(i);
            }
            else if (i > 0)
            {
                var position = _segmentPositions[i - 1] +
                               (_segmentPositions[i] - _segmentPositions[i - 1]).normalized * DistancePerSegment;
                _segmentPositions[i] = Vector3.SmoothDamp(_segmentPositions[i],
                    position, ref _segmentVelocities[i], SmoothSpeed);
            }

            ResetBodyParts(i);
        }

        protected override void ResetSegment(int i)
        {
            if (i == 0)
            {
                base.ResetSegment(i);
            }
            else if (i > 0)
            {
                _segmentPositions[i] = _segmentPositions[i - 1] +
                                       (_segmentPositions[i] - _segmentPositions[i - 1]).normalized *
                                       DistancePerSegment;
                if (i < BodyParts.Length)
                    BodyParts[i - 1].position = _segmentPositions[i];
            }

            ResetBodyParts(i);
        }

        protected virtual void ResetBodyParts(int i)
        {
            if (BodyParts.Length == 0) return;

            var currentBodyLength = 0;
            var currentStartIndex = 0;

            var segmentLength = _segmentPositions.Length - 2;
            if (segmentLength > 0)
            {
                if (i == 0) return;
                if (i == _segmentPositions.Length - 1) return;

                if (segmentLength > BodyParts.Length)
                {
                    var offset = Mathf.FloorToInt(segmentLength / BodyParts.Length);
                    var p1 = i / (float) offset;
                    var p2 = Mathf.FloorToInt(p1);
                    var p3 = p1 - p2;
                    if (p3 != 0) return;
                    currentBodyLength = 1;
                    currentStartIndex = p2 - 1;
                }
                else if (segmentLength == BodyParts.Length)
                {
                    currentBodyLength = 1;
                    currentStartIndex = i - 1;
                }
                else
                {
                    currentBodyLength = 1;
                    if (i == 1)
                        currentBodyLength += BodyParts.Length - segmentLength;
                    currentStartIndex = i - 1;
                    if (i > 1)
                        currentStartIndex += BodyParts.Length - segmentLength;
                }
            }
            else
            {
                currentBodyLength = BodyParts.Length;
            }

            if (currentBodyLength == 0) return;
            if (currentStartIndex >= BodyParts.Length) return;

            for (var i2 = currentStartIndex; i2 < currentStartIndex + currentBodyLength; i2++)
                BodyParts[i2].position = _segmentPositions[i];
        }
    }
}