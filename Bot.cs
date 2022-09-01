using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dotmaco
{
    public class Bot
    {
        public static int getEditDistance(string X, string Y)
        {
            int m = X.Length;
            int n = Y.Length;

            int[][] T = new int[m + 1][];
            for (int i = 0; i < m + 1; ++i)
            {
                T[i] = new int[n + 1];
            }

            for (int i = 1; i <= m; i++)
            {
                T[i][0] = i;
            }
            for (int j = 1; j <= n; j++)
            {
                T[0][j] = j;
            }

            int cost;
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    cost = X[i - 1] == Y[j - 1] ? 0 : 1;
                    T[i][j] = Math.Min(Math.Min(T[i - 1][j] + 1, T[i][j - 1] + 1),
                            T[i - 1][j - 1] + cost);
                }
            }

            return T[m][n];
        }

        public static double findSimilarity(string x, string y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            double maxLength = Math.Max(x.Length, y.Length);
            if (maxLength > 0)
            {
                // optionally ignore case if needed
                return (maxLength - getEditDistance(x, y)) / maxLength;
            }
            return 1.0;
        }
        static Random rnd = new Random();
        public DiscordClient Client { get; private set; }
//       public CommandsNextExtension Commands { get; private set; }
        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration()
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
            String line;
            var inputMessageList = new List<string>();
            var outputMessageList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader("inputsave.txt");
                StreamReader sr2 = new StreamReader("outputsave.txt");
                line = sr.ReadLine();
                while (line != null)
                {
                    inputMessageList.Add(line);
                    line = sr.ReadLine();
                }
                line = sr2.ReadLine();
                while (line != null) 
                {
                    outputMessageList.Add(line);
                    line = sr2.ReadLine();
                }
                sr.Close();
                sr2.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            
            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;
            Client.MessageCreated += (DiscordClient s, MessageCreateEventArgs e) =>
            MessageCreatedHandler(s, e, inputMessageList, outputMessageList);

 //           var commandsConfig = new CommandsNextConfiguration 
 //           { 
 //               StringPrefixes = new string[] {configJson.Prefix},
 //               EnableDms = false,
 //               EnableMentionPrefix = true
            
 //           };
 //           Commands = Client.UseCommandsNext(commandsConfig);

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }
        private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        private async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e, List<string> inpmsg, List<string> outmsg) 
        {
            Console.WriteLine(e.Channel.Name);

            if (e.Channel.Name == "slicing-the-meme" & e.Author.IsBot == false)
            {
                String line2;
                var lastline = "Dotmaco";
                try
                {
                    StreamReader sr3 = new StreamReader("DotLog.txt");
                    line2 = sr3.ReadLine();
                    if (line2 != null) { lastline = line2; }
                    sr3.Close();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Exception: " + exc.Message);
                }
                var inputMessage = e.Message.Content.Replace("\n", " ").Replace("\r", " ");
                var outputMessage = "yee";
                var similarCandidates = new List<string>();
                var similarity = new double();
                var currentSim = new double();
                var similarInputs = new List<string>();
                var similar_input = "None";
                similarity = 0;
                for (int i = 0; i < inpmsg.Count; i++)
                {
                    currentSim = findSimilarity(inpmsg[i], inputMessage);
                    if (currentSim > similarity)
                    {
                        similarCandidates.Clear();
                        similarInputs.Clear();
                        similarity = currentSim;
                        similarCandidates.Add(outmsg[i]);
                        similarInputs.Add(inpmsg[i]);
                    }
                    else if (currentSim == similarity)
                    {
                        similarCandidates.Add(outmsg[i]);
                        similarInputs.Add(inpmsg[i]);
                    }  
                }
                if (similarCandidates.Count > 0)
                {
                    int r = rnd.Next(similarCandidates.Count);
                    outputMessage = similarCandidates[r];
                    similar_input = similarInputs[r];
                }
                Console.WriteLine("Input chosen: "+similar_input+", with similarity "+similarity+" to "+inputMessage+". Output: "+outputMessage);
                inpmsg.Add(lastline);
                outmsg.Add(inputMessage);
                try
                {
                    StreamWriter sw = new StreamWriter("inputsave.txt");
                    StreamWriter sw2 = new StreamWriter("outputsave.txt");
                    StreamWriter sw3 = new StreamWriter("DotLog.txt");
                    for (int i = 0; i < inpmsg.Count; i++)
                    {
                        sw.WriteLine(inpmsg[i]);
                    }
                    for (int i = 0; i < outmsg.Count; i++)
                    {
                        sw2.WriteLine(outmsg[i]);
                    }
                    sw3.WriteLine(outputMessage);
                    sw.Close();
                    sw2.Close();
                    sw3.Close();
                }
                catch (Exception excc)
                {
                    Console.WriteLine("Exception: " + excc.Message);
                }
                await s.SendMessageAsync(e.Channel,outputMessage);
            }
        }
    }
}
