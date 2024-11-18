using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ScrapRobot : RobotAI
{
    [SerializeField] float _spawnTime;

    [SerializeField] GameObject[] _robotPrefabs;

    [SerializeField] GameObject _robotToSpawn;

    [SerializeField] Transform _spawnPosition;

    [SerializeField] Transform _grabBone;

    [SerializeField] Transform _grabPosition;
    public Transform GrabPosition
    { get { return _grabBone; } }

    [SerializeField] Transform _scrapToCollect;

    [SerializeField] float _stopDistance;

    [SerializeField] bool _isSpawning;

    public bool hasPoweredOn;

    public override void Start()
    {
        base.Start();

        _robotToSpawn = _robotPrefabs[0];

        Target = Player.GetComponent<PlayerController>().SquadRangePos;
    }

    public override void Update()
    {
        base.Update();

        CheckForEntityInRange(8);
    }

    public override void CheckForEntityInRange(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, DetectionLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[0].GetComponentInParent<Entity>().entityType == EntityType.SCRAP)
            {
                _scrapToCollect = colliders[0].transform;

                ChangeState(State.GATHER);
            }
        }
    }

    /// <summary>
    /// Change robot state
    /// </summary>
    /// <param name="newState"></param>
    public override void ChangeState(State newState)
    {
        switch (_currentState)
        {
            case State.IDLE:
                StopIdle();
                break;
            case State.FOLLOW:
                StopFollow();
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
            case State.GATHER:
                StartGather();
                break;
            default:
                Debug.Log($"{newState} is not supported on this robot");
                break;
        }
    }

    /// <summary>
    /// Update robot state
    /// </summary>
    public override void CheckState()
    {
        switch (_currentState)
        {
            case State.IDLE:
                Idle();
                break;
            case State.FOLLOW:
                Follow();
                break;
            case State.GATHER:
                Gather();
                break;
        }
    }

    public override void StartIdle()
    {
        _currentState = State.IDLE;

        BodyAnimator.SetBool("Walking", false);

        if (Agent.isActiveAndEnabled && !Agent.isStopped)
        {
            Agent.isStopped = true;
        }
    }

    public override void StartFollow()
    {
        _currentState = State.FOLLOW;

        BodyAnimator.SetTrigger("Power");

        Target = Player.GetComponent<PlayerController>().SquadRangePos;

        Agent.enabled = true;
    }

    public override void Follow()
    {
        if (hasPoweredOn)
        {
            base.Follow();
        }
    }

    /// <summary>
    /// Stops and starts the scrap robot when recalling
    /// </summary>
    public override void Recal()
    {
        if (_currentState == State.IDLE)
        {
            ChangeState(State.FOLLOW);
        }
        else if (_currentState == State.FOLLOW)
        {
            ChangeState(State.IDLE);
        }
    }

    /// <summary>
    /// Collect scrap that is brought by robots gathering scrap
    /// </summary>
    /// <param name="scrapWorth"></param>
    /// <param name="scrap"></param>
    public void CollectScrap(int scrapWorth, Transform scrap)
    {
        if (!_isSpawning)
        {
            scrap.GetComponent<NavMeshAgent>().enabled = false;

            scrap.position = _grabBone.position;

            scrap.SetParent(_grabBone);

            WeaponAnimator.SetTrigger("Oppakken");


            StartCoroutine(SpawnNewRobot(scrapWorth, scrap));
        }
    }

    /// <summary>
    /// Spawn new robots based on scrap worth
    /// </summary>
    /// <param name="robotsToSpawn"></param>
    /// <param name="scrap"></param>
    /// <returns></returns>
    IEnumerator SpawnNewRobot(int robotsToSpawn, Transform scrap)
    {
        _isSpawning = true;
        WeaponAnimator.SetBool("Maken", true);

        scrap.localScale = Vector3.one * scrap.GetComponent<Scrap>().scrapSizeModified;

        for (int i = 0; i < robotsToSpawn; i++)
        {
            yield return new WaitForSeconds(_spawnTime);

            GameObject newRobot = Instantiate(_robotToSpawn);

            newRobot.transform.position = _spawnPosition.position;

            newRobot.GetComponent<RobotAI>().ChangeState(State.FOLLOW);

            PlayManager.OnRobotSpawned(newRobot.GetComponent<RobotAI>());
        }

        Destroy(scrap.gameObject); // do not do when removed

        WeaponAnimator.SetBool("Maken", false);
        _isSpawning = false;
    }

    /// <summary>
    /// Makes sure you always have a robot to kill enemies 
    /// </summary>
    public void RobotFailSafe()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject newRobot = Instantiate(_robotPrefabs[0]);

            newRobot.transform.position = _spawnPosition.position;

            PlayManager.OnRobotSpawned(newRobot.GetComponent<RobotAI>());
        }
    }

    public void ChangeRobot(int robotIndex)
    {
        _robotToSpawn = _robotPrefabs[robotIndex];
    }
}
