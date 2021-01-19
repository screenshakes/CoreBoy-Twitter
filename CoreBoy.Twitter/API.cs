using System;

using Tweetinvi;
using Tweetinvi.Models;

namespace CoreBoy.Twitter
{
    static class API
    {
        public static TwitterClient Client;

        public static void Initialize()
        {
            var credentials = IO.ReadAllLines(Paths.Credentials);
            
            Client = new TwitterClient(new TwitterCredentials(credentials[0], credentials[1], credentials[2], credentials[3]));
            
            Console.WriteLine("[API::Initialized]");
            Console.WriteLine();
        }
    }
}