using ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace ecs.Systems
{
    internal class ProgressSimpleRangeAttackSystem :
        ProgressUnitActionSystem<SimpleRangeAttackComponent>, IProgressUnitActionInit, IProgressUnitActionTarget
    {
        private EcsPool<BulletSettingComponent> _bulletSettingPool;
        private EcsWorld _world;

        public void Initialize(EcsSystems systems)
        {
            _bulletSettingPool = Config.WorldBullet.GetPool<BulletSettingComponent>();
            _world = systems.GetWorld();
        }

        public void TargetAction(EcsSystems ecsSystems, int entity)
        {
            ref var ua = ref Filter.Inc2().Get(entity);
            var bullet = Config.GameConfig.CreateBullet(ua.bulletSetting.bulletPrefab, Config.bulletPool);
            bullet.gameObject.SetActive(true);
            // bullet.transform.SetPositionAndRotation(ua.bulletPoint.position, ua.bulletPoint.rotation);
            bullet.transform.position = ua.bulletPoint.position;
            if (ua.UnitAction.target.Unpack(_world, out var targetEntity))
            {
                bullet.transform.rotation =
                    Quaternion.LookRotation(
                        Filter.Inc1().Get(targetEntity).Pos - bullet.transform.position);

                var bid = Config.WorldBullet.NewEntity();
                ua.bulletSetting.SetEntityData(bid, Config.WorldBullet);

                ref var bulletSetting = ref _bulletSettingPool.Get(bid);
                bulletSetting.transform = bullet.transform;
                bulletSetting.target = ua.UnitAction.target;
                bulletSetting.attacker = _world.PackEntity(entity);

                bulletSetting.teamAttacker = Filter.Inc1().Get(entity).teamId;
                bulletSetting.pos = Filter.Inc1().Get(targetEntity).Pos;
            }
        }
    }
}