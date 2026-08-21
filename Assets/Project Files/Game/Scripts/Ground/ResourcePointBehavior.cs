using System;
using UnityEngine;

namespace Watermelon
{
    public abstract class ResourcePointBehavior : MonoBehaviour
    {
        public static ResourceCarrierType[] CARRIER_TYPES = (ResourceCarrierType[])Enum.GetValues(typeof(ResourceCarrierType));

        public static bool LogStaleCarriers = false;

        private const float MIN_VALID_RADIUS = 0.01f;
        private const int OVERLAP_BUFFER_SIZE = 16;

        private static readonly Collider[] overlapBuffer = new Collider[OVERLAP_BUFFER_SIZE];

        [SerializeField] ResourceCarrierType carrierType = ResourceCarrierType.Player | ResourceCarrierType.Helper;

        /// <summary>
        /// The amount of time (in seconds) the object is 'sleeping' after taking one resource
        /// </summary>
        [SerializeField] protected float cooldown = 0.1f;

        [SerializeField, Range(0f, 4f)] float rangePadding = 1f;

        private Collider pointCollider;

        private Vector3 colliderLocalCenter;
        private float colliderLocalRadius;

        private int carrierLayerMask;
        private bool hasValidRadius;

        public Vector3 PointCenter => transform.TransformPoint(colliderLocalCenter);

        public float ValidationRadius
        {
            get
            {
                var scale = transform.lossyScale;
                var scaledRadius = colliderLocalRadius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));

                return scaledRadius <= MIN_VALID_RADIUS ? 0f : scaledRadius + rangePadding;
            }
        }

        protected float ValidationRadiusSqr { get; private set; }

        protected abstract void AddResourceCarrier(GameObject carrierObject);
        protected abstract void RemoveResourceCarrier(GameObject carrierObject);

        protected abstract void ClearCarriers();

        protected abstract void PruneCarriers();

        public abstract void ForceRemoveCarrier(IResourceCarrier carrier);

        protected virtual void Awake()
        {
            CacheColliderData();
        }

        protected virtual void OnEnable()
        {
            ClearCarriers();

            hasValidRadius = false;

            ResourceCarrierRegistry.RegisterPoint(this);

            RefreshValidation();
        }

        protected virtual void OnDisable()
        {
            ResourceCarrierRegistry.UnregisterPoint(this);

            ClearCarriers();
        }

        private void CacheColliderData()
        {
            pointCollider = GetComponent<Collider>();

            if (pointCollider is SphereCollider sphereCollider)
            {
                colliderLocalCenter = sphereCollider.center;
                colliderLocalRadius = sphereCollider.radius;
            }
            else if (pointCollider is CapsuleCollider capsuleCollider)
            {
                colliderLocalCenter = capsuleCollider.center;
                colliderLocalRadius = capsuleCollider.radius;
            }
            else if (pointCollider is BoxCollider boxCollider)
            {
                colliderLocalCenter = boxCollider.center;
                colliderLocalRadius = new Vector2(boxCollider.size.x, boxCollider.size.z).magnitude * 0.5f;
            }
            else
            {
                colliderLocalCenter = Vector3.zero;
                colliderLocalRadius = 0f;

                Debug.LogError($"[Resource Point] '{name}' needs a Sphere, Capsule or Box collider to validate the range of its carriers. The distance check is disabled.", this);
            }

            carrierLayerMask = 0;

            for (var i = 0; i < CARRIER_TYPES.Length; i++)
            {
                var testType = CARRIER_TYPES[i];

                if ((carrierType & testType) != 0)
                    carrierLayerMask |= 1 << GetCarrierlayer(testType);
            }
        }

        protected void RefreshValidation()
        {
            var radius = ValidationRadius;
            var isRadiusValid = radius > 0f;

            ValidationRadiusSqr = isRadiusValid ? radius * radius : 0f;

            if (!isRadiusValid)
            {
                hasValidRadius = false;

                return;
            }

            if (hasValidRadius)
                return;

            hasValidRadius = true;

            ResyncOverlaps(radius);
        }

        private void ResyncOverlaps(float radius)
        {
            if (carrierLayerMask == 0)
                return;

            var count = Physics.OverlapSphereNonAlloc(PointCenter, radius, overlapBuffer, carrierLayerMask, QueryTriggerInteraction.Collide);

            for (var i = 0; i < count; i++)
            {
                var overlap = overlapBuffer[i];

                if (overlap == null)
                    continue;

                AddResourceCarrier(overlap.gameObject);
            }
        }

        protected bool IsCarrierValid(IResourceCarrier carrier)
        {
            if (carrier == null)
                return false;

            var carrierTransform = carrier.CarrierTransform;

            if (carrierTransform == null)
                return false;

            if (!carrier.IsCarrierActive || !carrierTransform.gameObject.activeInHierarchy)
                return false;

            if (ValidationRadiusSqr <= 0f)
                return true;

            var offset = carrierTransform.position - PointCenter;
            offset.y = 0f;

            if (offset.sqrMagnitude <= ValidationRadiusSqr)
                return true;

            if (LogStaleCarriers)
                Debug.LogWarning($"[Resource Point] '{name}' dropped a stale carrier '{carrierTransform.name}' standing {offset.magnitude:F1}m away (range {ValidationRadius:F1}m). The trigger exit was never delivered.", this);

            return false;
        }

        protected static IResourceCarrier ResolveCarrier(GameObject carrierObject)
        {
            var carrier = carrierObject.GetComponent<IResourceCarrier>();

            if (carrier == null)
                Debug.LogError($"Game Object {carrierObject.name} does not implement IResourceCarrier interface", carrierObject);

            return carrier;
        }

        private void OnTriggerEnter(Collider other)
        {
            for(int i = 0; i < CARRIER_TYPES.Length; i++)
            {
                var testType = CARRIER_TYPES[i];
                if ((carrierType & testType) != 0)
                {
                    if (other.gameObject.layer == GetCarrierlayer(testType))
                    {
                        AddResourceCarrier(other.gameObject);
                        break;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < CARRIER_TYPES.Length; i++)
            {
                var testType = CARRIER_TYPES[i];

                if ((carrierType & testType) != 0)
                {
                    if (other.gameObject.layer == GetCarrierlayer(testType))
                    {
                        RemoveResourceCarrier(other.gameObject);
                        break;
                    }
                }
            }
        }

        protected struct CarrierState
        {
            public IResourceCarrier Carrier;
            public int TransfersCount;

            public CarrierState(IResourceCarrier carrier)
            {
                Carrier = carrier;
                TransfersCount = 0;
            }
        }

        protected static int GetCarrierlayer(ResourceCarrierType carrierType) 
            => carrierType switch
            {
                ResourceCarrierType.Helper => PhysicsHelper.LAYER_HELPER,
                ResourceCarrierType.Player => PhysicsHelper.LAYER_CHARACTER,
                _ => 0,
            };

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                CacheColliderData();

            var radius = ValidationRadius;

            if (radius <= 0f)
                return;

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.35f);
            Gizmos.DrawWireSphere(PointCenter, radius);
        }
#endif
    }
}
