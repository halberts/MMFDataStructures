using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Collections.Generic;

namespace MMDataStructures.Tests
{
    /// <summary>
    /// Summary description for MemoryMappedDictionaryBenchmarkTest
    /// </summary>
    [TestFixture]
    class MemoryMappedDictionaryBenchmarkTest
    {
        [Test]
        public void AddValues_VerifyValues()
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory;
            //String fileName = "test01";
            //BackingUnknownSize<string, string> backingFile = new BackingUnknownSize<string, string>(fileName, 2000000);
            String fileName2 = "test02";
            //Dictionary<string, Customer> dict = new Dictionary<string, Customer>(fileName2, 1024);

            MMFDictionary<string, string> dict = new MMFDictionary<string, string>(fileName2, 1024, PersistenceMode.Persist);

            string prevKey = null;
            string prevVal = null;
            Stopwatch sw = new Stopwatch();

            List<String> listKey = new List<string>();
            sw.Start();
            string value;
            dict.TryGetValue("key-1", out value);
            if (value == null)
            {
                for (int i = 0; i < 500000; i++)
                {
                    //string key = Guid.NewGuid().ToString();
                    //string value = Guid.NewGuid().ToString();
                    //Customer customer = GetCustomer();
                    string key = "key-" + i;
                    value = "value-" + i;
                    dict.Add(key, value);
                    //dict.Add(key, customer);
                    listKey.Add(key);
                    /*
                    if (prevKey != null)
                    {
                        string result = dict[prevKey];
                        Assert.AreEqual(prevVal, result);
                    }
                    prevKey = key;
                    prevVal = value;
                     * */
                }
            }
            sw.Stop();
            //Console.WriteLine("add spend time = {0}", sw.Elapsed.TotalMilliseconds);
            Console.Out.WriteLine("add spend time = {0}  count={1}", sw.Elapsed.TotalMilliseconds, dict.Count);
            //Trace.WriteLine("add spend time = {0}  count={1}", sw.Elapsed.TotalMilliseconds, dict.Count);
            sw.Restart();
            for (int i = 0; i < 500000; i++)
            {
                string key = "key-" + i;
                value = "value-" + i;
                string tempValue;
                dict.TryGetValue(key, out tempValue);
                if (tempValue != null && !value.Equals(tempValue))
                {
                    Console.WriteLine(" value doesn't match {0} {1}", value, tempValue);
                }
            }
            /*
            foreach (string key in listKey) {
                //Customer tempValue;
                string tempValue;
                dict.TryGetValue(key,out tempValue);  
            }
             */
            sw.Stop();
            //Console.WriteLine("get spend time = {0}", sw.Elapsed.TotalMilliseconds);
            Console.Out.WriteLine("get spend time = {0}  count={1}", sw.Elapsed.TotalMilliseconds, dict.Count);

            /*
            listKey.Clear();
            ConcurrentDictionary<string,string> currDict = new ConcurrentDictionary<string,string>();
            sw.Restart();
            for (int i = 0; i < 500000; i++)
            {
                string key = Guid.NewGuid().ToString();
                string value = Guid.NewGuid().ToString();
                //currDict.Add(key, value);
                currDict.TryAdd(key, value);
                listKey.Add(key);
            }
            sw.Stop();
            Console.Out.WriteLine("ConcurrentDictionary add spend time = {0}  count={1}", sw.Elapsed.TotalMilliseconds, dict.Count);

            sw.Restart();
            foreach (string key in listKey)
            {
               string tempValue;
               currDict.TryGetValue(key, out tempValue);
            }
            sw.Stop();
            Console.Out.WriteLine("ConcurrentDictionary get spend time = {0}  count={1}", sw.Elapsed.TotalMilliseconds, dict.Count);
            */
        }

        [Test]
        public void LoadLargeMMFValue()
        {
            Int32 valueSize = 1024 * 1024 * 1024;
            String fileName2 = "MMFLargeValue1";
            MMFDictionary<string, byte[]> dict = new MMFDictionary<string, byte[]>(fileName2, 10240, PersistenceMode.Persist);
            byte[] value;
            string mkey = "MMF-key-1";
            dict.TryGetValue(mkey, out value);


            if (value == null || value.Length < 1)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                value = new byte[valueSize];
                for (int index = 0; index < valueSize; index++)
                {
                    value[index] = 48;
                }
                dict.Add(mkey, value);
                sw.Stop();
                Console.Out.WriteLine("LargeMMFValue add/write spend time = {0}  count={1} valueSize={2} m", sw.Elapsed.TotalMilliseconds, dict.Count, value.Length / 1024 / 1024);
            }
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            byte lastByte = value[valueSize - 1];
            for (int index = 0; index < valueSize; index++)
            {
                if (value[index] != 48)
                {
                    Console.Out.WriteLine("valueByte is not correct ,valueByte={0}", value[index]);
                }
            }
            sw1.Stop();
            Console.Out.WriteLine("LoadLargeMMFValue read spend time = {0}  count={1} valueSize={2} m lastByte={3}", sw1.Elapsed.TotalMilliseconds, dict.Count, value.Length / 1024 / 1024, lastByte);
            //Console.Out.WriteLine("LoadLargeMMFValue add spend ElapsedMilliseconds time = {0}  count={1} valueSize={2} m lastByte={3}", sw.ElapsedMilliseconds, dict.Count, value.Length / 1024 / 1024, lastByte);
        }

        [Test]
        public void WriteReadSmallValueRecords()
        {
            Int32 maxRecords = 1000000;
            String fileName2 = "MMFSmallValueRedords";
            List<String> listKey = new List<string>();
            MMFDictionary<string, string> dict = new MMFDictionary<string, string>(fileName2, 10240, PersistenceMode.TemporaryPersist);
            
            #region test write
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int index = 0; index < maxRecords; index++)
            {
                string key = "key-" + index;
                string value = "value-" + index;
                dict.Add(key, value);
                listKey.Add(key);
            }
            sw.Stop();
            Console.Out.WriteLine("MMFDictionary smallValue write spend time = {0}  count={1} , performance {2}", sw.Elapsed.TotalMilliseconds, dict.Count, dict.Count * 1000 / sw.Elapsed.TotalMilliseconds);
                       
            #endregion test write


            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            foreach (string key in listKey)
            {
                //string key = "key-" + i;
                //value = "value-" + i;
                string tempValue;
                dict.TryGetValue(key, out tempValue);
                /*
                if (tempValue != null && !value.Equals(tempValue))
                {
                    Console.WriteLine(" value doesn't match {0} {1}", value, tempValue);
                }
                */
            }
            /*
            foreach (string key in listKey) {
                //Customer tempValue;
                string tempValue;
                dict.TryGetValue(key,out tempValue);  
            }
             */
            sw2.Stop();
            Console.Out.WriteLine("MMFDictionary smallValue read spend time = {0}  count={1} , performance {2}", sw2.Elapsed.TotalMilliseconds, dict.Count, dict.Count * 1000 / sw2.Elapsed.TotalMilliseconds);

        }



        [Test]
        public void WriteDuplicateKeyValue()
        {
            Int32 valueSize = 1024 * 1024*100;
            String fileName2 = "MMFLargeValue2";
            MMFDictionary<string, byte[]> dict = new MMFDictionary<string, byte[]>(fileName2, 10240, PersistenceMode.Persist);
            byte[] value;
            
            string mkey = "mmf-key-2";
            dict.Remove(mkey);
            dict.TryGetValue(mkey, out value);
            //if (value == null || value.Length < 1)
            //{
                Stopwatch sw = new Stopwatch();
                sw.Start();
                value = new byte[valueSize];
                for (int index = 0; index < valueSize; index++)
                {
                    value[index] = 48;
                }
                dict.Add(mkey, value);
                sw.Stop();
                Console.Out.WriteLine("LargeMMFValue add spend time = {0}  count={1} valueSize={2} m", sw.Elapsed.TotalMilliseconds, dict.Count, value.Length / 1024 / 1024);
            //}
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            byte lastByte = value[valueSize - 1];
            for (int index = 0; index < valueSize; index++)
            {
                if (value[index] != 48)
                {
                    Console.Out.WriteLine("valueByte is not correct ,valueByte={0}", value[index]);
                }
            }
            sw1.Stop();
            Console.Out.WriteLine("LoadLargeMMFValue add spend time = {0}  count={1} valueSize={2} m lastByte={3}", sw1.Elapsed.TotalMilliseconds, dict.Count, value.Length / 1024 / 1024, lastByte);
            //Console.Out.WriteLine("LoadLargeMMFValue add spend ElapsedMilliseconds time = {0}  count={1} valueSize={2} m lastByte={3}", sw.ElapsedMilliseconds, dict.Count, value.Length / 1024 / 1024, lastByte);
        }

         [Test]
        public void TestEsentCollections()
        {
            /*
            PersistentDictionary<string, string> dictionary = new PersistentDictionary<string, string>("Names");
            Console.WriteLine("What is your first name?");
            string firstName = "Test01";
            if (dictionary.ContainsKey(firstName))
            {
                Console.WriteLine("Welcome back {0} {1}",firstName,dictionary[firstName]);
            }
            else
            {
                Console.WriteLine(
                    "I don't know you, {0}. What is your last name?",
                    firstName);
                dictionary[firstName] = "Test01";
            }
             */
            
            string _path = AppDomain.CurrentDomain.BaseDirectory;
            byte[] value;
            PersistentDictionary<string, string> dict = new PersistentDictionary<string,string>("Names01");
            Int32 valueSize = 1024 * 1024 * 512;
             
            string mkey = "key-1";
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            String outValueString;            
            dict.TryGetValue(mkey, out outValueString);
            if(outValueString!=null)
            {

                value = Convert.FromBase64String(outValueString);
                sw1.Stop();
                Console.Out.WriteLine("EsentCollections read spend time = {0} outValueStringLength={1} ,lastValue={2}, valueSize={3} m", sw1.Elapsed.TotalMilliseconds, outValueString.Length, value[valueSize - 1], value.Length / 1024 / 1024);                 
                dict.Remove(mkey);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            value = new byte[valueSize];
            for (int index = 0; index < valueSize; index++)
            {
                value[index] = 48;
            }
            string temp = Convert.ToBase64String(value);
            Console.WriteLine("temp string length = " + temp.Length);
            dict.Add(mkey, temp);
            sw.Stop();
            Console.Out.WriteLine("EsentCollections add spend time = {0}  count={1} valueSize={2} m", sw.Elapsed.TotalMilliseconds, dict.Count, value.Length / 1024 / 1024);            
        }
    }
}
