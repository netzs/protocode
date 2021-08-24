using UnityEngine;
using System;

namespace ecs.Components
{
    public struct WaitCommandComponent
    {
        public float Time;
        public bool IsReWait;
    }

    public struct StartMoveCommandComponent
    {
        public Vector3 Pos;
        public float Time;
    }

    public struct MoveCommandComponent
    {
        public Vector3 Pos;
        public float Time;
    }

    public struct UnitActionCommandComponent
    {
        public UnitAction Action;

        public override string ToString()
        {
            return typeof(Action) + ":" + Action;
        }
    }

    public struct ResetActionCommandComponent {
        public UnitAction Action;
    }
}