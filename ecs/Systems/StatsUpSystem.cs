using System;
using System.Collections.Generic;
using System.Linq;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using Leopotam.EcsLite.Unity.Ugui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ecs.Systems
{
    internal class StatsUpSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilterExt<EcsUguiClickEvent> _filter;
        private EcsFilterExt<HpComponent, MainPlayerComponent, CameraPlayerComponent, BaseUnitComponent> _filterA;
        private Config _config;
        private EcsPool<UnitActionsComponent> _unitActionsPool;
        private EcsPool<SpawnSlaveComponent> _spawnSlave;
        private EcsPool<DestroyComponent> _destroyPool;
        private EcsPool<UnitGroupDataComponent> _unitGroupDataPool;
        private EcsPool<NeedInitComponent> _needPool;
        private EcsPool<BaseUnitComponent> _baseUnit;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldEvents);
            _filterA.Validate(_config.WorldDefault);
            _unitActionsPool = _config.WorldDefault.GetPool<UnitActionsComponent>();
            _spawnSlave = _config.WorldDefault.GetPool<SpawnSlaveComponent>();
            _destroyPool = _config.WorldDefault.GetPool<DestroyComponent>();
            _unitGroupDataPool = _config.WorldGroup.GetPool<UnitGroupDataComponent>();
            _needPool = _config.WorldDefault.GetPool<NeedInitComponent>();
            _baseUnit = _config.WorldDefault.GetPool<BaseUnitComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                ref var item = ref _filter.Inc1().Get(e);
                Debug.Log(item.WidgetName);

                if (item.WidgetName == "atk")
                {
                    if (_config.Coin >= 10)
                    {
                        _config.Coin -= 10;
                        _config.DamageFactor++;
                        Up();
                    }
                }

                if (item.WidgetName == "armor")
                {
                    if (_config.Coin >= 10)
                    {
                        _config.Coin -= 10;
                        _config.ArmorFactor++;
                        Up();
                    }
                }

                if (item.WidgetName == "addcoin")
                {
                    _config.Coin += 1000;
                }

                if (item.WidgetName == "hp")
                {
                    if (_config.Coin >= 10)
                    {
                        _config.Coin -= 10;
                        foreach (var i in _filterA.Filter())
                        {
                            ref var hp = ref _filterA.Inc1().Get(i);
                            hp.maxValue += 10;
                            if (hp.value > 0) hp.value += 10;
                            hp.isNeedUpdate = true;
                        }
                        Up();
                    }
                }

                if (item.WidgetName == "spell")
                {
                    if (_config.Coin >= 20 && _config.Spells.Convert.Length > _config.Spell)
                    {
                        _config.Coin -= 20;
                        foreach (var i in _filterA.Filter())
                        {
                            _config.Spells.Convert[_config.Spell].Convert(i, _config.WorldDefault);

                            object[] lst = null;
                            _config.WorldDefault.GetComponents(i, ref lst);
                            var l = new List<UnitAction>();
                            foreach (var it in lst)
                                if (it is IUnitAction a)
                                {
                                    a.UnitAction.startTime = -1000;
                                    l.Add(a.UnitAction);
                                }

                            _unitActionsPool.Get(i).Actions = l.ToArray();
                        }


                        _config.Spell++;
                        Up();
                    }
                }

                if (item.WidgetName == "bow")
                {
                    var maxUnit = 10;
                    if (_config.Coin >= 20 &&
                        _config.SummonsCount + 1 < maxUnit * _config.GameConfig.unitsSummon.Length)
                    {
                        _config.Coin -= 20;
                        _config.SummonsCount++;
                        Up();

                        foreach (var i in _filterA.Filter())
                        {
                            ref var group = ref _spawnSlave.Get(i);
                            var units = group.units.ToList();
                            if (units.Count < maxUnit)
                            {
                                units.Add(_config.GameConfig.unitsSummon.First());
                                _spawnSlave.Get(i).units = units.ToArray();
                            }
                            else
                            {
                                _spawnSlave.Get(i).units[_config.SummonsCount % _spawnSlave.Get(i).units.Length] =
                                    _config.GameConfig.unitsSummon[(_config.SummonsCount / units.Count)];

                                if (group.Group.Unpack(_config.WorldGroup, out var g))
                                {
                                    ref var gr = ref _unitGroupDataPool.Get(g);
                                    if (gr.Units[_config.SummonsCount % units.Count]
                                        .Unpack(_config.WorldDefault, out var t))
                                    {
                                        _destroyPool.Add(t);
                                    }
                                }
                            }
                        }
                    }
                }

                if (item.WidgetName == "mainSummon")
                {
                    var maxSummon = 3;
                    if (_config.Coin >= 20 && _config.MainSummonsCount < 3)
                    {
                        _config.Coin -= 20;
                        Up();
                        _config.MainSummonsCount++;

                        var obj = GameObject.Instantiate(_config.GameConfig.mainSummon);

                        var ms = EcsUtility.Instantiate(obj, systems);
                        ref var baseUnitComponent = ref _baseUnit.Get(ms);
                        baseUnitComponent.cur.transform.position = new Vector3();
                        _needPool.Get(ms).teamId = Config.TeamId;
                    }
                }
            }
        }

        private void Up()
        {
            foreach (var i in _filterA.Filter())
            {
                var cur = _filterA.Inc4().Get(i).cur;
                var e = Object.Instantiate(_config.GameConfig.fxLvlUp, cur );
            }
        }
    }
}