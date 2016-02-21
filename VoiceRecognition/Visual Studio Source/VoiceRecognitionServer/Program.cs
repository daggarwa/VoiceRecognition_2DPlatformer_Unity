using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Speech.Recognition;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace VoiceRecognitionServer
{
    class Program
    {
        public static SpeechRecognitionEngine recognitionEng;
        public static string reception = "#";
        public static string endWord = "finish";
        public static string IP = "127.0.0.1";
        public static byte[] data = new byte[512];
        public static int port = 26000;
        public static double validity = 0.70f;

        public static void Main(string[] args)
        {

            recognitionEng = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
            try
            {
                
                recognitionEng.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);

               
                try
                {
                    Choices texts = new Choices();
                    string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\grammar.txt");
                    foreach (string line in lines)
                    {
                        
                        if (line.StartsWith("#P"))
                        {
                            var parts = line.Split(new char[] { ' ' });
                            port = Convert.ToInt32(parts[1]);
                            Console.WriteLine("Port : " + parts[1]);
                            continue;
                        }
                        
                        if (line.StartsWith("#E"))
                        {
                            var parts = line.Split(new char[] { ' ' });
                            endWord = parts[1];
                            Console.WriteLine("End Word : " + parts[1]);
                            continue;
                        }
                        
                        if (line.StartsWith("#I"))
                        {
                            var parts = line.Split(new char[] { ' ' });
                            IP = parts[1];
                            Console.WriteLine("IP : " + parts[1]);
                            continue;
                        }
                        
                        if (line.StartsWith("#V"))
                        {
                            var parts = line.Split(new char[] { ' ' });
                            validity = Convert.ToInt32(parts[1]) / 100.0f;
                            Console.WriteLine("Validity : " + parts[1]);
                            continue;
                        }

                        
                        if (line.StartsWith("#") || line == String.Empty) continue;

                        texts.Add(line);
                    }
                    Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                    recognitionEng.LoadGrammar(wordsList);
                }
                catch (Exception ex)
                {
                    throw ex;
                    //System.Environment.Exit(0);
                }

                
                recognitionEng.SetInputToDefaultAudioDevice();
                
                recognitionEng.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "MicroPhone?");
                recognitionEng.RecognizeAsyncStop();
                recognitionEng.Dispose();
                System.Environment.Exit(0);
            }
            
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(IP), port);
            
            Console.WriteLine("Ready.....");

            while (true)
            {
                if (reception != "#")
                {
                    data = Encoding.ASCII.GetBytes(reception);
                    server.SendTo(data, iep);
                    reception = "#";
                }
                Thread.Sleep(2);
            }

        } 

        public static void engine_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            Console.WriteLine(e.AudioLevel);
        }


        public static void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= validity)
            {
                reception = e.Result.Text;
                
                if (e.Result.Text == endWord)
                {
                    recognitionEng.RecognizeAsyncStop();
                    recognitionEng.Dispose();
                    System.Environment.Exit(0);
                }
            }
            else
            {
                reception = "#";
            }
        }

    }
}
