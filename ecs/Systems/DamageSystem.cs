using System;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class DamageSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Config _config;
        private EcsWorld _world;

        private EcsFilterExt<DamageComponent> _filter;
        private EcsPool<HpComponent> _hpPool;
        private EcsPool<BaseUnitComponent> _baseUnitPool;
        private EcsPool<DeadComponent> _deadPool;
        private EcsPool<AnimatorComponent> _animatorPool;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldEffect);

            _hpPool = _world.GetPool<HpComponent>();
            _baseUnitPool = _world.GetPool<BaseUnitComponent>();
            _deadPool = _world.GetPool<DeadComponent>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var damage = ref _filter.Inc1().Get(entity);

                if (damage.Target.Unpack(_world, out var targetEntity) && !_deadPool.Has(targetEntity) &&
                    _baseUnitPool.Has(targetEntity))
                {
                    ref var uTarget = ref _baseUnitPool.Get(targetEntity);
                    var dmg = damage.Damage + _config.GetDamageFactor(damage.TeamAttacker) -
                              _config.GetArmorFactor(uTarget.teamId);

                    dmg = Math.Max(dmg, 1);

                    ref var hp = ref _hpPool.Get(targetEntity);
                    hp.value = Math.Max(0, hp.value - dmg);

                    hp.isNeedUpdate = true;

                    if (hp.value == 0)
                    {
                        ref var deadComponent = ref _deadPool.Add(targetEntity);
                        deadComponent.Time = _config.Time;

                        if (_animatorPool.Has(targetEntity))
                        {
                            ref var anim = ref _animatorPool.Get(targetEntity);
                            anim.isNeedUpdate = true;
                            anim.deathTrigger = true;
                        }

                        _world.ResetCommand(targetEntity);
                        if (_world.GetPool<MoveConfigComponent>().Has(targetEntity))
                        {
                            _world.GetPool<MoveConfigComponent>().Get(targetEntity).agent.enabled = false;
                        }
                    }
                }

                _filter.Inc1().Del(entity);
            }
        }
    }
}