using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patterns
{
    public class State
    {
        protected FSM fsm;
        
        public State(FSM f)
        {
            fsm = f;
        }
        
        public virtual void Enter() { }
        
        public virtual void Exit() { }
        
        public virtual void Update() { }
        
        public virtual void FixedUpdate() { }
    }
}