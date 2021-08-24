using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class MainPlayerMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<MainPlayerComponent, WaitCommandComponent> _filter;
        private EcsFilterExt<MainPlayerComponent, MoveCommandComponent> _filterMove;
        private Config _config;
        private EcsPool<StartMoveCommandComponent> _startMovePool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(systems.GetWorld());
            _filterMove.Validate(systems.GetWorld());
            _startMovePool = systems.GetWorld().GetPool<StartMoveCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                ref var item = ref _filter.Inc1().Get(e);
                if (item.IsActiveMove)
                {
                    item.IsActiveMove = false;
                    _filter.Inc2().Del(e);
                    ref var move = ref _startMovePool.Add(e);
                    move.Pos = item.Pos;
                    move.Time = _config.Time + 1;
                }
            }

            foreach (var e in _filterMove.Filter())
            {
                ref var item = ref _filterMove.Inc1().Get(e);
                if (item.IsActiveMove)
                {
                    item.IsActiveMove = false;
                    _filterMove.Inc2().Del(e);
                    ref var move = ref _startMovePool.Add(e);
                    move.Pos = item.Pos;
                    move.Time = _config.Time + 1;
                }
            }
        }
    }
}