﻿using GameSrv.Actor;
using SystemModule.Consts;

namespace GameSrv.Monster.Monsters {
    public class ScultureMonster : MonsterObject {
        public ScultureMonster() : base() {
            SearchTime = M2Share.RandomNumber.Random(1500) + 1500;
            ViewRange = 7;
            StoneMode = true;
            CharStatusEx = PoisonState.STONEMODE;
        }

        private void MeltStone() {
            CharStatusEx = 0;
            CharStatus = GetCharStatus();
            SendRefMsg(Messages.RM_DIGUP, Dir, CurrX, CurrY, 0, "");
            StoneMode = false;
        }

        private void MeltStoneAll() {
            MeltStone();
            IList<BaseObject> list10 = new List<BaseObject>();
            GetMapBaseObjects(Envir, CurrX, CurrY, 7, list10);
            for (int i = 0; i < list10.Count; i++) {
                BaseObject baseObject = list10[i];
                if (baseObject.StoneMode) {
                    if (baseObject is ScultureMonster) {
                        ((ScultureMonster)baseObject).MeltStone();
                    }
                }
            }
        }

        public override void Run() {
            if (CanMove() && (HUtil32.GetTickCount() - WalkTick) >= WalkSpeed) {
                if (StoneMode) {
                    for (int i = 0; i < VisibleActors.Count; i++) {
                        BaseObject baseObject = VisibleActors[i].BaseObject;
                        if (baseObject.Death) {
                            continue;
                        }
                        if (IsProperTarget(baseObject)) {
                            if (!baseObject.HideMode || CoolEye) {
                                if (Math.Abs(CurrX - baseObject.CurrX) <= 2 && Math.Abs(CurrY - baseObject.CurrY) <= 2) {
                                    MeltStoneAll();
                                    break;
                                }
                            }
                        }
                    }
                }
                else {
                    if ((HUtil32.GetTickCount() - SearchEnemyTick) > 8000 || (HUtil32.GetTickCount() - SearchEnemyTick) > 1000 && TargetCret == null) {
                        SearchEnemyTick = HUtil32.GetTickCount();
                        SearchTarget();
                    }
                }
            }
            base.Run();
        }
    }
}

