
using System;
using System.Collections.Generic;
using ecs;
using ecs.Components;
using LeoEcsPhysics;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedFilters;
using Leopotam.EcsLite.ExtendedSystems;
using Leopotam.EcsLite.Unity.Ugui;
using UnityEngine;
using UnityEngine.AI;
using Voody.UniLeo.Lite;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ecs.Systems
{
    internal sealed class EcsStartup : MonoBehaviour
    {
        [SerializeField] public Variant slon;
        [SerializeField] public string slon2;
        [SerializeField] public V2 slon3;
        private EcsSystems _systems;
        [SerializeField] public Vector3 aaa;
        [SerializeField] public Dictionary<string, string> a;
        

        [SerializeField] public EcsUguiEmitter uguiEmitter;
        [SerializeField] public GameConfig config;
        [SerializeField] public GameObject bulletPool;
        [SerializeField] public GameObject cameraCnt;
        [SerializeField] public Collider terrain;
        [SerializeField] public SpellsData spells;

        private readonly Config _config = new Config();

        private void Start()
        {
            _config.GameConfig = config;

            _config.Camera = cameraCnt;
            _config.Spells = spells;


            _config.WorldDefault = new EcsWorld();
            _config.WorldBullet = new EcsWorld();
            _config.WorldEffect = new EcsWorld();
            _config.WorldEvents = new EcsWorld();
            _config.WorldUI = new EcsWorld();
            _config.WorldGroup = new EcsWorld();
            _config.WorldPhysics = new EcsWorld();

            NavMesh.pathfindingIterationsPerFrame = 2000;
            Debug.Log(NavMesh.pathfindingIterationsPerFrame);

            _systems = new EcsSystems(_config.WorldDefault, _config);
            _systems.AddWorld(_config.WorldBullet, "bullet");
            _systems.AddWorld(_config.WorldEffect, "effect");
            _systems.AddWorld(_config.WorldEvents, Config.EventsWorldName);
            _systems.AddWorld(_config.WorldGroup, "group");
            _systems.AddWorld(_config.WorldPhysics, "physics");
            _systems.AddWorld(_config.WorldUI, "ui");

            EcsPhysicsEvents.ecsWorld = _config.WorldPhysics;

            _config.EmptyEcsPackEntity = new EcsPackedEntity();

            _systems
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem("bullet"))
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem("effect"))
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem("group"))
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem("events"))
#endif
                .ConvertScene()
                // .Add(new InitBattleSystem())
                .Add(new SpawnSystem())
                .Add(new HpSystem())
                .Add(new NeedInitSystem())
                .Add(new CameraSystem())
                // .Add(new InputSystem())
                .Add(new InputPlayerSystem(terrain))
                .Add(new MainPlayerMoveSystem())
                .Add(new SlaveDistanceSystem())
                .Add(new ProgressSpawnSlaveSystem())
                .Add(new StartSpawnSlaveSystem())

                //unitAction
                .Add(new StartUnitActionSystem<ResurrectSpellComponent>())
                .Add(new StartSpecial2HandAttackSystem())
                .Add(new StartUnitActionSystem<SimpleAttackStoneComponent>())
                .Add(new StartUnitActionSystem<MeleeSplashAttackComponent>())
                .Add(new StartUnitActionSystem<SimpleAttackComponent>())
                .Add(new StartUnitActionSystem<SimpleRangeAttackComponent>())
                .Add(new StartUnitActionSystem<HealerActionComponent>())
                .Add(new ActionMoveSystem())
                .Add(new ProgressResurrectSpellSystem())
                .Add(new ProgressSimpleAttackSystem())
                .Add(new ProgressSpecial2HandAttackSystem())
                .Add(new ProgressMeleeSplashAttackSystem())
                .Add(new ProgressSimpleRangeAttackSystem())
                .Add(new ProgressHealerActionSystem())
                //end unitAcion
                .Add(new BulletProgressSystem())
                .Add(new ImpulseSystem())
                .Add(new DamageSystem())
                .Add(new HpsSystem())

                //reset
                .Add(new ResetSpecial2HandAttackSystem())
                .DelHere<ResetActionCommandComponent>()
                .Add(new DefaultMoveSystem())
                .Add(new StartMoveSystem())
                .Add(new MoveProgressSystem())

                // register additional worlds here, for example:
                .Add(new ReWaitSystem())
                .Add(new PersonAnimatorSystem())
                .Add(new DeadSystem())
                .Add(new DeadReturnGroupSystem())
                .Add(new DeadDropSystem())
                .Add(new SelfResurrectSystem())
                .Add(new ResurrectSystem())
                .Add(new DestroySystem())
                // .Add(new UISystem())
                .Add(new PhysicsSystem())
                .Add(new SpawnColliderSystem())

                // UI
                .Add(new CoinPanelSystem())
                .Add(new StatsUpSystem())
                .DelHerePhysics("physics")
                .Inject()
                .InjectUgui(uguiEmitter, Config.EventsWorldName);

            _config.FilterConfig.Init(_systems.GetWorld());
            _config.bulletPool = bulletPool;

            _systems.Init();
        }

        private void FixedUpdate()
        {
            _config.DeltaTime = Time.fixedDeltaTime;
            _config.Time += Time.fixedDeltaTime;
            _systems?.Run();
        }

        private void OnDestroy()
        {
            if (_systems != null)
            {
                _systems.Destroy();
                // add here cleanup for custom worlds, for example:

                _config.WorldDefault.Destroy();
                _config.WorldBullet.Destroy();
                _config.WorldEffect.Destroy();
                _config.WorldEvents.Destroy();
                _config.WorldGroup.Destroy();
                _config.WorldPhysics.Destroy();

                EcsPhysicsEvents.ecsWorld = null;

                _systems = null;
            }
        }
    }

    internal class HpsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<HpsComponent, HpComponent>.Exc<DeadComponent> _filter;
        private Config _config;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                ref var hps = ref _filter.Inc1().Get(e);
                if (hps.time < _config.Time)
                {
                    hps.time += hps.delay;
                    ref var hp = ref _filter.Inc2().Get(e);
                    if (hp.value < hp.maxValue)
                    {
                        hp.value = Math.Min(hp.value + hps.count, hp.maxValue);
                        hp.isNeedUpdate = true;
                    }
                }
            }
        }
    }

    internal class SelfResurrectSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<SelfResurrectComponent, BaseUnitComponent> _filter;
        private EcsFilterExt<SelfResurrectComponent, BaseUnitComponent, DeadComponent> _filter2;
        private Config _config;
        private EcsPool<ResurrectComponent> _resPool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
            _filter2.Validate(_config.WorldDefault);
            _resPool = _config.WorldDefault.GetPool<ResurrectComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                ref var res = ref _filter.Inc1().Get(e);
                if (res.StartPos == Vector3.zero)
                {
                    res.StartPos = _filter.Inc2().Get(e).Pos;
                }
            }

            foreach (var e in _filter2.Filter())
            {
                ref var sr = ref _filter2.Inc1().Get(e);
                if (!sr.IsStart)
                {
                    sr.IsStart = true;
                    sr.Timer = _config.Time + 3f;
                }
                else
                {
                    if (sr.Timer < _config.Time)
                    {
                        sr.IsStart = false;
                        _resPool.Add(e);
                        _filter2.Inc2().Get(e).cur.transform.position = _filter.Inc1().Get(e).StartPos;
                    }
                }
            }
        }
    }

    internal class SpawnColliderSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<OnCollisionEnterEvent> _filterEnter;
        private EcsFilterExt<OnCollisionExitEvent> _filterExit;
        private EcsFilterExt<OnTriggerEnterEvent> _filterEnterT;
        private EcsFilterExt<OnTriggerExitEvent> _filterExitT;
        private EcsFilterExt<SpawnComponent> _filterSpawner;
        private Config _config;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filterEnter.Validate(_config.WorldPhysics);
            _filterExit.Validate(_config.WorldPhysics);
            _filterEnterT.Validate(_config.WorldPhysics);
            _filterExitT.Validate(_config.WorldPhysics);
            _filterSpawner.Validate(_config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var ec in _filterEnter.Filter())
            {
                Debug.Log("enter");
                var go = _filterEnter.Inc1().Get(ec).senderGameObject;
                foreach (var e in _filterSpawner.Filter())
                {
                    ref var spawn = ref _filterSpawner.Inc1().Get(e);
                    if (spawn.collider == go)
                    {
                        spawn.isActive = true;
                        spawn.isNeedUpdate = true;
                    }
                }
            }

            foreach (var ec in _filterExit.Filter())
            {
                Debug.Log("exit");
                var go = _filterExit.Inc1().Get(ec).senderGameObject;
                foreach (var e in _filterSpawner.Filter())
                {
                    ref var spawn = ref _filterSpawner.Inc1().Get(e);
                    if (spawn.collider == go)
                    {
                        spawn.isActive = false;
                        spawn.isNeedUpdate = true;
                    }
                }
            }

            foreach (var ec in _filterEnterT.Filter())
            {
                Debug.Log("enter");
                var go = _filterEnterT.Inc1().Get(ec).senderGameObject;
                foreach (var e in _filterSpawner.Filter())
                {
                    ref var spawn = ref _filterSpawner.Inc1().Get(e);
                    if (spawn.collider == go)
                    {
                        spawn.isActive = true;
                        spawn.isNeedUpdate = true;
                    }
                }
            }

            foreach (var ec in _filterExitT.Filter())
            {
                Debug.Log("exit");
                var go = _filterExitT.Inc1().Get(ec).senderGameObject;
                foreach (var e in _filterSpawner.Filter())
                {
                    ref var spawn = ref _filterSpawner.Inc1().Get(e);
                    if (spawn.collider == go)
                    {
                        spawn.isActive = false;
                        spawn.isNeedUpdate = true;
                    }
                }
            }
        }
    }

    internal class HpSystem : IEcsSystem, IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<HpComponent> _filter;
        private Config _config;
        private Camera _camera;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
            _camera = Camera.main;
        }

        public void Run(EcsSystems systems)
        {
            foreach (var item in _filter.Filter())
            {
                ref var i = ref _filter.Inc1().Get(item);
                if (i.text != null)
                {
                    if (i.isNeedUpdate)
                    {
                        if (i.value == 0)
                        {
                            i.text.text = "";
                        }
                        else
                        {
                            i.text.text = $"{i.value}/{i.maxValue}";
                        }

                        i.isNeedUpdate = false;
                    }

                    i.text.transform.LookAt(_camera.transform);
                }
            }
        }
    }


    public interface EscSIR : IEcsInitSystem, IEcsRunSystem
    {
    }


    internal class PhysicsSystem : EscSIR
    {
        private EcsFilterExt<OnTriggerEnterEvent> _filter;
        private EcsFilterExt<OnCollisionEnterEvent> _filterC;
        private Config _config;

        [EcsFilter(Config.EventsWorldName, typeof(BaseUnitComponent), typeof(HpComponent))]
        readonly EcsFilter _filter2 = default;

        private EcsFilterExt<MainPlayerComponent, BaseUnitComponent> main;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldPhysics);
            _filterC.Validate(_config.WorldPhysics);
            main.Validate(_config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var item in _filter2)
            {
                Debug.Log(item);
            }

            foreach (var entity in _filter.Filter())
            {
                var trigger = _filter.Inc1().Get(entity).senderGameObject.GetComponent<TriggerEvents>();
                if (trigger != null)
                {
                    trigger.Run();
                    Debug.Log("trigger:" + trigger.num);

                    if (trigger.num == 1)
                    {
                        // var pool = _config.WorldDefault.GetPool<Special2HandAttackComponent>();
                        // var flt = _config.WorldDefault.Filter<Special2HandAttackComponent>();
                        // foreach (var f in flt.End())
                        // {
                        //     pool.Get(f).UnitAction.isLock = false;
                        // }
                    }

                    if (trigger.num == 2)
                    {
                        // var pool = _config.WorldDefault.GetPool<SpawnSlaveComponent>();
                        // var flt = _config.WorldDefault.Filter<SpawnSlaveComponent>();
                        // foreach (var f in flt.End())
                        // {
                        //     pool.Get(f).UnitAction.isLock = false;
                        // }
                    }
                }

                // _filter.Inc1().Del(entity);
            }

            // foreach (var entity in _filterC.Filter())
            // {
            //     _filter.Inc1().Del(entity);
            // }
        }
    }

    internal class CameraSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<CameraPlayerComponent, BaseUnitComponent> _filter;
        private Config _config;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                var pos = _filter.Inc2().Get(entity).Pos;
                if ((_config.Camera.transform.position - pos).sqrMagnitude > 5f)
                {
                    _config.Camera.transform.position += 0.05f * (pos - _config.Camera.transform.position);
                }
            }
        }
    }
}

internal class UISystem : IEcsInitSystem, IEcsRunSystem
{
    public EcsFilterExt<EcsUguiScrollViewEvent> _filter;
    private Config _config;
    private Camera _camera;

    public void Init(EcsSystems systems)
    {
        _config = systems.GetShared<Config>();
        _filter.Validate(_config.WorldEvents);

        _camera = Camera.main;
    }

    public void Run(EcsSystems systems)
    {
        foreach (var entity in _filter.Filter())
        {
            ref var item = ref _filter.Inc1().Get(entity);
            // _camera.transform.position = new Vector3((item.Value.x - 0.5f) * 150, _camera.transform.position.y, ( item.Value.y - 0.5f) * 150);
            _filter.Inc1().Del(entity);
        }
    }
}


internal class InputSystem : IEcsRunSystem, IEcsInitSystem
{
    private EcsFilterExt<BaseUnitComponent> _filter;
    private Config _config;
    private EcsWorld _world;
    private Camera _camera;
    private Vector3 _oldPoint;
    private float _timer;
    private Plane _plane;

    public void Init(EcsSystems systems)
    {
        _world = systems.GetWorld();
        _filter.Validate(_world);
        _config = systems.GetShared<Config>();
        _camera = Camera.main;
        _plane = new Plane(Vector3.up, 0);
    }

    public void Run(EcsSystems systems)
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (var entity in _filter.Filter())
            {
                ref var unit = ref _filter.Inc1().Get(entity);
                var startPoint = new Vector3(0f, 0, 0);
                var endPoint = new Vector3(100f, 0, 100f);
                var initVec = (endPoint - startPoint).normalized;
                var perp = Quaternion.Euler(0, 90, 0) * initVec;

                var unVec = unit.Pos - startPoint;

                var move = Vector3.Project(unVec, perp).normalized;

                // RaycastHit()

                // _filter.Inc1().Get(entity).pos += move;
            }
        }

        var deltaTime = 0.1f;

        if (Input.GetMouseButton(0))
        {
            _timer -= _config.DeltaTime;
            if (_timer < 0)
            {
                _timer += deltaTime;
                var mousePos = Input.mousePosition;
                mousePos.z = _camera.transform.position.y;
                var worldPosition = _camera.ScreenToWorldPoint(mousePos);
                if (_oldPoint == Vector3.zero)
                {
                    _oldPoint = worldPosition;
                    _oldPoint.x += Random.Range(-1f, 1f);
                    _oldPoint.z += Random.Range(-1f, 1f);
                }
                else
                {
                    var ray = _camera.ScreenPointToRay(Input.mousePosition);

                    if (_plane.Raycast(ray, out var distance))
                    {
                        worldPosition = ray.GetPoint(distance);
                        var obj = Object.Instantiate(_config.GameConfig.unitSwin);
                        obj.transform.position = worldPosition;
                    }
                }

                _oldPoint = worldPosition;
            }
        }
        else
        {
            _oldPoint = Vector3.zero;
            _timer = 0;
        }
    }

    private void Move(Vector3 startPoint, Vector3 endPoint, EcsSystems systems)
    {
        startPoint.y = 0f;
        endPoint.y = 0f;
        var initVec = (endPoint - startPoint).normalized;
        var perp = Quaternion.Euler(0, 90, 0) * initVec;
        foreach (var entity in _filter.Filter())
        {
            ref var unit = ref _filter.Inc1().Get(entity);
            if ((unit.Pos - startPoint).sqrMagnitude < 10f ||
                (unit.Pos - endPoint).sqrMagnitude < 10f)
            {
                var unVec = unit.Pos - startPoint;
                var move = Vector3.Project(unVec, perp).normalized;

                // unit.pos += move;
            }
        }
    }
}