using System.Collections;
using UnityEngine;

public class CDDFromBottom : MonoBehaviour
{
    public Transform Joint0;
    public Transform Joint1;
    public Transform Joint2;
    public Transform Joint3;
    public Transform Joint4;
    public Transform endEffector;

    public Transform target;

    public float tolerance = 1.0f;
    public float maxIterations = 1e5f;
    public float delay = 20.0f; // Delay in seconds for slow motion.

    private int iterationCount = 0;
    private int index = 0;

    private float rotation;
    private Vector3 axis;

    private Vector3[] Joints = new Vector3[5];
    private Vector3[] Links;

    private Coroutine ccdCoroutine;

    void Start()
    {
        // Initialize joint positions.
        Joints[0] = Joint0.position;
        Joints[1] = Joint1.position;
        Joints[2] = Joint2.position;
        Joints[3] = Joint3.position;
        Joints[4] = Joint4.position;

        // Compute the initial links.
        getLinks();

        // Start the CCD process with slow motion.
        ccdCoroutine = StartCoroutine(PerformCCDWithSlowMotion());
    }

    void getLinks()
    {
        Links = new Vector3[5];
        for (int i = 0; i < 4; i++)
        {
            Links[i] = Joints[i + 1] - Joints[i];
        }
        Links[4] = endEffector.position - Joints[4];
    }

    Vector3[] GetVectors(Vector3 Pd)
    {
        Vector3[] referenceVectors = new Vector3[2];
        referenceVectors[0] = Vector3.Normalize(endEffector.position - Pd);
        referenceVectors[1] = Vector3.Normalize(target.position - Pd);
        return referenceVectors;
    }

    float GetAngle(Vector3[] referenceVectors)
    {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(referenceVectors[0], referenceVectors[1]), -1.0f, 1.0f));
    }

    Vector3 GetAxis(Vector3[] referenceVectors)
    {
        return Vector3.Cross(referenceVectors[0], referenceVectors[1]);
    }

    void UpdatePosition(int index, float rotation, Vector3 axis)
    {
        Quaternion q = Quaternion.AngleAxis(rotation * Mathf.Rad2Deg, axis);
        if (index <= 3)
        {
            for (int i = index; i <= 3; i++)
            {
                Joints[i + 1] = Joints[i] + q * Links[i];
            }
        }

        endEffector.position = Joints[4] + q * Links[4];

        // Update Unity transforms.
        Joint1.position = Joints[1];
        Joint2.position = Joints[2];
        Joint3.position = Joints[3];
        Joint4.position = Joints[4];

        // Update links.
        getLinks();
    }

    IEnumerator PerformCCDWithSlowMotion()
    {
        while (iterationCount < maxIterations && Vector3.Distance(endEffector.position, target.position) > tolerance)
        {
            // Compute rotation for the current joint.
            Vector3 Pd = Joints[index];
            Vector3[] referenceVectors = GetVectors(Pd);
            rotation = GetAngle(referenceVectors);
            axis = GetAxis(referenceVectors);

            // Update the position.
            UpdatePosition(index, rotation, axis);

            // Move to the next joint.
            if (index == 4)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            iterationCount++;
            Debug.Log($"Iteration count: {iterationCount}, Index: {index}");

            // Add delay for slow motion.
            yield return new WaitForSeconds(delay);
        }
    }
}
