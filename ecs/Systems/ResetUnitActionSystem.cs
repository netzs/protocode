using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ResetUnitActionSystem<T> : IEcsInitSystem, IEcsRunSystem where T : struct, IUnitAction
    {
        protected EcsFilterExt<ResetActionCommandComponent, BaseUnitComponent, T> Filter;
        protected Config Config;

        public virtual void Init(EcsSystems systems)
        {
            Config = systems.GetShared<Config>();
            Filter.Validate(Config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in Filter.Filter())
            {
                ref var unitActionComponent = ref Filter.Inc1().Get(entity);
                ref var action = ref Filter.Inc3().Get(entity);
                if (unitActionComponent.Action == action.UnitAction)
                {
                    Reset(systems, entity);
                }
            }
        }

        protected virtual void Reset(EcsSystems systems, int entity)
        {
        }
    }
}