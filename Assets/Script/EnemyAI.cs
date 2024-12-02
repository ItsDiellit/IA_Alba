using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,

        Searching
    }

    public EnemyState currentState;


    private NavMeshAgent _AIAgent;

    [SerializeField] Transform[] _patrolPoints;
    // Start is called before the first frame update

    void Awake()
    {
        _AIAgent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        currentState = EnemyState.Patrolling;
        SetRandomPatrolPoint();
    }

    // Update is called once per frame
    void Update()
    {
            switch(currentState)
            {
                case EnemyState.Patrolling:
                    Patrol();

                break;
                case EnemyState.Chasing:
                    Chase();
                break;
                case EnemyState.Searching:
                    Search();

                break;
            }
    }

    void Patrol()
    {
        if(_AIAgent.remainingDistance < 0.5f)
        {
            SetRandomPatrolPoint();
        }
        
    }

    void Chase()
    {

    }

     void Search()
    {

    }

    void SetRandomPatrolPoint()
    {
        _AIAgent.destination = _patrolPoints[Random.Range(0, _patrolPoints.Length)].position;
    }

    void OnDrawGizmos()
    {
        foreach(Transform point in _patrolPoints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(point.position, 1);
        }
    }
   
}
