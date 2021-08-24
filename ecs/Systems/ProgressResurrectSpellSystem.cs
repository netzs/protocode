using ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace ecs.Systems
{
    internal class ProgressResurrectSpellSystem : ProgressUnitActionSystem<ResurrectSpellComponent>,
        IProgressUnitActionInit, IProgressUnitActionTarget
    {
        private EcsPool<DeadComponent> _deadPool;
        private EcsPool<HpComponent> _hpPool;

        public void Initialize(EcsSystems systems)
        {
            _deadPool = Config.WorldDefault.GetPool<DeadComponent>();
            _hpPool = Config.WorldDefault.GetPool<HpComponent>();
            WaitPool = Config.WorldDefault.GetPool<WaitCommandComponent>();
        }

        public void TargetAction(EcsSystems ecsSystems, int entity)
        {
            ref var unit = ref Filter.Inc1().Get(entity);
            if (Filter.Inc2().Get(entity).UnitAction.target.Unpack(World, out var target))
            {
                if (_deadPool.Has(target) && _hpPool.Has(target))
                {
                    _deadPool.Del(target);
                    ref var hp = ref _hpPool.Get(target);
                    hp.value = hp.maxValue;
                    hp.isNeedUpdate = true;
                    WaitPool.Add(target);

                    Object.Instantiate(Filter.Inc2().Get(entity).fx, Filter.Inc1().Get(target).cur);

                    if (World.GetPool<MoveConfigComponent>().Has(target))
                    {
                        World.GetPool<MoveConfigComponent>().Get(target).agent.enabled = true;
                    }
                }
            }
        }
    }
}