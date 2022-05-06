using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    public GameObject target;
    public GameObject root;
    public GameObject ee;
    private List<GameObject> jointList = new List<GameObject>();
    private float boneLengthSum = 0.0f;
    private float distThreshold = 0.1f;
    private int hierarchicalDist = 0;
    void Start()
    {
        ComputeHierarchicalDistance();
    }

    private void ComputeHierarchicalDistance()
    {
        GameObject joint = ee;
        while(joint.transform.parent != null && joint.GetInstanceID() != root.GetInstanceID())
        {
            hierarchicalDist++;
            boneLengthSum += Vector3.Distance(joint.transform.position, joint.transform.parent.position);
            jointList.Insert(0, joint);
            joint = joint.transform.parent.gameObject;
        }
        jointList.Insert(0, root);

        if(joint.transform.parent == null)
        {
            hierarchicalDist = -1;
        }
    }

    void Update()
    {
        SolveFABRIK();
    }

    void SolveTwoBoneIK()
    {
        if(hierarchicalDist != 2)
        {
            return;
        }

        Vector3 a_pos = root.transform.position;
        Vector3 b_pos = ee.transform.parent.position;
        Vector3 c_pos = ee.transform.position;
        Vector3 t_pos = target.transform.position;

        float eps = float.Epsilon;
        float lab = (a_pos - b_pos).magnitude;
        float lcb = (b_pos - c_pos).magnitude;
        float lat = Mathf.Clamp((a_pos - t_pos).magnitude, eps, lab + lcb - eps);

        Vector3 ab = b_pos - a_pos;
        Vector3 ac = c_pos - a_pos;
        Vector3 bc = c_pos - b_pos;
        Vector3 at = t_pos - a_pos;

        float ac_ab_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, ab.normalized), -1.0f, 1.0f));
        float ba_bc_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-ab.normalized, bc.normalized), -1.0f, 1.0f));
        float ac_at_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, at.normalized), -1.0f, 1.0f));

        float ac_ab_1 = Mathf.Acos(Mathf.Clamp(( lcb*lcb - lab*lab - lat*lat ) / ( -2.0f*lab*lat ), -1.0f, 1.0f));
        float ba_bc_1 = Mathf.Acos(Mathf.Clamp(( lat*lat - lab*lab - lcb*lcb ) / ( -2.0f*lab*lcb ), -1.0f, 1.0f));

        Vector3 d = ee.transform.parent.rotation * new Vector3(0.0f, 0.0f, -1.0f);
        Vector3 axis0 = Vector3.Cross(ac, d).normalized;
        Vector3 axis1 = Vector3.Cross(ac, at).normalized;

        Quaternion r0 = Quaternion.AngleAxis((ac_ab_1 - ac_ab_0) * Mathf.Rad2Deg, Quaternion.Inverse(root.transform.rotation) * axis0);
        Quaternion r1 = Quaternion.AngleAxis((ba_bc_1 - ba_bc_0) * Mathf.Rad2Deg, Quaternion.Inverse(ee.transform.parent.rotation) * axis0);
        Quaternion r2 = Quaternion.AngleAxis((ac_at_0) * Mathf.Rad2Deg, Quaternion.Inverse(root.transform.rotation) * axis1);

        root.transform.localRotation *= (r0 * r2);
        ee.transform.parent.localRotation *= r1;
    }

    void SolveCCDIK()
    {
        if(hierarchicalDist <= 0)
        {
            return;
        }

        int iter = 0;
        int maxIter = 512;
        float dist = float.MaxValue;
        while(iter++ < maxIter && distThreshold < dist)
        {
            for(int i = jointList.Count - 2; i >= 0; --i)
            {
                Vector3 toTarget = (target.transform.position - jointList[i].transform.position).normalized;
                Vector3 toEnd = (ee.transform.position - jointList[i].transform.position).normalized;

                float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(toEnd, toTarget), -1.0f, 1.0f));
                Vector3 axis = Vector3.Cross(toEnd, toTarget).normalized;
                if(axis.magnitude > 0.0f)
                {
                    Quaternion q = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
                    Quaternion newQuat = (jointList[i].transform.rotation * q).normalized;
                    jointList[i].transform.rotation = newQuat;
                }
            }
            Vector3 rootToTarget = target.transform.position - root.transform.position;
            Vector3 stableRootToTarget = root.transform.position + rootToTarget.normalized * Mathf.Min(rootToTarget.magnitude, boneLengthSum);
            dist = Vector3.Distance(ee.transform.position, stableRootToTarget);
        }
    }

    void SolveFABRIK()
    {
        if(hierarchicalDist <= 0)
        {
            return;
        }

        int iter = 0;
        int maxIter = 512;
        float dist = float.MaxValue;
        while(iter++ < maxIter && distThreshold < dist)
        {
            Vector3 rootPosition = root.transform.position;
            Vector3 targetPosition = target.transform.position;
            float rootToTarget = (root.transform.position - target.transform.position).magnitude;

            // unreachable
            if(rootToTarget > boneLengthSum)
            {
                for(int i = 0; i < jointList.Count - 1; ++i)
                {
                    Vector3 jointPosition = jointList[i].transform.position;
                    float jointToTarget = (jointPosition - targetPosition).magnitude;
                    float lambda = Vector3.Distance(jointPosition, jointList[i+1].transform.position) / jointToTarget;

                    Vector3 newPosition = (1.0f - lambda) * jointPosition + lambda * targetPosition;
                    jointList[i+1].transform.position = newPosition;
                }
            }
            // reachable
            else
            {
                // forward

                // backward
            }
        }
    }
}