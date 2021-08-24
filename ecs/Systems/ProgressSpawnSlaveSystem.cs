using ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace ecs.Systems
{
    internal class ProgressSpawnSlaveSystem : ProgressUnitActionSystem<SpawnSlaveComponent>, IProgressUnitActionNonTarget, IProgressUnitActionInit
    {
        private EcsPool<UnitGroupDataComponent> _groupPool;
        private EcsPool<NeedInitComponent> _needPool;
        private EcsPool<UnitGroupComponent> _unitGroup;
        private Config _config;

        public void Initialize(EcsSystems systems)
        {
            _groupPool = Config.WorldGroup.GetPool<UnitGroupDataComponent>();
            _needPool = Config.WorldDefault.GetPool<NeedInitComponent>();
            _unitGroup = Config.WorldDefault.GetPool<UnitGroupComponent>();
            _config = systems.GetShared<Config>();
        }

        public void NonTargetAction(EcsSystems ecsSystems, int entity)
        {
            ref var spawnSlaveComponent = ref Filter.Inc2().Get(entity);
            ref var unit = ref Filter.Inc1().Get(entity);

            if (spawnSlaveComponent.Group.Unpack(Config.WorldGroup, out var g))
            {
                ref var group = ref _groupPool.Get(g);
                if (group.Count < spawnSlaveComponent.units.Length)
                {
                    for (var i = 0; i < group.Units.Length; i++)
                    {
                        if (group.Units[i].Equals(_config.EmptyEcsPackEntity))
                        {
                            var prefab = Object.Instantiate(spawnSlaveComponent.units[i]);
                            var e = EcsUtility.Instantiate(prefab, ecsSystems);
                            ref var baseUnitComponent = ref Filter.Inc1().Get(e);
                            baseUnitComponent.cur.transform.position =
                                unit.Pos + new Vector3(Random.value * 0.4f, 0, Random.value * 0.4f);

                            baseUnitComponent.teamId = _needPool.Get(e).teamId = unit.teamId;

                            group.Units[i] = Config.WorldDefault.PackEntity(e);
                            group.Count++;

                            _unitGroup.Add(e).Group = spawnSlaveComponent.Group;

                            Object.Instantiate(spawnSlaveComponent.fxSummon, baseUnitComponent.cur);

                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log("excess spawn");
                }
            }
        }
    }
}