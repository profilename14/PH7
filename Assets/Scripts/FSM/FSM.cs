using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patterns
{
    public class FSM
    {
        protected Dictionary<string, State> states = new Dictionary<string, State>();
        protected State currentState;

        public FSM()
        {
        }

        public void Add(string key, State state)
        {
            states.Add(key, state);
        }

        public State GetState(string key)
        {
            return states[key];
        }

        public string GetCurrentState()
        {
            return ((EnemyState)currentState).StateName;
        }

        public void SetCurrentState(string stateKey)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = GetState(stateKey);

            if (currentState != null)
            {
                currentState.Enter();
            }
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }

        public void FixedUpdate()
        {
            if (currentState != null)
            {
                currentState.FixedUpdate();
            }
        }
    }
}