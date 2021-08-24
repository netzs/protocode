using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine.AI;

namespace ecs.Systems
{
    internal class MoveProgressSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<MoveCommandComponent, BaseUnitComponent, MoveConfigComponent> _filter;
        private Config _config;
        private EcsWorld _world;

        private EcsPool<WaitCommandComponent> _waitPool;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _config = systems.GetShared<Config>();
            _waitPool = _world.GetPool<WaitCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var moveCommandComponent = ref _filter.Inc1().Get(entity);
                ref var unit = ref _filter.Inc2().Get(entity);

                    var agent = _filter.Inc3().Get(entity).agent;
                    agent.stoppingDistance = 0.2f;
                if (moveCommandComponent.Time < _config.Time ||
                    (moveCommandComponent.Pos - unit.Pos).sqrMagnitude < Config.SqrDeltaLen)
                {
                    _filter.Inc1().Del(entity);
                    _waitPool.Add(entity);

                    agent.ResetPath();
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                }
            }
        }
    }
}