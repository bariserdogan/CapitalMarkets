using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;

namespace ConsoleApplication1
{
    class Program
    {
        static readonly string textFile = @"c:\users\lenovo\documents\visual studio 2015\Projects\ConsoleApplication1\ConsoleApplication1\Sentence.txt";
        private static object SyncRoot = new object();
        static List<Sentence> sentenceList = new List<Sentence>();

        static int defaultHelperThreadNum = 4;

        public static List<GlobalListItem> globalList; 

        static void Main(string[] args)
        {
            Thread mainThread = new Thread(new ThreadStart(readAndParseFile));
         
            string value = ConfigurationSettings.AppSettings["helperThreadCount"];
            int helperThreadCount = string.IsNullOrEmpty(value)
                ? defaultHelperThreadNum : Convert.ToInt32(value);

            globalList = new List<GlobalListItem>();

            

            for (int i = 0; i < helperThreadCount; i++)
            {
                Thread my_thread = new Thread(()=>Work(sentenceList[i]));
                my_thread.Start();
            }

            

        }
        public static void Work(Sentence sentence)
        {
            string[] delimitedSentence = sentence.text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string word in delimitedSentence)
            {
                
                if(globalList.Any(g => g.itemKey.ToString().Equals(word)))
                {
                    GlobalListItem listItem = globalList.Where(g => g.itemKey.ToString().Equals(word)).First();
                    listItem.itemCount += 1;
                }
                else
                {
                    GlobalListItem glb = new GlobalListItem()
                    {
                        itemKey = word,
                        itemCount = 1
                    };
                }
                //foreach (GlobalListItem item in globalList)
                //{
                //    if (word.Equals(item.itemKey))
                //    {
                //        isExist = true;
                //        item.itemCount++;   
                //    }
                //}
            }
        }

        public static void readAndParseFile()
        {
            lock (SyncRoot)
            {
                if (File.Exists(textFile))
                {
                    string allText = Regex.Replace(File.ReadAllText(textFile), @"[\r\n\t ]+", " ");

                    string[] textArray = allText.Split(new char[] { '.', '?' }, StringSplitOptions.RemoveEmptyEntries);

                    if (textArray.Length > 0)
                    {
                        int sequence = 1;

                        foreach (string s in textArray)
                        {
                            string[] delimitedSentence = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            Sentence sentence = new Sentence()
                            {
                                Sequence = sequence,
                                text = s.ToString(),
                                WordCount = delimitedSentence.Length
                            };
                            sentenceList.Add(sentence);

                            sequence++;
                        }
                    }
                    DisplayInfo(sentenceList);
                }
            }
        }
        public static void DisplayInfo(List<Sentence> sentenceList)
        {
            int averageWord = CalculateAverageWord(sentenceList);
            Console.WriteLine("Sentence Count: {0}", sentenceList.Count);
            Console.WriteLine("Avg. Word Count: {0}", averageWord);
        }
        public static int CalculateAverageWord(List<Sentence> sentenceList)
        {
            return sentenceList.Sum(x => x.WordCount) / sentenceList.Count;
        }
    }
}
