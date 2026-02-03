using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class enemy : MonoBehaviour
{
    Transform target;
    NavMeshAgent agent;
    public float LookRadius;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.player.transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, LookRadius);
    }
}
