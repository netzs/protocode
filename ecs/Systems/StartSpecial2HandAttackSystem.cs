using ecs.Components;
using Leopotam.EcsLite;

namespace ecs.Systems
{
    internal class StartSpecial2HandAttackSystem : StartUnitActionSystem<Special2HandAttackComponent>
    {
        protected override void Anim(EcsSystems systems, int entity, float unitActionActionTime)
        {
            ref var anim = ref AnimatorPool.Get(entity);
            anim.isNeedUpdate = true;
            // anim.attackTrigger = true;
            anim.specialStartTrigger = true;
            anim.timeAttack = unitActionActionTime;
        }
    }
}