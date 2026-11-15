using System;
using System.Reflection;
using HarmonyLib;
using Virial.Attributes;
using Virial.Runtime;

namespace FriendCodeRestoreAddon;

[NebulaPreprocess(PreprocessPhase.PostLoadAddons)]
public static class Loader
{
    public static void Preprocess(NebulaPreprocessor preprocessor)
    {
        try
        {
            var harmony = new Harmony("FriendCodeRestore");

            var lobbySetUp = AccessTools.Method(typeof(LobbyPlayerBar), "SetUp");
            if (lobbySetUp != null)
            {
                harmony.Patch(lobbySetUp, postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.LobbySetUpPost)));
            }

            var type = FindType("Nebula.Online.FriendListView");
            if (type != null)
            {
                var refresh = type.GetMethod("RefreshLobbyPlayers", BindingFlags.Public | BindingFlags.Static);
                if (refresh != null)
                {
                    harmony.Patch(refresh, postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.RefreshPost)));
                }

                var status = type.GetMethod("SetLobbyStatus", BindingFlags.NonPublic | BindingFlags.Static);
                if (status != null)
                {
                    harmony.Patch(status, postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.StatusPost)));
                }
            }

            var deserialize = AccessTools.Method(typeof(NetworkedPlayerInfo), "Deserialize");
            if (deserialize != null)
            {
                harmony.Patch(deserialize, postfix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.PlayerInfoPost)));
            }
        }
        catch
        {
        }
    }

    private static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(fullName);
                if (t != null)
                {
                    return t;
                }
            }
            catch
            {
            }
        }

        return null;
    }
}

public static class Hooks
{
    public static void LobbySetUpPost(LobbyPlayerBar __instance, string playerInGameName)
    {
        Restore(__instance, playerInGameName);
    }

    public static void StatusPost(object[] __args)
    {
        try
        {
            if (__args == null || __args.Length == 0)
            {
                return;
            }

            var bar = __args[0] as LobbyPlayerBar;
            Restore(bar, null);
        }
        catch
        {
        }
    }

    public static void RefreshPost()
    {
        RestoreAll();
    }

    public static void PlayerInfoPost(NetworkedPlayerInfo __instance)
    {
        RestoreAll();
    }

    private static void RestoreAll()
    {
        try
        {
            var bars = UnityEngine.Object.FindObjectsOfType<LobbyPlayerBar>();
            if (bars == null)
            {
                return;
            }

            foreach (var bar in bars)
            {
                if (bar != null && bar.playerName != null)
                {
                    Restore(bar, null);
                }
            }
        }
        catch
        {
        }
    }

    private static void Restore(LobbyPlayerBar bar, string name)
    {
        try
        {
            if (bar == null)
            {
                return;
            }

            var playerName = string.IsNullOrEmpty(name) ? bar.playerName : name;

            NetworkedPlayerInfo data = FindData(playerName);
            if (data == null && !string.IsNullOrEmpty(bar.puid))
            {
                data = FindDataByUid(bar.puid);
            }

            if (data == null)
            {
                return;
            }

            var sender = bar.SenderName;
            if (sender != null && string.IsNullOrEmpty(sender.text)
                && !string.IsNullOrEmpty(data.FriendCode) && data.FriendCode.Contains("#"))
            {
                sender.text = data.FriendCode;
            }

            if (string.IsNullOrEmpty(bar.puid) && !string.IsNullOrEmpty(data.Puid))
            {
                bar.puid = data.Puid;
            }

            // 只保留好友码：其他按钮全部隐藏
            SetActive(bar.AddFriendButton, false);
            SetActive(bar.BlockButton, false);
            SetActive(bar.ReportButton, false);

            try
            {
                bar.GetAndSetPlatform();
            }
            catch
            {
            }
        }
        catch
        {
        }
    }

    private static NetworkedPlayerInfo FindData(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            return null;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null)
            {
                continue;
            }

            var d = pc.Data;
            if (d != null && d.PlayerName == playerName)
            {
                return d;
            }
        }

        return null;
    }

    private static NetworkedPlayerInfo FindDataByUid(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            return null;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null)
            {
                continue;
            }

            var d = pc.Data;
            if (d != null && d.Puid == uid)
            {
                return d;
            }
        }

        return null;
    }

    private static void SetActive(UnityEngine.Component component, bool active)
    {
        try
        {
            if (component != null && component.gameObject != null)
            {
                component.gameObject.SetActive(active);
            }
        }
        catch
        {
        }
    }
}
