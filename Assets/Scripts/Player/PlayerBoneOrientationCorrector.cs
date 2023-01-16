using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoneOrientationCorrector : MonoBehaviour
{
    private readonly Vector3 ReferenceBoneOrientation = new Vector3(0.0f, 1.0f, 0.0f);

    [SerializeField] private Transform _rootBone;

    private List<Quaternion> _rotationOffsets;
    private List<Vector3> _translationOffsets;
    private List<Quaternion> _results;
    private List<Vector3> _translationResults;
    private int _currentBone = 0;
    private Transform _rootRoot;

    private void Awake()
    {
        _rotationOffsets = new List<Quaternion>(50);
        _translationOffsets = new List<Vector3>(50);
        _results = new List<Quaternion>(50);
        _translationResults = new List<Vector3>(50);
    }

    private void Start()
    {
        _rootRoot = _rootBone.parent;
        _currentBone = 0;
        AddBoneOffsets(_rootBone);
    }

    private void LateUpdate()
    {
        _results.Clear();

        _currentBone = 0;
        AddNewResults(_rootBone, Quaternion.identity);

        _currentBone = 0;
        ApplyNewResults(_rootBone);
    }

    private void AddBoneOffsets(Transform bone)
    {
        if (IsValidBone(bone))
        {
            //Quaternion rotation = Quaternion.FromToRotation(ReferenceBoneOrientation, bone.forward);
            //Quaternion rotation = Quaternion.FromToRotation(ReferenceBoneOrientation, bone.localRotation * Vector3.forward);
            Quaternion rotation = bone.localRotation;
            _rotationOffsets.Insert(_currentBone++, rotation);
            _translationOffsets.Insert(_currentBone - 1, bone.localPosition);

            Debug.Log("Adding offset for: " + bone.name + " with " + rotation.eulerAngles + " from forward " + bone.forward);
        }

        foreach (Transform t in bone)
        {
            Debug.DrawLine(bone.position, t.position, Color.red, 20.0f);
            AddBoneOffsets(t);
        }
    }

    private void AddNewResults(Transform bone, Quaternion accumulatedRotation)
    {
        if (IsValidBone(bone))
        {
            accumulatedRotation = _rotationOffsets[_currentBone++];
            Quaternion newRotation = accumulatedRotation * bone.localRotation;
            Vector3 newTranslation = bone.position + _translationOffsets[_currentBone - 1];
            
            _results.Add(newRotation);
            _translationResults.Add(newTranslation);
        }

        foreach (Transform t in bone)
        {
            AddNewResults(t, accumulatedRotation);
        }
    }

    private void ApplyNewResults(Transform bone)
    {
        if (IsValidBone(bone))
        {
            bone.localRotation = _results[_currentBone++];
        }

        foreach(Transform t in bone)
        {
            ApplyNewResults(t);
        }
    }

    private bool IsValidBone(Transform t)
    {
        return t.CompareTag("Bone");
    }
}
