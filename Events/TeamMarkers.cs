using Hkmp.Game;
using Hkmp.Math;
using HkmpPouch;
using System.Collections.Generic;
using static CaptureTheFlag.Utilities;
namespace CaptureTheFlag.Events
{
    internal class TeamMarkers : PipeEvent
    {

        public string MarkerSceneName;
        public Dictionary<int, Vector2> Position = new()
        {
            { (int)Team.Moss, new Vector2(0f, 0f) },
            { (int)Team.Hive, new Vector2(0f, 0f) },
            { (int)Team.Grimm, new Vector2(0f, 0f) },
            { (int)Team.Lifeblood, new Vector2(0f, 0f) }
        };

        public string GetMarkerPositionString()
        {
            return $"{v2s(Position[(int)Team.Moss])}{Constants.Separator}{v2s(Position[(int)Team.Hive])}{Constants.Separator}{v2s(Position[(int)Team.Grimm])}{Constants.Separator}{v2s(Position[(int)Team.Lifeblood])}";
        }

        public static string Name = "TeamMarkers";
        public override string GetName() => Name;
        public override string ToString() => $"{MarkerSceneName}{Constants.Separator}{GetMarkerPositionString()}";
    }
    internal class TeamMarkersFactory : IEventFactory
    {
        public static TeamMarkersFactory Instance = new TeamMarkersFactory();
        public PipeEvent FromSerializedString(string serializedData)
        {
            var split = serializedData.Split(Constants.SplitSep);
            return new TeamMarkers { MarkerSceneName = split[0], Position = GetMarkerPostiion(split) };
        }

        private Dictionary<int, Vector2> GetMarkerPostiion(string[] split)
        {
            var dict = new Dictionary<int,Vector2>();
            var team = 1;
            for(var i = 1 ; i < split.Length; i+=2)
            {
                dict[team] = new Vector2(s2f(split[i]), s2f(split[i + 1]));
                team++;
            }
            return dict;
        }

        public string GetName() => TeamMarkers.Name;
    }
}
