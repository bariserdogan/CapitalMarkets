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
        #region Variables

        static readonly string textFile = @"C:\Users\Lenovo\Documents\Visual Studio 2015\Projects\ConsoleApplication1\ConsoleApplication1\Sentence.txt";

        //private static object SyncRoot = new object();

        static List<Sentence> sentenceList = new List<Sentence>();

        static string defaultHelperThreadNum = null;

        public static List<GlobalListItem> globalList = new List<GlobalListItem>();

        static int _sentenceCount = 0;
        static int _averageWordCount = 0;

        #endregion Variables

        public static void Main(string[] args)
        {
            if (ConfigurationSettings.AppSettings["defaultHelperThreadNum"] != null)
            {
                defaultHelperThreadNum = ConfigurationSettings.AppSettings["defaultHelperThreadNum"] as string;
            }

            Thread  _mainThread = new Thread(() => {
                _sentenceCount = readAndParseFile();
            });
            _mainThread.Start();

            string helperThreadValue = ConfigurationSettings.AppSettings["helperThreadCount"];

            int helperThreadCount = string.IsNullOrEmpty(helperThreadValue)
                ? Convert.ToInt32(defaultHelperThreadNum) : Convert.ToInt32(helperThreadValue);

            //for (int i = 0; i < helperThreadCount; i++)
            //{
            //    Thread my_thread = new Thread(() => Work(sentenceList[i]));
            //    my_thread.Start();
            //}
            DisplayInfo();
            Console.ReadLine();
        }

        public static int readAndParseFile()
        {
            if (!File.Exists(textFile))
                return 0;

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
            if (sentenceList.Count > 0)
            {
                _sentenceCount = sentenceList.Count;
                _averageWordCount = CalculateAverageWord(sentenceList);
            }
            return _sentenceCount;
        }


        public static void Work(Sentence sentence)
        {
            string[] delimitedSentence = sentence.text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in delimitedSentence)
            {
                if (globalList.Any(g => g.itemKey.ToString().Equals(word, StringComparison.OrdinalIgnoreCase)))
                {
                    GlobalListItem listItem = globalList.Where(g => g.itemKey.ToString().Equals(word)).First();
                    listItem.itemCount++;
                }
                else
                {
                    globalList.Add(new GlobalListItem()
                    {
                        itemKey = word,
                        itemCount = 1
                    });
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

        public static int CalculateAverageWord(List<Sentence> sentenceList)
        {
            return sentenceList.Sum(x => x.WordCount) / sentenceList.Count;
        }

        public static void DisplayInfo()
        {
            Console.WriteLine("Sentence Count: {0}", sentenceList.Count);
            Console.WriteLine("Avg. Word Count: {0}", CalculateAverageWord(sentenceList));
        }

        public static void DisplayGlobalListItem(List<GlobalListItem> histogram)
        {
            Console.WriteLine("\n");
            foreach (GlobalListItem item in histogram.OrderByDescending(x => x.itemCount))
            {
                Console.WriteLine($"{item.itemKey}" + " " + $"{item.itemCount}");
            }
        }

    }
}
