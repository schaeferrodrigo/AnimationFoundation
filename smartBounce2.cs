using System.Linq;
using UnityEngine;

public class smartBounce2 : MonoBehaviour
{

    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform Joint3;
    public Transform Joint4;
    public Transform endEffector;

    public Transform target;

    public float tolerance;

    public float maxIterations;

    private int iterationCount;

    private float rotation;

    private Vector3 axis;

    private int index;

    private Vector3[] Links;




    Vector3[] Joints = new Vector3[5];

    // Start is called before the first frame update
    void Start()
    {
        tolerance = 1.0f;
        maxIterations= 1e5f;
        iterationCount = 0;
        index = 4;
        Joints[0] = Joint0.position;
        Joints[1] = Joint1.position;
        Joints[2] = Joint2.position;
        Joints[3] = Joint3.position;
        Joints[4] = Joint4.position;

        getLinks();
       
    

    }

    // Update is called once per frame
    void Update()
    {
        if( iterationCount < maxIterations &&  Vector3.Distance(endEffector.position, target.position) > tolerance  ){
          
          getRotationAxisIndex();

          UpdatePosition(index, rotation, axis);

          if (index==0){
            index = 4;
    
          }else {index --;}

          iterationCount ++;
        Debug.Log("ïteration count = " + iterationCount);
        Debug.Log("ïndex = " + index);

        }
        
    }

    void getLinks(){

    Links = new Vector3[5];

    for(int i=0; i<4; i++){
        Links[i] = Joints[i+1] - Joints[i];
    } 
        Links[4] = endEffector.position - Joints[4];
    }
    
    Vector3[] GetVectors(Vector3 Pd){
        
        Vector3[] referenceVectors = new Vector3[2];
        referenceVectors[0] = Vector3.Normalize(endEffector.position - Pd);
        referenceVectors[1] = Vector3.Normalize(target.position - Pd);
       

        return referenceVectors;
    }
    float GetAngle(Vector3[] referenceVectors){
        float theta;
        theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(referenceVectors[0],referenceVectors[1]),-1.0f, 1.0f));

        return theta;

    }


    Vector3 GetAxis(Vector3[] referenceVectors){

        Vector3 r;
        r = Vector3.Cross(referenceVectors[0],referenceVectors[1]);
        return r;

    }
    void UpdatePosition(int index, float rotation, Vector3 axis){

        Quaternion q = Quaternion.AngleAxis(rotation*180/Mathf.PI, axis);
        if (index <=3){

        for(int i = index; i<=3; i++ )
        {
         Joints[i+1] = Joints[i] + q*Links[i];

        }
        }

        Debug.Log(endEffector.position);

        endEffector.position = Joints[4] + q*Links[4];

        Debug.Log(endEffector.position);

        Joint1.position = Joints[1];
        Joint2.position = Joints[2];
        Joint3.position = Joints[3];
        Joint4.position = Joints[4];


        getLinks();
        
    }

    void getRotationAxisIndex(){
         
        index = 0;
        rotation =0;
        axis = Vector3.zero;
        for(int i = 0; i<5; i++){
            Vector3 Pd = Joints[i];
            Debug.Log("Joint position" + Pd);
            Vector3[] referenceVectors;
            referenceVectors = GetVectors(Pd);
            float candidateRotation =  GetAngle(referenceVectors);
            Vector3 candidateAxis = GetAxis(referenceVectors);
            if(Mathf.Abs(candidateRotation) > Mathf.Abs(rotation)){
                rotation = candidateRotation;
                axis = candidateAxis;
                index = i;
            }
        }

    }
}
