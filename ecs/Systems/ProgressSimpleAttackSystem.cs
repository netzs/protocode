using ecs.Components;
using Leopotam.EcsLite;

namespace ecs.Systems
{
    internal class ProgressSimpleAttackSystem : ProgressUnitActionSystem<SimpleAttackComponent>,
        IProgressUnitActionInit, IProgressUnitActionTarget
    {
        private EcsPool<DamageComponent> _damagePool;

        public void Initialize(EcsSystems systems)
        {
            _damagePool = Config.WorldEffect.GetPool<DamageComponent>();
        }

        public void TargetAction(EcsSystems ecsSystems, int entity)
        {
            var dmg = Config.WorldEffect.NewEntity();
            ref var damage = ref _damagePool.Add(dmg);
            var ua = Filter.Inc2().Get(entity).UnitAction;
            damage.Attacker = World.PackEntity(entity);
            damage.Target = ua.target;
            ref var act = ref Filter.Inc2().Get(entity);
            damage.Damage = act.damage;
            damage.TeamAttacker = Filter.Inc1().Get(entity).teamId;
        }
    }
}