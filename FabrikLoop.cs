using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class FabrikLoop : MonoBehaviour
{
    public List<Transform> Joints;
    public Transform target;
    private float[] LinkLengths;
    private int numberOfJoints;
    private Vector3 endEffector;
    private int endEffectorIndex;

    private int initialPositionIndex;
            
    
    //tolerance and max iterations 
    public float tolerance = 1.0f;
    public float maxIterations = 40;
    private int countIterations = 0;

    //method's parameter
    private float lambda;
    private Vector3 initialPosition;


    // Start is called before the first frame update
    void Start()
    {
        numberOfJoints = Joints.Count;
        InitializeLinkLengths(); 
        DefineInitialPosition();
        DefineEndEffectorJoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (countIterations < maxIterations && 
            Vector3.Distance(endEffector, target.position) > tolerance) {

            Forward();
            Backward();
            countIterations++;

            endEffector = Joints[endEffectorIndex].position;
        }
        
    }

    void InitializeLinkLengths()
    {
        LinkLengths = new float[numberOfJoints];
        for (int i = 0; i < numberOfJoints; i++)
        {
            int nextIndex = (i + 1) % numberOfJoints;
            LinkLengths[i] = Vector3.Distance(Joints[i].position, Joints[nextIndex].position);
        }
    }

    void Forward() {

        Joints[endEffectorIndex].position = target.position;

        int i = (endEffectorIndex - 1 + numberOfJoints) % numberOfJoints;
        
        int jointsProcessed = 0;

        while (jointsProcessed < numberOfJoints - 1)
        {
            float distance = LinkLengths[i];

            int nextIndex = (i + 1) % numberOfJoints;

                        // Get the direction from current to next joint
            Vector3 direction = (Joints[nextIndex].position - Joints[i].position).normalized;
            
            // Move current joint to maintain original link length
            Joints[i].position = Joints[nextIndex].position - direction * distance;

            //float denominator = Vector3.Distance(Joints[i].position, Joints[nextIndex].position);
            //lambda = distance / denominator;
            //Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[nextIndex].position - Joints[i].position);

            i = (i - 1 + numberOfJoints) % numberOfJoints;
            jointsProcessed++;

        }
        i = (endEffectorIndex + 1 + numberOfJoints) % numberOfJoints;
        
        jointsProcessed = 0;


        jointsProcessed = 0;

           while (jointsProcessed < numberOfJoints - 1)
        {
            float distance = LinkLengths[i];

            int nextIndex = (i - 1+numberOfJoints) % numberOfJoints;

                        // Get the direction from current to next joint
            Vector3 direction = (Joints[nextIndex].position - Joints[i].position).normalized;
            
            // Move current joint to maintain original link length
            Joints[i].position = Joints[nextIndex].position - direction * distance;

            //float denominator = Vector3.Distance(Joints[i].position, Joints[nextIndex].position);
            //lambda = distance / denominator;
            //Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[nextIndex].position - Joints[i].position);

            i = (i + 1 + numberOfJoints) % numberOfJoints;
            jointsProcessed++;

        }
        
        
/*
        for (int i = numberOfJoints - 2; i >= 0; i--) {

            float distance = Vector3.Magnitude(Links[i]);
            float denominator = Vector3.Distance(Joints[i].position, Joints[i + 1].position);
            lambda = distance/denominator;
            Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[i + 1].position - Joints[i].position);
        
        }
        */
    }

    void Backward()
    {

        Joints[initialPositionIndex].position = initialPosition;

   
           int i =  (initialPositionIndex + 1) % numberOfJoints;
           int jointsProcessed = 0;
               
        while(jointsProcessed < numberOfJoints - 1)
        {
            int prevIndex = (i - 1 + numberOfJoints) % numberOfJoints;
            float distance = LinkLengths[prevIndex];  
            
            Vector3 direction = (Joints[i].position - Joints[prevIndex].position).normalized;
            Joints[i].position = Joints[prevIndex].position + direction * distance;

            //float denominator = Vector3.Distance(Joints[prevIndex].position, Joints[i].position);
            //lambda = distance / denominator;
            //Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[prevIndex].position - Joints[i].position);

            i = (i + 1) % numberOfJoints;
            jointsProcessed++;
        }

         i =  (initialPositionIndex - 1 + numberOfJoints) % numberOfJoints;
         jointsProcessed = 0;
               
        while(jointsProcessed < numberOfJoints - 1)
        {
            int prevIndex = (i + 1 + numberOfJoints) % numberOfJoints;
            float distance = LinkLengths[prevIndex];  
            
            Vector3 direction = (Joints[i].position - Joints[prevIndex].position).normalized;
            Joints[i].position = Joints[prevIndex].position + direction * distance;

            //float denominator = Vector3.Distance(Joints[prevIndex].position, Joints[i].position);
            //lambda = distance / denominator;
            //Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[prevIndex].position - Joints[i].position);

            i = (i - 1 + numberOfJoints) % numberOfJoints;
            jointsProcessed++;
        }
 
/*

        for (int i = 1; i < numberOfJoints; i++)
        {

            float distance = Vector3.Magnitude(Links[i - 1]);
            float denominator = Vector3.Distance(Joints[i - 1].position, Joints[i].position);
            lambda = distance / denominator;
            Joints[i].position = Joints[i].position + (1 - lambda) * (Joints[i - 1].position - Joints[i].position);

        }   */
    }

    void DefineEndEffectorJoint()
    {
        endEffectorIndex = 0;
        for (int i = 1; i < numberOfJoints; i++)
        {
            if (Vector3.Distance(Joints[i].position, target.position) <
                Vector3.Distance(Joints[endEffectorIndex].position, target.position))
            {
                endEffectorIndex = i;
            }
        }
        endEffector = Joints[endEffectorIndex].position;

    }

    void DefineInitialPosition()
    {
        initialPositionIndex = 0;
        for (int i = 1; i < numberOfJoints; i++)
        {
            if (Vector3.Distance(Joints[i].position, target.position) >
                Vector3.Distance(Joints[initialPositionIndex].position, target.position))
            {
                initialPositionIndex = i;
            }
            
        }
        initialPosition = Joints[initialPositionIndex].position;
    }

        void OnDrawGizmos()
    {
        if (Joints == null || Joints.Count < 2) return;
        
        Gizmos.color = Color.green;
        for (int i = 0; i < Joints.Count; i++)
        {
            int nextIndex = (i + 1) % Joints.Count;
            if (Joints[i] != null && Joints[nextIndex] != null)
            {
                Gizmos.DrawLine(Joints[i].position, Joints[nextIndex].position);
            }
        }
        
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(target.position, 0.1f);
        }
    }


}
