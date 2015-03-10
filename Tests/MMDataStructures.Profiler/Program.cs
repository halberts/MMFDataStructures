﻿using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.Isam.Esent.Interop;
using MMDataStructures;
using MMDataStructures.DictionaryBacking;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using EsentCollections = Microsoft.Isam.Esent.Collections.Generic;

namespace BenchmarkConsoleApp
{
    public  class Program
    {
        #region Sample class to test protocolbuffer 
        public class Person 
        {
            int _id = 0;
           
            public int Id 
            {
                get { return _id; }
                set { _id = value; }
            }

            string _name = string.Empty;


            public string Name 
            {
                get { return _name; }
                set { _name = value; }
            } 
        }
        #endregion

        private static int MaxCount = 10000000;

        public static int Main(string[] args)
        {
            ThreadPool.SetMaxThreads(10, 1000);

            TextWriterTraceListener tr1 = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(tr1);

            //SingelThread_HashInMemory();
            SingelThread_HashOnDisk();
            //Threaded_HashInMemory();
            //Threaded_HashOnDisk();

            //SingelThread_SQLite();
            //SingelThread_Esent();
            /*//Test mapper.
            SingelThread_HashCompositeOnDisk();
            Threaded_HashCompositeOnDisk();
            */
            Console.ReadKey();
            return 0;
        }

        private static void Threaded_HashOnDisk()
        {
            Console.WriteLine("Threaded_HashOnDisk");
            var dict = new MMFDictionary<string, string>("Threaded_HashOnDisk", MaxCount);

            Console.WriteLine("Queuing {0} items to Thread Pool", MaxCount);
            Console.WriteLine("Queue to Thread Pool 0");
            var handles = new System.Collections.Generic.List<WaitHandle>();
            var sw = Stopwatch.StartNew();
            for (int iItem = 1; iItem < 20; iItem++)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                handles.Add(mre);
                ThreadPool.QueueUserWorkItem(d =>
                                                 {
                                                     for (int i = 0; i < MaxCount/20; i++)
                                                     {
                                                         string key = Guid.NewGuid().ToString();
                                                         dict.Add(key, key);
                                                         if(string.IsNullOrEmpty(dict[key])) throw new Exception();
                                                     }
                                                     mre.Set();
                                                 }, null);
            }
            Console.WriteLine("Waiting for Thread Pool to drain");
            WaitHandle.WaitAll(handles.ToArray());
            sw.Stop();
            Console.WriteLine("Thread Pool has been drained (Event fired)");
            Console.WriteLine(sw.Elapsed);
        }

        private static void Threaded_HashCompositeOnDisk()
        {
            Console.WriteLine("Threaded_HashOnDisk");
            var dict = new MMFDictionary<string, Person>("Threaded_HashCompositeOnDisk", 100000);

            Console.WriteLine("Queuing {0} items to Thread Pool", MaxCount);
            Console.WriteLine("Queue to Thread Pool 0");
            System.Collections.Generic.List<WaitHandle> handles = new System.Collections.Generic.List<WaitHandle>();
            Stopwatch sw = Stopwatch.StartNew();
            for (int iItem = 1; iItem < 20; iItem++)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                handles.Add(mre);
                ThreadPool.QueueUserWorkItem(d =>
                {
                    for (int i = 0; i < MaxCount / 20; i++)
                    {
                        string key = Guid.NewGuid().ToString();
                        dict.Add(key, new Person { Id = i, Name = "Name" + i });
                        Person p = null;
                        if (!dict.TryGetValue(key,out p)) throw new Exception();
                    }
                    mre.Set();
                }, null);
            }
            Console.WriteLine("Waiting for Thread Pool to drain");
            WaitHandle.WaitAll(handles.ToArray());
            sw.Stop();
            Console.WriteLine("Thread Pool has been drained (Event fired)");
            Console.WriteLine(sw.Elapsed);
        }
        /*
        private static void Threaded_HashInMemory()
        {
            Console.WriteLine("Threaded_HashInMemory");
            string path = AppDomain.CurrentDomain.BaseDirectory;
            DictionaryPersist<string, string> backingFile = new DictionaryPersist<string, string>(path, MaxCount);

            Dictionary<string, string> dict = new Dictionary<string, string>(backingFile);

            Console.WriteLine("Queuing {0} items to Thread Pool", MaxCount);
            Console.WriteLine("Queue to Thread Pool 0");
            System.Collections.Generic.List<WaitHandle> handles = new System.Collections.Generic.List<WaitHandle>();
            Stopwatch sw = Stopwatch.StartNew();
            for (int iItem = 1; iItem < 20; iItem++)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                handles.Add(mre);
                ThreadPool.QueueUserWorkItem(d =>
                                                 {
                                                     for (int i = 0; i < MaxCount/20; i++)
                                                     {
                                                         string key = Guid.NewGuid().ToString();
                                                         dict.Add(key, key);
                                                         if (string.IsNullOrEmpty(dict[key])) throw new Exception();
                                                     }
                                                     mre.Set();
                                                 }, null);
            }
            Console.WriteLine("Waiting for Thread Pool to drain");
            WaitHandle.WaitAll(handles.ToArray());
            sw.Stop();
            Console.WriteLine("Thread Pool has been drained (Event fired)");
            Console.WriteLine(sw.Elapsed);
        }
        */

        private static void SingelThread_HashOnDisk()
        {
            Console.WriteLine("SingelThread_HashOnDisk");
            //var dict = new Dictionary<string, string>("SingelThread_HashOnDisk", MaxCount);
            var dict = new MMFDictionary<long, long>("SingelThread_HashOnDisk", MaxCount, PersistenceMode.Persist);

            Stopwatch sw = Stopwatch.StartNew();
            for (long i = 0; i < MaxCount; i++)
            {
                //string key = Guid.NewGuid().ToString();
                //dict.Add(key, key);
                //dict.Add(i, i);
                //if (string.IsNullOrEmpty(dict[key])) throw new Exception();
                if (dict[i] != i) throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void SingelThread_HashCompositeOnDisk()
        {
            Console.WriteLine("SingelThread_HashOnDisk");
            var dict = new MMFDictionary<int, Person>("SingelThread_HashCompositeOnDisk", 100000);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < MaxCount; i++)
            {

                dict.Add(i, new Person() { Id = i, Name = "Name" + i });
                Person p = null;
                if (!dict.TryGetValue(i,out p)) throw new Exception();
                if (i/100000 == 0) {
                    Console.WriteLine("Count: " + i);
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /*
        private static void SingelThread_HashInMemory()
        {
            Console.WriteLine("SingelThread_HashInMemory");
            string path = AppDomain.CurrentDomain.BaseDirectory;
            DictionaryPersist<string, string> backingFile = new DictionaryPersist<string, string>(path, MaxCount);

            Dictionary<string, string> dict = new Dictionary<string, string>(backingFile);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < MaxCount; i++)
            {
                string key = Guid.NewGuid().ToString();
                dict.Add(key, key);
                if (string.IsNullOrEmpty(dict[key])) throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
        */

        private class StringPOCO {
            [PrimaryKey]
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private static void SingelThread_SQLite() {
            var dbFactory = new OrmLiteConnectionFactory(
    "App_Data/db.sqlite", SqliteDialect.Provider);
            using (IDbConnection db = dbFactory.Open()) {
                db.DropAndCreateTable<StringPOCO>();
                Console.WriteLine("SingelThread_SQLite");
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < 1000; i++) {
                    string key = Guid.NewGuid().ToString();
                    var poco = new StringPOCO() {
                        Key = key,
                        Value = key,
                    };
                    db.Save(poco);
                    var savedpoco = db.SingleById<StringPOCO>(key);
                    if (savedpoco.Value != key) throw new Exception();
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }

            
        }

        private static void SingelThread_Esent() {
            Console.WriteLine("SingelThread_Esent");
            //var dict = new Dictionary<string, string>("SingelThread_HashOnDisk", MaxCount);
            //var dict = new Dictionary<long, long>("SingelThread_HashOnDisk", MaxCount);
            var dict = new EsentCollections.PersistentDictionary<long, long>("./App_data");
            
            Stopwatch sw = Stopwatch.StartNew();
            for (long i = 0; i < MaxCount; i++) {
                //string key = Guid.NewGuid().ToString();
                //dict.Add(key, key);
                //dict.Add(i, i);
                //dict[i] = i;
                //if (string.IsNullOrEmpty(dict[key])) throw new Exception();
                if (dict[i] != i) throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            dict.Dispose();
        }

    }
}