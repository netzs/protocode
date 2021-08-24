using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class BulletProgressSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<BulletSettingComponent> _filter;
        private Config _config;
        private EcsPool<DamageComponent> _damagePool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldBullet);
            _damagePool = _config.WorldEffect.GetPool<DamageComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var bullet = ref _filter.Inc1().Get(entity);
                var direction = (bullet.pos - bullet.transform.position);
                bullet.transform.position += direction.normalized * _config.DeltaTime * bullet.speed;

                if (direction.sqrMagnitude < 0.1f)
                {
                    var dmg = _config.WorldEffect.NewEntity();
                    ref var damage = ref _damagePool.Add(dmg);
                    // var ua = Filter.Inc2().Get(entity).UnitAction;
                    damage.Attacker = bullet.attacker;
                    damage.Target = bullet.target;
                    damage.Damage = bullet.damage;

                    damage.TeamAttacker = bullet.teamAttacker;

                    bullet.transform.gameObject.SetActive(false);
                    _config.GameConfig.ReleaseBullet(bullet.transform.gameObject);
                    _filter.Inc1().Del(entity);
                }
            }
        }
    }
}