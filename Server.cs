using CaptureTheFlag.Events;
using Hkmp.Api.Server;
using Hkmp.Game;
using Hkmp.Math;
using HkmpPouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureTheFlag
{
    internal class Server : ServerAddon
    {
        public override bool NeedsNetwork => false;

        protected override string Name => Constants.Name;

        protected override string Version => Constants.Version;

        internal IServerApi serverApi { get; set; }

        public PipeServer pipe = new PipeServer(Constants.Name);

        public ServerGameInfo gameInfo = new ServerGameInfo();


        public override void Initialize(IServerApi serverApi)
        {
            this.serverApi = serverApi;
            pipe.On(GetTeamMarkersFactory.Instance).Do<GetTeamMarkers>(OnGetTeamMarker);
            pipe.On(StartGameFactory.Instance).Do<StartGame>(OnStartGame);
            pipe.On(FlagEventFactory.Instance).Do<FlagEvent>(OnFlagEvent);
            pipe.ServerApi.ServerManager.PlayerDisconnectEvent += ServerManager_PlayerDisconnectEvent;
            pipe.OnRecieve += (s, e) => { gameInfo.GetScore(); };
        }

        private void ServerManager_PlayerDisconnectEvent(IServerPlayer player)
        {
            // get the flag that the player was carrying
            if(gameInfo.PlayerFlags.TryGetValue(player.Id, out var flagId))
            {
                if (flagId == 0) { return; }
                MakeDrop(player, flagId);
            }
        }
        private void MakeDrop(IServerPlayer player,int flagId)
        {
            if (flagId == 0) { return; }
            // tell others that the flag was dropped
            pipe.BroadcastInScene(new FlagEvent { Action = Constants.Drop, TeamId = flagId, PlayerId = player.Id }, gameInfo.SceneName);
            BroadcastMessage($"{player.Username} has Dropped Team {((Team)flagId)}'s flag");
            // remove the flag from the player
            gameInfo.PlayerFlags[player.Id] = 0;
        }
        private void OnFlagEvent(FlagEvent e)
        {
            var player = pipe.ServerApi.ServerManager.GetPlayer(e.FromPlayer);
            if(e.Action == Constants.Capture)
            {
                if (e.TeamId == 0) { return; }
                //broadcast that this team's flag is captured by whom
                pipe.BroadcastInScene(new FlagEvent { Action = Constants.Capture, TeamId = e.TeamId , PlayerId = e.FromPlayer}, gameInfo.SceneName);
                // store the team id of the flag this player is carrying
                gameInfo.PlayerFlags[e.FromPlayer] = e.TeamId;
                BroadcastMessage($"{player.Username} has picked Team {((Team)e.TeamId)}'s flag");
            }
            if (e.Action == Constants.Deposit)
            {
                // get the flag that the player was carrying
                var flagId = gameInfo.PlayerFlags[e.FromPlayer];
                if(flagId == 0) { return; }
                // increase team score
                gameInfo.Score[(int)player.Team]++;
                // tell others that the flag was captured
                pipe.BroadcastInScene(new FlagEvent { Action = Constants.Deposit, TeamId = flagId, PlayerId = e.FromPlayer }, gameInfo.SceneName);
                BroadcastMessage($"{player.Username} has captured Team {((Team)flagId)}'s flag");
                // remove the flag from the player
                gameInfo.PlayerFlags[e.FromPlayer] = 0;
            }
            if (e.Action == Constants.Drop)
            {
                // get the flag that the player was carrying
                if (gameInfo.PlayerFlags.TryGetValue(player.Id, out var flagId))
                {
                    if (flagId == 0) { return; }
                    MakeDrop(player, flagId);
                }
            }
        }

        private void BroadcastMessage(string s)
        {
            //todo currently this sends to ALL the players in all the rooms, limit this to current scene
            pipe.ServerApi.ServerManager.BroadcastMessage(s);
        }

        private void OnStartGame(StartGame obj)
        {
            if (gameInfo.IsOngoing)
            {
                pipe.ServerApi.ServerManager.SendMessage(obj.FromPlayer, $"A CTF Game is already ongoing, use {ChatCommands.joinCommand.Trigger} to join the game");
            } else
            {
                gameInfo.StartGame();
                gameInfo.OnEndgame += GameInfo_OnEndgame;
                var player = pipe.ServerApi.ServerManager.GetPlayer(obj.FromPlayer);
                pipe.ServerApi.ServerManager.BroadcastMessage($"A CTF Game has started in {gameInfo.SceneName} by {player.Username}, use {ChatCommands.joinCommand.Trigger} to join the game");
                pipe.BroadcastInScene(new StartGame { }, gameInfo.SceneName);
            }
        }

        private void GameInfo_OnEndgame(object sender, EventArgs e)
        {
            gameInfo.OnEndgame -= GameInfo_OnEndgame;
            BroadcastMessage($"A CTF game concluded in {gameInfo.SceneName}");
            pipe.BroadcastInScene(new EndGame{ }, gameInfo.SceneName);
            gameInfo = new ServerGameInfo();
        }

        private void OnGetTeamMarker(GetTeamMarkers e)
        {
            if (!gameInfo.IsOngoing)
            {
                pipe.ServerApi.ServerManager.SendMessage(e.FromPlayer, $"A CTF Game is NOT ongoing, use {ChatCommands.startCommand.Trigger} to start a game");
            }
            else 
            { 
                TeamMarkers markers = gameInfo.GetTeamMarkers();
                pipe.SendToPlayer(e.FromPlayer, markers);
            }
        }
    }
}
