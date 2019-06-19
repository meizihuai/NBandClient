using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBandcc
{
    //传输队列
    class SendQueue
    {
        public string ID { get; set; }
        public int Index { get; set; }
        public long Length { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long TotalLength { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool IsSendOver { get; set; }
        public string Status { get; set; }
        public byte[] Buffer { get; set; }
        public object[] ToLv()
        {
            return new object[]
            {
               FileName,Index,Length,Status
            };
        }
        public void ReadBuffer()
        {
            if (!File.Exists(FilePath)) return;
            FileStream stream = new FileStream(FilePath, FileMode.Open);
            byte[] by = new byte[Length];
            stream.Position = this.Start;
            stream.Read(by, 0, (int)Length);
            stream.Close();
            this.Buffer = by;
        }
    }
}
