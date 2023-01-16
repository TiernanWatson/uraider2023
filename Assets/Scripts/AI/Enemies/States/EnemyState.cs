using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour 
{
    public abstract class EnemyState : StateBase<Human>
    {
        public EnemyState(Human owner) : base(owner)
        {
        }

        public virtual void Damage(float strength)
        {
            _owner._health -= strength;
        }
    }
}
