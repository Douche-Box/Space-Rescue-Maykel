using UnityEngine;

public class BlockBot : EnemyAI
{
    /// <summary>
    /// Overide for the base Attack logic 
    /// </summary>
    public override void Attack()
    {
        if (TargetTransform != null)
        {
            Vector3 targetWithOffset = new Vector3(TargetTransform.position.x, transform.position.y, TargetTransform.position.z);
            DistanceFromTarget = Vector3.Distance(transform.position, targetWithOffset);

            Vector3 patrolWithOffset = new Vector3(PatrolPoints[CurrentPatrol].position.x, transform.position.y, PatrolPoints[CurrentPatrol].position.z);
            DistanceFromPatrol = Vector3.Distance(transform.position, patrolWithOffset);

            if (!IsAttacking && DistanceFromTarget > MaxAttackRange)
            {
                Agent.isStopped = false;
                Agent.SetDestination(TargetTransform.position);
                Animator.SetBool("Walking", true);
            }

            if (!IsAttacking && Physics.SphereCast(transform.position, 0.1f, transform.forward, out RaycastHit hitInfo, MaxAttackRange, PreyMask))
            {
                float distanceFromPrey = Vector3.Distance(transform.position, TargetTransform.position);

                // When not too close to the prey attack
                if (distanceFromPrey <= MaxAttackRange && distanceFromPrey >= MaxAttackRange - 1.5f)
                {
                    IsAttacking = true;

                    Agent.isStopped = true;
                    Agent.velocity = Vector3.zero;

                    Animator.SetBool("Walking", false);
                    AttackController.DoRandomAttack();
                }
                // When close enough to the enemy tries to run them over
                else
                {
                    Agent.isStopped = false;
                    Animator.SetBool("Walking", true);
                    Agent.SetDestination(TargetTransform.position);
                }
            }
            else if (!IsAttacking)
            {
                Animator.SetBool("Walking", true);


                Vector3 directionToTarget = (targetWithOffset - transform.position).normalized;

                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else if (TargetTransform == null && PossibleTargets.Count > 0 && !IsAttacking)
        {
            for (int i = 0; i < PossibleTargets.Count; i++)
            {
                if (PossibleTargets[i] == null)
                {
                    PossibleTargets.RemoveAt(i);
                }
            }
            if (PossibleTargets.Count > 0)
            {
                TargetTransform = PossibleTargets[0];
            }
        }
        else if (TargetTransform == null && PossibleTargets.Count == 0)
        {
            ChangeState(State.SEARCH);
        }

        if (DistanceFromPatrol > PatrolReturnDistance)
        {
            ChangeState(State.PATROL);
        }
    }

    public override void GizmosLogic()
    {
        Gizmos.color = Color.blue;

        base.GizmosLogic();
    }
}
