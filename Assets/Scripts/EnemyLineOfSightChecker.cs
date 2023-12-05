using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    public SphereCollider Collider;
    public LayerMask lineOfSightLayers;

    public delegate void GainSightEvent(Player player);
    public GainSightEvent onGainSight;
    public delegate void LoseSightEvent(Player player);
    public LoseSightEvent onLoseSight;


    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player;
        if(other.TryGetComponent<Player>(out player))
        {
            onGainSight?.Invoke(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player;
        if (other.TryGetComponent<Player>(out player))
        {
            onLoseSight?.Invoke(player);
        }
    }

}
