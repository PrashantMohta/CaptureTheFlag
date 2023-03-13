using HkmpPouch;

namespace CaptureTheFlag.Events
{
    internal class GetTeamMarkers : PipeEvent
    {
        public static string Name = "GetTeamMarkers";

        public override string GetName() => Name;

        public override string ToString() => $"";
    }

    internal class GetTeamMarkersFactory : IEventFactory
    {
        public static GetTeamMarkersFactory Instance = new GetTeamMarkersFactory();
        public string GetName() => GetTeamMarkers.Name;
        public PipeEvent FromSerializedString(string serializedData)
        {
            return new GetTeamMarkers {};
        }

    }
}
