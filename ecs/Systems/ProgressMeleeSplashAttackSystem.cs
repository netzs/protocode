using ecs.Components;
using Leopotam.EcsLite;

namespace ecs.Systems
{
    internal class ProgressMeleeSplashAttackSystem :
        ProgressUnitActionSystem<MeleeSplashAttackComponent>, IProgressUnitActionInit, IProgressUnitActionTarget
    {
        private EcsPool<DamageComponent> _damagePool;
        private EcsPool<ImpulseComponent> _impulsePool;
        private EcsPool<GiantComponent> _giantPool;

        public void Initialize(EcsSystems systems)
        {
            _damagePool = Config.WorldEffect.GetPool<DamageComponent>();
            _impulsePool = World.GetPool<ImpulseComponent>();
            _giantPool = World.GetPool<GiantComponent>();
        }

        // protected override void TargetAction(EcsSystems ecsSystems, int entity)
        // {
        //     var dmg = Config.WorldEffect.NewEntity();
        //     ref var damage = ref _damagePool.Add(dmg);
        //     var ua = Filter.Inc2().Get(entity).UnitAction;
        //     damage.Attacker = new IdStruct() {EntityId = entity, ID = Filter.Inc1().Get(entity).id};
        //     damage.Target = ua.target;
        //     damage.Damage = 1;
        // }

        public void TargetAction(EcsSystems ecsSystems, int entity)
        {
            ref var unit = ref Filter.Inc1().Get(entity);
            ref var attackComponent = ref Filter.Inc2().Get(entity);
            var targets = EcsUtility.FindAreaTarget(ecsSystems, unit, attackComponent.UnitAction.posTarget,
                attackComponent.radius, attackComponent.UnitAction.typeTarget);

            foreach (var tid in targets)
            {
                var dmg = Config.WorldEffect.NewEntity();
                ref var damage = ref _damagePool.Add(dmg);
                // var ua = Filter.Inc2().Get(tid).UnitAction;
                damage.Attacker = World.PackEntity(entity);
                damage.Target = World.PackEntity(tid);
                damage.Damage = attackComponent.damage;

                damage.TeamAttacker = unit.teamId;

                if (!_giantPool.Has(tid))
                {
                    var un = Filter.Inc1().Get(tid).Pos;
                    if (_impulsePool.Has(tid))
                    {
                        ref var imp = ref _impulsePool.Get(tid);
                        var im = (un - attackComponent.UnitAction.posTarget).normalized;
                        imp.Pos = im * 3;
                        imp.Factor = 0.3f;
                    }
                    else
                    {
                        ref var imp = ref _impulsePool.Add(tid);
                        var im = (un - attackComponent.UnitAction.posTarget).normalized;
                        imp.Pos = im * 3;
                        imp.Factor = 0.3f;
                    }
                }
            }
        }
    }
}