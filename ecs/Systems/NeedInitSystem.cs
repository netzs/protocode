using System;
using System.Collections.Generic;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    public class NeedInitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<BaseUnitComponent, NeedInitComponent> _filter;
        private EcsWorld _world;
        private EcsPool<WaitCommandComponent> _poolWait;
        private EcsPool<UnitActionsComponent> _unitActionsPool;
        private EcsPool<TeamComponent> _poolTeam;
        private Config _config;
        private EcsPool<MoveConfigComponent> _poolMove;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _poolWait = _world.GetPool<WaitCommandComponent>();
            _unitActionsPool = _world.GetPool<UnitActionsComponent>();
            _poolTeam = _world.GetPool<TeamComponent>();
            _poolMove = _world.GetPool<MoveConfigComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                var teamId = _filter.Inc2().Get(entity).teamId;
                _filter.Inc2().Del(entity);

                ref var baseUnitComponent = ref _filter.Inc1().Get(entity);
                baseUnitComponent.teamId = teamId;
                // _poolWait.Add(entity).Time = Random.Range(0f, 5f);
                _poolWait.Add(entity);

                teamId = Math.Min(teamId, _config.GameConfig.teams.Length - 1);

                if (_poolTeam.Has(entity) && _poolTeam.Get(entity).mesh != null &&
                    0 <= teamId && teamId < _config.GameConfig.teams.Length)
                {
                    _poolTeam.Get(entity).mesh.material = _config.GameConfig.teams[teamId];
                }

                if (_poolTeam.Has(entity) && _poolTeam.Get(entity).particle != null &&
                    0 <= teamId && teamId < _config.GameConfig.teams.Length)
                {
                    _poolTeam.Get(entity).particle.material = _config.GameConfig.teams[teamId];
                }

                if (_poolMove.Has(entity))
                {
                    _poolMove.Get(entity).agent.enabled = true;
                }


                object[] lst = null;
                _world.GetComponents(entity, ref lst);
                var l = new List<UnitAction>();
                foreach (var item in lst)
                    if (item is IUnitAction a)
                    {
                        a.UnitAction.startTime = -1000;
                        l.Add(a.UnitAction);
                    }

                _unitActionsPool.Add(entity).Actions = l.ToArray();
            }
        }
    }
}