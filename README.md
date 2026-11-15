# 好友代码显示修复 by神缘

Among Us / **Nebula on the Ship (NOS)** 的大厅好友代码显示修复插件。

## 问题

NOS 的 `FriendListView.SetUpLobbyPlayerBar` 会把 `LobbyPlayerBar.SenderName`（原版显示好友代码的那行）覆盖成「好友/屏蔽/新手」状态文本。普通玩家没有状态，返回空字符串，于是大厅条目上好友代码消失。

## 修复

在 Nebula 处理之后把真实好友代码写回条目：

- `LobbyPlayerBar.SetUp`、`FriendListView.RefreshLobbyPlayers`、`FriendListView.SetLobbyStatus` 的 Postfix 里补数据
- `NetworkedPlayerInfo.Deserialize` 的 Postfix 在玩家信息到达后主动刷新（解决首次打开面板没有码的问题）
- 加好友 / 屏蔽 / 举报按钮隐藏，只保留好友代码显示

## 安装

把 `FriendCodeRestore.zip` 放进游戏目录的 `Addons/` 文件夹（不需要解压），启动游戏即可。

也可以手动解压到 `Addons/FriendCodeRestore/`，结构：

```
Addons/
└── FriendCodeRestore/
    ├── addon.meta
    └── Scripts/
        └── FriendCodeRestore.cs
```

## 依赖

- Among Us 19.0（2026.9.29）
- Nebula on the Ship 3.6.x
- 服务端需下发真实好友代码

## 已知限制

原版「选择举报原因」弹窗在私服环境下无法通过外部调用显示（`OpenReportScreen` 空引用、`ReportReasonScreen.Show` 调用成功但不渲染），所以本插件当前不提供举报入口，只做好友代码显示。

## 许可

MIT
