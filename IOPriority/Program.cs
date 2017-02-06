using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IOPriority
{
    class Program
    {
        
        public const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;        
        static uint ioPriority = 0; //0, 1, 2

        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int pid =  Process.GetCurrentProcess().Id;
            int hProcess = Win32.OpenProcess(PROCESS_RIGHTS.PROCESS_ALL_ACCESS, false, pid); 
            setIOPriority(hProcess, ioPriority);
            printIOPriority(hProcess);
            OperateWithFiles();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("END: " + DateTime.Now.ToString("h:mm:ss tt") + "ELASPESD: " + elapsedMs);
            
            Console.ReadLine();
        }        

        unsafe static void setIOPriority(int hProcess, uint newPrio)
        {
            uint ioPrio = newPrio;
            Win32.NtSetInformationProcess(hProcess, PROCESS_INFORMATION_CLASS.ProcessIoPriority,
                 (IntPtr)(&ioPrio), 4);
        }

        unsafe static void printIOPriority(int hProcess)
        {
            int sizeofResult = 0;
            uint ioPrio;
            Win32.NtQueryInformationProcess(hProcess, PROCESS_INFORMATION_CLASS.ProcessIoPriority,
                 (IntPtr)(&ioPrio), 4, ref sizeofResult);
            Console.WriteLine("new IOPrio: " + ioPrio);
        }
        static void OperateWithFiles()
        {
            Parallel.For(0, 50000, new ParallelOptions { MaxDegreeOfParallelism = 10 }, count =>
            {
                            
                string file = Guid.NewGuid().ToString();
                Console.WriteLine("Start process {0} file {1}.txt", count, file);
                string path = @"C:\DiskTester\" + file + ".txt";
                using (StreamWriter toFile = new StreamWriter(File.Create(path)))
                {
                    toFile.Write(String.Concat(Enumerable.Repeat(file, 100)));
                    toFile.Close();
                }
            });
        }
    }
}
