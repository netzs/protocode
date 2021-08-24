using System;
using ecs.Components;
using Leopotam.EcsLite;
using Object = UnityEngine.Object;

namespace ecs.Systems
{
    internal class ProgressHealerActionSystem : ProgressUnitActionSystem<HealerActionComponent>, IProgressUnitActionTarget, IProgressUnitActionInit
    {
        private EcsPool<HpComponent> _hpPool;
        private EcsPool<DeadComponent> _deadPool;

        public void Initialize(EcsSystems systems)
        {
            _hpPool = Config.WorldDefault.GetPool<HpComponent>();
            WaitPool = Config.WorldDefault.GetPool<WaitCommandComponent>();
            _deadPool = Config.WorldDefault.GetPool<DeadComponent>();
        }

        public void TargetAction(EcsSystems ecsSystems, int entity)
        {
            ref var act = ref Filter.Inc2().Get(entity);
            if (act.UnitAction.target.Unpack(World, out var target) && !_deadPool.Has(target))
            {
                ref var hp = ref _hpPool.Get(target);
                hp.value = Math.Min(hp.value + act.power, hp.maxValue);
                hp.isNeedUpdate = true;
                Object.Instantiate(Filter.Inc2().Get(entity).fx, Filter.Inc1().Get(target).cur);
            }
        }
    }
}
