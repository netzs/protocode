using System;
using System.Collections.Generic;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;
using Voody.UniLeo.Lite;
using Object = UnityEngine.Object;

namespace ecs
{
    internal static class EcsUtility
    {
        public static int Instantiate(GameObject gameObject, EcsSystems systems) // COPY WorldInitSystem.cs unileo pack
        {
            var convertComponent = gameObject.GetComponent<ConvertToEntity>();
            var worldName = convertComponent.GetWorldName();
            var nameValue = worldName == "" ? null : worldName;
            var spawnWorld = systems.GetWorld(nameValue);
            var entity = spawnWorld.NewEntity();

            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (component is IConvertToEntity entityComponent)
                {
                    // Adding Component to entity
                    entityComponent.Convert(entity, spawnWorld);
                    Object.Destroy(component);
                }
            }

            switch (convertComponent.GetConvertMode())
            {
                case ConvertMode.ConvertAndDestroy:
                    Object.Destroy(gameObject);
                    break;
                case ConvertMode.ConvertAndInject:
                    Object.Destroy(convertComponent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return entity;
        }

        public static List<int> FindAreaTarget(EcsSystems systems, BaseUnitComponent unit, Vector3 targetPos,
            float radius, TypeTarget tp)
        {
            EcsFilterExt<BaseUnitComponent> filter;
            filter.Validate(systems.GetWorld());
            var ids = new List<int>();
            var config = systems.GetShared<Config>();

            foreach (var i in filter.Filter())
            {
                ref var otherUnit = ref filter.Inc1().Get(i);
                var len = (unit.Pos - otherUnit.Pos).sqrMagnitude;
                if (len < radius * radius)
                {
                    if (CheckTargetType(unit, tp, otherUnit, i, config))
                    {
                        ids.Add(i);
                    }
                }
            }

            return ids;
        }

        /*
        public static (int entity, float mx) FindTarget(EcsSystems systems, BaseUnitComponent unit)
        {
            EcsFilterExt<BaseUnitComponent>.Exc<DeadComponent>
                filter; // в этом фильтре должно быть куча проверок на возможность аттаковать и все такое
            filter.Validate(systems.GetWorld());
            var entity = -1;
            var mx = -1f;

            foreach (var i in filter.Filter())
            {
                ref var un = ref filter.Inc1().Get(i);
                if (un.teamId != unit.teamId)
                {
                    var len = (un.pos - unit.pos).sqrMagnitude;
                    if (mx < 0 || len < mx)
                    {
                        entity = i;
                        mx = len;
                    }
                }
            }

            return (entity, mx);
        }
        */

        public static int FindSingleTarget(EcsSystems systems, BaseUnitComponent unit, IEnumerable<UnitAction> list,
            bool isMove)
        {
            EcsFilterExt<BaseUnitComponent> filter;
            filter.Validate(systems.GetWorld());
            var entity = -1;
            var mx = -1f;
            var priority = -1;

            var config = systems.GetShared<Config>();

            foreach (var ua in list)
            {
                if (ua.TimeActionRestored(config.Time))
                {
                    foreach (var i in filter.Filter())
                    {
                        ref var otherUnit = ref filter.Inc1().Get(i);
                        if (CheckTargetType(unit, ua.typeTarget, otherUnit, i, config))
                        {
                            var len = (unit.Pos - otherUnit.Pos).sqrMagnitude;
                            if (len < (isMove
                                    ? ua.moveRange * ua.moveRange
                                    : ua.rangeUse * ua.rangeUse) &&
                                (mx < 0 || ua.movePriority > priority || len < mx))
                            {
                                entity = i;
                                mx = len;
                                priority = ua.movePriority;
                            }
                        }
                    }
                }
            }

            return entity;
        }

        private static bool CheckTargetType(BaseUnitComponent selfUnit, TypeTarget unitActionTypeTarget,
            BaseUnitComponent otherUnit, int otherEntity, Config config)
        {
            switch (unitActionTypeTarget)
            {
                case TypeTarget.Enemy:
                    return selfUnit.teamId != otherUnit.teamId && !config.FilterConfig.DeadPool.Has(otherEntity) &&
                           config.FilterConfig.UnitPool.Has(otherEntity);
                case TypeTarget.DeadAlly:
                    return selfUnit.teamId == otherUnit.teamId && config.FilterConfig.DeadPool.Has(otherEntity) &&
                           config.FilterConfig.UnitPool.Has(otherEntity);
                case TypeTarget.NotFullHp:
                    return !config.FilterConfig.DeadPool.Has(otherEntity) &&
                           config.FilterConfig.HpPool.Has(otherEntity) &&
                           config.FilterConfig.HpPool.Get(otherEntity).value <
                           config.FilterConfig.HpPool.Get(otherEntity).maxValue;
                case TypeTarget.Stone:
                    return Config.StoneId == otherUnit.teamId && config.FilterConfig.StonePool.Has(otherEntity);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitActionTypeTarget), unitActionTypeTarget, null);
            }
        }


        public static float Sqr(float a)
        {
            return a * a;
        }

        public static void ResetCommand(this EcsWorld world, int entity)
        {
            world.GetPool<WaitCommandComponent>().Del(entity);
            world.GetPool<MoveCommandComponent>().Del(entity);
            var ua = world.GetPool<UnitActionCommandComponent>();
            if (ua.Has(entity))
            {
                world.GetPool<ResetActionCommandComponent>().Add(entity).Action = ua.Get(entity).Action;
                ua.Del(entity);
            }

            world.GetPool<StartMoveCommandComponent>().Del(entity);

            var pool = world.GetPool<MoveConfigComponent>();
            if (pool.Has(entity))
            {
                pool.Get(entity).agent.ResetPath();
            }
        }
    }
}