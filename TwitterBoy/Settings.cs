using System;

namespace TwitterBoy
{
    static class Settings
    {
        public static double AvatarRefreshDelay;
        public static double MentionsPullDelay;
        public static long Status;
        public static bool SaveFrames;

        public static void Initialize()
        {
            var settings = IO.ReadAllLines(Paths.Settings);
            AvatarRefreshDelay = long.Parse(settings[0]);
            MentionsPullDelay  = long.Parse(settings[1]);
            Status             = long.Parse(settings[2]);
            SaveFrames         = settings[3] == "True";

            Console.WriteLine("[Settings::Initialized]");
            Console.WriteLine("├ Avatar refresh delay: " + AvatarRefreshDelay);
            Console.WriteLine("├ Mentions pull delay: " + MentionsPullDelay);
            Console.WriteLine("├ Status: " + Status);
            Console.WriteLine("└ Save frames: " + SaveFrames);
            Console.WriteLine();
        }
    }
}