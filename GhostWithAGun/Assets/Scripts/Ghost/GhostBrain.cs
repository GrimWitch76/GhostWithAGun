using UnityEngine;
public enum GhostStates
{
    Wander,
    TargetedWander,
    Chase,
    Search,
    InvestigateSound,
    InvestigateRoom,
    RetrieveGun,
    ArmedChase
}

public class GhostBrain : MonoBehaviour
{
    [SerializeField] private GhostTuning _tuning; // << plug night profile here

    [Header("Runtime")]
    [SerializeField] private float _suspicion = 0f;
    [SerializeField] private float _frustration = 0f;
    [SerializeField] private bool _hasGun = false;

    [Header("Refs")]
    [SerializeField] private VisionSensor _vision;
    [SerializeField] private GhostMovement _movement;
    [SerializeField] private GhostWeaponController gun;
    //[SerializeField] private GhostCollisionController _collisionController;
    [SerializeField] private GameObject _gunRoot;
    [SerializeField] private Transform gunPickupPoint;
    private PlayerController _player;


    [Header("Debug Gizmos")]
    [SerializeField] private bool _drawDebugGizmos = true;
    [SerializeField] private Color _targetColor = Color.cyan;
    [SerializeField] private Color _suspectColor = Color.magenta;
    [SerializeField] private Color _wanderRadiusColor = Color.green;
    [SerializeField] private Color _targetedRadiusColor = Color.blue;

    private GhostStates _currentState = GhostStates.Wander;
    private Vector3 lastKnownPlayerPos;
    public float Suspicion => _suspicion;
    public float Frustration => _frustration;
    public bool HasGun => _hasGun;
    public GhostTuning Tuning => _tuning;

    public GhostWeaponController Gun => gun;

    private float shootTimer = 0f;
    private float searchTimer;
    private float lookTimer;
    private float targetedWanderTimer;
    private float targetedWanderRadius;
    private float tightenTimer;
    private bool _everSeenPlayer;
    // For sound handling priority
    private float _currentSoundPriority;

    // where we dropped the gun
    private Vector3 _droppedGunPos;
    private bool _gunDroppedThisCycle;

    void Start()
    {
        _everSeenPlayer = false;
        _movement.StartWander();
        ApplyTuningToSensors();
        _player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        // Suspicion decay when no strong stimuli
        if (!_vision.CanSeePlayer && _currentSoundPriority <= 1f)
        {
            _suspicion = Mathf.Max(0, _suspicion - _tuning.suspicionDecay * Time.deltaTime);
            AccumulateFrustrationIdle();
        }
        else
        {
            _frustration = Mathf.Max(0, _frustration - _tuning.frustrationDecayPerSecondWhenStimulated * Time.deltaTime);
        }

        // State update
        switch (_currentState)
        {
            case GhostStates.Wander: HandleWander(); break;
            case GhostStates.Chase: HandleChase(); break;
            case GhostStates.Search: HandleSearch(); break;
            case GhostStates.InvestigateSound: HandleInvestigateSound(); break;
            case GhostStates.InvestigateRoom: HandleInvestigateRoom(); break; // NEW
            case GhostStates.RetrieveGun: HandleRetrieveGun(); break;
            case GhostStates.ArmedChase: HandleArmedChase(); break;
            case GhostStates.TargetedWander: HandleTargetedWander(); break;
        }
    }

    private void AccumulateFrustrationIdle()
    {
        float add = (_currentState == GhostStates.Wander)
            ? _tuning.frustrationGainPerSecondWandering
            : _tuning.frustrationGainPerSecondSearching;

        _frustration = Mathf.Min(_tuning.frustrationMax, _frustration + add * Time.deltaTime);

        // Trigger “drop gun & confirm” if we’re armed and frustrated enough
        if (_hasGun && !_gunDroppedThisCycle && _frustration >= _tuning.frustrationToDropGun)
        {
            DropGunAndInvestigate();
        }
    }

    private void DropGunAndInvestigate()
    {
        // “Drop” the weapon: disable gun root, remember location
        _droppedGunPos = transform.position;
        _gunRoot.SetActive(false);
        _hasGun = false;
        _gunDroppedThisCycle = true;
        //_collisionController.SetDoorCollision(false);

        // Move to last known player pos and search thoroughly unarmed
        SetState(GhostStates.InvestigateRoom);
        searchTimer = _tuning.investigateRoomDuration;
        lookTimer = 0f;

        if (lastKnownPlayerPos != Vector3.zero)
            _movement.MoveToPoint(lastKnownPlayerPos);
        else
            StartTargetedWander();
    }

    public void ReArm()
    {
        SetState(GhostStates.RetrieveGun);
        // Prefer the dropped gun if near; otherwise the pickup point
        var target = (_droppedGunPos != Vector3.zero) ? _droppedGunPos : gunPickupPoint.position;
        _movement.MoveToPoint(target);
    }

    private void HandleInvestigateRoom()
    {
        // If we see the player at any point, escalate: re-arm and break in
        if (_vision.CanSeePlayer)
        {
            lastKnownPlayerPos = _vision.LastSeenPosition;
            ReArm();
            return;
        }

        // Sweep pattern: look around while at destination, then request nearby points
        if (_movement.AtDestination())
        {
            searchTimer -= Time.deltaTime;
            lookTimer -= Time.deltaTime;

            if (lookTimer <= 0f)
            {
                lookTimer = _tuning.lookAroundInterval;
                _movement.LookRandomDirection();
            }

            if (searchTimer <= 0f)
            {
                // Done confirming; re-arm to continue normal hunt flow
                ReArm();
            }
            else
            {
                // Ask movement to pick a few nearby sweep points
                _movement.SearchNearby(lastKnownPlayerPos, radius: 5f, count: 3);
            }
        }
    }

    public void OnHeardSound(SoundEvent sound, float effectiveStrength)
    {
        // Only replace priority if higher
        if (sound.Importance > _currentSoundPriority)
            _currentSoundPriority = sound.Importance;

        AddSuspicion(effectiveStrength * 10f, sound.Source.transform.position);

        switch (_currentState)
        {
            case GhostStates.Wander:
            case GhostStates.Search:
            case GhostStates.InvestigateRoom:
                lastKnownPlayerPos = sound.Position;
                SetState(GhostStates.InvestigateSound);
                _movement.MoveToPoint(lastKnownPlayerPos);

                // If we dropped the gun but hear a very high priority sound (3–4),
                // you can optionally short-circuit to ReArm() here based on tuning.
                break;

            case GhostStates.Chase:
            case GhostStates.ArmedChase:
                // Ignore while we have visual
                break;
        }
    }

    private void HandleWander()
    {
        if (_vision.CanSeePlayer && _suspicion >= _tuning.suspicionThresholdChase)
        {
            if (_hasGun) SetState(GhostStates.ArmedChase);
            else SetState(GhostStates.Chase);
        }
    }

    private void HandleTargetedWander()
    {
        // Basic lifetime
        targetedWanderTimer += Time.deltaTime;
        tightenTimer += Time.deltaTime;

        // Shrink search radius over time
        if (tightenTimer >= _tuning.targetedWanderTightenInterval)
        {
            tightenTimer = 0f;
            targetedWanderRadius *= _tuning.targetedWanderTightenRate;
            targetedWanderRadius = Mathf.Max(targetedWanderRadius, _tuning.targetedWanderMinRadius);
        }

        // Pick random nearby destination if idle
        if (_movement.AtDestination())
        {
            Vector3 playerPos = _player.gameObject.transform.position;
            Vector3 offset = Random.insideUnitSphere * targetedWanderRadius;
            offset.y = 0;
            _movement.MoveToPoint(playerPos + offset);
        }

        // Exit conditions
        if (targetedWanderTimer >= _tuning.targetedWanderDuration)
        {
            SetState(GhostStates.Wander);
            _movement.StartWander();
        }

        // Reactivity
        if (_vision.CanSeePlayer)
        {
            _everSeenPlayer = true;
            ReArm(); // resume normal chase flow
        }
    }

    private void HandleChase()
    {
        if (_suspicion >= _tuning.suspicionMax && !_hasGun)
        {
            SetState(GhostStates.RetrieveGun);
            _movement.MoveToPoint(gunPickupPoint.position);
            return;
        }

        if (_suspicion <= 0)
        {
            SetState(GhostStates.Wander);
            _movement.StartWander();
            return;
        }

        if (_vision.CanSeePlayer)
        {
            lastKnownPlayerPos = _vision.LastSeenPosition;
            _movement.MoveToPoint(lastKnownPlayerPos);
        }
        else
        {
            SetState(GhostStates.Search);
            searchTimer = _tuning.searchDuration;
            lookTimer = 0f;
            _movement.MoveToPoint(lastKnownPlayerPos);
        }
    }

    private void HandleSearch()
    {
        if (_vision.CanSeePlayer && _suspicion >= _tuning.suspicionThresholdChase)
        {
            SetState(_hasGun ? GhostStates.ArmedChase : GhostStates.Chase);
            return;
        }

        if (_movement.AtDestination())
        {
            searchTimer -= Time.deltaTime;
            lookTimer -= Time.deltaTime;

            if (lookTimer <= 0f)
            {
                lookTimer = _tuning.lookAroundInterval;
                _movement.LookRandomDirection();
            }

            if (searchTimer <= 0f)
            {
                SetState(GhostStates.Wander);
                _movement.StartWander();
            }
            else
            {
                // optional micro-search points
                _movement.SearchNearby(lastKnownPlayerPos, radius: 4f, count: 2);
            }
        }
    }

    private void HandleInvestigateSound()
    {
        if (_vision.CanSeePlayer && _suspicion >= _tuning.suspicionThresholdChase)
        {
            SetState(_hasGun ? GhostStates.ArmedChase : GhostStates.Chase);
            return;
        }

        if (_movement.AtDestination())
        {
            searchTimer -= Time.deltaTime;
            lookTimer -= Time.deltaTime;

            if (lookTimer <= 0f)
            {
                lookTimer = _tuning.lookAroundInterval;
                _movement.LookRandomDirection();
            }

            if (searchTimer <= 0f)
            {
                _currentSoundPriority = 0;
                SetState(GhostStates.Wander);
                _movement.StartWander();
            }
        }
    }

    private void HandleRetrieveGun()
    {
        if (_movement.AtDestination())
        {
            _hasGun = true;
            //_collisionController.SetDoorCollision(true); // re-enable collisions

            _gunRoot.SetActive(true);
            gun.PlayPickUpSfx();

            // After re-arming, go toward last known pos and Search/Chase
            if (lastKnownPlayerPos != Vector3.zero)
            {
                _movement.MoveToPoint(lastKnownPlayerPos);
                SetState(GhostStates.Search);
            }
            else
            {
                SetState(GhostStates.Wander);
                _movement.StartWander();
            }
        }
    }

    private void HandleArmedChase()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        if (_vision.CanSeePlayer)
        {
            Vector3 playerPos = _vision.LastSeenPosition;
            float dist = Vector3.Distance(transform.position, playerPos);

            if (dist > _tuning.shootRange * 0.9f)
            {
                _movement.MoveToPoint(playerPos);
            }
            else
            {
                _movement.Stop();
                _movement.LookAt(playerPos);

                if (shootTimer <= 0f)
                {
                    if (gun.CanFire)
                    {
                        gun.Fire(origin, playerPos);
                        Debug.Log("Bang");
                    } 
                    else gun.Reload();

                    shootTimer = _tuning.shootCooldown;
                }
            }
        }
        else
        {
            SetState(GhostStates.Search);
            _movement.MoveToPoint(_vision.LastSeenPosition);
        }

        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
    }

    public void AddSuspicion(float amount, Vector3 source)
    {
        _suspicion = Mathf.Min(_suspicion + amount, _tuning.suspicionMax);
        lastKnownPlayerPos = source;
    }

    private void ApplyTuningToSensors()
    {
        if (_vision != null)
            _vision.ApplyTuning(_tuning);
    }

    private void SetState(GhostStates newState)
    {
        _currentState = newState;
    }
    private void StartTargetedWander()
    {
        SetState(GhostStates.TargetedWander);
        targetedWanderTimer = 0f;
        tightenTimer = 0f;
        targetedWanderRadius = _tuning.targetedWanderMaxRadius;

        // initial move near player
        Vector3 playerPos = _player.gameObject.transform.position;
        Vector3 offset = Random.insideUnitSphere * targetedWanderRadius;
        offset.y = 0;
        _movement.MoveToPoint(playerPos + offset);
    }



    private void OnDrawGizmos()
    {
        if (!_drawDebugGizmos) return;

        // --- Suspicion bar (existing) ---
        Gizmos.color = Color.Lerp(Color.green, Color.red, _suspicion / _tuning.suspicionMax);
        Gizmos.DrawCube(transform.position + Vector3.up * 2f, new Vector3(1f, 0.2f, 0.2f));

        // --- Target position ---
        if (_movement != null)
        {
            Vector3 target = _movement.GetDestination();
            Gizmos.color = _targetColor;
            Gizmos.DrawSphere(target + Vector3.up * 0.2f, 0.2f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, target + Vector3.up * 0.2f);
        }

        // --- Last known player position ---
        if (lastKnownPlayerPos != Vector3.zero)
        {
            Gizmos.color = _suspectColor;
            Gizmos.DrawSphere(lastKnownPlayerPos + Vector3.up * 0.2f, 0.25f);
        }

        // --- Wander / targeted wander radius ---
        if (_currentState == GhostStates.Wander)
        {
            Gizmos.color = _wanderRadiusColor;
            Gizmos.DrawWireSphere(transform.position, _tuning.roamRadius);
        }
        else if (_currentState == GhostStates.TargetedWander)
        {
            Gizmos.color = _targetedRadiusColor;
            Gizmos.DrawWireSphere(_player.transform.position, targetedWanderRadius);
        }

        // --- Gun range ---
        if (HasGun && gun != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, _tuning.shootRange);

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _tuning.shootRange);
#endif
        }
    }

}