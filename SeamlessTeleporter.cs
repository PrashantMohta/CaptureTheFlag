using GlobalEnums;
using MonoMod.Utils;
using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaptureTheFlag
{
    /// <summary>
    /// SeamlessTeleporter built using https://github.com/jngo102/HollowKnight.FastTravel as a refernce
    /// </summary>
    public static class SeamlessTeleporter
    {
        private static Vector3 position = Vector3.zero;
        private static string sceneName;
        private static readonly FastReflectionDelegate SetState =
            typeof(HeroController)
            .GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetFastDelegate();
        public static event EventHandler<EventArgs> OnTeleportCompleted;
        static SeamlessTeleporter()
        {
            On.GameManager.EnterHero += OnHeroEnter;
        }


        private static void OnHeroEnter(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            orig(self, additiveGateSearch);
            if ((self.entryGateName == "CTFSpawn") && position != Vector3.zero)
            {
                HeroController.instance.SetHazardRespawn(new Vector3(position.x, position.y, HeroController.instance.transform.position.z), false);
                GameManager.instance.HazardRespawn();
                position = Vector3.zero;
            }
        }
        public static void Teleport(string sceneName, Vector2 position)
        {
            SeamlessTeleporter.position = position;
            SeamlessTeleporter.sceneName = sceneName;

            CoroutineHelper.GetRunner().StartCoroutine(DoTransition(SeamlessTeleporter.sceneName));
        }
        private static IEnumerator DoTransition(string sceneName)
        {
            
            GameManager.SceneTransitionBegan += Began;
            // Get off bench before doing scene transition or else
            // map shortcut becomes disabled until benching again
            var bench = UnityEngine.Object.FindObjectOfType<RestBench>();
            var benchCtrl = bench?.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.ActiveStateName == "Resting");
            benchCtrl?.SendEvent("GET UP");
            if (benchCtrl != null)
            {
                yield return new WaitUntil(() => benchCtrl?.ActiveStateName == "Idle");
            }

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = sceneName,
                EntryGateName = "CTFSpawn",
                EntryDelay = 0,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                PreventCameraFadeOut = false,
                WaitForSceneTransitionCameraFade = true,
                AlwaysUnloadUnusedAssets = false,
            });
        }


        private static void Began(SceneLoad load)
        {
            load.Finish += () => CoroutineHelper.GetRunner().StartCoroutine(RemoveBlankers());
        }

        private static IEnumerator RemoveBlankers()
        {
            yield return new WaitUntil(() => GameManager.instance.gameState == GameState.PLAYING);

            GameManager.instance.FadeSceneIn();

            PlayMakerFSM.BroadcastEvent("BOX DOWN");
            PlayMakerFSM.BroadcastEvent("BOX DOWN DREAM");

            var hc = HeroController.instance;

            yield return new WaitUntil(() => hc.transitionState == HeroTransitionState.EXITING_SCENE);

            // Force being able to input to avoid having to wait for the decade long walk-in anim
            hc.AcceptInput();

            SetState(hc, ActorStates.idle);

            GameManager.SceneTransitionBegan -= Began;
            OnTeleportCompleted?.Invoke(null, new EventArgs());
        }
    }
}
