using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal class DestroySystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilterExt<DestroyComponent, BaseUnitComponent> _filterUnit;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filterUnit.Validate(_world);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterUnit.Filter())
            {
                Object.Destroy(_filterUnit.Inc2().Get(entity).cur.gameObject);
                _world.DelEntity(entity);
            }
        }
    }
}