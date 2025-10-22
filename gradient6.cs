using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaterpillarIK : MonoBehaviour
{
    [Header("Lagarta Settings")]
    public Transform[] segments; // Array de segmentos (arraste no Inspector)
    public float segmentLength = 0.3f; // Distância entre segmentos
    
    [Header("Target")]
    public Transform target;
    public float targetMoveSpeed = 1.5f;

    [Header("Optimization")]
    public float alpha = 0.02f;
    private float tolerance = 0.2f;
    private float costFunctionValue; 
    
    private float[] angles; // Ângulos para cada junta
    private Vector3 basePosition;
    private float movementSpeed = 0.8f; // Velocidade de movimento da lagarta

    void Start()
    {
        // Inicializar ângulos
        angles = new float[segments.Length];
        for (int i = 0; i < angles.Length; i++)
        {
            angles[i] = 0.1f; // Pequena curvatura inicial
        }
        
        basePosition = transform.position;
        UpdateCaterpillar();
        costFunctionValue = CalculateCost();
    }

    void Update()
    {   
        // Movimento simples do target
        float time = Time.time * targetMoveSpeed;
        target.position = new Vector3(
            Mathf.Sin(time) * 4f,
            Mathf.Cos(time * 0.8f) * 3f,
            0
        );
       
        if (costFunctionValue > tolerance) 
        {
            // Gradient descent simples
            float[] gradient = CalculateGradient();
            
            for (int i = 0; i < angles.Length; i++)
            {
                angles[i] -= alpha * gradient[i];
                // Limitar ângulos
                angles[i] = Mathf.Clamp(angles[i], -Mathf.PI * 0.4f, Mathf.PI * 0.4f);
            }

            UpdateCaterpillar();
            
            // CORREÇÃO: Mover a basePosition para frente baseado na direção da cabeça
            Vector3 headDirection = (GetHeadPosition(angles) - basePosition).normalized;
            basePosition += headDirection * movementSpeed * Time.deltaTime;
        }

        costFunctionValue = CalculateCost();
    }

    void UpdateCaterpillar()
    {
        Vector3 currentPosition = basePosition;
        Quaternion currentRotation = Quaternion.identity;

        for (int i = 0; i < segments.Length; i++)
        {
            // Aplicar rotação
            currentRotation = currentRotation * Quaternion.Euler(0, 0, angles[i] * Mathf.Rad2Deg);
            
            // Mover para próxima posição
            currentPosition += currentRotation * Vector3.right * segmentLength;
            
            // Atualizar segmento
            segments[i].position = currentPosition;
            segments[i].rotation = currentRotation;
        }
    }

    Vector3 GetHeadPosition(float[] currentAngles)
    {
        Vector3 currentPosition = basePosition;
        Quaternion currentRotation = Quaternion.identity;

        for (int i = 0; i < segments.Length; i++)
        {
            currentRotation = currentRotation * Quaternion.Euler(0, 0, currentAngles[i] * Mathf.Rad2Deg);
            currentPosition += currentRotation * Vector3.right * segmentLength;
        }

        return currentPosition;
    }

    float CalculateCost()
    {
        Vector3 headPos = GetHeadPosition(angles);
        return Vector3.SqrMagnitude(headPos - target.position);
    }

    float CostFunction(float[] currentAngles)
    {
        Vector3 headPos = GetHeadPosition(currentAngles);
        return Vector3.SqrMagnitude(headPos - target.position);
    }

    float[] CalculateGradient() 
    {
        float step = 0.001f;
        float[] gradient = new float[angles.Length];

        for (int i = 0; i < angles.Length; i++)
        {
            float[] anglesPlus = (float[])angles.Clone();
            anglesPlus[i] += step;

            gradient[i] = (CostFunction(anglesPlus) - CostFunction(angles)) / step;
        }

        return gradient;
    }

    // Debug visual
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || segments == null || segments.Length == 0) return;

        // Linha da cabeça até o target
        Gizmos.color = Color.green;
        Vector3 headPos = GetHeadPosition(angles);
        Gizmos.DrawLine(headPos, target.position);

        // Mostrar posição da cabeça
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(headPos, 0.1f);

        // Mostrar base position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(basePosition, 0.1f);
    }
}