using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal class CoinPanelSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilterExt<CoinPanelComponent> _filter;
        private Config _config;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldUI);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                _filter.Inc1().Get(e).text.text = _config.Coin.ToString();
            }
        }
    }
}