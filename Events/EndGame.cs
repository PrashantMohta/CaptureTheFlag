using HkmpPouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureTheFlag.Events
{
    internal class EndGame : PipeEvent
    {
        public static string Name = "EndGame";
        public override string GetName() => Name;

        public override string ToString() => $"";
        
    }

    internal class EndGameFactory : IEventFactory
    {
        public static EndGameFactory Instance = new EndGameFactory();

        public PipeEvent FromSerializedString(string serializedData)
        {
            return new EndGame { };
        }

        public string GetName() => EndGame.Name;
    }
}
