using HkmpPouch;

namespace CaptureTheFlag.Events
{
    internal class StartGame : PipeEvent
    {
        public static string Name = "StartGame";
        public override string GetName() => Name;

        public override string ToString() => $"";
        
    }

    internal class StartGameFactory : IEventFactory
    {
        public static StartGameFactory Instance = new StartGameFactory();

        public PipeEvent FromSerializedString(string serializedData)
        {
            return new StartGame { };
        }

        public string GetName() => StartGame.Name;
    }
}
