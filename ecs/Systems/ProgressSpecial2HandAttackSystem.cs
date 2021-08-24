using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ProgressSpecial2HandAttackSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<BaseUnitComponent, Special2HandAttackComponent, UnitActionCommandComponent> _filter;
        private Config _config;
        private EcsWorld _world;
        private EcsPool<WaitCommandComponent> _waitPool;
        private EcsPool<AnimHitComponent> _hitAnimPool;
        private EcsPool<AnimatorComponent> _animatorPool;
        private EcsPool<DamageComponent> _damagePool;
        private EcsPool<MainPlayerComponent> _mainPlayer;


        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _config = systems.GetShared<Config>();
            _waitPool = _world.GetPool<WaitCommandComponent>();
            _hitAnimPool = _world.GetPool<AnimHitComponent>();
            _damagePool = _config.WorldEffect.GetPool<DamageComponent>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var unitActionComponent = ref _filter.Inc2().Get(entity);
                if (unitActionComponent.UnitAction == _filter.Inc3().Get(entity).Action)
                {
                    if (_hitAnimPool.Has(entity))
                    {
                        _hitAnimPool.Del(entity);
                        unitActionComponent.UnitAction.isCompleteAction = true;
                        // PositionAction(systems, entity);
                        unitActionComponent.endTime = _config.Time + unitActionComponent.duration;
                        // Debug.Log("start");
                        unitActionComponent.particle.SetActive(true);
                    }

                    if (unitActionComponent.UnitAction.isCompleteAction)
                    {
                        unitActionComponent.currentPeriod += _config.DeltaTime;
                        if (unitActionComponent.currentPeriod > unitActionComponent.periodDamageTime)
                        {
                            unitActionComponent.currentPeriod -= unitActionComponent.periodDamageTime;
                            SplashDamage(systems, entity);
                        }

                    }

                    if (unitActionComponent.UnitAction.startTime + unitActionComponent.UnitAction.actionTime +
                        unitActionComponent.duration < _config.Time && unitActionComponent.UnitAction.isCompleteAction)
                    {
                        unitActionComponent.particle.SetActive(false);
                        _filter.Inc3().Del(entity);
                        _waitPool.Add(entity);

                        ref var anim = ref _animatorPool.Get(entity);
                        anim.isNeedUpdate = true;
                        // anim.specialEndTrigger = true;
                        // anim.timeAttack = 0.5f;
                    }
                }
            }
        }

        private void SplashDamage(EcsSystems ecsSystems, int entity)
        {
            ref var unit = ref _filter.Inc1().Get(entity);
            ref var attackComponent = ref _filter.Inc2().Get(entity);
            var targets = EcsUtility.FindAreaTarget(ecsSystems, unit, unit.Pos,
                attackComponent.radius, attackComponent.UnitAction.typeTarget);

            foreach (var tid in targets)
            {
                var dmg = _config.WorldEffect.NewEntity();
                ref var damage = ref _damagePool.Add(dmg);
                damage.Attacker = _world.PackEntity(entity);
                damage.Target = _world.PackEntity(tid);
                damage.Damage = attackComponent.damage;
                damage.TeamAttacker = unit.teamId;
            }
        }
    }
}