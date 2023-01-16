using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LedgePointPair = PlayerController.HitObjectPair<LedgePoint>;

public partial class PlayerController : MonoBehaviour
{
    public enum AutoGrabDetectFlags
    {
        Ledge = 1,
        Wallclimb = 2,
        Pole = 4,
        Monkey = 8,

        All = Ledge | Wallclimb | Pole | Monkey,
        None = 0
    }

    public struct HitObjectPair<T>
    {
        public readonly T value;
        public readonly RaycastHit hit;

        public HitObjectPair(T value, RaycastHit hit)
        {
            this.value = value;
            this.hit = hit;
        }
    }

    public class AutoGrabDetector
    {
        public AutoGrabDetectFlags Allow { get; set; }
        public LayerMask Layers { get; set; }
        public float RayIncrement { get; set; }

        public AutoGrabDetectFlags LastDetected { get; private set; } = AutoGrabDetectFlags.None;

        public IReadOnlyList<HitObjectPair<LedgePoint>> Ledges => _ledges.AsReadOnly();
        public IReadOnlyList<HitObjectPair<WallclimbSurface>> Wallclimbs => _wallclimbs.AsReadOnly();
        public IReadOnlyList<HitObjectPair<MonkeySurface>> Monkeys => _monkeys.AsReadOnly();
        public IReadOnlyList<HitObjectPair<PolePoint>> Poles => _poles.AsReadOnly();

        private List<HitObjectPair<LedgePoint>> _ledges;
        private List<HitObjectPair<WallclimbSurface>> _wallclimbs;
        private List<HitObjectPair<MonkeySurface>> _monkeys;
        private List<HitObjectPair<PolePoint>> _poles;

        public AutoGrabDetector(AutoGrabDetectFlags allow, LayerMask layers, float rayIncrement)
        {
            Allow = allow;
            Layers = layers;
            RayIncrement = rayIncrement;

            _ledges = new List<HitObjectPair<LedgePoint>>();
            _wallclimbs = new List<HitObjectPair<WallclimbSurface>>();
            _monkeys = new List<HitObjectPair<MonkeySurface>>();
            _poles = new List<HitObjectPair<PolePoint>>();
        }

        /// <summary>
        /// Try to detect something
        /// </summary>
        /// <param name="position">Position to start from</param>
        /// <param name="maxHeight">Maximum height to test</param>
        /// <param name="maxDistance">Maximum distance to test</param>
        /// <returns>Was antyhing detected?</returns>
        public bool DetectFrom(Vector3 position, Vector3 direction, float maxDistance, float maxHeight, float downwardsAmount = 1.0f)
        {
            ClearAll();

            LastDetected = AutoGrabDetectFlags.None;

            // Test for forward jump things
            for (float step = -downwardsAmount; step < maxHeight; step += RayIncrement)
            {
                Ray forwardRay = new Ray(position + Vector3.up * step, direction);

                Debug.DrawRay(forwardRay.origin, direction * maxDistance, Color.cyan, 5.0f);

                if (Physics.Raycast(forwardRay, out RaycastHit forwardHit, maxDistance, Layers.value, QueryTriggerInteraction.Collide))
                {
                    if (Allow.HasFlag(AutoGrabDetectFlags.Ledge))
                    {
                        LedgePoint ledge = forwardHit.collider.GetComponent<LedgePoint>();
                        if (ledge)
                        {
                            _ledges.Add(new HitObjectPair<LedgePoint>(ledge, forwardHit));
                            LastDetected |= AutoGrabDetectFlags.Ledge;
                        }
                    }

                    if (Allow.HasFlag(AutoGrabDetectFlags.Wallclimb))
                    {
                        WallclimbSurface wallclimb = forwardHit.collider.GetComponent<WallclimbSurface>();
                        if (wallclimb)
                        {
                            _wallclimbs.Add(new HitObjectPair<WallclimbSurface>(wallclimb, forwardHit));
                            LastDetected |= AutoGrabDetectFlags.Wallclimb;
                        }
                    }

                    if (Allow.HasFlag(AutoGrabDetectFlags.Pole))
                    {
                        PolePoint pole = forwardHit.collider.GetComponent<PolePoint>();
                        if (pole)
                        {
                            _poles.Add(new HitObjectPair<PolePoint>(pole, forwardHit));
                            LastDetected |= AutoGrabDetectFlags.Pole;
                        }
                    }
                }
            }

            // Test for things above
            Ray upwardsRay = new Ray(position, Vector3.up);

            Debug.DrawRay(upwardsRay.origin, upwardsRay.direction * maxDistance, Color.cyan, 5.0f);

            if (Physics.Raycast(upwardsRay, out RaycastHit upwardsHit, maxHeight, Layers.value, QueryTriggerInteraction.Collide))
            {
                if (Allow.HasFlag(AutoGrabDetectFlags.Monkey))
                {
                    MonkeySurface monkey = upwardsHit.collider.GetComponent<MonkeySurface>();
                    if (monkey)
                    {
                        _monkeys.Add(new HitObjectPair<MonkeySurface>(monkey, upwardsHit));
                        LastDetected |= AutoGrabDetectFlags.Monkey;
                    }
                }

                if (Allow.HasFlag(AutoGrabDetectFlags.Pole))
                {
                    PolePoint pole = upwardsHit.collider.GetComponent<PolePoint>();
                    if (pole)
                    {
                        _poles.Add(new HitObjectPair<PolePoint>(pole, upwardsHit));
                        LastDetected |= AutoGrabDetectFlags.Pole;
                    }
                }
            }

            return LastDetected != AutoGrabDetectFlags.None;
        }

        private void ClearAll()
        {
            _ledges.Clear();
            _wallclimbs.Clear();
            _poles.Clear();
            _monkeys.Clear();
        }
    }
}
