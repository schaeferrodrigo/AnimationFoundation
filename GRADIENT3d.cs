using UnityEditor;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{    

    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform endFactor;
    public Transform target;

    private float costFunction;

    private Vector3 D1;
    private Vector3 D2; 
    private Vector3 D3;
    public float alpha = 0.1f;
    private Vector4 theta;
    public float tolerance = 1f;



    // Start is called before the first frame update
    void Start()
    {
        D1 = Joint1.position - Joint0.position;
        D2 = Joint2.position - Joint1.position;
        D3 = endFactor.position - Joint2.position;
        theta = Vector4.zero;
        costFunction = Vector3.Distance(target.position, endFactor.position )*Vector3.Distance(target.position, endFactor.position );
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log("cost Function = "+ costFunction);
        if (costFunction > tolerance){

            Vector4 gradient = GetGradient(theta);
            Debug.Log("Gradient vector"+gradient);
            
            theta -= alpha * gradient;
            Vector3[] newPosition =  endFactorFunction(theta);
            Joint1.position = newPosition[0];
            Joint2.position = newPosition[1];
            endFactor.position = newPosition[2];
       }

        costFunction = lossCostFunction(theta);

    }

    Vector3[] endFactorFunction(Vector4 theta)
    
    {
      Quaternion[] q = new Quaternion[4];
      q[0] = Quaternion.AngleAxis(theta.x, Vector3.up);
      q[1] = Quaternion.AngleAxis(theta.y, Vector3.forward);
      q[2] = Quaternion.AngleAxis(theta.z, Vector3.up);
      q[3] = Quaternion.AngleAxis(theta.w , Vector3Int.forward);

      Vector3  j1 = Joint0.position + q[0]*q[1]*D1;
      Vector3  j2 = j1 + q[0]*q[1]*q[2]*D2;
      Vector3  endfactor = j2 + q[0]*q[1]*q[2]*q[3]*D3;

      Vector3[] result = new Vector3[3];

      result[0] = j1;
      result[1] = j2;
      result[2] = endfactor;

      return result;
    }

    float lossCostFunction(Vector4 theta){

        Vector3 endPosition = endFactorFunction(theta)[2];

        return Vector3.Distance(endPosition, target.position)*Vector3.Distance(endPosition, target.position);

    }

    Vector4 GetGradient(Vector4 theta){
        
        Vector4 gradientVector;

        float step = 1e-2f;

        Vector4 thetaPlus = theta;

        // x
        
        thetaPlus.x = theta.x + step;
        gradientVector.x = (lossCostFunction(thetaPlus)-lossCostFunction(theta))/step;


        thetaPlus = theta;
        thetaPlus.y = theta.y + step;
        gradientVector.y = (lossCostFunction(thetaPlus)-lossCostFunction(theta))/step;

        thetaPlus = theta;
        thetaPlus.z = theta.z + step;
        gradientVector.z = (lossCostFunction(thetaPlus)-lossCostFunction(theta))/step;

        thetaPlus = theta;
        thetaPlus.w = theta.w + step;
        gradientVector.w = (lossCostFunction(thetaPlus)-lossCostFunction(theta))/step;

        gradientVector.Normalize();

        return gradientVector;
    }



}
