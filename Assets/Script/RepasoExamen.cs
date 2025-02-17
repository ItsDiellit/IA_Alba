using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RepasoExamen : MonoBehaviour
{

    private NavMeshAgent _agent;


    public enum State
    {
        Patrolling,

        Searching,

        Chasing,

        Waiting,

        Attacking
    }


    public State currentState;

    [SerializeField] private Transform[] _patrolPoints;

    private int _patrolIndex;

    private Transform _playerTransform;

    [SerializeField] private float _visionRange = 15;

        [SerializeField] private float _attackRange = 3;



    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        currentState = State.Patrolling;
        SetPatrolPoint();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:

            Patrol();

            break;
            case State.Searching:
            Search();

            break;
            case State.Chasing:
            Chase();

            break;
            case State.Waiting:
            Wait();

            break;
            case State.Attacking:
            Attack();

            break;
        }
    }

    void Patrol()
    {
        
        if(InRange(_visionRange))
        {
            currentState = State.Chasing;
        }

        if(_agent.remainingDistance < 0.5f)
        {
            SetPatrolPoint();
        }

    }

    void SetPatrolPoint()
    {
        //Definir punto aleatorio
        _agent.destination = _patrolPoints[Random.Range(0, _patrolPoints.Length)].position;

        //Para que vaya en orden con los puntos
        //_patrolIndex++;
        //if(_patrolIndex >= _patrolPoints.Length)
        // {_patrolIndex = 0;}
        //_agent.destination = _patrolPoints[_patrolIndex].position;
    }

    void Search()
    {
        
    }

    void Chase()
    {
         if(!InRange(_visionRange))
        {
            currentState = State.Patrolling;
        }

        if(InRange(_attackRange))
        {
            currentState = State.Attacking;
        }



        _agent.destination = _playerTransform.position;
    }

    bool InRange(float range)
    {
        return Vector3.Distance(transform.position, _playerTransform.position) < range;
    }

    void Wait()
    {
        
    }

    void Attack()
    {
        Debug.Log("Atacando");
        currentState = State.Chasing;
    }
}
