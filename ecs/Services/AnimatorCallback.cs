using ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Serialization;

namespace ecs
{
    public class AnimatorCallback : MonoBehaviour
    {
        private int _id;
        private EcsSystems _systems;
        private Config _config;
        private EcsWorld _world;
        private EcsPool<AnimHitComponent> _hitPool;
         public bool isInit = false;

        public void Init(int id, EcsSystems systems)
        {
            isInit = true;
            _id = id;
            _systems = systems;
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _hitPool = _world.GetPool<AnimHitComponent>();
        }

        public void FootR()
        {
        }

        public void FootL()
        {
        }

        public void Hit()
        {
            _hitPool.Add(_id);
        }

        public void Shoot()
        {
            _hitPool.Add(_id);
        }
    }
}