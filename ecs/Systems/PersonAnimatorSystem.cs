using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal class PersonAnimatorSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<AnimatorComponent, BaseUnitComponent> _animFilter;
        private EcsWorld _world;
        private static readonly int AttackMul = Animator.StringToHash("AttackMul");
        private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");
        private static readonly int RunTrigger = Animator.StringToHash("RunTrigger");
        private static readonly int Weapon = Animator.StringToHash("Weapon");
        private static readonly int IdleTrigger = Animator.StringToHash("IdleTrigger");
        private static readonly int DeathTrigger = Animator.StringToHash("DeathTrigger");
        private static readonly int AttackTriggerParam = Animator.StringToHash("AttackTriggerParam");
        private static readonly int SpecialStartTrigger = Animator.StringToHash("SpecialStartTrigger");
        private static readonly int SpecialEndTrigger = Animator.StringToHash("SpecialEndTrigger");

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _animFilter.Validate(_world);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _animFilter.Filter())
            {
                ref var baseUnit = ref _animFilter.Inc2().Get(entity);
                if (baseUnit.animatorCallback != null && !baseUnit.animatorCallback.isInit)
                {
                    baseUnit.animatorCallback.Init(entity, systems);
                }

                ref var animatorComponent = ref _animFilter.Inc1().Get(entity);
                if (animatorComponent.isNeedUpdate)
                {
                    animatorComponent.isNeedUpdate = false;
                    if (animatorComponent.weapon != animatorComponent.oldWeapon)
                    {
                        animatorComponent.oldWeapon = animatorComponent.weapon;
                        animatorComponent.animator.SetInteger(Weapon, animatorComponent.weapon);
                    }

                    if (animatorComponent.attackTrigger)
                    {
                        // animatorComponent.animator.SetFloat(AttackMul, 1 / animatorComponent.timeAttack);
                        animatorComponent.animator.SetFloat(AttackMul, 1f / animatorComponent.timeAttack * 30 / 24);

                        var t = animatorComponent.weapon switch
                        {
                            0 => (Random.value < 0.5 ? -1 : 1) * Random.Range(1, 4),
                            1 => Random.Range(1, 8),
                            2 => Random.Range(1, 4),
                            4 => Random.Range(1, 7),
                            5 => Random.Range(1, 12),
                            11 => Random.Range(1, 5),
                            12 => Random.Range(1, 3),
                            13 => Random.Range(1, 3),
                            _ => 0
                        };
                        animatorComponent.animator.SetInteger(AttackTriggerParam, t);

                        animatorComponent.animator.SetTrigger(AttackTrigger);
                        animatorComponent.attackTrigger = false;
                        animatorComponent.isAlreadyRun = false;
                    }

                    if (animatorComponent.specialStartTrigger)
                    {
                        animatorComponent.animator.SetTrigger(SpecialStartTrigger);
                        animatorComponent.specialStartTrigger = false;
                        animatorComponent.isAlreadyRun = false;
                    }

                    if (animatorComponent.specialEndTrigger)
                    {
                        animatorComponent.animator.SetTrigger(SpecialEndTrigger);
                        animatorComponent.specialEndTrigger = false;
                        animatorComponent.isAlreadyRun = false;
                    }

                    if (animatorComponent.deathTrigger)
                    {
                        animatorComponent.animator.SetTrigger(DeathTrigger);
                        animatorComponent.deathTrigger = false;
                        animatorComponent.isAlreadyRun = false;
                    }

                    if (animatorComponent.idleTrigger)
                    {
                        animatorComponent.animator.SetTrigger(IdleTrigger);
                        animatorComponent.idleTrigger = false;
                        animatorComponent.isAlreadyRun = false;
                    }

                    if (animatorComponent.runTrigger)
                    {
                        animatorComponent.runTrigger = false;
                        if (!animatorComponent.isAlreadyRun)
                        {
                            animatorComponent.animator.SetTrigger(RunTrigger);
                            animatorComponent.isAlreadyRun = true;
                        }
                    }
                }
            }
        }
    }
}