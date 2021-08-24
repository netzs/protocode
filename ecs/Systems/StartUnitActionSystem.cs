using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal class StartUnitActionSystem<T> : IEcsInitSystem, IEcsRunSystem where T : struct, IUnitAction
    {
        protected EcsFilterExt<WaitCommandComponent, BaseUnitComponent, T> Filter;
        protected Config Config;
        protected EcsWorld World;
        protected EcsPool<AnimatorComponent> AnimatorPool;
        private EcsPool<UnitActionCommandComponent> _unitActionCommandPool;

        public virtual void Init(EcsSystems systems)
        {
            World = systems.GetWorld();
            Filter.Validate(World);
            Config = systems.GetShared<Config>();
            AnimatorPool = World.GetPool<AnimatorComponent>();
            _unitActionCommandPool = World.GetPool<UnitActionCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in Filter.Filter())
            {
                ref var unitActionComponent = ref Filter.Inc3().Get(entity);
                ref var wait = ref Filter.Inc1().Get(entity);
                if (wait.Time < Config.Time && unitActionComponent.UnitAction.TimeActionRestored(Config.Time))
                {
                    ref var unit = ref Filter.Inc2().Get(entity);
                    var target =
                        EcsUtility.FindSingleTarget(systems, unit, new[] {unitActionComponent.UnitAction}, false);

                    if (target >= 0)
                    {
                        Filter.Inc1().Del(entity);

                        _unitActionCommandPool.Add(entity).Action = unitActionComponent.UnitAction;
                        unitActionComponent.UnitAction.target = World.PackEntity(target);
                        unitActionComponent.UnitAction.posTarget = Filter.Inc2().Get(target).Pos;

                        unitActionComponent.UnitAction.Start(Config.Time);

                        // TargetAction(systems, entity, target);

                        if (AnimatorPool.Has(entity))
                        {
                            Anim(systems, entity, unitActionComponent.UnitAction.actionTime);
                        }

                        unit.cur.rotation =
                            Quaternion.LookRotation(unitActionComponent.UnitAction.posTarget - unit.Pos);
                    }
                }
            }
        }

        protected virtual void Anim(EcsSystems systems, int entity, float unitActionActionTime)
        {
            ref var anim = ref AnimatorPool.Get(entity);
            anim.isNeedUpdate = true;
            anim.attackTrigger = true;
            anim.timeAttack = unitActionActionTime;
        }

    }
}