using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Patterns
{
    public class EnemyState : State
    {
        protected EnemyAI ai;
        private string stateName;
        public string StateName { get { return stateName; } }
        public EnemyState(FSM fsm, string name, EnemyAI enemy) : base(fsm)
        {
            ai = enemy;
            stateName = name;
        }
        public delegate void StateDelegate();
        public StateDelegate OnEnterDelegate { get; set; } = null;
        public StateDelegate OnExitDelegate { get; set; } = null;
        public StateDelegate OnUpdateDelegate { get; set; } = null;
        public StateDelegate OnFixedUpdateDelegate { get; set; } = null;
        public override void Enter()
        {
            OnEnterDelegate?.Invoke();
        }
        public override void Exit()
        {
            OnExitDelegate?.Invoke();
        }
        public override void Update()
        {
            OnUpdateDelegate?.Invoke();
        }
        public override void FixedUpdate()
        {
            OnFixedUpdateDelegate?.Invoke();
        }
    }
}