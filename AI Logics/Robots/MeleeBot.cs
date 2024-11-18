using System;
using System.Collections;
using UnityEngine;

public class MeleeBot : RobotAI
{
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

    /// <summary>
    /// Overrides the attack logic and also attaches the robot to the enemy
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

    public override void Attack()
    {
        if (!IsAttacking)
        {
            AttackEffect.enabled = true;
            AttackEffect.Play();
            AttackSource.Play();
            StartCoroutine(StartAttacking());
        }
    }

    public override void StopAttack()
    {
        base.StopAttack();

        AttackEffect.Stop();
    }

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
                        Target = colliders[0].transform;
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

            WeaponAnimator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(RobotInfo.fireRate);

        if (IsAttacking)
        {
            StartCoroutine(Attacking());
        }
    }

}
