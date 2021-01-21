using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using CoreBoy;
using CoreBoy.controller;
using CoreBoy.gui;
using Button = CoreBoy.controller.Button;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using Tweetinvi.Parameters;

namespace TwitterBoy
{
    public class Program
    {
        static void Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            var arguments = GameboyOptions.Parse(args);
            var emulator = new Emulator(arguments);

            Paths.Initialize();
            Settings.Initialize();
            API.Initialize();

            if(!arguments.RomSpecified)
            {
                GameboyOptions.PrintUsage(Console.Out);
                Console.Out.Flush();
                Environment.Exit(1);
            }

            var ui = new CommandLineInteractivity();
            emulator.Controller = ui;
            emulator.Display.OnFrameProduced += ui.UpdateDisplay; 
            emulator.Run(cancellation.Token);

            MemoryEditor.Initialize(emulator.Gameboy.Mmu);

            ui.ProcessInput();

            cancellation.Cancel();
        }
    }

    public class CommandLineInteractivity : IController
    {
        IButtonListener listener;
        readonly Dictionary<ConsoleKey, Button> controls;
        readonly Dictionary<string, Button> commentsControls;
        readonly Dictionary<ConsoleKey, Action> commands;

        ConsoleKey lastConsoleKey;

        long frame;
        DateTime lastFrame, lastInput;
        bool useComments;

        public CommandLineInteractivity()
        {
            controls = new Dictionary<ConsoleKey, Button>
            {
                { ConsoleKey.LeftArrow, Button.Left },
                { ConsoleKey.RightArrow, Button.Right },
                { ConsoleKey.UpArrow, Button.Up },
                { ConsoleKey.DownArrow, Button.Down },
                { ConsoleKey.Z, Button.A },
                { ConsoleKey.X, Button.B },
                { ConsoleKey.C, Button.Start },
                { ConsoleKey.V, Button.Select }
            };

            commands = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.Q, () => { useComments = !useComments; Console.WriteLine("Input mode: {0}", useComments ? "Comments" : "Manual"); } }, // Toggle manual / comments driven
                { ConsoleKey.W, Settings.Initialize } // Reload Settings
            };

            commentsControls = new Dictionary<string, Button>
            {
                { "LEFT", Button.Left },
                { "RIGHT", Button.Right },
                { "UP", Button.Up },
                { "DOWN", Button.Down },
                { "A", Button.A },
                { "B", Button.B },
                { "START", Button.Start },
                { "SELECT", Button.Select },
            };

            lastConsoleKey = ConsoleKey.NoName;
        }

        public void SetButtonListener(IButtonListener listener) => this.listener = listener;

        public void ProcessInput()
        {
            Button lastButton = null;
            var input = ConsoleKey.NoName;

            while (input != ConsoleKey.Escape)
            {
                if(Console.KeyAvailable)
                     input = Console.ReadKey(true).Key;
                else input = ConsoleKey.NoName;

                if(commands.ContainsKey(input) && lastConsoleKey != input)
                    commands[input].Invoke();

                lastConsoleKey = input;

                Button button;

                if(useComments) 
                     CommentsInputs(out button);
                else ManualInputs(input, out button);

                if (button != null)
                {
                    if (lastButton != button)
                        listener?.OnButtonRelease(lastButton);

                    listener?.OnButtonPress(button);

                    var snapshot = button;
                    new Thread(() =>
                    {
                        Thread.Sleep(300);
                        listener?.OnButtonRelease(snapshot);
                    }).Start();

                    lastButton = button;
                }
            }
        }

        public void UpdateDisplay(object sender, byte[] framedata)
        {
            if((DateTime.UtcNow - lastFrame).TotalSeconds < Settings.AvatarRefreshDelay) return;
            
            if(!Settings.SaveFrames && File.Exists(Paths.OutputDirectory + (frame - 1) + ".png"))
                File.Delete(Paths.OutputDirectory + (frame - 1) + ".png");

            var path = Paths.OutputDirectory + frame + ".png";
            using (var fileStream = File.Create(path))
            {
                var image = Image.Load(framedata);
                image.Mutate(x => x.Resize(160 * 4, 144 * 4, KnownResamplers.NearestNeighbor));
                image.SaveAsPng(fileStream);
            }

            API.Client.AccountSettings.UpdateProfileImageAsync(File.ReadAllBytes(path));
            System.Console.WriteLine("[Avatar Updated ({0})]", frame);
            ++frame;
            lastFrame = DateTime.UtcNow;
        }

        void CommentsInputs(out Button button)
        {
            button = null;

            if((DateTime.UtcNow - lastInput).TotalSeconds < Settings.MentionsPullDelay) return;
            lastInput = DateTime.UtcNow;
            
            var buttonsCount = new Dictionary<string, int>(){
                { "LEFT", 0 },
                { "RIGHT", 0},
                { "UP", 0},
                { "DOWN", 0 },
                { "A", 0 },
                { "B", 0 },
                { "START", 0 },
                { "SELECT", 0 }
            };

            var lastComment = long.Parse(IO.ReadAllText(Paths.LastComment));

            var tweetsTask = Task.Run(() =>
            {
                return API.Client.Timelines.GetMentionsTimelineAsync(new GetMentionsTimelineParameters() { SinceId = lastComment });
            });

            bool isCompletedSuccessfully = tweetsTask.Wait(TimeSpan.FromMilliseconds(3000));

            if(!isCompletedSuccessfully)
            {
                Console.WriteLine("Failed getting mentions.");
                return;
            }

            var tweets = tweetsTask.Result;

            Console.WriteLine("Pulled {0} comments.", tweets.Length);

            int count = 0;

            foreach(var t in tweets)
            {
                var input = t.Text.ToUpper().Split(" ")[0];
                if(t.InReplyToStatusId == Settings.Status
                && buttonsCount.ContainsKey(input))
                {
                    Console.WriteLine("- {0}", t.Text);
                    lastComment = Math.Max(lastComment, t.Id);
                    ++buttonsCount[input];
                    ++count;
                }
            }

            if(count > 0)
            {
                var input = buttonsCount.OrderByDescending(x => x.Value).FirstOrDefault();
                Console.WriteLine("Chosen Button: {0} ({1} comments).", input.Key, input.Value);
                button = commentsControls[input.Key];
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                }).Start();

                File.WriteAllText(Paths.LastComment, lastComment.ToString());
            }
        }

        void ManualInputs(ConsoleKey key, out Button button)
        {
            button = null;
            if(controls.ContainsKey(key))
                button = controls[key];
        }
    }
}
