using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Entity/EnemyData", order = 2)]

public class EnemySO : ScriptableObject
{
    // general

    public float health;

    public float speed;
    public float rotationSpeed;

    public float damage;

    public float maxAttackRange;
    public float minAttackRange;

    public float regenRate;
    public float regenAmount;

    // patrol

    public float patrolRadius;
    public float patrolReturnDistance;
    public int patrolPoints;

    // search

    public float searchRadius;
    public float timeToSearch;
    public int searchCount;



}
