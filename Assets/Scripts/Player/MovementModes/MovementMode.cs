using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Abstraction of player movement.  Cleaner storage of variables related 
    /// to this type of movement and allows start/end functions.
    /// </summary>
    public abstract class MovementMode
    {
        protected PlayerMovement _movement;

        public MovementMode(PlayerMovement movement)
        {
            _movement = movement;
        }

        /// <summary>
        /// Called by user to switch to this movement mode
        /// </summary>
        public void Activate()
        {
            _movement.ChangeMode(this);
        }

        /// <summary>
        /// Called whenever this movement mode is switched to
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Called when switching away from this movement mode
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Call once per frame to update velocity and rotation
        /// </summary>
        public abstract void Resolve();

        /// <summary>
        /// Moves the player to a location without any input (i.e. to align with something)
        /// </summary>
        /// <param name="location">Target location</param>
        /// <param name="rotation">Target rotation</param>
        /// <returns>True if finished moving, false otherwise</returns>
        public virtual bool AutoMove(Vector3 location, Quaternion rotation)
        {
            _movement.Velocity = _movement.TargetVelocity = (location - _movement.transform.position).normalized * 3.5f;
            _movement.TargetRotation = rotation;

            _movement.transform.position = Vector3.MoveTowards(_movement.transform.position, location, Time.deltaTime * 3.5f);
            _movement.transform.rotation = _movement.Rotation = Quaternion.RotateTowards(_movement.transform.rotation, rotation, Time.deltaTime * 240.0f);

            float distanceLeft = Vector3.Distance(_movement.transform.position, location);
            float angleLeft = Quaternion.Angle(_movement.transform.rotation, rotation);
            if (distanceLeft < 0.01f && angleLeft < 5.0f)
            {
                _movement.transform.position = location;
                _movement.transform.rotation = _movement.Rotation = rotation;
                return true;
            }

            return false;
        }
    }
}
