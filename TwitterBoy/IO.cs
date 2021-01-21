using System.IO;

namespace TwitterBoy
{
    static class IO
    {
        public static string[] ReadAllLines(string path)
        {
            var lines = File.ReadAllLines(path);
            for(int i = 0; i < lines.Length; ++i)
                lines[i] = lines[i].Substring(0, lines[i].IndexOf(' '));
            return lines;
        }

        public static string ReadAllText(string path) => File.ReadAllText(path);
    }
}