/* Written by David Crowder
 * AZ Partsmaster, Inc.
 * 
MIT License

Copyright (c) 2016, AZ Partsmaster

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingleRun2 {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //MessageBox.Show("test message", "test caption", MessageBoxButtons.OK, MessageBoxIcon.Error);

            string path = "";
            var prog = "";
            if (args.Length == 0) {
                MessageBox.Show("Please include a program to start.\n\nExample: \nSingleRun.exe \"C:\\Program Files\\Windows NT\\Accessories\\wordpad.exe\"", "Error: No Program", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(1);
            }
            else {
                path = args[0];
                if (!File.Exists(path)) {
                    MessageBox.Show("\"" + path + "\" does not exist.\nPlease check the location.\nAnything with spaces in it needs to be inside double-quotes.", "Error: Program Does Not Exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Environment.Exit(1);
                }
                else {
                    var splits = path.Split('\\');
                    prog = splits[splits.Length - 1];
                    foreach (var p in Process.GetProcessesByName(prog.Replace(".exe", ""))) {
                        string processOwner;
                        try {
                            processOwner = p.WindowsIdentity().Name;
                        }
                        catch (Exception ex) {
                            processOwner = ex.Message; // Probably "Access is denied"
                        }
                        if (processOwner.Contains(Environment.UserName)) {
                            MessageBox.Show("Program already running with PID " + p.Id + "\n\nOnly one copy of this program may be run at a time.\n\nIf you feel that you have received this message in error,\nplease contact your IT staff.", "Error: " + prog, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            System.Environment.Exit(1);
                        }
                    }
                    Process newProcess = Process.Start(path);
                    Console.WriteLine("Launching " + prog + " with PID: " + newProcess.Id);
                }
            }
        }
    }


    /// <summary>
    /// Extension Methods for the System.Diagnostics.Process Class.
    /// </summary>
    public static class ProcessExtensions {
        /// <summary>
        /// Required to query an access token.
        /// </summary>
        private static uint TOKEN_QUERY = 0x0008;

        /// <summary>
        /// Returns the WindowsIdentity associated to a Process
        /// </summary>
        /// <param name="process">The Windows Process.</param>
        /// <returns>The WindowsIdentity of the Process.</returns>
        /// <remarks>Be prepared for 'Access Denied' Exceptions</remarks>
        public static WindowsIdentity WindowsIdentity(this Process process) {
            IntPtr ph = IntPtr.Zero;
            WindowsIdentity wi = null;
            try {
                OpenProcessToken(process.Handle, TOKEN_QUERY, out ph);
                wi = new WindowsIdentity(ph);
            }
            catch (Exception) {
                throw;
            }
            finally {
                if (ph != IntPtr.Zero) {
                    CloseHandle(ph);
                }
            }

            return wi;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }


}
