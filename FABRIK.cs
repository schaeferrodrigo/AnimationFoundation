using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class FABRIK : MonoBehaviour
{
    
    public List<Transform> Joints;
    public Transform target;
    public float tolerance = 1.0f;
    public float maxIterations = 1e5f;
    private float lambda;
    private Vector3[] Links;
    private int  countIterations = 0;
    private int numberOfJoints;
    private Vector3 initialRootPostion;


    
    
    // Start is called before the first frame update
    void Start()
    {
       numberOfJoints = Joints.Count;
       Debug.Log("number of joints"+ numberOfJoints);
       getLinks();
       Debug.Log("stage1");
       initialRootPostion = Joints[0].position;
       Debug.Log(initialRootPostion);
    }

    // Update is called once per frame
    void Update()
    {
       if( countIterations < maxIterations && Vector3.Distance(target.position, Joints[numberOfJoints-1].position)> tolerance){
           
          Debug.Log("count = "+ countIterations); 
          Forward();
          Backward();

          countIterations++;
         
       } 
    }

    void getLinks(){
         Links = new Vector3[numberOfJoints-1];
         for (int i= 0; i < numberOfJoints -1; i++ ){
            Debug.Log("index = "+ i);
            Links[i] = Joints[i+1].position - Joints[i].position;
            Debug.Log(Links[i]);
        }
    }

    void Forward(){

        Joints[numberOfJoints-1].position = target.position;
        int index = numberOfJoints-2;
        Debug.Log("HEi"  + index); 
        for(int i = index; i >-1; i-- ){
            Debug.Log("HEI");
            float distance = Vector3.Magnitude(Links[i]);
            Debug.Log("distance = "+ distance);
            float lMagnitude = Vector3.Distance(Joints[i].position, Joints[i+1].position); 
            float lambda = distance/lMagnitude;
            Vector3 temp = lambda*Joints[i].position + (1-lambda)*Joints[i+1].position;     
            Joints[i].position =temp;
        }         

    }

    void Backward(){
       
       Joints[0].position = initialRootPostion;

       for(int i = 1; i< numberOfJoints; i++){
            float distance = Vector3.Magnitude(Links[i-1]);
            float lMagnitude = Vector3.Distance(Joints[i].position, Joints[i-1].position); 
            float lambda = distance/lMagnitude;
            Vector3 temp = lambda*Joints[i].position + (1-lambda)*Joints[i-1].position;     
            Joints[i].position = temp;
       }

    }
}
