using System;
using System.IO;

namespace TwitterBoy
{
    static class Paths
    {
        public static string LocalDirectory { get; private set; }
        public static string Credentials { get; private set; }
        public static string Settings { get; private set; }
        public static string Status { get; private set; }
        public static string LastComment { get; private set; }
        public static string OutputDirectory { get; private set; }

        public static void Initialize()
        {
            LocalDirectory = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
            Credentials = LocalDirectory + "/credentials";
            Settings = LocalDirectory + "/settings";
            LastComment = LocalDirectory + "/lastComment";
            OutputDirectory = LocalDirectory + "/output/";

            Directory.CreateDirectory(OutputDirectory);
            if(!File.Exists(LastComment)) File.WriteAllText(LastComment, "1347589296593788933");
            
            Console.WriteLine("[Paths::Initialized]");
            Console.WriteLine("├ LocalDirectory: " + LocalDirectory);
            Console.WriteLine("├ Credentials: " + Credentials);
            Console.WriteLine("├ Settings: " + Settings);
            Console.WriteLine("├ Last comment: " + LastComment);
            Console.WriteLine("└ OutputDirectory: " + OutputDirectory);
            Console.WriteLine();
        }
    }
}