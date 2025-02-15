﻿using GameSrv.Event.Events;
using GameSrv.Npc;
using NLog;
using SystemModule.Data;

namespace GameSrv.Maps {
    public class MapManager {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, Envirnoment> _mapList = new Dictionary<string, Envirnoment>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 地图上门列表
        /// </summary>
        private readonly IList<Envirnoment> _mapDoorList = new List<Envirnoment>();
        /// <summary>
        /// 矿物地图列表
        /// </summary>
        private readonly IList<Envirnoment> _mapMineList = new List<Envirnoment>();

        public IList<Envirnoment> Maps => _mapList.Values.ToList();

        /// <summary>
        /// 地图安全区
        /// </summary>
        public void MakeSafePkZone() {
            for (int i = 0; i < M2Share.StartPointList.Count; i++) {
                StartPoint startPoint = M2Share.StartPointList[i];
                if (string.IsNullOrEmpty(startPoint.MapName) && startPoint.Type > 0) {
                    Envirnoment envir = FindMap(startPoint.MapName);
                    if (envir != null) {
                        int nMinX = startPoint.CurrX - startPoint.Range;
                        int nMaxX = startPoint.CurrX + startPoint.Range;
                        int nMinY = startPoint.CurrY - startPoint.Range;
                        int nMaxY = startPoint.CurrY + startPoint.Range;
                        for (int nX = nMinX; nX <= nMaxX; nX++) {
                            for (int nY = nMinY; nY <= nMaxY; nY++) {
                                if (nX < nMaxX && nY == nMinY || nY < nMaxY && nX == nMinX || nX == nMaxX || nY == nMaxY) {
                                    SafeEvent safeEvent = new SafeEvent(envir, nX, nY, startPoint.Type);
                                    M2Share.EventMgr.AddEvent(safeEvent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public IList<Envirnoment> GetMineMaps() {
            return _mapMineList;
        }

        public IList<Envirnoment> GetDoorMapList() {
            return _mapDoorList;
        }

        public void AddMapInfo(string sMapName, string sMapDesc, byte nServerNumber, MapInfoFlag mapFlag, Merchant questNpc) {
            string sMapFileName = string.Empty;
            string sTempName = sMapName;
            if (sTempName.IndexOf('|') > -1) {
                sMapFileName = HUtil32.GetValidStr3(sTempName, ref sMapName, '|');
            }
            else {
                sTempName = HUtil32.ArrestStringEx(sTempName, "<", ">", ref sMapFileName);
                if (string.IsNullOrEmpty(sMapFileName)) {
                    sMapFileName = sMapName;
                }
                else {
                    sMapName = sTempName;
                }
            }
            Envirnoment envirnoment = new Envirnoment {
                MapName = sMapName,
                MapFileName = sMapFileName,
                MapDesc = sMapDesc,
                ServerIndex = nServerNumber,
                Flag = mapFlag,
                QuestNpc = questNpc
            };
            if (M2Share.MiniMapList.TryGetValue(envirnoment.MapName, out var minMap)) {
                envirnoment.MinMap = minMap;
            }
            if (envirnoment.LoadMapData(Path.Combine(M2Share.BasePath, M2Share.Config.MapDir, sMapFileName + ".map"))) {
                if (!_mapList.ContainsKey(sMapName)) {
                    _mapList.Add(sMapName, envirnoment);
                }
                else {
                    _logger.Error("地图名称重复 [" + sMapName + "]，请确认配置文件是否正确.");
                }
                if (envirnoment.DoorList.Count > 0) {
                    _mapDoorList.Add(envirnoment);
                }
                if (envirnoment.Flag.Mine || envirnoment.Flag.boMINE2) {
                    _mapMineList.Add(envirnoment);
                }
            }
            else {
                _logger.Error("地图文件:" + sMapName + ".map" + "未找到,或者加载出错!!!");
            }
        }

        public bool AddMapRoute(string sSMapNo, int nSMapX, int nSMapY, string sDMapNo, int nDMapX, int nDMapY) {
            bool result = false;
            Envirnoment sEnvir = FindMap(sSMapNo);
            Envirnoment dEnvir = FindMap(sDMapNo);
            if (sEnvir != null && dEnvir != null) {
                MapRouteItem gateObj = new MapRouteItem {
                    RouteId = M2Share.ActorMgr.GetNextIdentity(),
                    Flag = false,
                    Envir = dEnvir,
                    X = (short)nDMapX,
                    Y = (short)nDMapY
                };
                sEnvir.AddToMap(nSMapX, nSMapY, CellType.MapRoute, gateObj.RouteId, gateObj);
                result = true;
            }
            return result;
        }

        public Envirnoment FindMap(string sMapName) {
            return _mapList.TryGetValue(sMapName, out Envirnoment map) ? map : null;
        }

        public Envirnoment GetMapInfo(int nServerIdx, string sMapName) {
            Envirnoment result = null;
            if (_mapList.TryGetValue(sMapName, out Envirnoment envirnoment)) {
                if (envirnoment.ServerIndex == nServerIdx) {
                    result = envirnoment;
                }
            }
            return result;
        }

        /// <summary>
        /// 取地图编号服务器
        /// </summary>
        /// <param name="sMapName"></param>
        /// <returns></returns>
        public int GetMapOfServerIndex(string sMapName) {
            if (_mapList.TryGetValue(sMapName, out Envirnoment envirnoment)) {
                return envirnoment.ServerIndex;
            }
            return 0;
        }

        public void LoadMapDoor() {
            for (int i = 0; i < Maps.Count; i++) {
                this.Maps[i].AddDoorToMap();
            }
            _logger.Info("地图环境加载成功...");
        }

        public static void ProcessMapDoor() {

        }

        public static void ReSetMinMap() {
            // for (var I = 0; I < this.Count; I ++ )
            // {
            //     var Envirnoment = ((this.Items[I]) as TEnvirnoment);
            //     for (var II = 0; II < M2Share.MiniMapList.Count; II ++ )
            //     {
            //         if ((M2Share.MiniMapList[II]).CompareTo((Envirnoment.sMapName)) == 0)
            //         {
            //             Envirnoment.nMinMap = ((int)M2Share.MiniMapList.Values[II]);
            //             break;
            //         }
            //     }
            // }
        }

        public static void Run() {

        }
    }
}