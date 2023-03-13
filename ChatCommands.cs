using CaptureTheFlag.Events;
using Hkmp.Api.Command.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureTheFlag
{
    internal class Start : IClientCommand
    {
        public string Trigger => "/CTFStart";

        public string[] Aliases => new string[] { "/StartCTF", "/startCTF", "/ctfstart", "/ctfStart" };

        public void Execute(string[] arguments)
        {
            var playerTeam = CaptureTheFlag.pipe.ClientApi.ClientManager.Team;
            if (playerTeam == Hkmp.Game.Team.None)
            {
                CaptureTheFlag.pipe.ClientApi.UiManager.ChatBox.AddMessage("You must select a team to start the game");
            } else
            {
                CaptureTheFlag.pipe.SendToServer(new StartGame { });
                CaptureTheFlag.pipe.SendToServer(new GetTeamMarkers { });
            }
        }
    }
    internal class Join : IClientCommand
    {
        public string Trigger => "/CTFJoin";

        public string[] Aliases => new string[] { "/JoinCTF", "/joinCTF", "/ctfjoin", "/ctfJoin" };

        public void Execute(string[] arguments)
        {
            var playerTeam = CaptureTheFlag.pipe.ClientApi.ClientManager.Team;
            if (playerTeam == Hkmp.Game.Team.None)
            {
                CaptureTheFlag.pipe.ClientApi.UiManager.ChatBox.AddMessage("You must select a team to join the game");
            }
            else
            {
                CaptureTheFlag.pipe.SendToServer(new GetTeamMarkers { });
            }
        }
    }
    internal static class ChatCommands
    {
        public static Start startCommand = new Start();
        public static Join joinCommand = new Join();

        public static void RegisterCommands()
        {
            CaptureTheFlag.pipe.ClientApi.CommandManager.RegisterCommand(startCommand);
            CaptureTheFlag.pipe.ClientApi.CommandManager.RegisterCommand(joinCommand);
        }

        public static void DeRegisterCommands()
        {
            CaptureTheFlag.pipe.ClientApi.CommandManager.DeregisterCommand(startCommand);
            CaptureTheFlag.pipe.ClientApi.CommandManager.DeregisterCommand(joinCommand);
        }
    }
}
