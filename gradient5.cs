using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradient5 : MonoBehaviour
{
    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform endFactor;

    //target
    public Transform target;
    public List<Transform> targetSequence = new List<Transform>();
    public bool useDynamicTarget = false;
    public float targetMoveSpeed = 2.0f;
    public float waypointTolerance = 0.5f;

    [Header("Targets Múltiplos")]
    public bool useTargetSequence = false;
    public float waypointWaitTime = 1.0f;
    private int currentTargetIndex = 0;
    private float waypointTimer = 0f;
    private bool isWaitingAtWaypoint = false;

    [Header("Target Dinâmico")]
    public bool useCircularMotion = false;
    public float circleRadius = 3.0f;
    public float circleSpeed = 1.0f;
    private Vector3 dynamicTargetStartPos;
    private float dynamicTargetAngle = 0f;

    public float alpha = 0.01f;
    private float tolerance = 0.1f;
    private float costFunctionValue; 
    private Vector3 gradient;
    private Vector3 theta; 

    private float l1;
    private float l2;
    private float l3;

    // Adam optimizer parameters
    private Vector3 m_t = Vector3.zero;
    private Vector3 v_t = Vector3.zero;
    private float beta_1 = 0.9f;
    private float beta_2 = 0.999f;
    private float epsilon = 1e-8f;
    private int t = 0;

    // Angle constraints
    public bool useAngleConstraints = true;
    public Vector2 joint1Limits = new Vector2(-Mathf.PI * 0.75f, Mathf.PI * 0.75f);
    public Vector2 joint2Limits = new Vector2(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);
    public Vector2 joint3Limits = new Vector2(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);
    private bool[] isJointAtLimit = new bool[3];

    void Start()
    {
        l1 = Vector3.Distance(Joint0.position, Joint1.position);
        l2 = Vector3.Distance(Joint1.position, Joint2.position);
        l3 = Vector3.Distance(Joint2.position, endFactor.position);

        theta = new Vector3(0.1f, 0.1f, 0.1f);
        UpdateArmPositions();
        
        costFunctionValue = CalculateCost();

        if (useDynamicTarget && target != null)
        {
            dynamicTargetStartPos = target.position;
        }
        
        Debug.Log("Sistema iniciado con cinemática directa por quaternions");
    }

    void Update()
    {   
        UpdateTarget();
       
        if (costFunctionValue > tolerance) 
        {
            t++;

            gradient = CalculateGradient();
            Vector3 alphaVec = AdaptiveLearningRate(gradient);
            theta += -alphaVec; 

            if (useAngleConstraints)
            {
                theta = ApplyAngleConstraints(theta);
            }

            UpdateArmPositions();
        }

        costFunctionValue = CalculateCost();
    }

    void UpdateArmPositions()
    {
        // PRIMERO calcular todas las rotaciones acumulativas
        Quaternion rot0 = Joint0.rotation; // Rotación base
        Quaternion rot1 = rot0 * Quaternion.Euler(0, 0, theta.x * Mathf.Rad2Deg);
        Quaternion rot2 = rot1 * Quaternion.Euler(0, 0, theta.y * Mathf.Rad2Deg);
        Quaternion rot3 = rot2 * Quaternion.Euler(0, 0, theta.z * Mathf.Rad2Deg);

        // LUEGO calcular posiciones usando las rotaciones
        Joint1.position = Joint0.position + rot0 * new Vector3(l1 * Mathf.Cos(theta.x), l1 * Mathf.Sin(theta.x), 0);
        Joint2.position = Joint1.position + rot1 * new Vector3(l2 * Mathf.Cos(theta.y), l2 * Mathf.Sin(theta.y), 0);
        endFactor.position = Joint2.position + rot2 * new Vector3(l3 * Mathf.Cos(theta.z), l3 * Mathf.Sin(theta.z), 0);

        // FINALMENTE aplicar las rotaciones a los objetos
        Joint1.rotation = rot1;
        Joint2.rotation = rot2;
        endFactor.rotation = rot3;
    }

    // CORRECCIÓN: FK consistente para cálculo del gradiente
    Vector3 GetEndEffectorPosition(Vector3 angles)
    {
        // Usar el mismo cálculo que en UpdateArmPositions pero sin modificar los objetos
        Quaternion rot0 = Joint0.rotation;
        Quaternion rot1 = rot0 * Quaternion.Euler(0, 0, angles.x * Mathf.Rad2Deg);
        Quaternion rot2 = rot1 * Quaternion.Euler(0, 0, angles.y * Mathf.Rad2Deg);

        Vector3 pos1 = Joint0.position + rot0 * new Vector3(l1 * Mathf.Cos(angles.x), l1 * Mathf.Sin(angles.x), 0);
        Vector3 pos2 = pos1 + rot1 * new Vector3(l2 * Mathf.Cos(angles.y), l2 * Mathf.Sin(angles.y), 0);
        Vector3 pos3 = pos2 + rot2 * new Vector3(l3 * Mathf.Cos(angles.z), l3 * Mathf.Sin(angles.z), 0);

        return pos3;
    }

    float CalculateCost()
    {
        return Vector3.SqrMagnitude(endFactor.position - target.position);
    }

    float costFunction(Vector3 theta)
    {
        Vector3 endEffectorPosition = GetEndEffectorPosition(theta);
        return Vector3.SqrMagnitude(endEffectorPosition - target.position);
    }

    Vector3 AdaptiveLearningRate(Vector3 gradient)
    {
        m_t = beta_1 * m_t + (1 - beta_1) * gradient;
        v_t = beta_2 * v_t + (1 - beta_2) * Vector3.Scale(gradient, gradient);

        Vector3 m_hat = m_t / (1 - Mathf.Pow(beta_1, t));
        Vector3 v_hat = v_t / (1 - Mathf.Pow(beta_2, t));

        Vector3 adaptiveAlphaVec = new Vector3(
            alpha * m_hat.x / (Mathf.Sqrt(v_hat.x) + epsilon),
            alpha * m_hat.y / (Mathf.Sqrt(v_hat.y) + epsilon),
            alpha * m_hat.z / (Mathf.Sqrt(v_hat.z) + epsilon)
        );
        
        return adaptiveAlphaVec;
    }

    Vector3 ApplyAngleConstraints(Vector3 proposedAngles)
    {
        Vector3 constrainedAngles = proposedAngles;
        
        isJointAtLimit[0] = false;
        if (proposedAngles.x < joint1Limits.x)
        {
            constrainedAngles.x = joint1Limits.x;
            isJointAtLimit[0] = true;
        }
        else if (proposedAngles.x > joint1Limits.y)
        {
            constrainedAngles.x = joint1Limits.y;
            isJointAtLimit[0] = true;
        }
        
        isJointAtLimit[1] = false;
        if (proposedAngles.y < joint2Limits.x)
        {
            constrainedAngles.y = joint2Limits.x;
            isJointAtLimit[1] = true;
        }
        else if (proposedAngles.y > joint2Limits.y)
        {
            constrainedAngles.y = joint2Limits.y;
            isJointAtLimit[1] = true;
        }
        
        isJointAtLimit[2] = false;
        if (proposedAngles.z < joint3Limits.x)
        {
            constrainedAngles.z = joint3Limits.x;
            isJointAtLimit[2] = true;
        }
        else if (proposedAngles.z > joint3Limits.y)
        {
            constrainedAngles.z = joint3Limits.y;
            isJointAtLimit[2] = true;
        }
        
        return constrainedAngles;
    }
   
    Vector3 CalculateGradient() 
    {
        float step = 0.001f;

        Vector3 thetaXPlus = new Vector3(theta.x + step, theta.y, theta.z);
        Vector3 thetaYPlus = new Vector3(theta.x, theta.y + step, theta.z);
        Vector3 thetaZPlus = new Vector3(theta.x, theta.y, theta.z + step);

        float dCostdThetaX = (costFunction(thetaXPlus) - costFunction(theta)) / step;
        float dCostdThetaY = (costFunction(thetaYPlus) - costFunction(theta)) / step;
        float dCostdThetaZ = (costFunction(thetaZPlus) - costFunction(theta)) / step;

        return new Vector3(dCostdThetaX, dCostdThetaY, dCostdThetaZ);
    }

    void UpdateTarget()
    {
        if (useDynamicTarget)
        {
            UpdateDynamicTarget();
        }

        if (useTargetSequence && targetSequence.Count > 0)
        {
            UpdateTargetSequence();
        }
    }
    
    void UpdateDynamicTarget()
    {
        if (target == null) return;

        if (useCircularMotion)
        {
            dynamicTargetAngle += circleSpeed * Time.deltaTime;
            Vector3 newPos = dynamicTargetStartPos + new Vector3(
                Mathf.Cos(dynamicTargetAngle) * circleRadius,
                Mathf.Sin(dynamicTargetAngle) * circleRadius,
                0
            );
            target.position = newPos;
        }
        else
        {
            float time = Time.time * targetMoveSpeed;
            Vector3 newPos = dynamicTargetStartPos + new Vector3(
                Mathf.Sin(time) * 2f,
                Mathf.Cos(time * 0.7f) * 1.5f,
                0
            );
            target.position = newPos;
        }
    }

    void UpdateTargetSequence()
    {
        if (isWaitingAtWaypoint)
        {
            waypointTimer += Time.deltaTime;
            if (waypointTimer >= waypointWaitTime)
            {
                isWaitingAtWaypoint = false;
                MoveToNextTarget();
            }
        }
        else if (costFunctionValue <= waypointTolerance)
        {
            isWaitingAtWaypoint = true;
            waypointTimer = 0f;
            Debug.Log($"Waypoint {currentTargetIndex} alcançado!");
        }
    }

    void MoveToNextTarget()
    {
        if (targetSequence.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % targetSequence.Count;
        target = targetSequence[currentTargetIndex];
        waypointTimer = 0f;
        isWaitingAtWaypoint = false;

        // Reset parcial del optimizador
        m_t *= 0.5f;
        v_t *= 0.5f;
        
        Debug.Log($"Nuevo target: {currentTargetIndex}");
    }

    public void AddTarget(Vector3 position)
    {
        GameObject newTarget = new GameObject($"DynamicTarget_{targetSequence.Count}");
        newTarget.transform.position = position;
        targetSequence.Add(newTarget.transform);
        
        if (!useTargetSequence)
        {
            useTargetSequence = true;
            target = newTarget.transform;
        }
    }

    public void SetCurrentTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Para debug visual
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.2f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(endFactor.position, 0.15f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(endFactor.position, target.position);
        }
    }
}