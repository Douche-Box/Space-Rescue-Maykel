using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scrap", menuName = "Entity/ScrapData", order = 3)]

public class ScrapSO : ScriptableObject
{
    public int weight;

    public int worth;

    public float baseSpeed;
}
