using System;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Serialization;

namespace ecs.Components
{
    [Serializable]
    public class UnitAction
    {
        public override string ToString()
        {
            return $"UnitAction:{isCompleteAction}:{startTime}";
        }

        public float delay; // задержка в использовании действия
        public int movePriority; // приоритет того к какому объекту идет юнит
        public float moveRange; // максимальное расстояние для поиска цели чтобы к ней идти
        public float rangeUse; // расстояние применения действия
        public float actionTime; // время с начала действия, до повяления сущности действия (замах)
        public TypeTarget typeTarget; // тип поиска цели

        [HideInInspector] public bool isCompleteAction; // породилась сущность действия

        [HideInInspector] public float startTime; // время начала действия

        // [HideInInspector] public bool isActionCommand;
        [HideInInspector] public EcsPackedEntity target;
        [HideInInspector] public Vector3 posTarget;


        public bool TimeActionRestored(float tm)
        {
            return startTime + delay < tm && startTime + actionTime < tm;
        }

        public bool TimePostDelay(float tm)
        {
            return startTime + actionTime < tm;
        }

        public void Start(float tm)
        {
            startTime = tm;
            isCompleteAction = false;
        }
    }
}