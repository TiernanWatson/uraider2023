using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplinePoint))]
public class SplineCollider : MonoBehaviour
{
    public enum SplineForward
    {
        X,
        Z
    }

    public SplinePoint Point => _point;
    public BoxCollider Collider => _collider;

    [SerializeField] private SplineForward _forward = SplineForward.X;
    [SerializeField] private float _height = 0.25f;
    [SerializeField] private float _depth = 0.1f;
    
    private SplinePoint _point;
    private BoxCollider _collider;

    private void Awake()
    {
        _point = GetComponent<SplinePoint>();
        
        if (_point && _point.Next)
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.isTrigger = true;

            bool zForward = _forward == SplineForward.Z;
            float width = Vector3.Distance(_point.transform.position, _point.Next.transform.position);

            float depthScaled = _depth / (zForward ? transform.lossyScale.x : transform.lossyScale.z);
            float heightScaled = _height / transform.lossyScale.y;

            float xSize = zForward ? depthScaled : width;
            float zSize = zForward ? width : depthScaled;
            _collider.size = new Vector3(xSize, heightScaled, zSize);

            float xCenter = zForward ? 0.0f : width / 2.0f;
            float zCenter = zForward ? width / 2.0f : 0.0f;
            _collider.center = new Vector3(xCenter, 0.0f, zCenter);
        }
        else
        {
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnSplineEnter(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnSplineExit(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnSplineStay(this);
        }
    }

    public Vector3 GetClosestPoint(Vector3 other)
    {
        Vector3 rawPoint = Physics.ClosestPoint(other, GetComponent<Collider>(), transform.position, transform.rotation);
        Vector3 result = new Vector3(rawPoint.x, transform.position.y, rawPoint.z);
        result += transform.forward * _collider.size.z * 0.5f;

        return result;
    }
}
