using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    public interface IProgressUnitActionTarget
    {
        void TargetAction(EcsSystems ecsSystems, int entity);
    }

    public interface IProgressUnitActionNonTarget
    {
        void NonTargetAction(EcsSystems ecsSystems, int entity);
    }

    public interface IProgressUnitActionInit
    {
        void Initialize(EcsSystems ecsSystems);
    }


    internal class ProgressUnitActionSystem<T> : IEcsInitSystem, IEcsRunSystem where T : struct, IUnitAction
    {
        protected EcsFilterExt<BaseUnitComponent, T, UnitActionCommandComponent> Filter;
        protected Config Config;
        protected EcsWorld World;
        protected EcsPool<WaitCommandComponent> WaitPool;
        private EcsPool<AnimHitComponent> _hitAnimPool;
        private EcsPool<AnimatorComponent> _animatorPool;

        public void Init(EcsSystems systems)
        {
            World = systems.GetWorld();
            Filter.Validate(World);
            Config = systems.GetShared<Config>();
            WaitPool = World.GetPool<WaitCommandComponent>();
            _hitAnimPool = World.GetPool<AnimHitComponent>();
            _animatorPool = World.GetPool<AnimatorComponent>();

            if (this is IProgressUnitActionInit init)
            {
                init.Initialize(systems);
            }
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in Filter.Filter())
            {
                ref var unitActionComponent = ref Filter.Inc2().Get(entity);


                if (unitActionComponent.UnitAction == Filter.Inc3().Get(entity).Action)
                {
                    var isAnim = _animatorPool.Has(entity);
                    var isHit = _hitAnimPool.Has(entity);
                    if (isAnim && isHit || !isAnim && unitActionComponent.UnitAction.TimePostDelay(Config.Time))
                    {
                        if (isHit)
                        {
                            _hitAnimPool.Del(entity);
                        }

                        unitActionComponent.UnitAction.isCompleteAction = true;

                        if (this is IProgressUnitActionTarget targetSystem)
                        {
                            if (unitActionComponent.UnitAction.target.Unpack(World, out var targetEntity) &&
                                Filter.Inc1().Has(targetEntity))
                            {
                                targetSystem.TargetAction(systems, entity);
                            }
                        }


                        if (this is IProgressUnitActionNonTarget nonTargetSystem)
                        {
                            nonTargetSystem.NonTargetAction(systems, entity);
                        }
                    }


                    if (unitActionComponent.UnitAction.TimePostDelay(Config.Time) &&
                        unitActionComponent.UnitAction.isCompleteAction)
                    {
                        Filter.Inc3().Del(entity);
                        WaitPool.Add(entity);
                    }
                }
            }
        }
    }
}