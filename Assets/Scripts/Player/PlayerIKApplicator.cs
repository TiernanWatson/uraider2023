using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads animation curves from animator and applies them to feet IK
/// </summary>
public class PlayerIKApplicator : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _threshold = 0.01f;
    [SerializeField] private float _feetOffset = 0.1f;
    [SerializeField] private float _maxHipAdjustment = 0.3f;
    [SerializeField] private LayerMask _layers;
    [SerializeField] private Transform _hip;

    private PlayerAnim _anim;
    private PlayerIKSolver _solver;

    private void Awake()
    {
        _anim = GetComponent<PlayerAnim>();
        _solver = GetComponent<PlayerIKSolver>();
    }

    private void LateUpdate()
    {
        float leftFootWeight = _anim.LeftFootIK;
        float rightFootWeight = _anim.RightFootIK;

        bool hasLeftPosition = FindTargetPosition(PlayerIKEffector.LeftFoot, out Vector3 leftGoal);
        bool hasRightPosition = FindTargetPosition(PlayerIKEffector.RightFoot, out Vector3 rightGoal);

        leftGoal = Vector3.Lerp(_solver.GetEffectorTransform(PlayerIKEffector.LeftFoot).position, leftGoal, leftFootWeight);
        rightGoal = Vector3.Lerp(_solver.GetEffectorTransform(PlayerIKEffector.RightFoot).position, rightGoal, rightFootWeight);

        float hipAdjustment = 0.0f;
        if (hasLeftPosition)
        {
            if (!_solver.CanReach(PlayerIKEffector.LeftFoot, leftGoal))
            {
                float distance = Vector3.Distance(leftGoal, _solver.GetEffectorTransform(PlayerIKEffector.LeftFoot).position);
                if (distance < _maxHipAdjustment)
                {
                    hipAdjustment = -distance;
                }
            }
        }

        if (hasRightPosition)
        {
            if (!_solver.CanReach(PlayerIKEffector.RightFoot, rightGoal))
            {
                float distance = Vector3.Distance(rightGoal, _solver.GetEffectorTransform(PlayerIKEffector.RightFoot).position);
                if (distance < _maxHipAdjustment && -distance < hipAdjustment)
                {
                    hipAdjustment = -distance;
                }
            }
        }

        _hip.transform.position = _hip.transform.position + Vector3.up * hipAdjustment;

        if (leftFootWeight > _threshold)
        {
            _solver.Solve(PlayerIKEffector.LeftFoot, leftGoal);
        }

        if (rightFootWeight > _threshold)
        {
            _solver.Solve(PlayerIKEffector.RightFoot, rightGoal);
        }
    }

    private bool FindTargetPosition(PlayerIKEffector effector, out Vector3 goal)
    {
        Transform transform = _solver.GetEffectorTransform(effector);
        Ray ray = new Ray(transform.position, Vector3.down);
        float distance = _feetOffset + _maxHipAdjustment;

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.cyan);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, _layers.value, QueryTriggerInteraction.Ignore))
        {
            goal = hit.point + Vector3.up * _feetOffset;
            return true;
        }

        goal = Vector3.zero;
        return false;
    }
}
