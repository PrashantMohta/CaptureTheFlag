using CaptureTheFlag.Events;
using Hkmp.Math;
using System;
using System.Collections.Generic;
using System.Timers;
using Hkmp.Game;

namespace CaptureTheFlag
{
    public class ServerGameInfo
    {
        //todo fill these out
        public string SceneName = "Ruins1_03";
        public Dictionary<int, Vector2> Position = new()
        {
            { (int)Team.Moss, new Vector2(128.5f, 8.4f) },
            { (int)Team.Hive, new Vector2(80.5f, 58.4f) },
            { (int)Team.Grimm, new Vector2(23.2f, 8.4f) },
            { (int)Team.Lifeblood, new Vector2(128.5f, 40.4f) }
        };

        public Dictionary<int, int> Score = new()
        {
            { (int)Team.Moss, 0 },
            { (int)Team.Hive, 0 },
            { (int)Team.Grimm, 0 },
            { (int)Team.Lifeblood, 0 }
        };

        public Dictionary<int, int> PlayerFlags = new();

        internal bool IsOngoing;
        private Timer GameTimer;

        public event EventHandler<EventArgs> OnEndgame;

        public ServerGameInfo()
        {
        }

        internal TeamMarkers GetTeamMarkers()
        {
            return new TeamMarkers { MarkerSceneName = SceneName , Position = Position };
        }

        internal void StartGame()
        {
            IsOngoing = true;
            // start a timer to end game
            GameTimer = new Timer(Constants.DefaultGameTime);
            GameTimer.Elapsed += GameTimer_Elapsed;
            GameTimer.Enabled = true;

        }
        public void GetScore()
        {
            CaptureTheFlag.pipe.Logger.Info($"Moss:{Score[1]},Hive:{Score[2]},Grimm:{Score[3]},Lifeblood:{Score[4]}");
        }
        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EndGame();
        }

        internal void EndGame()
        {
            IsOngoing = false;
            OnEndgame?.Invoke(this, EventArgs.Empty);
        }
    }
}