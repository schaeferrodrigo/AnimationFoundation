using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradient1 : MonoBehaviour
{
    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform endFactor;
    public Transform target;


    public float alpha = 0.01f;
    private float tolerance = 0.1f;
    private float costFunctionValue; 
    private Vector3 gradient;
    private Vector3 theta; 


    private float l1;
    private float l2;
    private float l3;

    // Start is called before the first frame update
    void Start()
    {
        l1 = Vector3.Distance(Joint0.position, Joint1.position);
        l2 = Vector3.Distance(Joint1.position, Joint2.position);
        l3 = Vector3.Distance(Joint2.position, endFactor.position);

        costFunctionValue = Vector3.Distance(endFactor.position, target.position) * Vector3.Distance(endFactor.position, target.position);
        theta = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (costFunctionValue > tolerance) {

            gradient = CalculateGradient();
            theta += -alpha * gradient;
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

    Vector3 CalculateGradient() {

        Vector3 gradientVector;

        float step = 0.0001f;

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


}