﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;

namespace TwitchChatVotingProxy
{
    class TwitchChatVotingProxy
    {
        private static StreamReader _StreamReader;
        private static StreamWriter _StreamWriter;
        private static TwitchClient _TwitchClient;
        private static string _TwitchChannelName;
        private static bool _VoteRunning = false;
        private static int[] _Votes = new int[3];
        private static List<string> _AlreadyVotedUsers = new List<string>();

        private static void Main(string[] args)
        {
            NamedPipeClientStream pipe = new NamedPipeClientStream(".", "ChaosModVTwitchChatPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                pipe.Connect(1000);
            }
            catch (IOException)
            {

            }
            catch (TimeoutException)
            {

            }

            if (!pipe.IsConnected)
            {
                Console.WriteLine("Error while connecting to pipe!");

                return;
            }

            Console.WriteLine("Connected to pipe!");

            _StreamReader = new StreamReader(pipe);
            _StreamWriter = new StreamWriter(pipe);
            _StreamWriter.AutoFlush = true;

            if (!TwitchLogin())
            {
                return;
            }

            while (pipe.IsConnected)
            {
                PipeStreamReadTick();
                PipeStreamWriteTick();
            }
        }

        private static bool TwitchLogin()
        {
            string twitchUsername = null;
            string twitchOAuth = null;

            string data = File.ReadAllText("chaosmod/config.ini");
            foreach (string line in data.Split('\n'))
            {
                string[] text = line.Split('=');
                if (text.Length < 2)
                {
                    continue;
                }

                switch (text[0])
                {
                    case "TwitchChannelName":
                        _TwitchChannelName = text[1].Trim();
                        break;
                    case "TwitchUserName":
                        twitchUsername = text[1].Trim();
                        break;
                    case "TwitchChannelOAuth":
                        twitchOAuth = text[1].Trim();
                        break;
                }
            }

            if (_TwitchChannelName == null || twitchUsername == null || twitchOAuth == null)
            {
                _StreamWriter.Write("invalid_login\0");

                return false;
            }

            ConnectionCredentials credentials = new ConnectionCredentials(twitchUsername, twitchOAuth);
            WebSocketClient webSocketClient = new WebSocketClient();

            _TwitchClient = new TwitchClient(webSocketClient);
            _TwitchClient.Initialize(credentials, _TwitchChannelName);

            _TwitchClient.OnMessageReceived += OnMessageRecieved;

            bool failed = false;
            bool done = false;
            _TwitchClient.Connect();

            _TwitchClient.OnConnectionError += (object sender, OnConnectionErrorArgs e) =>
            {
                failed = true;
                done = true;
            };

            _TwitchClient.OnConnected += (object sender, OnConnectedArgs e) =>
            {
                done = true;
            };

            while (!done)
            {

            }

            if (failed)
            {
                _StreamWriter.Write("invalid_login\0");

                return false;
            }

            Console.WriteLine("Logged into Twitch Account!");

            done = false;

            _TwitchClient.OnJoinedChannel += (object sender, OnJoinedChannelArgs e) =>
            {
                if (e.Channel.ToLower() == _TwitchChannelName.ToLower())
                {
                    done = true;
                }
            };

            int lastTick = Environment.TickCount;
            while (!done)
            {
                if (lastTick < Environment.TickCount - 1500)
                {
                    failed = true;
                    done = true;
                }
            }

            if (failed)
            {
                _StreamWriter.Write("invalid_channel\0");

                return false;
            }

            Console.WriteLine("Connected to Twitch Channel!");

            return true;
        }

        private static void OnMessageRecieved(object sender, OnMessageReceivedArgs e)
        {
            if (_VoteRunning)
            {
                ChatMessage chatMessage = e.ChatMessage;
                string userId = chatMessage.UserId;

                if (_AlreadyVotedUsers.Contains(userId))
                {
                    return;
                }

                string msg = chatMessage.Message;
                bool successfulVote = true;
                switch (msg.Trim())
                {
                    case "1":
                        _Votes[0]++;
                        break;
                    case "2":
                        _Votes[1]++;
                        break;
                    case "3":
                        _Votes[2]++;
                        break;
                    default:
                        successfulVote = false;
                        break;
                }

                if (successfulVote)
                {
                    _AlreadyVotedUsers.Add(userId);
                }
            }
        }

        static Task<string> _LineReadTask = null;
        private static void PipeStreamReadTick()
        {
            if (_LineReadTask == null)
            {
                _LineReadTask = _StreamReader.ReadLineAsync();
            }
            else if (_LineReadTask.IsCompleted)
            {
                string line = _LineReadTask.Result;
                _LineReadTask = null;

                Console.WriteLine(line);

                if (line.StartsWith("vote:"))
                {
                    if (_VoteRunning)
                    {
                        return;
                    }

                    string[] data = line.Split(':');

                    _Votes[0] = 0;
                    _Votes[1] = 0;
                    _Votes[2] = 0;
                    _AlreadyVotedUsers.Clear();
                    _VoteRunning = true;

                    _TwitchClient.SendMessage(_TwitchChannelName, "Time for a new effect! Vote between:");
                    _TwitchClient.SendMessage(_TwitchChannelName, $"1: {data[1]}");
                    _TwitchClient.SendMessage(_TwitchChannelName, $"2: {data[2]}");
                    _TwitchClient.SendMessage(_TwitchChannelName, $"3: {data[3]}");
                }
                else if (line == "getvoteresult")
                {
                    if (!_VoteRunning)
                    {
                        return;
                    }

                    List<int> chosenEffects = new List<int>();
                    int highestVotes = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        int votes = _Votes[i];
                        if (votes > highestVotes)
                        {
                            chosenEffects.Clear();
                            chosenEffects.Add(i);

                            highestVotes = votes;
                        }
                        else if (votes == highestVotes)
                        {
                            chosenEffects.Add(i);
                        }
                    }

                    int count = chosenEffects.Count;
                    if (count > 1)
                    {
                        int chosen = new Random().Next(0, count);
                        chosenEffects.Clear();
                        chosenEffects.Add(chosen);
                    }

                    _StreamWriter.Write("voteresults:" + chosenEffects[0] + "\0");

                    _VoteRunning = false;
                }
                else if (line == "novoteround")
                {
                    _TwitchClient.SendMessage(_TwitchChannelName, "No voting this time! Chaos Mod will decide for an effect itself.");
                }
            }
        }

        static int _LastTick = Environment.TickCount;
        private static void PipeStreamWriteTick()
        {
            int curTick = Environment.TickCount;

            if (_LastTick < curTick - 1000)
            {
                _LastTick = curTick;

                _StreamWriter.Write("ping\0");
            }
        }
    }
}