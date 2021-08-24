using System;
using ecs;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

internal class SpawnSystem : IEcsInitSystem, IEcsRunSystem
{
    public EcsFilterExt<SpawnComponent, BaseUnitComponent> _filter;
    private EcsWorld _world;
    private Config _config;
    private EcsPool<BaseUnitComponent> _base;
    private EcsPool<NeedInitComponent> _needPool;
    private EcsPool<DefaultMove> _movePool;
    private EcsPool<UnitGroupDataComponent> _groupPool;
    private EcsPool<UnitGroupComponent> _unitGroup;
    private EcsPool<DestroyComponent> _destPool;

    public void Init(EcsSystems systems)
    {
        _world = systems.GetWorld();
        _filter.Validate(_world);
        _config = systems.GetShared<Config>();
        _base = _world.GetPool<BaseUnitComponent>();

        _needPool = _world.GetPool<NeedInitComponent>();
        _movePool = _world.GetPool<DefaultMove>();
        _groupPool = _config.WorldGroup.GetPool<UnitGroupDataComponent>();
        _unitGroup = _world.GetPool<UnitGroupComponent>();
        _destPool = _world.GetPool<DestroyComponent>();
    }

    public void Run(EcsSystems systems)
    {
        foreach (var entity in _filter.Filter())
        {
            ref var sp = ref _filter.Inc1().Get(entity);


            ref var spBase = ref _filter.Inc2().Get(entity);
            sp.currentTime -= _config.DeltaTime;
            if (sp.currentTime < 0)
            {
                sp.currentTime += sp.time;

                int gr;
                if (sp.Group.Unpack(_config.WorldGroup, out var g))
                {
                    gr = g;
                }
                else
                {
                    gr = _config.WorldGroup.NewEntity();
                    ref var groupDataComponent = ref _groupPool.Add(gr);
                    groupDataComponent.Leader = _world.PackEntity(entity);
                    Array.Resize(ref groupDataComponent.Units, sp.units.Length);
                    for (var index = 0; index < groupDataComponent.Units.Length; index++)
                    {
                        groupDataComponent.Units[index] = _config.EmptyEcsPackEntity;
                    }

                    groupDataComponent.Count = 0;
                    groupDataComponent.Distance = 20;
                    sp.Group = _config.WorldGroup.PackEntity(gr);
                }


                if (sp.isNeedUpdate)
                {
                    if (!sp.isActive)
                    {
                        ref var group = ref _groupPool.Get(gr);
                        for (var i = 0; i < group.Units.Length; i++)
                        {
                            if (group.Units[i].Unpack(_config.WorldDefault, out var e))
                            {
                                _destPool.Add(e);
                            }
                        }
                    }

                    sp.isNeedUpdate = false;
                }

                if (sp.isActive)
                {
                    ref var group = ref _groupPool.Get(gr);

                    if (group.Count < sp.units.Length)
                    {
                        for (var i = 0; i < group.Units.Length; i++)
                        {
                            if (group.Units[i].Equals(_config.EmptyEcsPackEntity))
                            {
                                var prefab = Object.Instantiate(sp.units[i]);
                                var e = EcsUtility.Instantiate(prefab, systems);
                                ref var baseUnitComponent = ref _base.Get(e);
                                baseUnitComponent.cur.transform.position =
                                    spBase.Pos + new Vector3(Random.value * 2.0f, 0f, Random.value * 2.0f);

                                baseUnitComponent.teamId = _needPool.Get(e).teamId = spBase.teamId;

                                group.Units[i] = _world.PackEntity(e);
                                group.Count++;

                                _unitGroup.Add(e).Group = sp.Group;

                                Object.Instantiate(sp.fxSummon, baseUnitComponent.cur);

                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}