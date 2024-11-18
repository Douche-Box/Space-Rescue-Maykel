using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaboomBot : RobotAI
{

    [SerializeField] float _range;

    [SerializeField] GameObject _attackExplosion;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void ChangeState(State newState)
    {
        base.ChangeState(newState);
    }

    public override void CheckState()
    {
        base.CheckState();
    }

    public override void Thrown()
    {
        if (IsThrown)
        {
            CheckForEntityInRange(CheckRadius);
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.1f, GroundMask))
        {
            Debug.Log("Hit Ground");
            CheckForEntityInRange(GroundedCheckRadius);
        }
    }

    /// <summary>
    /// Override attack logic and explodes the robot on attack
    /// </summary>
    public override void StartAttack()
    {
        _currentState = State.ATTACK;
        Agent.enabled = false;

        Vector3 direction = Target.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 4f, DetectionLayer))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            transform.SetParent(Target, true);

            transform.position = hit.point;

            WeaponAnimator.SetTrigger("Ready");

            Vector3 forwardDirection = Vector3.ProjectOnPlane(direction, hit.normal).normalized;

            if (forwardDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forwardDirection, hit.normal);
            }
            else
            {
                Debug.LogWarning("Forward direction is zero, cannot rotate.");
            }
        }
        else
        {
            Debug.Log("No raycast hit detected.");

            ChangeState(State.IDLE);
        }
        if (Target != null)
        {
            Target = Target.GetComponentInParent<EnemyAI>().transform;

            Target.GetComponent<EnemyAI>().AttachRobot(this);
        }
    }

    [SerializeField] List<EnemyAI> enemiesBeenAttacked = new();

    /// <summary>
    /// Do damage to enemies in range and explode
    /// </summary>
    public void DeathAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _range, DetectionLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            EnemyAI enemyToAttack = colliders[i].GetComponentInParent<EnemyAI>();
            if (i == 0)
            {
                enemiesBeenAttacked.Add(enemyToAttack);
                enemyToAttack.TakeDamage(damage);
            }

            if (enemyToAttack.health < 0 && !enemiesBeenAttacked.Contains(enemyToAttack))
            {
                enemiesBeenAttacked.Add(enemyToAttack);
                enemyToAttack.TakeDamage(damage);
            }
        }

        GameObject effect = Instantiate(_attackExplosion);

        effect.transform.position = transform.position;
        effect.GetComponent<AudioSource>().clip = AttackClip;
        effect.GetComponent<AudioSource>().Play();

        Destroy(effect, 1f);

        Destroy(gameObject);
    }

    public override void Attack()
    {
        if (Target != null && Agent.isActiveAndEnabled)
        {
            Agent.SetDestination(Target.position);

            Vector3 targetWithOffset = new Vector3(Target.position.x, transform.position.y, Target.position.z);

            DistanceFromTarget = Vector3.Distance(transform.position, targetWithOffset);

            if (_range >= DistanceFromTarget)
            {
                Agent.isStopped = true;
                BodyAnimator.SetBool("Walking", false);
                WeaponAnimator.SetTrigger("Ready");
            }
        }
    }

    /// <summary>
    /// Overide for entity check to make it work for the KaboomBot
    /// </summary>
    /// <param name="radius"></param>
    public override void CheckForEntityInRange(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, DetectionLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            switch (colliders[0].GetComponentInParent<Entity>().entityType)
            {
                case EntityType.SCRAP:
                    Target = colliders[0].transform;

                    ChangeState(State.GATHER);
                    break;
                case EntityType.ENEMY:
                    if (colliders[0].GetComponentInParent<Entity>().health > 0)
                    {
                        Target = colliders[0].GetComponentInParent<EnemyAI>().transform;
                        ChangeState(State.ATTACK);
                    }
                    break;
            }
        }
    }

    public override IEnumerator Attacking()
    {
        if (Target != null)
        {
            Target.GetComponent<Entity>().TakeDamage(RobotInfo.damage); // this still happens with this because it tries to attack the player ( probably )

            WeaponAnimator.SetTrigger("Ready");
        }

        yield return new WaitForSeconds(RobotInfo.fireRate);

        if (IsAttacking)
        {
            StartCoroutine(Attacking());
        }
    }
}
