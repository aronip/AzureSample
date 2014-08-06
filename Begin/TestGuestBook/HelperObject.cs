using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TestGuestBook
{
    class HelperObject
    {
        public static string AzureSDK
        {
            get 
            {
                string path = @"C:\Program Files\Microsoft SDKs\Windows Azure";

                return path;
            
            }
           
        }

        public static string AzureCsrun
        {
            get
            {
                string path = Path.Combine(AzureSDK, @"Emulator\csrun.exe");
                return path;
            }
        }
        public static string AzureDsInit
        {
            get 
            {
                string path = Path.Combine(AzureSDK, @"Emulator\devstore\DSInit.exe");
                return path;
            }
        }

        public static void RunCsrun(string arguments) 
        {
            var process = System.Diagnostics.Process.Start(AzureCsrun, arguments);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException(string.Format("process exit code is nonzero ({0})", process.ExitCode));
            }
        }

        public static void InitDb()
        {
            var process = System.Diagnostics.Process.Start(AzureDsInit, "/forceCreate");
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException(string.Format("process exit code is nonzero ({0})", process.ExitCode));
            }
        }

        public static bool IsProcessOpen(string name)
        {
            //here we're going to get a list of all running processes on
            //the computer
            foreach (Process clsProcess in Process.GetProcesses())
            {
                //now we're going to see if any of the running processes
                //match the currently running processes. Be sure to not
                //add the .exe to the name you provide, i.e: NOTEPAD,
                //not NOTEPAD.EXE or false is always returned even if
                //notepad is running.
                //Remember, if you have the process running more than once, 
                //say IE open 4 times the loop the way it is now will close all 4,
                //if you want it to just close the first one it finds
                //then add a return; after the Kill
                if (clsProcess.ProcessName.Contains(name))
                {
                    //if the process is found to be running then we
                    //return a true
                    return true;
                }
            }
            //otherwise we return a false
            return false;

        }

        public static bool FabricLoaded()
        {
            return IsProcessOpen("DFService") & IsProcessOpen("DFAgent") & IsProcessOpen("dfMonitor");
        }
    }
}
