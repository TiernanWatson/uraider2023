using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public class CombatState : PlayerState
    {
        private const float kHolsterDelay = 0.35f;

        private bool _foundTarget;
        private bool _rightLastFired;
        private bool _wantsToFire;
        private bool _wantsToHolster;
        private float _lastFireTime;
        private float _lastDrawTime;

        private Human _targetAI;
        private Target _target;
        private Transform _targetTransform;
        private Transform _aimPoint;
        private HashSet<PlayerState> _noCombatStates;

        public CombatState(PlayerController owner) : base(owner)
        {
            _noCombatStates = new HashSet<PlayerState>();
            _aimPoint = _owner._aimReticle.transform;
            _aimPoint.gameObject.SetActive(false);
        }

        public override void OnEnter()
        {
            _foundTarget = false;
            _rightLastFired = false;
            _lastFireTime = Time.time;
            _wantsToHolster = false;
            _wantsToFire = false;
            _lastDrawTime = Time.time;

            SetAnimAimAngle();

            _owner.AnimControl.InCombat = true;
        }

        public override void OnExit()
        {
            _owner.AnimControl.InCombat = false;
            _owner.AnimControl.IsFiring = false;
            _aimPoint.gameObject.SetActive(false);

            _owner.CameraControl.TargetLock = null;

            _owner.TargetFound?.Invoke(null);
        }

        public override void Update()
        {
            _wantsToHolster = _owner.UInput.Holster.ReadValue<float>() < 0.1f;
            if (_wantsToHolster)
            {

            }

            if ((_wantsToHolster && Time.time - _lastDrawTime > kHolsterDelay) 
                || (_owner.StateMachine.State != _owner.BaseStates.Strafe && !_owner.CombatBaseStates.Contains(_owner.StateMachine.State))
                || _owner.AnimControl.IsIn("Dive"))
            {
                _owner.EquipedMachine.ChangeState(_owner.EquipedStates.NotEquiped);
            }
            else if (!_wantsToHolster)
            {
                _lastDrawTime = Time.time;

                if (!_foundTarget)
                {
                    SearchForTarget();
                }
                else if (_targetAI && _targetAI.IsDead)  // May be aiming at dummy
                {
                    _foundTarget = false;
                    _aimPoint.gameObject.SetActive(false);
                    _owner.TargetFound?.Invoke(null);
                    _owner.CameraControl.TargetLock = null;
                }
                else
                {
                    _aimPoint.position = _targetTransform.position;
                    _aimPoint.rotation = _owner.cam.transform.rotation;
                    _owner.Weapons.Equiped.LeftHand.TargetPosition = _aimPoint.position;
                    _owner.Weapons.Equiped.RightHand.TargetPosition = _aimPoint.position;
                }

                SetAnimAimAngle();

                _wantsToFire = _owner.UInput.Fire.ReadValue<float>() > 0.1f && _owner.AnimControl.IsIn(1, "PistolsBT");

                if (_wantsToFire)
                {
                    if (_rightLastFired)
                    {
                        if (Time.time - _lastFireTime > 0.2f)
                        {
                            _owner.AnimControl.Play("FireLeftPistol");
                            _owner.Weapons.Equiped.LeftHand.Fire();
                            _rightLastFired = false;
                            _lastFireTime = Time.time;
                            _wantsToFire = false;
                        }
                    }
                    else
                    {
                        if (Time.time - _lastFireTime > 0.2f)
                        {
                            _owner.AnimControl.Play("FireRightPistol");
                            _owner.Weapons.Equiped.RightHand.Fire();
                            _rightLastFired =  true;
                            _lastFireTime = Time.time;
                            _wantsToFire = false;
                        }
                    }
                }

                if (_wantsToFire && _foundTarget)
                {
                    _owner.CameraControl.TargetLock = _targetTransform;
                }
            }
        }

        private void SearchForTarget()
        {
            Collider[] cols = Physics.OverlapSphere(_owner.transform.position, 10.0f);

            Collider closest = null;
            float distClosest = Mathf.Infinity;
            foreach (Collider c in cols)
            {
                if (c.CompareTag("Enemy"))
                {
                    float dist = Vector3.Distance(c.transform.position, _owner.transform.position);
                    if (dist < distClosest)
                    {
                        closest = c;
                        distClosest = dist;
                    }
                }
            }

            _foundTarget = closest != null;
            if (_foundTarget)
            {
                _targetAI = closest.GetComponent<Human>();
                _target = closest.GetComponent<Target>();
                _targetTransform = _target.AimPoint;
                _owner.TargetFound?.Invoke(_targetTransform);
                _aimPoint.gameObject.SetActive(true);
            }
        }

        private void SetAnimAimAngle()
        {
            float angleToTarget = Vector3.SignedAngle(_owner.GetCameraForward(), _owner.transform.forward, Vector3.up);
            _owner.AnimControl.AimAngle = angleToTarget;
            
            if (_target && _owner.CameraControl.TargetLock)
            {
                // TODO: Adjust depending on anim
                float offset = 1.5f;

                Vector3 referencePosition = _owner.transform.position + Vector3.up * offset;
                Vector3 playerToEnemy = _target.transform.position - referencePosition;
                playerToEnemy.Normalize();

                Vector3 verticalReference = playerToEnemy;
                verticalReference.y = 0.0f;
                verticalReference.Normalize();
                
                float upAngleToTarget = Vector3.SignedAngle(playerToEnemy, verticalReference, _owner.Camera.transform.right);
                _owner.AnimControl.AimAngleUp = upAngleToTarget;
            }
            else
            {
                _owner.AnimControl.AimAngleUp = Mathf.Lerp(_owner.AnimControl.AimAngleUp, 0.0f, Time.deltaTime * 30.0f);
            }
        }

        public void ExcludeState(PlayerState state)
        {
            _noCombatStates.Add(state);
        }
    }
}
