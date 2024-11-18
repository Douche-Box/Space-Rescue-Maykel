using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Scrap : Entity
{
    [SerializeField] TMP_Text _robotsNeededTxt;

    [SerializeField] Transform _objectToCarry;

    [SerializeField] NavMeshAgent _agent;

    public float scrapSizeModified;

    [SerializeField] int _robotsToCarry;
    [SerializeField] int _extraRobots;

    [SerializeField] int _maxRobots;

    [SerializeField] int _robotsCarrying;

    [SerializeField] float _extraRobotPercent;

    [SerializeField] float _radius;
    [SerializeField] float _extraRadius;

    [SerializeField] Transform _player;

    [SerializeField] Transform _target;

    [SerializeField] List<Transform> _robotPos = new List<Transform>();
    [SerializeField] List<Transform> _extraRobotPos = new List<Transform>();

    [SerializeField] RobotAI[] _holdPos;
    [SerializeField] RobotAI[] _extraHoldPos;

    [SerializeField] RobotAI[] _atPos;
    [SerializeField] RobotAI[] _extraAtPos;

    [SerializeField] float _distanceFromTarget;

    [SerializeField] int _scrapWorth;

    [SerializeField] ScrapRobot _scrapRobot;

    [SerializeField] float _deliverRange;

    [SerializeField] bool _collectingScrap;
    public bool canGrabScrap;

    [SerializeField] bool _isWalking;

    public override void Start()
    {
        GeneratePositionTransforms();

        _agent.speed = speed;

        _scrapRobot = FindAnyObjectByType<ScrapRobot>();

        _target = _scrapRobot.GrabPosition;

        canGrabScrap = true;
    }

    // Scrap logic 
    public override void Update()
    {
        if (_robotsCarrying >= _robotsToCarry)
        {
            if (_agent.isActiveAndEnabled)
            {
                _agent.SetDestination(_target.position);

                if (!_isWalking)
                {
                    for (int i = 0; i < _atPos.Length; i++)
                    {
                        if (_atPos[i] != null)
                        {
                            _atPos[i].BodyAnimator.SetBool("Walking", true);
                        }
                    }
                    for (int i = 0; i < _extraAtPos.Length; i++)
                    {
                        if (_extraAtPos[i] != null)
                        {
                            _extraAtPos[i].BodyAnimator.SetBool("Walking", true);
                        }
                    }
                }

                _isWalking = true;

                _agent.isStopped = false;
            }

            Vector3 targetWithOffset = new Vector3(_target.position.x, transform.position.y, _target.position.z);

            _distanceFromTarget = Vector3.Distance(transform.position, targetWithOffset);
            // Collecting scrap
            if (_deliverRange >= _distanceFromTarget && _agent.isActiveAndEnabled && !_agent.isStopped)
            {
                canGrabScrap = false;

                for (int i = 0; i < _atPos.Length; i++)
                {
                    if (_atPos[i] != null)
                    {
                        _atPos[i].transform.localScale = Vector3.one;
                        _atPos[i].CollectScrapAtBase();
                        _atPos[i] = null;

                        if (_holdPos[i] != null)
                        {
                            _holdPos[i].transform.localScale = Vector3.one;
                            _holdPos[i].CollectScrapAtBase();
                            _holdPos[i] = null;
                        }
                    }

                }
                for (int i = 0; i < _extraAtPos.Length; i++)
                {
                    if (_extraAtPos[i] != null)
                    {
                        _extraAtPos[i].transform.localScale = Vector3.one;
                        _extraAtPos[i].CollectScrapAtBase();
                        _extraAtPos[i] = null;

                        if (_extraHoldPos[i] != null)
                        {
                            _extraHoldPos[i].transform.localScale = Vector3.one;
                            _extraHoldPos[i].CollectScrapAtBase();
                            _extraHoldPos[i] = null;
                        }
                    }
                }

                _collectingScrap = true;

                _robotsCarrying = 0;

                _scrapRobot.CollectScrap(_scrapWorth, transform);
            }
        }
        else if (_robotsCarrying < _robotsToCarry && _agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;

            if (_isWalking)
            {
                for (int i = 0; i < _atPos.Length; i++)
                {
                    if (_atPos[i] != null)
                    {
                        _atPos[i].BodyAnimator.SetBool("Walking", false);
                    }
                }
                for (int i = 0; i < _extraAtPos.Length; i++)
                {
                    if (_extraAtPos[i] != null)
                    {
                        _extraAtPos[i].BodyAnimator.SetBool("Walking", false);
                    }
                }
            }

            _isWalking = false;

            if (!_collectingScrap)
            {
                canGrabScrap = true;
            }
        }
        if (_robotsCarrying == _maxRobots)
        {
            canGrabScrap = false;
        }
    }

    private void GeneratePositionTransforms()
    {
        _maxRobots = Mathf.CeilToInt(_robotsToCarry * _extraRobotPercent);

        _extraRobots = _maxRobots - _robotsToCarry;

        GeneratePositions(_robotsToCarry);

        GenerateExtraPositions(_extraRobots);
    }

    /// <summary>
    /// Generates positions around the scrap
    /// </summary>
    /// <param name="posAmount"></param>
    void GeneratePositions(int posAmount)
    {
        _atPos = new RobotAI[posAmount];
        _holdPos = new RobotAI[posAmount];

        float angleStep = 360f / posAmount;

        for (int i = 0; i < posAmount; i++)
        {
            float angle = i * angleStep;
            float angleRad = Mathf.Deg2Rad * angle;

            Vector3 position = new Vector3(_objectToCarry.position.x + Mathf.Cos(angleRad) * _radius, _objectToCarry.position.y, _objectToCarry.position.z + Mathf.Sin(angleRad) * _radius);

            GameObject positionObject = new GameObject($"Hold_Pos ({i})");

            positionObject.transform.position = position;
            positionObject.transform.parent = transform;

            _robotPos.Add(positionObject.transform);
        }
    }

    /// <summary>
    /// Generates extra positions around the scrap
    /// </summary>
    /// <param name="posAmount"></param>
    void GenerateExtraPositions(int posAmount)
    {
        _extraAtPos = new RobotAI[posAmount];
        _extraHoldPos = new RobotAI[posAmount];

        float extraAngleStep = 360f / posAmount;

        for (int i = 0; i < posAmount; i++)
        {
            float angle = i * extraAngleStep;
            float angleRad = Mathf.Deg2Rad * angle;

            Vector3 position = new Vector3(_objectToCarry.position.x + Mathf.Cos(angleRad) * _extraRadius, _objectToCarry.position.y, _objectToCarry.position.z + Mathf.Sin(angleRad) * _extraRadius);

            GameObject positionObject = new GameObject($"Extra_Hold_Pos ({i})");

            positionObject.transform.position = position;
            positionObject.transform.parent = transform;

            _extraRobotPos.Add(positionObject.transform);
        }
    }

    /// <summary>
    /// Makes sure the robots end up in the corect spots around the scrap
    /// </summary>
    /// <param name="robot"></param>
    /// <returns></returns>
    public Transform GetGatherPosition(RobotAI robot)
    {
        if (!canGrabScrap)
        {
            return null;
        }

        for (int i = 0; i < _holdPos.Length; i++)
        {
            if (_holdPos[i] == null)
            {
                _holdPos[i] = robot;

                return _robotPos[i];
            }
        }

        for (int i = 0; i < _extraHoldPos.Length; i++)
        {
            if (_extraHoldPos[i] == null)
            {
                _extraHoldPos[i] = robot;

                return _extraRobotPos[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Add new robot to the scrap
    /// </summary>
    /// <param name="robot"></param>
    public void AddRobot(RobotAI robot)
    {
        bool hasAdded = false;

        for (int i = 0; i < _robotPos.Count; i++)
        {
            if (_robotPos[i] == robot.Target && !hasAdded)
            {
                hasAdded = true;

                _robotsNeededTxt.enabled = true;
                Debug.Log($"Adding robot at position {i}");

                _atPos[i] = robot;
            }
        }

        if (!hasAdded)
        {
            for (int i = 0; i < _extraRobotPos.Count; i++)
            {
                if (_extraRobotPos[i] == robot.Target && !hasAdded)
                {
                    hasAdded = true;

                    _robotsNeededTxt.enabled = true;
                    Debug.Log($"Adding robot at position {i}");

                    _extraAtPos[i] = robot;
                }
            }
        }

        _robotsCarrying = 0;

        for (int i = 0; i < _atPos.Length; i++)
        {
            if (_atPos[i] != null)
            {
                _robotsCarrying++;
            }
        }

        for (int i = 0; i < _extraAtPos.Length; i++)
        {
            if (_extraAtPos[i] != null)
            {
                _robotsCarrying++;
            }
        }

        _robotsNeededTxt.text = $"{_robotsCarrying}/{_robotsToCarry}";

        if (_robotsCarrying == _robotsToCarry)
        {
            _robotsNeededTxt.color = Color.green;
        }

        if (_robotsCarrying > _robotsToCarry)
        {
            _robotsNeededTxt.color = Color.blue;
        }

        if (_robotsCarrying < _robotsToCarry)
        {
            _robotsNeededTxt.color = Color.red;
        }

        Debug.Log($"Total robots carrying: {_robotsCarrying}");
    }

    /// <summary>
    /// Remove robot from the scrap
    /// </summary>
    /// <param name="robot"></param>
    public void RemoveRobot(RobotAI robot)
    {
        for (int i = 0; i < _atPos.Length; i++)
        {
            if (_atPos[i] == robot)
            {
                _atPos[i] = null;

                _holdPos[i] = null;

                _robotsCarrying--;

                RepositionRobot();

                Debug.Log($"({i})");
            }
        }

        for (int i = 0; i < _holdPos.Length; i++)
        {
            if (_holdPos[i] == robot)
            {
                _holdPos[i] = null;

                Debug.Log($"({i})");
            }
        }

        for (int i = 0; i < _extraAtPos.Length; i++)
        {
            if (_extraAtPos[i] == robot)
            {
                _extraAtPos[i] = null;

                _extraHoldPos[i] = null;

                _robotsCarrying--;
            }
        }

        for (int i = 0; i < _extraHoldPos.Length; i++)
        {
            if (_extraHoldPos[i] == robot)
            {
                _extraHoldPos[i] = null;
            }
        }

        if (_robotsCarrying == 0)
        {
            _robotsNeededTxt.enabled = false;
        }
    }

    /// <summary>
    /// Reposition robot when a robot leaves and is in an extra spot
    /// </summary>
    void RepositionRobot()
    {
        int neededRepositions = 0;

        int repositions = 0;

        for (int i = 0; i < _atPos.Length; i++)
        {
            if (_atPos[i] == null)
            {
                neededRepositions++;
            }
        }

        for (int i = 0; i < _extraAtPos.Length; i++)
        {
            if (_extraAtPos[i] != null && repositions < neededRepositions)
            {

                repositions++;
                _extraAtPos[i].ChangeGatherTarget();
            }
        }
    }

    [SerializeField] float gizmoSize = 0.1f;

    private void OnDrawGizmos()
    {
        if (_robotPos != null && _robotPos.Count > 0)
        {
            Gizmos.color = Color.white;

            foreach (Transform position in _robotPos)
            {
                if (position != null)
                {
                    Gizmos.DrawCube(position.position, Vector3.one * gizmoSize);
                }
            }
        }

        if (_extraRobotPos != null && _extraRobotPos.Count > 0)
        {
            Gizmos.color = Color.black;

            foreach (Transform position in _extraRobotPos)
            {
                if (position != null)
                {
                    Gizmos.DrawCube(position.position, Vector3.one * gizmoSize);
                }
            }
        }

        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
