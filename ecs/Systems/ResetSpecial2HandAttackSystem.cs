using ecs.Components;
using Leopotam.EcsLite;

namespace ecs.Systems
{
    internal class ResetSpecial2HandAttackSystem : ResetUnitActionSystem<Special2HandAttackComponent>
    {
        protected override void Reset(EcsSystems systems, int entity)
        {
            ref var unit = ref Filter.Inc3().Get(entity);
            unit.particle.SetActive(false);
        }
    }
}