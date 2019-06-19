using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace NBandcc
{
    class LoopUploader
    {
        private static bool IsNeedWork = false;
        private static Thread workThread;
        public static void StartWork()
        {
            IsNeedWork = true;
            workThread = new Thread(LoopWork);
            workThread.Start();
        }
        public static void Stop()
        {
            IsNeedWork = false;
            try
            {
                if (workThread != null)
                {
                    workThread.Abort();
                }
            }
            catch (Exception e)
            {

            }
        }
        private static async void LoopWork()
        {
            while (true)
            {
                Program.Log($">>检查待传输队列,文件数量{FileQueue.Count()}<<");
                FileQueue file = FileQueue.GetOne();
                if (file != null)
                {                 
                    await UploadWork(file);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
        private static async Task UploadWork(FileQueue file)
        {
            file.Status = "上传中";
            if (file.SendListLock == null) file.SendListLock = new object();
            bool isComplete = true;
            Program.Log($"文件{file.FileName}上传中...");
            for (int i = 0; i < file.SendList.Count; i++)
            {
                SendQueue itm = file.SendList[i];
                if (itm.IsSendOver) continue;
                itm.Status = "上传中";
                file.SendList[i] = itm;
                FileQueue.Save();
                Program.Log($"-->Part {i + 1} 正在上传");
                bool flag = await UploadBlock(itm);
                itm.IsSendOver = flag;
                if (itm.IsSendOver)
                {
                    Program.Log($"   -->[ok]Part {i + 1} 上传成功，总计 {file.SendList.Count}");
                    itm.Status = "已上传";
                }
                else
                {
                    isComplete = false;
                    Program.Log($"   -->[fail]Part {i+1} 上传失败，等待下次重传");
                }
                file.SendList[i] = itm;
                file.CurrentIndex = i + 1;
                FileQueue.Save();
            }
            if (isComplete)
            {
                Program.Log($"文件{file.FileName}上传成功！");
                FileQueue.RemoveFirst();
                FileQueue.Save();
            }
            else
            {
                Program.Log($"文件{file.FileName}部分块上传失败，等待重传");
            }

        }
        private static async Task<bool> UploadBlock(SendQueue block)
        {
            block.ReadBuffer();         
            if (block.Buffer == null || block.Buffer.Length != block.Length)
            {
                block.Buffer = null;
                return false;
            }
            int i = 0;
            while (i++ <5)
            {
                NormalResponse np =await API.UploadFileBlock(block);
                if (np != null)
                {
                    if (np.result)
                    {
                        block.Buffer = null;
                        return np.result;
                    }
                    else
                    {
                        Program.Log("   -->Retry code=1");
                    }
                }
                else
                {
                    Program.Log("   -->Retry code=2");
                }            
            }
            return false;
        }
    }
}
