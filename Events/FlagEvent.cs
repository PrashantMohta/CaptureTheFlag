using HkmpPouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CaptureTheFlag.Utilities;

namespace CaptureTheFlag.Events
{
    internal class FlagEvent : PipeEvent
    {
        public static string Name = "FlagEvent";

        public int TeamId = 0;
        public string Action = "";
        public int PlayerId = 0;

        public override string GetName() => Name;

        public override string ToString() => $"{Action}{Constants.Separator}{i2s(TeamId)}{Constants.Separator}{i2s(PlayerId)}";

    }

    internal class FlagEventFactory : IEventFactory
    {
        public static FlagEventFactory Instance = new FlagEventFactory();

        public PipeEvent FromSerializedString(string serializedData)
        {
            var split = serializedData.Split(Constants.SplitSep);
            return new FlagEvent { Action = split[0], TeamId = s2i(split[1]), PlayerId = s2i(split[2]) };
        }

        public string GetName() => FlagEvent.Name;
    }
}
