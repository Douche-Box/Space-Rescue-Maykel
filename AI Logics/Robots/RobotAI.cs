using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class RobotAI : Entity
{
    [SerializeField] PlayManager _playManager;
    public PlayManager PlayManager
    { get { return _playManager; } }

    [Header("Setup")]
    [SerializeField] RobotSO _robotInfo;
    public RobotSO RobotInfo
    { get { return _robotInfo; } }

    [SerializeField] bool _isInitialized;

    [SerializeField] RobotManager _manager;

    [SerializeField] NavMeshAgent _agent;
    public NavMeshAgent Agent
    { get { return _agent; } }

    [SerializeField] Animator _bodyAnimator;
    public Animator BodyAnimator
    { get { return _bodyAnimator; } }
    [SerializeField] Animator _weaponAnimator;
    public Animator WeaponAnimator
    { get { return _weaponAnimator; } }

    [SerializeField] Transform _target;
    public Transform Target
    { get { return _target; } set { _target = value; } }

    [SerializeField] PlayerController _player;
    public PlayerController Player
    { get { return _player; } }

    [SerializeField] Collider _collider;

    [SerializeField] Transform _scrapRobot;

    [SerializeField] bool _startUp;

    [SerializeField] bool _isAttacking;
    public bool IsAttacking
    { get { return _isAttacking; } }

    [SerializeField] float _distanceFromTarget;
    public float DistanceFromTarget
    { get { return _distanceFromTarget; } set { _distanceFromTarget = value; } }

    [SerializeField] float _checkRadius;
    public float CheckRadius
    { get { return _checkRadius; } }
    [SerializeField] float _groundedCheckRadius;
    public float GroundedCheckRadius
    { get { return _groundedCheckRadius; } }

    [SerializeField] LayerMask _detectionLayer;
    public LayerMask DetectionLayer
    { get { return _detectionLayer; } }


    [SerializeField] LayerMask _groundMask;
    public LayerMask GroundMask
    { get { return _groundMask; } }

    [SerializeField] bool _isThrown;
    public bool IsThrown
    { get { return _isThrown; } }

    [SerializeField] GameObject _deathEffectPrefab;

    [SerializeField] float _playerStopDistance;
    public float PlayerStopDistance
    { get { return _playerStopDistance; } set { _playerStopDistance = value; } }

    [SerializeField] bool _isInsideSquad;

    [SerializeField] VisualEffect _attackEffect;
    public VisualEffect AttackEffect
    { get { return _attackEffect; } }

    [SerializeField] AudioSource _attackSource;
    public AudioSource AttackSource
    { get { return _attackSource; } }
    [SerializeField] AudioClip _attackClip;
    public AudioClip AttackClip
    { get { return _attackClip; } }
    [SerializeField] AudioClip _deathClip;

    public override void Start()
    {
        _playManager = FindObjectOfType<PlayManager>();

        if (_robotInfo != null)
        {
            _agent.speed = _robotInfo.speed;
        }

        _manager = FindAnyObjectByType<RobotManager>();

        _player = FindAnyObjectByType<PlayerController>();

        _scrapRobot = FindAnyObjectByType<ScrapRobot>().transform;

        _bodyAnimator = GetComponentInChildren<Animator>();
    }

    public void Activate()
    {
        _agent.enabled = true;
        _startUp = true;
    }

    public override void Update()
    {
        if (_startUp)
        {
            CheckState();
        }
    }

    public override void Death()
    {
        GameObject effect = Instantiate(_deathEffectPrefab);

        effect.transform.position = transform.position;
        effect.GetComponent<AudioSource>().clip = _deathClip;
        effect.GetComponent<AudioSource>().Play();

        Destroy(effect, 1f);

        _manager.RemoveRobotFromSquad(this);

        _playManager.OnRobotDeath(this);

        base.Death();
    }

    /// <summary>
    /// Initialize the robot info from robot data
    /// </summary>
    public virtual void InitializeRobotInfo()
    {
        if (_robotInfo != null && !_isInitialized)
        {
            _isInitialized = true;

            maxHealth = _robotInfo.health;
            health = maxHealth;

            speed = _robotInfo.speed;
            if (_agent != null) { _agent.speed = speed; }

            damage = _robotInfo.damage;
        }
    }

    public State _currentState;

    public enum State
    {
        IDLE,
        FOLLOW,
        ATTACK,
        THROWN,
        GATHER,
    }

    /// <summary>
    /// Changes current state for the robot and exits logic of the previous state
    /// </summary>
    /// <param name="newState"></param>
    public virtual void ChangeState(State newState)
    {
        switch (_currentState)
        {
            case State.IDLE:
                StopIdle();
                break;
            case State.FOLLOW:
                StopFollow();
                break;
            case State.ATTACK:
                StopAttack();
                break;
            case State.THROWN:
                StopThrown();
                break;
            case State.GATHER:
                StopGather();
                break;
            default:
                Debug.Log($"{newState} is not supported on this robot");
                break;
        }

        switch (newState)
        {
            case State.IDLE:
                StartIdle();
                break;
            case State.FOLLOW:
                StartFollow();
                break;
            case State.ATTACK:
                StartAttack();
                break;
            case State.THROWN:
                StartThrown();
                break;
            case State.GATHER:
                StartGather();
                break;
            default:
                Debug.Log($"{newState} is not supported on this robot");
                break;
        }
    }

    /// <summary>
    /// Updates the robot state
    /// </summary>
    public virtual void CheckState()
    {
        switch (_currentState)
        {
            case State.IDLE:
                Idle();
                break;
            case State.FOLLOW:
                Follow();
                break;
            case State.ATTACK:
                Attack();
                break;
            case State.THROWN:
                Thrown();
                break;
            case State.GATHER:
                Gather();
                break;
        }
    }

    #region Idle

    public virtual void StartIdle()
    {
        _currentState = State.IDLE;

        _bodyAnimator.SetBool("Walking", false);
    }

    public virtual void Idle()
    {

    }

    public virtual void StopIdle()
    {

    }

    #endregion

    #region Follow

    /// <summary>
    /// Follow the player 
    /// </summary>
    public virtual void StartFollow()
    {
        _currentState = State.FOLLOW;

        if (_player != null)
        {
            _target = _player.GetComponent<PlayerController>().SquadRangePos;
        }
        else
        {
            _player = FindAnyObjectByType<PlayerController>();
            _target = _player.GetComponent<PlayerController>().SquadRangePos;
        }

        _agent.enabled = true;

        _agent.stoppingDistance = _robotInfo.followDistance;
    }

    public virtual void Follow()
    {
        _playerStopDistance = _player.RobotRemoveDistance;

        if (_target != null)
        {
            _agent.SetDestination(_target.position);

            Vector3 targetWithOffset = new Vector3(_target.position.x, transform.position.y, _target.position.z);

            _distanceFromTarget = Vector3.Distance(transform.position, targetWithOffset);


            if (_playerStopDistance >= _distanceFromTarget && _isInsideSquad)
            {
                _agent.isStopped = true;
                _bodyAnimator.SetBool("Walking", false);
            }
            else
            {
                _agent.isStopped = false;
                _bodyAnimator.SetBool("Walking", true);
            }
        }
    }

    public virtual void StopFollow()
    {

    }

    #endregion

    #region Attack

    public virtual void StartAttack()
    {
        _currentState = State.ATTACK;

        _agent.stoppingDistance = _robotInfo.range;

        _bodyAnimator.SetBool("Walking", false);

        if (_target != null)
        {
            _target = _target.GetComponentInParent<EnemyAI>().transform;
        }
    }

    /// <summary>
    /// Start attacking the enemy
    /// </summary>
    public virtual void Attack()
    {
        if (_target != null)
        {
            _distanceFromTarget = Vector3.Distance(transform.position, _target.position);
            transform.LookAt(_target);
        }

        if (_agent.stoppingDistance > _distanceFromTarget && !_isAttacking)
        {
            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
            }

            StartCoroutine(StartAttacking());
        }
        else
        {
            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = false;
            }
        }
    }

    public virtual void StopAttack()
    {
        StartCoroutine(StopAttacking());
    }

    public virtual IEnumerator StartAttacking()
    {
        _isAttacking = true;

        _weaponAnimator.SetBool("Ready", true);

        yield return new WaitForSeconds(_robotInfo.windUpTime);

        StartCoroutine(Attacking());
    }

    public virtual IEnumerator Attacking() // make sure it stops attacking before changing state
    {
        if (_target != null)
        {
            _target.GetComponent<Entity>().TakeDamage(_robotInfo.damage); // this still happens with this because it tries to attack the player ( probably )
            _attackEffect.enabled = true;
            _attackEffect.Play();
            _attackSource.Play();

            _bodyAnimator.SetTrigger("Attack");
            _weaponAnimator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(_robotInfo.fireRate);

        if (_isAttacking)
        {
            StartCoroutine(Attacking());
        }
    }

    public virtual IEnumerator StopAttacking()
    {
        _weaponAnimator.SetBool("Ready", false);

        yield return new WaitForSeconds(_robotInfo.windDownTime);
        _isAttacking = false;
    }

    #endregion

    #region Thrown

    public virtual void StartThrown()
    {
        _currentState = State.THROWN;

        _agent.enabled = false;
        _collider.isTrigger = true;

        _isThrown = true;
    }

    public virtual void Thrown()
    {
        if (_isThrown)
        {
            CheckForEntityInRange(_checkRadius);
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.1f, _groundMask))
        {
            Debug.Log("Hit Ground");
            ChangeState(State.IDLE);
            CheckForEntityInRange(_groundedCheckRadius);
        }
    }

    public virtual void StopThrown()
    {
        _agent.enabled = true;
        _collider.isTrigger = false;
    }

    #endregion

    #region Gather

    public virtual void StartGather()
    {
        _currentState = State.GATHER;

        GetNewGatherTargetPosition();

        _agent.stoppingDistance = 0.1f;
    }

    /// <summary>
    /// Gather scrap and bring it back to the scrap robot
    /// </summary>
    public virtual void Gather()
    {
        if (_target != null)
        {
            if (!_target.GetComponentInParent<Scrap>().canGrabScrap)
            {
                ChangeState(State.IDLE);
            }
            if (_agent.isActiveAndEnabled)
            {
                _bodyAnimator.SetBool("Walking", true);

                _agent.SetDestination(_target.position);
            }

            Vector3 targetWithOffset = new Vector3(_target.position.x, transform.position.y, _target.position.z);

            _distanceFromTarget = Vector3.Distance(transform.position, targetWithOffset);

            if (_agent.stoppingDistance >= _distanceFromTarget && _agent.isActiveAndEnabled && !_agent.isStopped)
            {
                Debug.Log("Stop");

                _bodyAnimator.SetBool("Walking", false);

                _target.GetComponentInParent<Scrap>().AddRobot(this);

                _agent.enabled = false;

                transform.SetParent(_target);
            }
        }
    }

    public virtual void StopGather()
    {
        transform.SetParent(null);

        if (_target != null)
        {
            _target.GetComponentInParent<Scrap>().RemoveRobot(this);
        }

        _agent.enabled = true;
    }

    public virtual void GetNewGatherTargetPosition()
    {
        _target = _target.GetComponentInParent<Scrap>().GetGatherPosition(this);

        if (_target == null)
        {
            ChangeState(State.IDLE);
        }
    }

    public void ChangeGatherTarget()
    {
        _agent.enabled = true;

        _target.GetComponentInParent<Scrap>().RemoveRobot(this);

        _target = _target.GetComponentInParent<Scrap>().GetGatherPosition(this);
    }

    #endregion

    public virtual void Recal()
    {
        ChangeState(State.FOLLOW);
    }

    public void EnterSquad()
    {
        _isInsideSquad = true;
        _bodyAnimator.SetBool("Walking", false);
    }

    public void LeaveSquad()
    {
        if (_agent.isActiveAndEnabled)
        {
            _isInsideSquad = false;
        }
    }

    public void RemoveAttachMent()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        _agent.enabled = true;

        _collider.isTrigger = false;

        ChangeState(State.IDLE);
    }

    /// <summary>
    /// Checking for entities in range like scrap or enemies
    /// </summary>
    /// <param name="radius"></param>
    public virtual void CheckForEntityInRange(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, _detectionLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            switch (colliders[0].GetComponentInParent<Entity>().entityType)
            {
                case EntityType.SCRAP:
                    _target = colliders[0].transform;

                    ChangeState(State.GATHER);
                    break;
                case EntityType.ENEMY:
                    if (colliders[0].GetComponentInParent<Entity>().health > 0)
                    {
                        _target = colliders[0].transform;
                        ChangeState(State.ATTACK);
                    }
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_currentState == State.THROWN)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("Hit ENEMY");

                other.GetComponentInParent<EnemyAI>().TakeDamage(_robotInfo.impactDamage);

                ChangeState(State.ATTACK);
            }
        }
    }

    public virtual void CollectScrapAtBase()
    {
        ChangeState(State.FOLLOW);
    }

    public void OnValidate()
    {
        InitializeRobotInfo();
    }
}
