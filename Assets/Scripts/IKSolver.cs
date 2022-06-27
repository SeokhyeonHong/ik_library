using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IKSolver : MonoBehaviour
{
    public Transform target;
    public Transform baseJoint;
    public Transform effector;
    public string ikSolver;
    private Vector3 _stableTarget;
    private List<Transform> _joints;
    private List<float> _boneLengths;
    private float _boneLengthSum = 0.0f;
    private float _epsilon = 0.01f;
    
    void Update()
    {
        ValidateJointHierarchy();
        Vector3 baseToTarget = target.position - baseJoint.position;
        _stableTarget = baseJoint.position + Mathf.Min(_boneLengthSum - _epsilon, baseToTarget.magnitude) * baseToTarget.normalized;
        
        switch(ikSolver)
        {
            case "None":
                break;

            case "Two Bone":
                SolveTwoBoneIK();
                break;
            
            case "CCD":
                SolveCCDIK();
                break;

            case "Jacobian Transpose":
                SolveJacobianTranspose();
                break;
            
            case "Jacobian Pseudoinverse":
                SolveJacobianPinv();
                break;

            case "Jacobian DLS":
                SolveJacobianDLS();
                break;

            default:
                break;
        }
    }

    void ValidateJointHierarchy()
    {
        Transform current = effector;
        _joints = new List<Transform>();
        _boneLengths = new List<float>();
        _boneLengthSum = 0.0f;
        while(current != null && current != baseJoint)
        {
            float boneLength = Vector3.Distance(current.position, current.parent.position);
            _boneLengthSum += boneLength;
            _joints.Insert(0, current);
            _boneLengths.Insert(0, boneLength);

            current = current.parent;
        }
        
        if(current != baseJoint)
        {
            throw new UnityException("Wrong hierarchy. Base Bone is NOT in the ancestor list of Effector");
        }
        else
        {
            _joints.Insert(0, current);
        }
    }

    void SolveTwoBoneIK()
    {
        if(_joints.Count != 3)
        {
            throw new UnityException("You are trying " + _joints.Count.ToString() + " joints in a chain. Two Bone IK requires exactly two joints.");
        }

        Vector3 a_pos = baseJoint.position;
        Vector3 b_pos = effector.parent.position;
        Vector3 c_pos = effector.position;
        Vector3 t_pos = target.position;

        float lab = (a_pos - b_pos).magnitude;
        float lcb = (b_pos - c_pos).magnitude;
        float lat = Mathf.Clamp((a_pos - t_pos).magnitude, _epsilon, lab + lcb - _epsilon);

        Vector3 ab = b_pos - a_pos;
        Vector3 ac = c_pos - a_pos;
        Vector3 bc = c_pos - b_pos;
        Vector3 at = t_pos - a_pos;

        float ac_ab_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, ab.normalized), -1.0f, 1.0f));
        float ba_bc_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-ab.normalized, bc.normalized), -1.0f, 1.0f));
        float ac_at_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(ac.normalized, at.normalized), -1.0f, 1.0f));

        float ac_ab_1 = Mathf.Acos(Mathf.Clamp(( lcb*lcb - lab*lab - lat*lat ) / ( -2.0f*lab*lat ), -1.0f, 1.0f));
        float ba_bc_1 = Mathf.Acos(Mathf.Clamp(( lat*lat - lab*lab - lcb*lcb ) / ( -2.0f*lab*lcb ), -1.0f, 1.0f));

        Vector3 d = effector.parent.rotation * new Vector3(0.0f, 0.0f, -1.0f);
        Vector3 axis0 = Vector3.Cross(ac, d).normalized;
        Vector3 axis1 = Vector3.Cross(ac, at).normalized;
        
        Quaternion r0 = Quaternion.AngleAxis((ac_ab_1 - ac_ab_0) * Mathf.Rad2Deg, Quaternion.Inverse(baseJoint.rotation) * axis0);
        Quaternion r1 = Quaternion.AngleAxis((ba_bc_1 - ba_bc_0) * Mathf.Rad2Deg, Quaternion.Inverse(effector.parent.rotation) * axis0);
        Quaternion r2 = Quaternion.AngleAxis((ac_at_0) * Mathf.Rad2Deg, Quaternion.Inverse(baseJoint.rotation) * axis1);

        baseJoint.localRotation *= (r0 * r2);
        effector.parent.localRotation *= r1;
    }

    void SolveCCDIK()
    {
        int iter = 0;
        int maxIter = 16;
        float dist = (target.position - effector.position).sqrMagnitude;
        while(iter++ < maxIter && _epsilon < dist)
        {
            for(int i = 0; i < _joints.Count - 1; ++i)
            {
                Vector3 toTarget = target.position - _joints[i].position;
                Vector3 toEffector = effector.position - _joints[i].position;
                Quaternion q = Quaternion.FromToRotation(toEffector, toTarget);
                _joints[i].rotation = q * _joints[i].rotation;
            }
            dist = (target.position - effector.position).sqrMagnitude;
        }
    }

    void SolveJacobianTranspose()
    {
        Matrix jacobian = GetJacobianMatrix();
        Matrix jacobianTranspose = jacobian.Transpose();
        Matrix jacobianSquare = jacobian.MatMul(jacobianTranspose);
        
        Matrix dEffector = new Matrix(3, 1);
        for(int i = 0; i < 3; ++i)
        {
            dEffector[i, 0] = (_stableTarget - effector.position)[i];
        }

        Matrix res = jacobianSquare.MatMul(dEffector);
        float alphaBottom = res.Dot(res);
        float alphaUp = dEffector.Dot(res);
        if(alphaBottom > _epsilon)
        {
            jacobianTranspose = (alphaUp / alphaBottom) * jacobianTranspose;
            Matrix deltaRotation = jacobianTranspose.MatMul(dEffector);
            for(int i = 0; i < deltaRotation.rows; ++i)
            {
                Vector3 toEffector = (effector.position - _joints[i].position).normalized;
                Vector3 toTarget = (target.position - _joints[i].position).normalized;
                Vector3 axis = Vector3.Cross(toEffector, toTarget).normalized;
                float angle = deltaRotation[i, 0];
                Quaternion q = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
                _joints[i].rotation = q * _joints[i].rotation;
            }
        }
    }

    void SolveJacobianPinv()
    {
        float dist = (_stableTarget - effector.position).magnitude;
        for(int i = 0; i < 1024 && dist > _epsilon; ++i)
        {
            Matrix jacobian = GetJacobianMatrix();
            Matrix jacobianPinv = jacobian.Pinv(_epsilon);
            ApplyDeltaRotation(jacobianPinv);
            
            dist = (_stableTarget - effector.position).magnitude;
        }
    }

    void SolveJacobianDLS()
    {
        float dist = (_stableTarget - effector.position).sqrMagnitude;
        for(int i = 0; i < 1024 && dist > _epsilon; ++i)
        {
            Matrix jacobian = GetJacobianMatrix();
            Matrix dls = new Matrix(jacobian.cols, jacobian.rows);
            List<List<Matrix>> svd = jacobian.SVD();
            List<Matrix> us = svd[0];
            List<Matrix> sigmas = svd[1];
            List<Matrix> vTs = svd[2];
            float lambda = 0.5f;
            for(int j = 0; j < sigmas.Count; ++j)
            {
                float s = sigmas[j][0, 0];
                dls += (s / (s*s + lambda*lambda)) * vTs[j].Transpose().MatMul(us[j].Transpose());
            }
            ApplyDeltaRotation(dls);
            dist = (_stableTarget - effector.position).magnitude;
        }
    }

    Matrix GetJacobianMatrix()
    {
        Matrix ret = new Matrix(3, _joints.Count - 1);
        for(int i = 0; i < _joints.Count - 1; ++i)
        {
            Vector3 toEffector = effector.position - _joints[i].position;
            Vector3 toTarget = target.position - _joints[i].position;
            Vector3 axis = Vector3.Cross(toEffector, toTarget).normalized;
            Vector3 velocity = Vector3.Cross(axis, toEffector);
            ret[0, i] = velocity.x;
            ret[1, i] = velocity.y;
            ret[2, i] = velocity.z;
        }
        return ret;
    }
    
    private void ApplyDeltaRotation(Matrix inv)
    {
        Matrix dEffector = new Matrix(3, 1);
        for(int i = 0; i < 3; ++i)
        {
            dEffector[i, 0] = (_stableTarget - effector.position)[i];
        }

        Matrix deltaRotation = inv.MatMul(dEffector);
        for(int i = 0; i < deltaRotation.rows; ++i)
        {
            Vector3 toEffector = (effector.position - _joints[i].position).normalized;
            Vector3 toTarget = (target.position - _joints[i].position).normalized;
            Vector3 axis = Vector3.Cross(toEffector, toTarget).normalized;
            float angle = deltaRotation[i, 0];
            Quaternion q = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
            _joints[i].rotation = q * _joints[i].rotation;
        }
    }
}