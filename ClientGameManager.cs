using CaptureTheFlag.Events;
using CaptureTheFlag.Flags;
using Hkmp.Math;
using Modding;
using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = Hkmp.Math.Vector2;
using UObject = UnityEngine.Object;
using Hkmp.Game;

namespace CaptureTheFlag
{
    internal static class ClientGameManager
    {
        public static bool RoomLocked = false;
        internal static bool HasFlag = false;

        public static bool GameJoined { get; internal set; }
        public static string SceneName { get; internal set; }
        public static Dictionary<int, Vector2> TeamPositions = new();
        public static Dictionary<int, FlagBehaviour> Flags = new();
        public static bool JustDied = false;

        public static int PickedFlag = 0;

        internal static void StartGame()
        {
            GameJoined = true;
            var playerTeam = (int)CaptureTheFlag.pipe.ClientApi.ClientManager.Team;
            CaptureTheFlag.pipe.ClientApi.UiManager.DisableTeamSelection();
            SeamlessTeleporter.Teleport(SceneName, (UnityEngine.Vector2)TeamPositions[playerTeam]);
            SeamlessTeleporter.OnTeleportCompleted += SeamlessTeleporter_OnTeleportCompleted;
            SpawnFlags();
            On.HeroController.Die += HeroController_Die;
            ModHooks.GetPlayerStringHook += ModHooks_GetPlayerStringHook;
            CaptureTheFlag.pipe.ClientApi.ClientManager.DisconnectEvent += ClientManager_DisconnectEvent;
            On.GameManager.FinishedEnteringScene += GameManager_FinishedEnteringScene;
            On.TransitionPoint.OnTriggerEnter2D += TransitionPoint_OnTriggerEnter2D;
        }

        private static void TransitionPoint_OnTriggerEnter2D(On.TransitionPoint.orig_OnTriggerEnter2D orig, TransitionPoint self, Collider2D movingObj)
        {
            var ts = self.targetScene;
            if (RoomLocked)
            {
                self.SetTargetScene("");
            }
            orig(self, movingObj);
            self.SetTargetScene(ts);
        }

        private static void GameManager_FinishedEnteringScene(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig(self);
            if (JustDied)
            {
                CoroutineHelper.GetRunner().StartCoroutine(TeleportToGameScene());
                JustDied = false;
            }
        }

        public static IEnumerator TeleportToGameScene()
        {
            yield return new WaitForSeconds(1.5f);
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (GameJoined && scene.name != SceneName)
            {
                var playerTeam = (int)CaptureTheFlag.pipe.ClientApi.ClientManager.Team;
                SeamlessTeleporter.Teleport(SceneName, (UnityEngine.Vector2)TeamPositions[playerTeam]);
            }
        }

        private static string ModHooks_GetPlayerStringHook(string name, string res)
        {
            /*if (name == nameof(PlayerData.respawnScene))
            {
                return SceneName;
            }*/
            return res;
        }

        private static void ClientManager_DisconnectEvent()
        {
            DropFlag();
            EndGame();
        }

        public static IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
        {
            DropFlag();
            yield return orig(self);
            JustDied = true;

        }
        private static void DespawnFlags()
        {
            foreach (var kvp in TeamPositions)
            {
                if (Flags.TryGetValue(kvp.Key, out var old))
                {
                    if (old != null && old.gameObject != null)
                    {
                        GameObject.DestroyImmediate(old.gameObject);
                    }
                }
            }
        }
        private static void SpawnFlags()
        {
            foreach(var kvp in TeamPositions)
            {
                if (Flags.TryGetValue(kvp.Key,out var old)) {
                    if(old != null && old.gameObject != null) { 
                        GameObject.DestroyImmediate(old.gameObject);
                    }
                }
                var go = new GameObject($"Flag {kvp.Key}");
                go.transform.position = (UnityEngine.Vector2)kvp.Value;
                GameObject.DontDestroyOnLoad(go);
                var flag = go.GetAddComponent<FlagBehaviour>();
                flag.TeamId = kvp.Key;
                flag.OnFlagCaptured += Flag_OnFlagCaptured;
                flag.OnFlagDeposited += Flag_OnFlagDeposited;
                Flags[kvp.Key] = flag;
            }
        }

        internal static void OnDrop(FlagEvent e)
        {
            Flags[e.TeamId].IsCaptured = false;
            HideFlagIcon(e.PlayerId, e.TeamId);
        }

        internal static void OnDeposit(FlagEvent e)
        {
            var TeamId = (int)CaptureTheFlag.pipe.ClientApi.ClientManager.Team;
            Flags[e.TeamId].IsCaptured = false;
            HideFlagIcon(e.PlayerId, e.TeamId);
        }

        internal static void OnCapture(FlagEvent e)
        {
            Flags[e.TeamId].IsCaptured = true;
            ShowFlagIcon(e.PlayerId,e.TeamId);
        }
        private static GameObject GetFlag(Team TeamId)
        {
            var flag = new GameObject($"flagIcon-{TeamId}");
            flag.transform.position = new UnityEngine.Vector3(0.1f, 2.1257f, 0f);
            var sr = flag.GetAddComponent<SpriteRenderer>();
            sr.sprite = AssemblyUtils.GetSpriteFromResources($"{(TeamId).ToString().ToLower()}.png");
            flag.SetActive(false);
            return flag;
        }
        private static GameObject FlagIconForPlayer(int playerId, int TeamId)
        {
            var player = CaptureTheFlag.pipe.ClientApi.ClientManager.GetPlayer((ushort)playerId);
            var team = TeamId;
            GameObject parent = HeroController.instance.gameObject;
            if (player != null && player.PlayerObject != null)
            {
                parent = player.PlayerObject;
            }
            if(parent == null)
            {
                CaptureTheFlag.pipe.Logger.Info("Why is HeroController.instance.gameObject object null?");
                return null;
            }
            var flagIcon = parent.FindGameObjectInChildren($"flagIcon-{team}");
            if (flagIcon == null)
            {
                flagIcon = GetFlag((Team)team);
                flagIcon.transform.SetParent(parent.transform, false);
            }
            flagIcon.SetScale(0.2f, 0.2f);
            return flagIcon;
        }
        private static void ShowFlagIcon(int playerId, int TeamId)
        {
            FlagIconForPlayer(playerId, TeamId).SetActive(true);
        }
        private static void HideFlagIcon(int playerId, int TeamId)
        {
            FlagIconForPlayer(playerId, TeamId).SetActive(false);
        }

        private static void Flag_OnFlagDeposited(object sender, FlagEventArgs e)
        {
            HasFlag = false;
            CaptureTheFlag.pipe.SendToServer(new FlagEvent { Action = Constants.Deposit });
        }

        private static void Flag_OnFlagCaptured(object sender, FlagEventArgs e)
        {
            HasFlag = true;
            PickedFlag = e.TeamId;
            CaptureTheFlag.pipe.SendToServer(new FlagEvent { Action = Constants.Capture, TeamId = e.TeamId });
        }

        private static void DropFlag()
        {
            HasFlag = false;
            CaptureTheFlag.pipe.SendToServer(new FlagEvent { Action = Constants.Drop });
        }

        private static void SeamlessTeleporter_OnTeleportCompleted(object sender, EventArgs e)
        {
            if (GameJoined && !RoomLocked)
            {
                LockRoom();
            }
        }

        internal static void EndGame()
        {
            GameJoined = false;
            On.HeroController.Die -= HeroController_Die;
            SeamlessTeleporter.OnTeleportCompleted -= SeamlessTeleporter_OnTeleportCompleted;
            DespawnFlags();
            UnlockRoom();
            CaptureTheFlag.pipe.ClientApi.UiManager.EnableTeamSelection();
        }

        internal static void LockRoom()
        {
            RoomLocked = true;
            var transitions = UObject.FindObjectsOfType<TransitionPoint>();
            foreach(var transition in transitions)
            {
                transition.gameObject.GetComponent<Collider2D>().isTrigger = false;
            }
        }

        private static void UnlockRoom()
        {
            RoomLocked = false;
            var transitions = UObject.FindObjectsOfType<TransitionPoint>();
            foreach (var transition in transitions)
            {
                transition.gameObject.GetComponent<Collider2D>().isTrigger = true;
            }
        }
    }
}
