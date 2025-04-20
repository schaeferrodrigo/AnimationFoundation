using UnityEngine;

public class bounce : MonoBehaviour
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

    private int bounceVariable;
    private int newIndex;




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
       
       bounceVariable=0;

       newIndex = index - bounceVariable;
    

    }

    // Update is called once per frame
    void Update()
    {
        
        for(int i = index; i>=newIndex; i--){
        
            if( iterationCount < maxIterations &&  Vector3.Distance(endEffector.position, target.position) > tolerance  ){
            
            Vector3 Pd = Joints[i];

            //Debug.Log("Joint position" + Pd);
            
            Vector3[] referenceVectors;
            referenceVectors = GetVectors(Pd);

            
            rotation =  GetAngle(referenceVectors);
            axis = GetAxis(referenceVectors);
            //Debug.Log("theta = "+ rotation);
            //Debug.Log("r = "+ axis);

            UpdatePosition(newIndex, rotation, axis);

            iterationCount ++;
               

            }

            Debug.Log("Reference joint = " + i);
        }
        bounceVariable ++;
        if(bounceVariable >4){bounceVariable = 0;}
        
        newIndex = index - bounceVariable;
            


        //Debug.Log("ïteration count = " + iterationCount);
        //Debug.Log("ïndex = " + newIndex);
        
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
    void UpdatePosition(int newIndex, float rotation, Vector3 axis){

      
        Quaternion q = Quaternion.AngleAxis(rotation*180/Mathf.PI, axis);
        if (newIndex <=3){

        for(int i = newIndex; i<=3; i++ )
        {
         Joints[i+1] = Joints[i] + q*Links[i];

        }
        }

        //Debug.Log(endEffector.position);

        endEffector.position = Joints[4] + q*Links[4];

        //Debug.Log(endEffector.position);

        Joint1.position = Joints[1];
        Joint2.position = Joints[2];
        Joint3.position = Joints[3];
        Joint4.position = Joints[4];


        getLinks();
        
    }
}

