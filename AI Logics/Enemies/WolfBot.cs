using UnityEngine;

public class WolfBot : EnemyAI
{
    [SerializeField] AudioClip _growlClip;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public void DoGrowl()
    {
        EnemyAudioSource.clip = _growlClip;
        EnemyAudioSource.Play();
    }

    public override void GizmosLogic()
    {
        Gizmos.color = Color.red;
        base.GizmosLogic();
    }
}
