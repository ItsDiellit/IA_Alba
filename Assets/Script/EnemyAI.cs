using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Waiting,
        Chasing,
        Searching,
        Attacking // Nuevo estado
    }

    public EnemyState currentState;

    private NavMeshAgent _AIAgent;
    private Transform _playerTransform;

    [SerializeField] Transform[] _patrolPoints; // Lista de puntos de patrulla
    private int _currentPatrolIndex = 0; // Índice del punto de patrulla actual

    [SerializeField] float _waitTimeAtPoint = 2f; // Tiempo que espera en cada punto
    private float _waitTimer = 0f; // Temporizador para el tiempo de espera

    [SerializeField] float _visionRange = 20;
    [SerializeField] float _visionAngle = 120;
    [SerializeField] float _attackRange = 1.5f; // Rango de ataque

    private Vector3 _playerLastPosition;

    // COSAS DE BÚSQUEDA
    float _searchTimer;
    float _searchWaitTime = 15;
    float _searchRadius = 10;

    void Awake()
    {
        _AIAgent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patrolling;
        SetNextPatrolPoint(); // Inicializa el primer punto de patrulla
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Waiting:
                Wait();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Searching:
                Search();
                break;
            case EnemyState.Attacking: // Nuevo estado
                Attack();
                break;
        }
    }

    void Patrol()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        // Si el agente ha llegado al punto, cambiar al estado de espera
        if (_AIAgent.remainingDistance < 0.5f && !_AIAgent.pathPending)
        {
            currentState = EnemyState.Waiting;
            _waitTimer = 0f; // Reiniciar el temporizador de espera
        }
    }

    void Wait()
    {
        // Verificar si el jugador está en rango incluso durante la espera
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        // Incrementar el temporizador de espera
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitTimeAtPoint)
        {
            SetNextPatrolPoint(); // Mover al siguiente punto de patrulla
            currentState = EnemyState.Patrolling;
        }
    }

    void Chase()
    {
        if (!OnRange())
        {
            currentState = EnemyState.Searching;
            return;
        }

        // Si el jugador está en rango de ataque, cambiar al estado Attacking
        if (Vector3.Distance(transform.position, _playerTransform.position) <= _attackRange)
        {
            currentState = EnemyState.Attacking;
            return;
        }

        _AIAgent.destination = _playerTransform.position;
    }

    void Search()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing;
            return;
        }

        _searchTimer += Time.deltaTime;

        if (_searchTimer < _searchWaitTime)
        {
            if (_AIAgent.remainingDistance < 0.5f && !_AIAgent.pathPending)
            {
                Vector3 randomPoint;
                if (RandomSearchPoint(_playerLastPosition, _searchRadius, out randomPoint))
                {
                    _AIAgent.destination = randomPoint;
                }
            }
        }
        else
        {
            currentState = EnemyState.Patrolling;
            _searchTimer = 0;
        }
    }

    void Attack()
    {
        // Mostrar mensaje en la consola indicando que el jugador está siendo atacado
        Debug.Log("Player is being attacked!");

        // Permanecer en este estado mientras el jugador esté dentro del rango de ataque
        if (Vector3.Distance(transform.position, _playerTransform.position) > _attackRange)
        {
            currentState = EnemyState.Chasing; // Volver al estado Chasing si el jugador se aleja
        }
    }

    void SetNextPatrolPoint()
    {
        // Si no hay puntos de patrulla, no hacer nada
        if (_patrolPoints.Length == 0) return;

        // Asignar el próximo destino al NavMeshAgent
        _AIAgent.destination = _patrolPoints[_currentPatrolIndex].position;

        // Incrementar el índice del punto de patrulla
        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length; // Volver al primer punto si alcanza el último
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer > _visionRange)
        {
            return false;
        }

        if (angleToPlayer > _visionAngle * 0.5f)
        {
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _playerLastPosition = _playerTransform.position;
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange); // Visualizar el rango de ataque

        Gizmos.color = Color.yellow;
        Vector3 fovLine1 = Quaternion.AngleAxis(_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        Gizmos.color = Color.blue;
        if (_patrolPoints != null)
        {
            foreach (Transform point in _patrolPoints)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}
