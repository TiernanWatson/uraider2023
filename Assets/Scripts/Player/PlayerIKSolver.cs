using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerIKEffector
{
    RightHand,
    LeftHand,
    RightFoot,
    LeftFoot
}

public struct PlayerIKConstraint
{
    Vector3 position;
    int boneIndex;

    public PlayerIKConstraint(Vector3 position, int boneIndex)
    {
        this.position = position;
        this.boneIndex = boneIndex;
    }
}

public class PlayerIKSolver : MonoBehaviour
{
    [SerializeField] private int _maxIterations = 8;
    [SerializeField] private float _tolerance = 0.0001f;
    [SerializeField] private Transform[] _leftHandChain;
    [SerializeField] private Transform[] _rightHandChain;
    [SerializeField] private Transform[] _leftFootChain;
    [SerializeField] private Transform[] _rightFootChain;

    private float[] _leftHandLengths;
    private float[] _rightHandLengths;
    private float[] _leftFootLengths;
    private float[] _rightFootLengths;

    private void Start()
    {
        if (_leftHandChain.Length > 1)
        {
            _leftHandLengths = CalculateLengths(_leftHandChain);
        }

        if (_rightHandChain.Length > 1)
        {
            _rightHandLengths = CalculateLengths(_rightHandChain);
        }

        if (_leftFootChain.Length > 1)
        {
            _leftFootLengths = CalculateLengths(_leftFootChain);
        }

        if (_rightFootChain.Length > 1)
        {
            _rightFootLengths = CalculateLengths(_rightFootChain);
        }
    }

    public void Solve(PlayerIKEffector effector, Vector3 goal, float weight = 1.0f, params PlayerIKConstraint[] constraints)
    {
        GetChainAndLengths(effector, out Transform[] chain, out float[] lengths);

        Debug.AssertFormat(chain != null && chain.Length > 1, "Null or short IK chain for {0}", effector.ToString());
        Debug.AssertFormat(lengths != null && lengths.Length > 0, "Null or empty IK lengths for {0}", effector.ToString());

        Vector3[] positions = new Vector3[chain.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = chain[i].position;
        }

        bool converged = false;

        if (!CanReach(chain, lengths, goal))
        {
            ElongateChain(positions, lengths, goal);
        }
        else
        {
            float difference = Vector3.Distance(chain[chain.Length - 1].position, goal);

            for (int iteration = 0; iteration < _maxIterations && difference > _tolerance; iteration++)
            {
                BackwardsStep(positions, lengths, goal);
                ForwardsStep(positions, lengths);

                difference = Vector3.Distance(chain[chain.Length - 1].position, goal);
            }

            if (difference < _tolerance)
            {
                converged = true;
            }
        }

        for (int i = 1; i < positions.Length; i++)
        {
            positions[i] = Vector3.Lerp(chain[i].position, positions[i], weight);
            Debug.DrawLine(positions[i - 1], positions[i], converged ? Color.green : Color.red);
        }

        // Setting bone position messes up model, so rotate at end (not throughout as length is restricted in calculations then)
        for (int i = 0; i < chain.Length - 1; i++)
        {
            RotateToPosition(chain[i], chain[i + 1], positions[i + 1]);
        }
    }

    public Transform GetEffectorTransform(PlayerIKEffector effector)
    {
        switch (effector)
        {
            case PlayerIKEffector.LeftFoot:
                return _leftFootChain[_leftFootChain.Length - 1];
            case PlayerIKEffector.RightFoot:
                return _rightFootChain[_rightFootChain.Length - 1];
            case PlayerIKEffector.LeftHand:
                return _leftHandChain[_leftHandChain.Length - 1];
            case PlayerIKEffector.RightHand:
                return _rightHandChain[_rightHandChain.Length - 1];
            default:
                Debug.LogError("Unrecognised IK effector type for retrieving transform");
                return null;
        }
    }

    public float GetLength(PlayerIKEffector effector, int boneIndex)
    {
        GetChainAndLengths(effector, out _, out float[] lengths);

        Debug.AssertFormat(boneIndex < lengths.Length, "Bone index outside range for {0}", effector);

        return lengths[boneIndex];
    }

    public bool CanReach(PlayerIKEffector effector, Vector3 position)
    {
        GetChainAndLengths(effector, out Transform[] chain, out float[] lengths);

        Debug.AssertFormat(chain != null && chain.Length > 1, "Null or short IK chain for {0}", effector.ToString());
        Debug.AssertFormat(lengths != null && lengths.Length > 0, "Null or empty IK lengths for {0}", effector.ToString());

        return CanReach(chain, lengths, position);
    }

    private bool CanReach(Transform[] chain, float[] lengths, Vector3 goal)
    {
        float totalDistance = Vector3.Distance(chain[0].position, goal);
        float totalLength = lengths.Sum();

        return totalLength > totalDistance;
    }

    private void ElongateChain(Vector3[] chain, float[] lengths, Vector3 goal)
    {
        Vector3 direction = (goal - chain[0]).normalized;

        for (int i = 1; i < chain.Length; i++)
        {
            chain[i] = chain[i - 1] + direction * lengths[i - 1];
        }
    }

    private void BackwardsStep(Vector3[] chain, float[] lengths, Vector3 goal)
    {
        chain[chain.Length - 1] = goal;

        for (int i = chain.Length - 2; i > 0; i--)
        {
            Vector3 direction = (chain[i] - chain[i + 1]).normalized;
            direction *= lengths[i];
            chain[i] = chain[i + 1] + direction;
        }
    }

    private void ForwardsStep(Vector3[] chain, float[] lengths)
    {
        for (int i = 1; i < chain.Length; i++)
        {
            Vector3 direction = (chain[i] - chain[i - 1]).normalized;
            direction *= lengths[i - 1];
            chain[i] = chain[i - 1] + direction;
        }
    }

    private void RotateToPosition(Transform p1, Transform p2, Vector3 position)
    {
        Vector3 oldDirection = (p2.position - p1.position).normalized;
        Vector3 newDirection = (position - p1.position).normalized;
        Quaternion rotation = Quaternion.FromToRotation(oldDirection, newDirection);
        p1.rotation = rotation * p1.rotation;
    }

    private float[] CalculateLengths(Transform[] chain)
    {
        float[] result = new float[chain.Length - 1];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Vector3.Distance(chain[i].position, chain[i + 1].position);
        }

        return result;
    }

    private void GetChainAndLengths(PlayerIKEffector effector, out Transform[] chain, out float[] lengths)
    {
        switch (effector)
        {
            case PlayerIKEffector.LeftFoot:
                chain = _leftFootChain;
                lengths = _leftFootLengths;
                break;
            case PlayerIKEffector.LeftHand:
                chain = _leftHandChain;
                lengths = _leftHandLengths;
                break;
            case PlayerIKEffector.RightFoot:
                chain = _rightFootChain;
                lengths = _rightFootLengths;
                break;
            case PlayerIKEffector.RightHand:
                chain = _rightHandChain;
                lengths = _rightHandLengths;
                break;
            default:
                chain = null;
                lengths = null;
                Debug.LogError("Unrecognised IK effector");
                break;
        }
    }
}
