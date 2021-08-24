using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;
using UnityEngine.AI;

namespace ecs.Systems
{
    internal class StartMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<MoveConfigComponent, StartMoveCommandComponent> _filter;

        private Config _config;
        private EcsWorld _world;

        private EcsPool<MoveCommandComponent> _movePool;
        private EcsPool<AnimatorComponent> _animatorPool;
        private EcsPool<WaitCommandComponent> _waitPool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _movePool = _world.GetPool<MoveCommandComponent>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
            _waitPool = _world.GetPool<WaitCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var startMove = ref _filter.Inc2().Get(entity);
                var agent = _filter.Inc1().Get(entity).agent;

                if (!agent.isActiveAndEnabled)
                {
                    Debug.Log("DISABLE agent");
                }
                var path = new NavMeshPath();
                agent.CalculatePath(startMove.Pos, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                    ref var mv = ref _movePool.Add(entity);
                    mv.Pos = startMove.Pos;
                    mv.Time = startMove.Time;


                    if (_animatorPool.Has(entity))
                    {
                        ref var anim = ref _animatorPool.Get(entity);
                        anim.isNeedUpdate = true;
                        anim.runTrigger = true;
                    }
                }
                else
                {
                    // Debug.Log("bad path");
                    _waitPool.Add(entity).Time = _config.Time + _config.GameConfig.waitTime;
                }
                _filter.Inc2().Del(entity);

            }
        }
    }
}