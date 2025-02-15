﻿using GameSrv.Player;
using SystemModule.Enums;

namespace GameSrv.GameCommand.Commands {
    [Command("ShowDenyIPaddrLogon", "", 10)]
    public class ShowDenyIPaddrLogonCommand : GameCommand {
        [ExecuteCommand]
        public void Execute(PlayObject playObject) {
            int nCount;
            try {
                nCount = M2Share.DenyIPAddrList.Count;
                if (M2Share.DenyIPAddrList.Count <= 0) {
                    playObject.SysMsg("禁止登录角色列表为空。", MsgColor.Green, MsgType.Hint);
                }
                if (nCount > 0) {
                    for (int i = 0; i < M2Share.DenyIPAddrList.Count; i++) {
                        //PlayObject.SysMsg(Settings.g_DenyIPAddrList[i], TMsgColor.c_Green, TMsgType.t_Hint);
                    }
                }
            }
            finally {
            }
        }
    }
}