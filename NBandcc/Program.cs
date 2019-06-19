using System;
using System.Diagnostics;
using System.IO;

namespace NBandcc
{
    class Program
    {
        static void Main(string[] args)
        {
            Log($"程序启动，版本:{Module.Version}");      
            ConfigHelper.ReadConfig();
            FileQueue.ReadLocalList();
            if (args==null || args.Length == 0)
            {

                int count = FileQueue.Count();
                if (count == 0)
                {
                    Log("没有待传文件");
                    return;
                }
                else
                {
                    Log("继续传本地缓存文件队列...");
                    Run();
                }
            }
            else
            {
                string str0 = args[0];
                if (str0 == "-e")
                {
                    if (args.Length < 3)
                    {
                        Log("请输入 inputfile 和 outputfile");
                        return;
                    }
                    else
                    {
                        string input = args[1];
                        string output = args[2];
                        double rate = 256000;
                        if (args.Length >= 4)
                        {
                            rate = double.Parse(args[3]);
                        }
                        EncodeFile(input, output, rate);
                        return;
                    }
                }
                else
                {
                    string path = args[0];
                    Log($"地址为:{path}");
                    Run(path);
                }                    
            }
            Console.ReadLine();
        }
        public static void Log(string str)
        {
           // str = DateTime.Now.ToString("[HH:mm:ss] ") + str;
            Console.WriteLine(str);
        }
        private static void EncodeFile(string input,string output,double rate=256000)
        {
            Log("开始视频文件编码...");
            FileInfo inputInfo = new FileInfo(input);
            FileInfo outputInfo = new FileInfo(output);
            if (!inputInfo.Exists)
            {
                Log("输入文件不存在");
                return;
            }
            Log($"输入文件路径:{inputInfo.FullName}");
            Log($"输出文件路径:{outputInfo.FullName}");
            string ffmpegExepath = $"{ConfigHelper.mConfig.FfmpagePath}ffmpeg";
            FileInfo ffmpegExeInfo = new FileInfo(ffmpegExepath);
            if (!ffmpegExeInfo.Exists)
            {
                Log("ffmpeg文件不存在");
                return;
            }
            
            ProcessStartInfo ps = new ProcessStartInfo($"{ffmpegExepath}", $"-y -i {input} -c:v libx264 -b:v {rate} -f mp4 {output}");
            ps.RedirectStandardOutput = true;
            var proc = Process.Start(ps);
            if (proc == null)
            {
                Log("无法执行该命令");
                return;
            }
            else
            {
                Log($"开始进行编码，文件名 {inputInfo.Name}");
            //    Log("-------------Start Exec standard output--------------");
                //开始读取
                using (var sr = proc.StandardOutput)
                {
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        //Console.WriteLine(sr.ReadLine());
                    }
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }
                Log($"[ok]文件编码完成");
                //  Log("---------------Exec end------------------");                
                //Console.WriteLine($"Exited Code ： {proc.ExitCode}");
                //Console.WriteLine($"Total execute time :{(proc.ExitTime - proc.StartTime).TotalMilliseconds} ms");
            }
            return;
            //ProcessStartInfo ps = new ProcessStartInfo("ping"," www.baidu.com");
            //ps.RedirectStandardOutput = true;
            //var proc = Process.Start(ps);
            //if (proc == null)
            //{
            //    Console.WriteLine("无法执行该命令");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("-------------Start read standard output--------------");
            //    //开始读取
            //    using (var sr = proc.StandardOutput)
            //    {
            //        while (!sr.EndOfStream)
            //        {
            //            Console.WriteLine(sr.ReadLine());
            //        }

            //        if (!proc.HasExited)
            //        {
            //            proc.Kill();
            //        }
            //    }
            //    Console.WriteLine("---------------Read end------------------");
            //    Console.WriteLine($"Total execute time :{(proc.ExitTime - proc.StartTime).TotalMilliseconds} ms");
            //    Console.WriteLine($"Exited Code ： {proc.ExitCode}");
            //}
        }
        static void Run(string path="")
        {
            
            if (!string.IsNullOrEmpty(path))
            {
         
                if (Directory.Exists(path))
                {
                    Log("检测到文件夹,读取文件中...");
                    DirectoryInfo dinfo = new DirectoryInfo(path);
                    foreach(var f in dinfo.GetFiles())
                    {
                        FileQueue.Add(f.FullName);
                    }
                    FileQueue.Save();
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        Log("文件不存在");

                    }
                    else
                    {
                        Log("新增文件...");
                        FileQueue.Add(path);
                        FileQueue.Save();
                    }
                   
                }           
            }
            
            try
            {
              
                LoopUploader.StartWork();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
           
        }
    }
}
