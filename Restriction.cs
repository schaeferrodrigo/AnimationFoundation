using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Restriction : MonoBehaviour
{

    public List<Transform> Joints;
    public Transform target;
    public float tolerance = 1.0f;
    public float maxIterations = 1e5f;
    private float lambda;
    private Vector3[] Links;
    private int countIterations = 0;
    private int numberOfJoints;
    private Vector3 initialRootPostion;

    private float angle = Mathf.PI/4;


    // Start is called before the first frame update
    void Start()
    {
        numberOfJoints = Joints.Count;
        //Debug.Log("number of joints" + numberOfJoints);
        getLinks();
        //Debug.Log("stage1");
        initialRootPostion = Joints[0].position;
        //Debug.Log(initialRootPostion);
    }

    // Update is called once per frame
    void Update()
    {
        if (countIterations < maxIterations && Vector3.Distance(target.position, Joints[numberOfJoints - 1].position) > tolerance)
        {

            //Debug.Log("count = " + countIterations);
            Forward();
            Backward();

            countIterations++;

        }
    }

    void getLinks()
    {
        Links = new Vector3[numberOfJoints - 1];
        for (int i = 0; i < numberOfJoints - 1; i++)
        {
            //Debug.Log("index = " + i);
            Links[i] = Joints[i + 1].position - Joints[i].position;
            //Debug.Log(Links[i]);
        }
    }

    void Forward()
    {
        Vector3 temp;
        Joints[numberOfJoints - 1].position = target.position;
        int index = numberOfJoints - 2;

        for (int i = index; i > -1; i--)
        {
            
            if (index - i >1){
              Vector3 nextJoint  = Constraints3D(Joints[i].position, Joints[i+1].position, Joints[i+2].position );
              Joints[i].position = nextJoint;
            }
            float distance = Vector3.Magnitude(Links[i]);
            float denominator = Vector3.Distance(Joints[i].position, Joints[i + 1].position);
            lambda = distance / denominator;
            Debug.Log("F Lambda= "+ lambda);
            temp = lambda * Joints[i].position + (1 - lambda) * Joints[i + 1].position;
            Joints[i].position = temp;
        }

    }

    void Backward()
    {
        Vector3 temp;
        Joints[0].position = initialRootPostion;

        for (int i = 1; i < numberOfJoints; i++)
        {
            
           if (i >1){
              Vector3 nextJoint = Constraints3D(Joints[i].position, Joints[i-1].position, Joints[i-2].position );
              Joints[i].position = nextJoint;
            }
            float distance = Vector3.Magnitude(Links[i - 1]);
            float denominator = Vector3.Distance(Joints[i].position, Joints[i - 1].position);
            lambda = distance / denominator;
            temp = lambda * Joints[i].position + (1 - lambda) * Joints[i - 1].position;
           
            Joints[i].position = temp;
        }

    }


    Vector3 Constraints3D(Vector3 nextJoint, Vector3 currentJoint, Vector3 previousJoint){

        Vector3 direction;
        Vector3 projectionNextJoint;
        float distance;
        Vector3 translationNextJoint;
        Vector3 rotationNextJoint;
        Vector3 newNextJoint;
        float rotation;
        Vector3 axis;
    

        direction = currentJoint - previousJoint;
        direction = direction.normalized;
        Vector3 projection = Vector3.Dot(nextJoint, direction)*direction;
        projectionNextJoint = currentJoint + projection;
        distance = Vector3.Magnitude(projectionNextJoint - currentJoint);

        // angle and axis 
        translationNextJoint = nextJoint - projectionNextJoint;
        Vector3 zAxis = new Vector3(0,0,1);
        float cos =Mathf.Clamp(Vector3.Dot(zAxis, projection.normalized), -1f, 1f);
        rotation = Mathf.Acos(cos)*Mathf.Rad2Deg;

        axis = Vector3.Cross(projection, zAxis).normalized;

        rotationNextJoint = Quaternion.AngleAxis(rotation, axis)*translationNextJoint;

        //Circular case
        if(Vector3.Magnitude(rotationNextJoint) < distance*Mathf.Tan(angle)){
            return nextJoint;

        }else{
            newNextJoint = distance*Mathf.Tan(angle)* rotationNextJoint/Vector3.Magnitude(rotationNextJoint);
            newNextJoint = Quaternion.AngleAxis(rotation, -axis)*newNextJoint;
            newNextJoint = newNextJoint + projectionNextJoint;
            return newNextJoint;
        }

    }
    
    /*
    Vector3 rotationConstraints(Vector3 target , Vector3 Joint, Vector3 previousJoint ){

        Vector3 newTarget;

        Vector3 direction = (Joint - previousJoint).normalized;
        Vector3 O = (Vector3.Dot(direction, target)/Vector3.Dot(direction,direction))*direction + previousJoint;
        float S = Vector3.Magnitude(O - Joint);

        Vector3 z = new Vector3(0,0,1);
        float cos = Vector3.Dot(z, direction);
        float rotation = Mathf.Acos(cos)*Mathf.Rad2Deg;

        Vector3 axis = Vector3.Cross(direction, z).normalized;

        Quaternion q = Quaternion.AngleAxis(rotation, axis );

        newTarget = q*(target-O); 

        Debug.Log("projection target"+ newTarget);
        
        if (Vector3.Magnitude(newTarget )<= S*Mathf.Tan(angle) ){

            return target;
        }else{

            if (newTarget.x< 0){
                newTarget = new Vector3(-S*Mathf.Tan(angle) , 0 ,0);
            }else{
                newTarget = new Vector3(S*Mathf.Tan(angle) , 0 ,0);
            }
        
            //newTarget = Quaternion.AngleAxis(-rotation,axis)*newTarget + O;
            
            return newTarget;

        }

    }
    */

    
}