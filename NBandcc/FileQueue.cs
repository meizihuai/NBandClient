using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NBandcc
{
    //待传文件队列
    class FileQueue
    {
        public string ID { get; set; }
        public string DateTime { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int CurrentIndex { get; set; }
        public int TotalIndex { get; set; }
        public long FileLen { get; set; }
        public double OverPercent { get; set; }
        public string Status { get; set; }
        public List<SendQueue> SendList { get; set; }
        public object SendListLock { get; set; }          
        private static List<FileQueue> mList;
        private static object fileQueueListLock = new object();
        public static bool Add(string filePath)
        {        
            if (!File.Exists(filePath))
            {
                return false;
            }
           
            FileInfo f = new FileInfo(filePath);
            FileQueue q = new FileQueue();
            q.ID = Guid.NewGuid().ToString();
          
            q.DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            q.FileName = f.Name;
            q.FilePath = f.FullName;         
            q.FileLen = f.Length;
            q.OverPercent = 0;
            q.Status = "未开启";
         
            q.ReSetSendQueue();
          
            q.SendListLock = new object();
            
            lock (fileQueueListLock)
            {
                if (mList == null) mList = new List<FileQueue>();
                mList.Add(q);
            }
            return true;
        }
        public static void Clear()
        {
            lock (fileQueueListLock)
            {
                mList.Clear();
            }
        }
        public static int Count()
        {
            lock (fileQueueListLock)
            {
                if (mList == null) return 0;
                return mList.Count;
            }
        }
        public void ReSetSendQueue()
        {
            long per = ConfigHelper.mConfig.Perlen*1024;
            this.SendList = new List<SendQueue>();
            long readIndex = 0;       
            while (readIndex < this.FileLen)
            {          
                long readLen = this.FileLen - readIndex;
                if (readLen > per) readLen = per;
                SendQueue send = new SendQueue();
                send.ID = this.ID;
                send.Index = this.SendList.Count + 1;
                send.Start = readIndex;
                send.End = readIndex + readLen;
                send.Length = readLen;
                send.TotalLength = this.FileLen;
                send.FileName = FileName;
                send.FilePath = FilePath;
                send.IsSendOver = false;
                readIndex += readLen;
                this.SendList.Add(send);
            }

            this.TotalIndex = this.SendList.Count;
        }
        public static void ReadLocalList()
        {
            string path = "localFileQueue.json";
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                lock (fileQueueListLock)
                {
                    mList = JsonConvert.DeserializeObject<List<FileQueue>>(json);
                }
            }
            catch (Exception e)
            {

            }
        }
        public static List<FileQueue> ToList()
        {
            lock (fileQueueListLock)
            {
                return mList;
            }
        }
        

        public static FileQueue GetOne()
        {
            lock (fileQueueListLock)
            {
                if (mList == null || mList.Count == 0) return null;
                return mList[0];
            }
        }
        public static FileQueue GetAt(int index)
        {
            lock (fileQueueListLock)
            {
                if (mList == null || mList.Count <= 0) return null;
                return mList[index];
            }

        }
        public static void RemoveFirst()
        {
            lock (fileQueueListLock)
            {
                if (mList == null || mList.Count == 0) return;
                mList.RemoveAt(0);
            }
        }
        public static void RemoveAt(int index)
        {
            lock (fileQueueListLock)
            {
                if (mList == null || mList.Count <= index) return;
                mList.RemoveAt(index);
            }
        }
        public static void Save()
        {
            string path = "localFileQueue.json";
            string json = "";
            lock (fileQueueListLock)
            {
                json = JsonConvert.SerializeObject(mList, Formatting.Indented);
            }
            if (!string.IsNullOrEmpty(json))
            {
                File.WriteAllText(path, json);
            }
        }
    }
}
