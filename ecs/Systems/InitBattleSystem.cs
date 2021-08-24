using ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;
using Voody.UniLeo.Lite;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ecs.Systems
{
    internal class InitBattleSystem : IEcsInitSystem
    {
        private EcsSystems _systems;
        private Config config;
        private EcsWorld world;

        public void Init(EcsSystems systems)
        {
            _systems = systems;
            config = systems.GetShared<Config>();
            world = systems.GetWorld();
            // var pool = world.GetPool<BaseUnitComponent>();
            // var poolWait = world.GetPool<WaitCommandComponent>();
            // var poolTeam = world.GetPool<TeamComponent>();
            // var unitActionsPool = world.GetPool<UnitActionsComponent>();


            if (config.GameConfig.isSwin)
            {
                var sw = Object.Instantiate(config.GameConfig.unitSwin);
                CreateUnit(sw, systems, 0,
                    new Vector3((10 + config.GameConfig.lineCount * 2), 0, 0)
                );
                sw = Object.Instantiate(config.GameConfig.unitSwin);
                CreateUnit(sw, systems, 1,
                    new Vector3(-(10 + config.GameConfig.lineCount * 2), 0, 0)
                );
            }

            for (var j = 0; j < config.GameConfig.lineCount; j++)
            {
                for (var i = 0; i < config.GameConfig.unitCount; i++)
                {
                    {
                        var unit = Object.Instantiate(
                            config.GameConfig.units[Random.Range(0, config.GameConfig.units.Length)]);

                        var e = CreateUnit(unit, systems, 0,
                            new Vector3((10 + j * 2), 0, i - config.GameConfig.unitCount / 2)
                        );
                    }

                    {
                        var unit = Object.Instantiate(
                            config.GameConfig.units[Random.Range(0, config.GameConfig.units.Length)]);

                        var e = CreateUnit(unit, systems, 1,
                            new Vector3(-(10 + j * 2), 0, i - config.GameConfig.unitCount / 2)
                        );
                    }
                }
            }


            // var sz = 10;
            // var roomSz = 30;
            // var door = 6;
            // for (var i = 0; i < sz; i++)
            // {
            //     for (var j = 0; j < sz; j++)
            //     {
            //         for (var k = 0; k < (roomSz - door) / 2; k++)
            //         {
            //             CR(new Vector3(i * roomSz, 0f,  j * roomSz + k));
            //         }
            //
            //         for (var k = 1; k < (roomSz - door) / 2; k++)
            //         {
            //             CR(new Vector3(i * roomSz + k, 3f, j * roomSz));
            //         }
            //
            //         for (var k = 0; k < (roomSz - door) / 2; k++)
            //         {
            //             CR(new Vector3(i * roomSz, 6f, j * roomSz + (roomSz - k - 1)));
            //         }
            //
            //         for (var k = 1; k < (roomSz - door) / 2; k++)
            //         {
            //             CR(new Vector3(i * roomSz + k, 0f, j * roomSz + roomSz - 1));
            //         }
            //     }
            // }
        }

        public void CR(Vector3 pos)
        {
            var go = GameObject.Instantiate(
                config.GameConfig.sphere);
            go.transform.position =
                new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);

            // var e = CreateUnit(unit, _systems, 10,
            //     new Vector3(pos.x, 0, pos.y)
            // );
        }

        private int CreateUnit(GameObject prefab, EcsSystems systems, int teamId, Vector3 pos)
        {
            var world = systems.GetWorld();
            var pool = world.GetPool<BaseUnitComponent>();
            var needPool = world.GetPool<NeedInitComponent>();

            var e = EcsUtility.Instantiate(prefab, systems);
            ref var baseUnitComponent = ref pool.Get(e);
            baseUnitComponent.cur.transform.position = pos;

            // baseUnitComponent.teamId = teamId;

            needPool.Get(e).teamId = teamId;


            // if (poolTeam.Get(e).mesh != null)
            //     poolTeam.Get(e).mesh.material = team;

            // object[] lst = null;
            // world.GetComponents(e, ref lst);
            // var l = new List<UnitAction>();
            // foreach (var item in lst)
            //     if (item is IUnitAction a)
            //     {
            //         a.UnitAction.startTime = -1000;
            //         l.Add(a.UnitAction);
            //     }
            //
            // unitActionsPool.Add(e).Actions = l.ToArray();
            return e;
        }
    }
}