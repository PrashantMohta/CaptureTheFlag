using CaptureTheFlag.Events;
using Hkmp.Api.Server;
using HkmpPouch;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureTheFlag
{
    public class CaptureTheFlag : Mod
    {
        public override string GetVersion()=> Constants.Version;

        internal static CaptureTheFlag Instance;
        internal static PipeClient pipe = new PipeClient(Constants.Name);
        internal static Server server;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            if (server == null)
            {
                server = new Server();
                ServerAddon.RegisterAddon(server);
            }
            pipe.OnReady += Pipe_OnReady;
            On.HeroController.Die += ClientGameManager.HeroController_Die;


        }

        private void Pipe_OnReady(object sender, EventArgs e)
        {
            if (pipe.ClientApi == null)
            {
                Log("No CLient APi");
            }
            pipe.ClientApi.ClientManager.ConnectEvent += ClientManager_ConnectEvent;
            pipe.ClientApi.ClientManager.DisconnectEvent += ClientManager_DisconnectEvent;
            pipe.On(TeamMarkersFactory.Instance).Do<TeamMarkers>(OnTeamMarkers);
            pipe.On(StartGameFactory.Instance).Do<StartGame>(OnStartGame);
            pipe.On(EndGameFactory.Instance).Do<EndGame>(OnEndGame);
            pipe.On(FlagEventFactory.Instance).Do<FlagEvent>(OnFlagEvent);

        }

        private void OnFlagEvent(FlagEvent e)
        {
            if (e.Action == Constants.Capture) {
                ClientGameManager.OnCapture(e);
            }
            if (e.Action == Constants.Deposit) {

                ClientGameManager.OnDeposit(e);
            }
            if (e.Action == Constants.Drop) {

                ClientGameManager.OnDrop(e);
            }
        }

        private void OnEndGame(EndGame obj)
        {
            ClientGameManager.EndGame();
        }

        private void OnStartGame(StartGame obj)
        {
           // do nothing for now
        }

        private void OnTeamMarkers(TeamMarkers markers)
        {
            ClientGameManager.SceneName = markers.MarkerSceneName;
            ClientGameManager.TeamPositions = markers.Position;
            if (!ClientGameManager.GameJoined)
            {
                ClientGameManager.StartGame();
            }
        }

        

        private void ClientManager_DisconnectEvent()
        {
            ChatCommands.DeRegisterCommands();
        }

        private void ClientManager_ConnectEvent()
        {

            pipe.ClientApi.UiManager.EnableTeamSelection();
            pipe.ClientApi.ClientManager.ChangeTeam(Hkmp.Game.Team.Hive);
            ChatCommands.RegisterCommands();
        }
    }
}