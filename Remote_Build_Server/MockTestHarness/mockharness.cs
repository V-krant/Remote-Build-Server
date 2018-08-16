//////////////////////////////////////////////////////////////////////////
// ChildBuilder.cs - Test libraries and generate test logs              //
//                                                                      //
// Author: Vikrant Bhopatrao (Email: vsbhopat@syr.edu)                  //
// Application: CSE681-Software Modeling and Analysis Project 4         //
// Environment: Console Application                                     //
//////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * 1. Receive test requests name and library file from child builder
 * 2. test the received library files
 * 3. generate test logs
 * 4. Send log to Repo
 * 
 * Public Interface 
 * ----------------
 *  public string loadAndExerciseTesters(); - Test Dll files and generate logs
 *  
 * Required Files:
 * ---------------
 * mockharness.cs
 * MPCommService.cs
 * IMPCommService.cs
 * testrequest.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 03 Dec 2017
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using Xml;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

namespace MockTestHarness
{
    class mockharness
    {
        private static Receiver harness = new Receiver();
        private static Sender repo;
        //private static Sender client;
        private static CommMessage rcvmsg;
        private static string[] logfilename = new string[1];
        private static string folder;
        private static int flag = 1;


        static void Main(string[] args)
        {
            Console.WriteLine("******Mock Test Harness started listening on Port 8078******");
            repo = new Sender("http://localhost", 8081);        //Start sender object for repo
            try
            {
                harness.start("http://localhost", 8078);        // Start receiver port of test harness
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to start receiver port of Mock Test Harness or port already in use...");
            }
            
            Thread receive = new Thread(RcvThread);
            receive.Start();
            receive.Join();
        }

        /*Start a receive thread to process incoming messages*/
        static void RcvThread()
        {

            while (true)
            {
                rcvmsg = harness.getMessage();
                if (rcvmsg.type == CommMessage.MessageType.closeReceiver)
                {                                           // Close sender objects of test harness
                    rcvmsg.show();
                    CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
                    repo.postMessage(closeS);
                    deleteStorage("../../../TestHarnessStorage");
                    break;
                }
                else
                {
                    processMessage();
                }
            }
        }

        /* Process incoming messages and take suitable action*/
        private static void processMessage()
        {
            switch (rcvmsg.command)
            {
                case "ExecuteTests":
                    {
                        rcvmsg.show();
                        if (flag == 0)
                            harness.postMessage(rcvmsg);    //Resend message to receiver queue
                        else
                        {
                            flag = 0;
                            folder = rcvmsg.arguments[0];
                            DllLoaderExec loader = new DllLoaderExec();         //Initialize test file's location
                            DllLoaderExec.testersLocation = "../../../TestHarnessStorage/" + rcvmsg.arguments[0];
                            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);
                            Console.WriteLine("\nRequirement: 9");
                            Console.WriteLine("\nLoading Test Modules from:\n    {0}\n", DllLoaderExec.testersLocation);
                            string result = loader.loadAndExerciseTesters();    //Perform Testing
                            Console.WriteLine("\n{0}\n", result);
                            connectRepo();
                        }
                        break;
                    }
                case "Connected":
                    {
                        rcvmsg.show();
                        getFilesHelper("../../../TestHarnessStorage/" + folder, "*.txt");
                        try
                        {
                            Console.WriteLine("\nRequirement: 9");
                            Console.WriteLine("\nSending test logs to RepoStorage"); 
                            repo.postFile(logfilename[0], "../../../RepoStorage", "../../../TestHarnessStorage/" + folder);
                        }                                       //Send log file to repository
                        catch (Exception)
                        {
                            Console.WriteLine("\nUnable to copy log file");
                        }                      
                        flag = 1;
                        deleteStorage("../../../TestHarnessStorage/" + folder); // delete test harness storage
                        break;
                    }
            }
        }

        /* Send a connection message to repo*/
        private static void connectRepo()
        {
            CommMessage mMsg = new CommMessage(CommMessage.MessageType.request);
            mMsg.command = "ConnectHarness";
            mMsg.author = "Client";
            mMsg.to = "http://localhost:8081/IPluggableComm";
            mMsg.from = "8078";
            repo.postMessage(mMsg);
        }

        /*Helper class to get files from storage*/
        private static void getFilesHelper(string path, string type)
        {
            logfilename = Directory.GetFiles(path, type);
            foreach (string f in logfilename)
            {
                string name = System.IO.Path.GetFileName(f);
                logfilename[0] = name;
            }
        }

        /*Delete files from storage and delete subfolders*/
        private static void deleteStorage(string directory)
        {
            try
            {
                string dir = Path.GetFullPath(directory);
                if (System.IO.Directory.Exists(dir))
                {                                                  // Delete al newly created testrequests
                    string[] tempfiles = Directory.GetFiles(dir, "*.*");
                    foreach (string f in tempfiles)
                    {
                        string fi = Path.GetFullPath(f);
                        System.IO.File.Delete(fi);
                    }
                    System.IO.Directory.Delete(dir);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to delete MockTestHarness Storage...");
            }          
        }
    }

    class DllLoaderExec
    {
        public static string testersLocation { get; set; } = ".";
        private static int tcount = 0;

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("\ncalled binding error event handler\n");
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        //----< load assemblies from testersLocation and run their tests >-----
        public string loadAndExerciseTesters()
        {
            tcount++;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);
            try
            {
                DllLoaderExec loader = new DllLoaderExec();

                // load each assembly found in testersLocation

                string[] files = Directory.GetFiles(testersLocation, "*.dll");
                foreach (string file in files)
                {
                    //Assembly asm = Assembly.LoadFrom(file);

                    Assembly asm = Assembly.Load(File.ReadAllBytes(file));
                    string fileName = Path.GetFileName(file);
                    Console.WriteLine("loaded {0}\n", fileName);

                    // exercise each tester found in assembly

                    Type[] types = asm.GetTypes();
                    int testno=0;
                    foreach (Type t in types)
                    {
                        // if type supports ITest interface then run test
                          
                        if (t.GetInterface("DllLoaderDemo.ITest", true) != null)
                            if (!loader.runSimulatedTest(t, asm,testno)) 
                                Console.WriteLine("test {0} failed to run\n", t.ToString());
                        testno++;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Simulated Testing completed";
        }

        //----< run tester t from assembly asm >-------------------------------

        bool runSimulatedTest(Type t, Assembly asm, int testno)
        {
            try
            {
                Console.WriteLine("attempting to create instance of {0}\n", t.ToString());
                object obj = asm.CreateInstance(t.ToString());

                // announce test

                MethodInfo method = t.GetMethod("say");
                if (method != null)
                    method.Invoke(obj, new object[0]);

                // run test

                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);

                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "passed";
                    return "failed";
                };
                Console.WriteLine("\n\n***test {0}***\n", act(status));
                using (System.IO.StreamWriter log = new System.IO.StreamWriter(DllLoaderExec.testersLocation + "/TestLog{" + tcount + "}" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt", true))
                {
                    log.WriteLine(DateTime.Now.ToString());                 //Generate log file
                    log.WriteLine("Test" + testno + " : " + act(status));
                }
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter log = new System.IO.StreamWriter(DllLoaderExec.testersLocation + "/TestLog{" + tcount + "}" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt", true))
                {
                    log.WriteLine(DateTime.Now.ToString());
                    log.WriteLine("Test" + testno + " : failed with message \"{0}\"", ex.Message);
                }
                Console.WriteLine("\n\n***test failed with message \"{0}\"***\n", ex.Message);
                return false;
            }
            return true;
        }
    }
}