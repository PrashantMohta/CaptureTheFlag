namespace CaptureTheFlag
{
    internal static class Constants
    {
        public const char Separator =  '|';
        public static char[] SplitSep = new char[] { Separator };

        public const string Name = "CaptureTheFlag";
        public const string Version = "0.0.1";

        public const float DefaultGameTime = 60f * 1000f; //60 seconds for now to test
        public const float DefaultFlagPick = 2f;
        public const float DefaultFlagDeposit = 2f;
        public const float DefaultFlagRange = 3f;

        public const string Deposit = "D";
        public const string Drop = "Dr";
        public const string Capture = "C";
    }
}
