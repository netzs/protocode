using System;
using System.Linq;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal sealed class StartSpawnSlaveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<WaitCommandComponent, BaseUnitComponent, SpawnSlaveComponent> _filter;
        private Config _config;
        private EcsWorld _world;
        private EcsPool<AnimatorComponent> _animatorPool;
        private EcsPool<UnitActionCommandComponent> _unitActionCommandPool;
        private EcsPool<UnitGroupDataComponent> _groupPool;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _config = systems.GetShared<Config>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
            _unitActionCommandPool = _world.GetPool<UnitActionCommandComponent>();

            _groupPool = _config.WorldGroup.GetPool<UnitGroupDataComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var wait = ref _filter.Inc1().Get(entity);
                ref var unitActionComponent = ref _filter.Inc3().Get(entity);
                if (wait.Time < _config.Time && unitActionComponent.UnitAction.TimeActionRestored(_config.Time))
                {
                    int gr;
                    if (unitActionComponent.Group.Unpack(_config.WorldGroup, out var g))
                    {
                        gr = g;
                    }
                    else
                    {
                        gr = _config.WorldGroup.NewEntity();
                        ref var groupDataComponent = ref _groupPool.Add(gr);
                        groupDataComponent.Leader = _world.PackEntity(entity);
                        Array.Resize(ref groupDataComponent.Units, unitActionComponent.units.Length);
                        for (var index = 0; index < groupDataComponent.Units.Length; index++)
                        {
                            groupDataComponent.Units[index] = _config.EmptyEcsPackEntity;
                        }

                        groupDataComponent.Count = 0;
                        groupDataComponent.Distance = unitActionComponent.distance;
                        unitActionComponent.Group = _config.WorldGroup.PackEntity(gr);
                    }


                    ref var group = ref _groupPool.Get(gr);
                    if (group.Units.Length < unitActionComponent.units.Length)
                    {
                        Array.Resize(ref group.Units, unitActionComponent.units.Length);
                        for (var i = group.Units.Length; i < unitActionComponent.units.Length; i++)
                        {
                            group.Units[i] = _config.EmptyEcsPackEntity;
                        }
                    }

                    if (group.Units.Contains(_config.EmptyEcsPackEntity))
                    {
                        _filter.Inc1().Del(entity);
                        _unitActionCommandPool.Add(entity).Action = unitActionComponent.UnitAction;
                        unitActionComponent.UnitAction.Start(_config.Time);

                        if (_animatorPool.Has(entity))
                        {
                            Anim(systems, entity, unitActionComponent.UnitAction.actionTime);
                        }
                    }
                }
            }
        }

        private void Anim(EcsSystems systems, int entity, float unitActionActionTime)
        {
            ref var anim = ref _animatorPool.Get(entity);
            anim.isNeedUpdate = true;
            anim.attackTrigger = true;
            anim.timeAttack = unitActionActionTime;
        }
    }
}