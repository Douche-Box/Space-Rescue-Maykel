using UnityEngine;

[CreateAssetMenu(fileName = "Robot", menuName = "Entity/RobotData", order = 1)]

public class RobotSO : ScriptableObject
{
    public float health;
    public float speed;

    public float range;
    public float followDistance;

    public float damage;
    public float impactDamage;

    public float fireRate;

    public float windUpTime;
    public float windDownTime;

    public float retreatTime;
}