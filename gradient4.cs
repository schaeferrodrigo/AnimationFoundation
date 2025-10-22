using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradient4 : MonoBehaviour
{
    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform endFactor;

    //target
    public Transform target;
    public List<Transform> targetSequence = new List<Transform>(); // Sequência de targets
    public bool useDynamicTarget = false; // Target que se move dinamicamente
    public float targetMoveSpeed = 2.0f;
    public float waypointTolerance = 0.5f; // Tolerância para mudar para próximo waypoint


      // NOVO: Controle de targets múltiplos
    [Header("Targets Múltiplos")]
    public bool useTargetSequence = false;
    public float waypointWaitTime = 1.0f; // Tempo em cada waypoint
    private int currentTargetIndex = 0;
    private float waypointTimer = 0f;
    private bool isWaitingAtWaypoint = false;

    // NOVO: Target dinâmico
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
    private int t = 0; // time step



    // Angle constraints
       public bool useAngleConstraints = true;
    
    public Vector2 joint1Limits = new Vector2(-Mathf.PI * 0.75f, Mathf.PI * 0.75f);   // -135° a +135°
    public Vector2 joint2Limits = new Vector2(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);     // -90° a +90°
    public Vector2 joint3Limits = new Vector2(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);     // -90° a +90°
    private bool[] isJointAtLimit = new bool[3];

    // Start is called before the first frame update
    void Start()
    {
        l1 = Vector3.Distance(Joint0.position, Joint1.position);
        l2 = Vector3.Distance(Joint1.position, Joint2.position);
        l3 = Vector3.Distance(Joint2.position, endFactor.position);

        costFunctionValue = Vector3.Distance(endFactor.position, target.position) * Vector3.Distance(endFactor.position, target.position);
        theta = Vector3.zero;

        if (useDynamicTarget && target != null)
        {
            dynamicTargetStartPos = target.position;
        }
    }

    // Update is called once per frame
    void Update()
    {   

        UpdateTarget();
       
        if (costFunctionValue > tolerance) {

            t++;

            gradient = CalculateGradient();
            Vector3 alphaVec = AdaptiveLearningRate(gradient); // Update learning rate using Adam optimizer
            theta += -alphaVec; 

            if (useAngleConstraints)
            {
                theta = ApplyAngleConstraints(theta);
            }

            endFactor.position = GetEndEffectorPosition(theta);
            Joint1.position = GetJoint1Position();
            Joint2.position = GetJoint2Position();
        }

        costFunctionValue = Vector3.Distance(endFactor.position, target.position) * Vector3.Distance(endFactor.position, target.position);



    }

    float costFunction(Vector3 theta)
    {

        Vector3 endEffectorPosition = GetEndEffectorPosition(theta);

        return Vector3.Distance(endEffectorPosition, target.position) * Vector3.Distance(endEffectorPosition, target.position);
    }


    Vector3 AdaptiveLearningRate(Vector3 gradient)
    {

        //Adam learning rate

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
        
        // Junta 1
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
        
        // Junta 2
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
        
        // Junta 3
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
   
    Vector3 CalculateGradient() {

        Vector3 gradientVector;

        float step = 0.001f;

        Vector3 thetaXPlus = new Vector3(theta.x + step, theta.y, theta.z);
        Vector3 thetaYPlus = new Vector3(theta.x, theta.y + step, theta.z);
        Vector3 thetaZPlus = new Vector3(theta.x, theta.y, theta.z + step);

        float dCostdThetaX = (costFunction(thetaXPlus) - costFunction(theta)) / step;
        float dCostdThetaY = (costFunction(thetaYPlus) - costFunction(theta)) / step;
        float dCostdThetaZ = (costFunction(thetaZPlus) - costFunction(theta)) / step;

        gradientVector = new Vector3(dCostdThetaX, dCostdThetaY, dCostdThetaZ);

        

        return gradientVector;
   
    }


    Vector3 GetEndEffectorPosition(Vector3 theta) 
    {
        Vector3 newPosition;

        newPosition.x = Joint0.position.x + l1 * Mathf.Cos(theta.x) 
                       + l2 * Mathf.Cos(theta.x + theta.y)
                       + l3*Mathf.Cos(theta.x + theta.y + theta.z); 
        newPosition.y = Joint0.position.y + l1 * Mathf.Sin(theta.x)
                       + l2 * Mathf.Sin(theta.x + theta.y)
                       + l3 * Mathf.Sin(theta.x + theta.y + theta.z);

        newPosition.z = 0;

        return newPosition;
    }

    Vector3 GetJoint2Position()
    {
        Vector3 newPosition;

        newPosition.x = Joint0.position.x + l1 * Mathf.Cos(theta.x)
                       + l2 * Mathf.Cos(theta.x + theta.y);
        newPosition.y = Joint0.position.y + l1 * Mathf.Sin(theta.x)
                       + l2 * Mathf.Sin(theta.x + theta.y);

        newPosition.z = 0;

        return newPosition;
    }

    Vector3 GetJoint1Position()
    {
        Vector3 newPosition;

        newPosition.x = Joint0.position.x + l1 * Mathf.Cos(theta.x);
        newPosition.y = Joint0.position.y + l1 * Mathf.Sin(theta.x);

        newPosition.z = 0;

        return newPosition;
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
            // Movimento circular
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
            // Movimento senoidal em múltiplos eixos
            float time = Time.time * targetMoveSpeed;
            Vector3 newPos = dynamicTargetStartPos + new Vector3(
                Mathf.Sin(time) * 2f,
                Mathf.Cos(time * 0.7f) * 1.5f,
                0
            );
            target.position = newPos;
        }

        // Reset convergência quando target se move
        //if (costFunctionValue <= tolerance && Vector3.Distance(endFactor.position, target.position) > tolerance)
        //{
           // isConverged = false;
        //}
    }

    // NOVO: Sequência de waypoints
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
            // Chegou no waypoint atual, inicia espera
            isWaitingAtWaypoint = true;
            waypointTimer = 0f;
            Debug.Log($"🎯 Waypoint {currentTargetIndex} alcançado! Esperando {waypointWaitTime}s");
        }
    }

    // NOVO: Avança para próximo target na sequência
    void MoveToNextTarget()
    {
        if (targetSequence.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % targetSequence.Count;
        target = targetSequence[currentTargetIndex];
        //isConverged = false;
        waypointTimer = 0f;
        isWaitingAtWaypoint = false;

        Debug.Log($"🔄 Movendo para Target {currentTargetIndex} em posição: {target.position}");

        // Reset parcial do otimizador para novo target
        m_t *= 0.5f;
        v_t *= 0.5f;
    }

    // NOVO: Método público para adicionar targets em tempo real
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

        Debug.Log($"➕ Novo target adicionado: {position}");
    }

    // NOVO: Método para definir target atual
    public void SetCurrentTarget(Transform newTarget)
    {
        target = newTarget;
        
        Debug.Log($"🎯 Target definido para: {newTarget.position}");
    }


}