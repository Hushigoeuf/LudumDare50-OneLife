using Sirenix.OdinInspector;
using UnityEngine;

namespace Hushigoeuf
{
    /// <summary>
    /// Работает по такому же принципу как и Tentacle, но устанавливает на объект следа для маяков.
    /// </summary>
    [AddComponentMenu(HGEditor.PATH_MENU_CURRENT + nameof(DeadRegion_Trail))]
    public class DeadRegion_Trail : HGBaseMonoBehaviour, HGEventListener<HGUpdateEvent>
    {
        [HGShowInSettings] [HGBorders] [MinValue(2)]
        public int Length = 30;

        [HGShowInSettings] [HGBorders] [MinValue(0)]
        public float SmoothSpeed = .02f;

        [HGShowInSettings] [HGBorders] [MinValue(0)]
        public float TrailSpeed = 350;

        [HGShowInDebug] [HGBorders] [ReadOnly] [ShowInInspector]
        private Transform _firstTarget;

        [HGShowInDebug] [HGBorders] [ReadOnly] [ShowInInspector]
        private Transform _lastTarget;

        private LineRenderer _target;
        private Vector3[] _segmentPositions;
        private Vector3[] _segmentVelocities;

        public virtual Transform FirstTarget
        {
            get => _firstTarget ? _firstTarget : Transform;
            set
            {
                _firstTarget = value;
                ResetSegments();
            }
        }

        public virtual Transform LastTarget
        {
            get => _lastTarget ? _lastTarget : Transform;
            set
            {
                _lastTarget = value;
                ResetSegments();
            }
        }

        public virtual Vector3 DirectionToLastTarget => (LastTarget.position - FirstTarget.position).normalized;

        public virtual float DistancePerSegment =>
            Vector3.Distance(FirstTarget.position, LastTarget.position) / (Length - 1);

        protected virtual void Awake()
        {
            _target = GetComponent<LineRenderer>();
            _target.positionCount = Length;
            _segmentPositions = new Vector3[Length];
            _segmentVelocities = new Vector3[Length];
        }

        protected virtual void OnEnable()
        {
            this.HGEventStartListening();
        }

        protected virtual void OnDisable()
        {
            this.HGEventStopListening();
        }

        protected virtual void OnHGUpdate(float dt)
        {
            Transform.position = FirstTarget.position;

            if (_segmentPositions.Length > 0)
                _segmentPositions[0] = FirstTarget.position;
            if (_segmentPositions.Length > 1)
                _segmentPositions[_segmentPositions.Length - 1] = LastTarget.position;
            if (_segmentPositions.Length > 2)
            {
                var directionToLastTarget = DirectionToLastTarget;
                var distancePerSegment = DistancePerSegment;
                for (var i = 1; i < _segmentPositions.Length - 1; i++)
                    _segmentPositions[i] = Vector3.SmoothDamp(_segmentPositions[i],
                        _segmentPositions[i - 1] + directionToLastTarget * distancePerSegment,
                        ref _segmentVelocities[i],
                        SmoothSpeed + (TrailSpeed > 0 ? i / TrailSpeed : 0));
            }

            _target.SetPositions(_segmentPositions);
        }

        public virtual void ResetSegments()
        {
            if (_segmentPositions.Length > 0)
                _segmentPositions[0] = FirstTarget.position;
            if (_segmentPositions.Length > 1)
                _segmentPositions[_segmentPositions.Length - 1] = LastTarget.position;
            if (_segmentPositions.Length > 2)
            {
                var directionToLastTarget = DirectionToLastTarget;
                var distancePerSegment = DistancePerSegment;
                for (var i = 1; i < _segmentPositions.Length - 1; i++)
                    _segmentPositions[i] = _segmentPositions[i - 1] + directionToLastTarget * distancePerSegment;
            }

            _target.SetPositions(_segmentPositions);
        }

        public virtual void OnHGEvent(HGUpdateEvent e)
        {
            if (e.EventType == HGUpdateEventTypes.Update)
                OnHGUpdate(e.DateTime);
        }
    }
}