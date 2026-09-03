using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class WanderingTraderBehavior : MonoBehaviour, IWorldElement, IGuardedRescueTarget, IMinigameStageProvider
    {
        private const int WATER_AREA_MASK = 8;

        private static readonly int IS_MOVING_HASH = Animator.StringToHash("IsMoving");
        private static readonly int IS_SWIMMING_HASH = Animator.StringToHash("IsSwimming");
        private static readonly int IS_SITTING_HASH = Animator.StringToHash("IsSitting");
        private static readonly int IS_CAPTURED_HASH = Animator.StringToHash("IsCaptured");

        public int InitialisationOrder => 10;

        [UniqueID, Order(-1)]
        [SerializeField] string id;
        public string ID => id;

        [Order(-1)]
        [SerializeField] Animator graphicsAnimator;

        [BoxGroup("Route")]
        [SerializeField] Transform[] routeWaypoints;
        [BoxGroup("Route")]
        [SerializeField] Transform basePoint;

        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float minRestTime = 180f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float maxRestTime = 300f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float minTradeTime = 45f;
        [BoxGroup("Timing")]
        [SerializeField, Min(0f)] float maxTradeTime = 75f;

        [BoxGroup("Movement")]
        [SerializeField, Min(0f)] float swimSpeed = 3f;
        [BoxGroup("Movement")]
        [SerializeField, Min(0f)] float rotationSpeed = 8f;
        [BoxGroup("Movement")]
        [SerializeField, Min(0.01f)] float waypointThreshold = 0.35f;
        [BoxGroup("Movement")]
        [SerializeField, Min(0.1f)] float groundSampleDistance = 2f;
        [BoxGroup("Movement")]
        [SerializeField, Min(0.1f)] float groundSnapSpeed = 10f;

        [BoxGroup("Trading")]
        [SerializeField] TraderOffersDatabase offersDatabase;
        [BoxGroup("Trading")]
        [SerializeField] TraderTradeButton tradeButton;
        [BoxGroup("Trading")]
        [SerializeField, Min(1)] int purchasesPerOffer = 3;

        [BoxGroup("Minigames", "Minigames")]
        [SerializeField] TraderMinigamesDatabase minigamesDatabase;

        [BoxGroup("Minigames")]
        [SerializeField] Transform minigamePlayerPoint;
        [BoxGroup("Minigames")]
        [SerializeField] Transform minigameFloorAnchor;
        [BoxGroup("Minigames")]
        [SerializeField] Transform minigameTableAnchor;

        [BoxGroup("Water Visuals")]
        [SerializeField] Transform graphicsRoot;
        [BoxGroup("Water Visuals")]
        [SerializeField] ParticleSystem waterTrail;
        [BoxGroup("Water Visuals")]
        [SerializeField] float swimWaterLineOffset = -0.15f;
        [BoxGroup("Water Visuals")]
        [SerializeField] float waterLevel = -0.9f;

        [BoxGroup("Rescue")]
        [SerializeField] bool requiresRescue;
        [BoxGroup("Rescue")]
        [SerializeField] Transform capturedPoint;
        [BoxGroup("Rescue")]
        [SerializeField] GroundTileComplexBehavior[] rescueLinkedTiles;
        [BoxGroup("Rescue")]
        [SerializeField] BuildingComplexBehavior[] rescueLinkedBuildings;
        [BoxGroup("Rescue")]
        [SerializeField, Min(0f)] float escortMoveSpeed = 4f;
        [BoxGroup("Rescue")]
        [SerializeField, Min(0.1f)] float escortArriveDistance = 1.5f;
        [BoxGroup("Rescue")]
        [SerializeField, Min(0f)] float escortPauseDuration = 0.5f;
        [BoxGroup("Rescue")]
        [SerializeField] Transform[] rescueReturnWaypoints;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public event SimpleCallback OffersChanged;
        public event SimpleCallback MinigameChanged;

        public Transform Transform => transform;
        public bool IsRescued => traderSave != null && traderSave.IsRescued;
        public bool IsRescueAreaUnlocked => rescueGate.IsUnlocked;
        public bool WaitForExternalRelease => true;
        public event SimpleCallback RescueAreaUnlocked;

        private readonly RescueAreaGate rescueGate = new RescueAreaGate();

        private readonly TraderMinigameSlot minigameSlot = new TraderMinigameSlot();
        public TraderMinigameSlot MinigameSlot => minigameSlot;

        private readonly TraderMinigameLauncher minigameLauncher = new TraderMinigameLauncher();

        private RescueState rescueState = RescueState.None;
        private float escortPauseLeft;

        private TraderSave traderSave;

        private Vector3 islandPosition;
        private Quaternion islandRotation;

        private readonly List<Vector3> path = new List<Vector3>();
        private int pathIndex;

        private bool swimming;
        private bool waterTrailPlaying;

        private bool isInitialised;

        private Phase CurrentPhase
        {
            get => (Phase)traderSave.Phase;
            set => traderSave.Phase = (int)value;
        }

        public bool IsTrading => isInitialised && CurrentPhase == Phase.AtBase;

        public void OnWorldLoaded()
        {
            islandPosition = transform.position;
            islandRotation = transform.rotation;

            var waterObject = GameObject.FindGameObjectWithTag("Water");
            if (waterObject != null)
                waterLevel = waterObject.transform.position.y;

            traderSave = SaveController.GetSaveObject<TraderSave>(LinkedWorldBehavior.WorldData.ID, "trader_" + id);

            minigameSlot.Initialise(minigamesDatabase, traderSave);
            minigameSlot.Changed += OnMinigameSlotChanged;

            if (tradeButton != null)
                tradeButton.Clicked += OpenTradeWindow;

            if (requiresRescue && !traderSave.IsRescued)
                EnterCapturedState();
            else
                RestoreState();

            isInitialised = true;
        }

        public void OnWorldUnloaded()
        {
            isInitialised = false;

            rescueGate.Dispose();

            minigameLauncher.Abort();

            minigameSlot.Changed -= OnMinigameSlotChanged;
            minigameSlot.Dispose();

            if (tradeButton != null)
            {
                tradeButton.Clicked -= OpenTradeWindow;
                tradeButton.Deactivate();
            }
        }

        private void RestoreState()
        {
            SetMoving(false);

            switch (CurrentPhase)
            {
                case Phase.Idle:
                    PlaceOnIsland();
                    SetSitting(true);
                    if (traderSave.TimeUntilArrival <= 0f)
                        traderSave.TimeUntilArrival = GetRandomRestTime();
                    tradeButton.Deactivate();
                    break;

                case Phase.SailingIn:
                    transform.position = islandPosition;
                    SetSitting(false);
                    BuildPathToBase();
                    SetMoving(true);
                    tradeButton.Deactivate();
                    break;

                case Phase.AtBase:
                    PlaceAtBase();
                    SetSitting(false);
                    EnsureOffersGenerated();
                    if (traderSave.VisitTimeLeft <= 0f)
                        traderSave.VisitTimeLeft = GetRandomTradeTime();
                    tradeButton.Activate();
                    break;

                case Phase.SailingOut:
                    transform.position = basePoint.position;
                    SetSitting(false);
                    BuildPathToIsland();
                    SetMoving(true);
                    tradeButton.Deactivate();
                    break;
            }
        }

        private void Update()
        {
            if (!isInitialised)
                return;

            if (rescueState != RescueState.None)
            {
                UpdateRescue();
                UpdateWaterVisuals();
                return;
            }

            switch (CurrentPhase)
            {
                case Phase.Idle:
                    swimming = false;
                    traderSave.TimeUntilArrival -= Time.deltaTime;
                    if (traderSave.TimeUntilArrival <= 0f)
                        StartSailingToBase();
                    break;

                case Phase.SailingIn:
                    if (MoveAlongPath(swimSpeed))
                        ArriveAtBase();
                    break;

                case Phase.AtBase:
                    swimming = false;

                    if (!minigameSlot.IsBusy)
                    {
                        traderSave.VisitTimeLeft -= Time.deltaTime;
                        if (traderSave.VisitTimeLeft <= 0f)
                            StartSailingToIsland();
                    }
                    break;

                case Phase.SailingOut:
                    if (MoveAlongPath(swimSpeed))
                        ArriveAtIsland();
                    break;
            }

            UpdateWaterVisuals();
        }

        private void StartSailingToBase()
        {
            CurrentPhase = Phase.SailingIn;
            SetSitting(false);
            BuildPathToBase();
            SetMoving(true);
            tradeButton.Deactivate();
            Save();
        }

        private void ArriveAtBase()
        {
            SetMoving(false);
            SetSitting(false);
            swimming = false;
            PlaceAtBase();

            CurrentPhase = Phase.AtBase;
            traderSave.VisitTimeLeft = GetRandomTradeTime();

            EnsureOffersGenerated();

            tradeButton.Activate();
            Save();
        }

        private void StartSailingToIsland()
        {
            CurrentPhase = Phase.SailingOut;
            SetSitting(false);
            BuildPathToIsland();
            SetMoving(true);

            tradeButton.Deactivate();
            CloseTradeWindowIfOpen();

            Save();
        }

        private void ArriveAtIsland()
        {
            SetMoving(false);
            PlaceOnIsland();
            SetSitting(true);
            swimming = false;

            CurrentPhase = Phase.Idle;
            traderSave.TimeUntilArrival = GetRandomRestTime();
            traderSave.ActiveOfferIndices.Clear();
            traderSave.OfferRemaining.Clear();

            minigameSlot.Clear();

            Save();
        }

        private void EnsureOffersGenerated()
        {
            minigameSlot.RollForVisit();

            if (offersDatabase == null || traderSave.ActiveOfferIndices.Count > 0)
                return;

            traderSave.ActiveOfferIndices = offersDatabase.GetRandomOfferIndices();
            traderSave.OfferRemaining = new List<int>();

            for (var i = 0; i < traderSave.ActiveOfferIndices.Count; i++)
                traderSave.OfferRemaining.Add(purchasesPerOffer);

            OffersChanged?.Invoke();
        }

        #region Rescue
        private void EnterCapturedState()
        {
            rescueState = RescueState.Captured;

            if (capturedPoint != null)
            {
                transform.position = capturedPoint.position;
                transform.rotation = capturedPoint.rotation;
            }

            SetMoving(false);
            SetSitting(false);
            SetSwimming(false);
            swimming = false;
            SetCaptured(true);

            if (tradeButton != null)
                tradeButton.Deactivate();

            rescueGate.Initialise(rescueLinkedTiles, rescueLinkedBuildings, OnRescueAreaUnlocked);
        }

        private void OnRescueAreaUnlocked()
        {
            RescueAreaUnlocked?.Invoke();
        }

        public bool TryRelease()
        {
            if (traderSave == null)
                return false;

            if (traderSave.IsRescued)
                return true;

            traderSave.IsRescued = true;
            Save();

            rescueGate.Dispose();

            SetCaptured(false);
            SetSitting(false);
            SetMoving(true);
            rescueState = RescueState.RunningToPlayer;

            return true;
        }

        private void UpdateRescue()
        {
            switch (rescueState)
            {
                case RescueState.Captured:
                    swimming = false;
                    break;

                case RescueState.RunningToPlayer:
                    if (MoveTowardsPlayer())
                        EnterEscortPause();
                    break;

                case RescueState.Pausing:
                    swimming = false;
                    escortPauseLeft -= Time.deltaTime;
                    if (escortPauseLeft <= 0f)
                        StartGoingHome();
                    break;

                case RescueState.GoingHome:
                    if (MoveAlongPath(escortMoveSpeed))
                        ArriveHomeAfterRescue();
                    break;
            }
        }

        private bool MoveTowardsPlayer()
        {
            var player = PlayerBehavior.GetBehavior();
            if (player == null)
                return true;

            return MoveTowardsPoint(player.Transform.position, escortArriveDistance);
        }

        private bool MoveTowardsPoint(Vector3 target, float arriveDistance)
        {
            StepTowards(target, escortMoveSpeed);

            var planarOffset = target - transform.position;
            planarOffset.y = 0f;

            return planarOffset.sqrMagnitude <= arriveDistance * arriveDistance;
        }

        private void EnterEscortPause()
        {
            SetMoving(false);
            swimming = false;
            SetSwimming(false);

            escortPauseLeft = escortPauseDuration;
            rescueState = RescueState.Pausing;
        }

        private void StartGoingHome()
        {
            SetMoving(true);
            BuildRescueReturnPath();
            rescueState = RescueState.GoingHome;
        }

        private void ArriveHomeAfterRescue()
        {
            SetMoving(false);
            SetSwimming(false);
            swimming = false;

            PlaceOnIsland();
            SetSitting(true);

            CurrentPhase = Phase.Idle;
            traderSave.TimeUntilArrival = GetRandomRestTime();
            traderSave.ActiveOfferIndices.Clear();
            traderSave.OfferRemaining.Clear();

            minigameSlot.Clear();

            rescueState = RescueState.None;
            Save();
        }

        private void SetCaptured(bool value)
        {
            if (graphicsAnimator != null)
                graphicsAnimator.SetBool(IS_CAPTURED_HASH, value);
        }
        #endregion

        #region Movement
        private void BuildPathToBase()
        {
            BuildPath(routeWaypoints, false, basePoint.position);
        }

        private void BuildPathToIsland()
        {
            BuildPath(routeWaypoints, true, islandPosition);
        }

        private void BuildRescueReturnPath()
        {
            if (rescueReturnWaypoints.IsNullOrEmpty())
            {
                BuildPathToIsland();
                return;
            }

            BuildPath(rescueReturnWaypoints, false, islandPosition);
        }

        private void BuildPath(Transform[] waypoints, bool reversed, Vector3 destination)
        {
            path.Clear();

            if (!waypoints.IsNullOrEmpty())
            {
                for (var i = 0; i < waypoints.Length; i++)
                {
                    var waypoint = waypoints[reversed ? waypoints.Length - 1 - i : i];

                    if (waypoint != null)
                        path.Add(waypoint.position);
                }
            }

            path.Add(destination);
            pathIndex = 0;
        }

        private bool MoveAlongPath(float speed)
        {
            if (pathIndex >= path.Count)
                return true;

            var target = path[pathIndex];

            StepTowards(target, speed);

            var planarOffset = target - transform.position;
            planarOffset.y = 0f;

            if (planarOffset.sqrMagnitude <= waypointThreshold * waypointThreshold)
            {
                pathIndex++;
                return pathIndex >= path.Count;
            }

            return false;
        }

        private void StepTowards(Vector3 target, float speed)
        {
            var position = transform.position;

            target.y = position.y;

            transform.position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);

            var direction = target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

            UpdateGrounding();
        }

        private void UpdateGrounding()
        {
            var isOnMesh = NavMesh.SamplePosition(transform.position, out var hit, groundSampleDistance, NavMesh.AllAreas);

            swimming = !isOnMesh || hit.mask == WATER_AREA_MASK;
            SetSwimming(swimming);

            var surfaceHeight = swimming ? waterLevel : hit.position.y;

            var position = transform.position;
            position.y = Mathf.Lerp(position.y, surfaceHeight, Time.deltaTime * groundSnapSpeed);
            transform.position = position;
        }

        private void UpdateWaterVisuals()
        {
            if (graphicsRoot != null)
            {
                var target = swimming ? (waterLevel + swimWaterLineOffset - transform.position.y) : 0f;

                var localPosition = graphicsRoot.localPosition;
                localPosition.y = Mathf.Lerp(localPosition.y, target, Time.deltaTime * 10f);
                graphicsRoot.localPosition = localPosition;
            }

            if (waterTrail != null)
            {
                if (swimming && !waterTrailPlaying)
                {
                    waterTrail.Play();
                    waterTrailPlaying = true;
                }
                else if (!swimming && waterTrailPlaying)
                {
                    waterTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    waterTrailPlaying = false;
                }
            }
        }

        private void PlaceOnIsland()
        {
            transform.position = islandPosition;
            transform.rotation = islandRotation;
        }

        private void PlaceAtBase()
        {
            transform.position = basePoint.position;
            transform.rotation = basePoint.rotation;
        }

        private void SetMoving(bool value)
        {
            if (graphicsAnimator != null)
                graphicsAnimator.SetBool(IS_MOVING_HASH, value);
        }

        private void SetSwimming(bool value)
        {
            if (graphicsAnimator != null)
                graphicsAnimator.SetBool(IS_SWIMMING_HASH, value);
        }

        private void SetSitting(bool value)
        {
            if (graphicsAnimator != null)
                graphicsAnimator.SetBool(IS_SITTING_HASH, value);
        }
        #endregion

        #region Trading
        public int ActiveOfferCount => traderSave != null ? traderSave.ActiveOfferIndices.Count : 0;

        public TraderOffer GetActiveOffer(int activeIndex)
        {
            if (traderSave == null || offersDatabase == null || activeIndex < 0 || activeIndex >= traderSave.ActiveOfferIndices.Count)
                return null;

            return offersDatabase.GetOffer(traderSave.ActiveOfferIndices[activeIndex]);
        }

        public int GetOfferRemaining(int activeIndex)
        {
            if (traderSave == null || activeIndex < 0 || activeIndex >= traderSave.OfferRemaining.Count)
                return 0;

            return traderSave.OfferRemaining[activeIndex];
        }

        public bool CanAfford(int activeIndex)
        {
            var offer = GetActiveOffer(activeIndex);
            if (offer == null || offer.Give == null)
                return false;

            for (var i = 0; i < offer.Give.Length; i++)
                if (!CurrencyController.HasAmount(offer.Give[i].currency, offer.Give[i].amount))
                    return false;

            return true;
        }

        public bool TryPurchase(int activeIndex)
        {
            if (CurrentPhase != Phase.AtBase || GetOfferRemaining(activeIndex) <= 0 || !CanAfford(activeIndex))
                return false;

            var offer = GetActiveOffer(activeIndex);
            if (offer == null)
                return false;

            for (var i = 0; i < offer.Give.Length; i++)
                CurrencyController.Substract(offer.Give[i].currency, offer.Give[i].amount, "trader");

            if (offer.Receive != null)
            {
                for (var i = 0; i < offer.Receive.Length; i++)
                    CurrencyController.Add(offer.Receive[i].currency, offer.Receive[i].amount, "trader");
            }

            traderSave.OfferRemaining[activeIndex]--;
            Save();

            OffersChanged?.Invoke();

            if (AreAllOffersExhausted() && !minigameSlot.IsBusy)
                StartSailingToIsland();

            return true;
        }

        private void OnMinigameSlotChanged()
        {
            MinigameChanged?.Invoke();
        }

        public void PlayMinigame()
        {
            minigameLauncher.TryLaunch(this);
        }

        public bool TryGetStage(MinigameStageType stageType, out MinigameStageAnchors anchors)
        {
            anchors = null;

            if (minigamePlayerPoint == null)
                return false;

            switch (stageType)
            {
                case MinigameStageType.Floor:
                    if (minigameFloorAnchor == null)
                        return false;

                    anchors = new MinigameStageAnchors(minigameFloorAnchor, minigamePlayerPoint);

                    return true;

                case MinigameStageType.Table:
                    if (minigameTableAnchor == null)
                        return false;

                    anchors = new MinigameStageAnchors(minigameTableAnchor, minigamePlayerPoint);

                    return true;

                default:
                    return false;
            }
        }

        private bool AreAllOffersExhausted()
        {
            if (traderSave.OfferRemaining.Count == 0)
                return false;

            for (var i = 0; i < traderSave.OfferRemaining.Count; i++)
                if (traderSave.OfferRemaining[i] > 0)
                    return false;

            return true;
        }

        private void OpenTradeWindow()
        {
            if (CurrentPhase != Phase.AtBase)
                return;

            var page = UIController.GetPage<UITrader>();
            page.SetTrader(this);

            UIController.ShowPage<UITrader>();
        }

        private void CloseTradeWindowIfOpen()
        {
            var page = UIController.GetPage<UITrader>();
            if (page != null && page.IsPageDisplayed && page.CurrentTrader == this)
                UIController.HidePage<UITrader>();
        }
        #endregion

        private float GetRandomRestTime()
        {
            return Random.Range(Mathf.Min(minRestTime, maxRestTime), Mathf.Max(minRestTime, maxRestTime));
        }

        private float GetRandomTradeTime()
        {
            return Random.Range(Mathf.Min(minTradeTime, maxTradeTime), Mathf.Max(minTradeTime, maxTradeTime));
        }

        private void Save()
        {
            SaveController.MarkAsSaveIsRequired();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            DrawRouteGizmo(transform.position, routeWaypoints, basePoint != null ? basePoint.position : (Vector3?)null);

            if (rescueReturnWaypoints.IsNullOrEmpty())
                return;

            Gizmos.color = Color.yellow;
            DrawRouteGizmo(transform.position, rescueReturnWaypoints, null, true);
        }

        private static void DrawRouteGizmo(Vector3 start, Transform[] waypoints, Vector3? destination, bool reversed = false)
        {
            var previous = start;

            if (!waypoints.IsNullOrEmpty())
            {
                for (var i = 0; i < waypoints.Length; i++)
                {
                    var waypoint = waypoints[reversed ? waypoints.Length - 1 - i : i];

                    if (waypoint == null)
                        continue;

                    Gizmos.DrawLine(previous, waypoint.position);
                    Gizmos.DrawWireSphere(waypoint.position, 0.4f);

                    previous = waypoint.position;
                }
            }

            if (!destination.HasValue)
                return;

            Gizmos.DrawLine(previous, destination.Value);
            Gizmos.DrawWireSphere(destination.Value, 0.5f);
        }

        private enum Phase
        {
            Idle = 0,
            SailingIn = 1,
            AtBase = 2,
            SailingOut = 3,
        }

        private enum RescueState
        {
            None = 0,
            Captured = 1,
            RunningToPlayer = 2,
            Pausing = 3,
            GoingHome = 4,
        }
    }
}
