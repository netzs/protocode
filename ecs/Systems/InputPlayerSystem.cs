using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ecs.Systems
{
    internal class InputPlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly Collider _terrain;
        private EcsFilterExt<MainPlayerComponent, BaseUnitComponent, CameraPlayerComponent> _filter;
        private EcsFilterExt<MainPlayerComponent, BaseUnitComponent> _filter2;
        private Camera _camera;
        private Plane _plane;
        private Config _config;
        private bool _isInit;
        private Vector3 startPos;

        public InputPlayerSystem(Collider terrain)
        {
            _terrain = terrain;
        }

        public void Init(EcsSystems systems)
        {
            _camera = Camera.main;
            _plane = new Plane(Vector3.up, 0);
            _config = systems.GetShared<Config>();
            _filter.Validate(systems.GetWorld());
            _filter2.Validate(systems.GetWorld());
        }

        public void Run(EcsSystems systems)
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!_isInit)
                {
                    startPos = Input.mousePosition;
                    _isInit = true;
                }
                else
                {
                    var diff = Input.mousePosition - startPos;
                    diff.z = diff.y;
                    diff.y = 0;
                    diff.Normalize();


                    var pos = Vector3.zero;
                    foreach (var e in _filter.Filter())
                    {
                        ref var bs = ref _filter.Inc2().Get(e);
                        pos = bs.Pos + diff * 2;
                    }
                    foreach (var e in _filter2.Filter())
                    {
                        ref var item = ref _filter.Inc1().Get(e);
                        item.IsActiveMove = true;
                        item.Pos = pos;
                    }
                }
            }
            else
            {
                _isInit = false;
            }


            return;
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                const float distance = 100f;
                if (_terrain.Raycast(ray, out var info, distance))
                {
                    foreach (var e in _filter.Filter())
                    {
                        ref var item = ref _filter.Inc1().Get(e);
                        item.IsActiveMove = true;
                        item.Pos = info.point;
                        if (item.Tick < 60) item.Tick++;
                    }
                }
            }
            else
            {
                foreach (var e in _filter.Filter())
                {
                    ref var item = ref _filter.Inc1().Get(e);
                    if (item.Tick > 0) item.Tick--;
                }
            }
        }
    }
}