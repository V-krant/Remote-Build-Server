//////////////////////////////////////////////////////////////////////////
// ChildBuilder.cs - Child Builder for building individual test requests//
//                                                                      //
// Author: Vikrant Bhopatrao (Email: vsbhopat@syr.edu)                  //
// Application: CSE681-Software Modeling and Analysis Project 4         //
// Environment: Console Application                                     //
//////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * 1. Receive test requests names from the Mother builder
 * 2. Ask for the testRequest file from the MockRepo
 * 3. Parse the test request and ask for files required for the test
 * 4. Receive files and build the source files
 * 5. Generate log file and send to Repo
 * 6. Send a ready message whenever finished building a test request
 * 
 * Public Interface 
 * ----------------
 *  public bool CreateDll(ref string logfile,string port,string folder); - Build Dll files and generate logs
 *  
 * Required Files:
 * ---------------
 * ChildBuilder.cs
 * MPCommService.cs
 * IMPCommService.cs
 * testrequest.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 04 Dec 2017
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

namespace ChildBuilder
{
    
    class ChildBuilder
    {
        private static Sender csndr;
        private static Sender repo;
        private static Sender harness;
        private static CommMessage rcv;
        private static CommMessage mMsg;
        private static int Count=0;
        private static string logfile;
        private static Receiver crcvr = new Receiver();

        /*Start a connection with repository*/
        private static void Parse_method(string portN)
        {
            Console.WriteLine("\nReceived build request from mother builder");
            Console.WriteLine("\ntest Request Name: " + rcv.arguments[0]);
            CommMessage repoMsg = new CommMessage(CommMessage.MessageType.request);
            repoMsg.command = "Xml";
            repoMsg.author = "Client";
            repoMsg.to = "http://localhost:8081/IPluggableComm";
            repoMsg.from = portN;
            repoMsg.arguments.Add(rcv.arguments[0]);
            repo.postMessage(repoMsg);                  //Send a connection message to repo
        }

        /*Build the received source files and generate logs*/
        private static void BuildCodes_method(string portN)
        {
            Console.WriteLine("\nRequirement: 7");
            Console.WriteLine("\nBuilding...");
            DllBuilder db1 = new DllBuilder();
            bool result = db1.CreateDll(ref logfile, portN, rcv.arguments[0]);       // Build the source files
            Count--;
            Console.WriteLine("\nRequirement: 8");
            Console.WriteLine("\nSending build log to RepoStorage");
            repo.postFile(logfile, "../../../RepoStorage", "../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/" + rcv.arguments[0]);
                                                                                    //Send log file to repo
            if (result == true)
            {
                Console.WriteLine("\nBuilding Complete\n");
                Console.WriteLine("\nRequirement: 8");
                Console.WriteLine("\nCreating new test request");
                TestRequest cbldr = new TestRequest();  //Else create new test request for test harness
                cbldr.author = "Client";
                cbldr.testDriver = "TestCase.dll";
                cbldr.makeRequest();                
                Console.WriteLine("\n{0}\n", cbldr.doc.ToString());
                cbldr.saveXml("../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/" + rcv.arguments[0], "TestRequest.xml");   //Save test request in RepoStorage
                Console.WriteLine("\nSending test request and libraries to TestHarnessStorage");    //Send test request and libraries to test harness
                harness.postFile("TestRequest.xml", "../../../TestHarnessStorage/" + "TempStorage" + Count.ToString(), "../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/" + rcv.arguments[0]);
                harness.postFile("TestCase.dll", "../../../TestHarnessStorage/" + "TempStorage" + Count.ToString(), "../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/" + rcv.arguments[0]);
                CommMessage test = new CommMessage(CommMessage.MessageType.request);
                test.command = "ExecuteTests";
                test.author = "Client";
                test.to = "http://localhost:8078/IPluggableComm";
                test.from = portN;
                test.arguments.Add("TempStorage" + Count.ToString());
                harness.postMessage(test);                  // Send a message to test harness to execute tests
            }
            string dir = Path.GetFullPath("../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/" + rcv.arguments[0]);
            deletefiles(dir);
            if (Count == 0)
            {
                Console.WriteLine("\nSending Ready message to Mother Builder..");
                csndr.postMessage(mMsg);
            }
        }

        /*send a message to repo to send xml file*/
        private static void XmlConnect_method(string portN)
        {
            CommMessage repoMsg2 = new CommMessage(CommMessage.MessageType.reply);
            repoMsg2.command = "sendXml";
            repoMsg2.author = "Client";
            repoMsg2.to = "http://localhost:8081/IPluggableComm";
            repoMsg2.from = portN;
            repoMsg2.arguments.Add(rcv.arguments[0]);
            repo.postMessage(repoMsg2);
        }

        /*Parse the received xml file and send message to repo to send source files*/
        private static void ParseXml_method(string portN)
        {
            Console.WriteLine("\nTest Request file received");
            Console.WriteLine("\nParsing test Request");
            Console.WriteLine("\nthe test request contains the following source files:");
            CommMessage repoMsg3 = new CommMessage(CommMessage.MessageType.request);
            repoMsg3.command = "sendFiles";
            repoMsg3.to = "http://localhost:8081/IPluggableComm";
            repoMsg3.from = portN;
            TestRequest chd = new TestRequest();
            chd.loadXml("../../../ChildBuilder" + (Int32.Parse(portN) - 8081) + "Storage/", rcv.arguments[0]);
            List<List<string>> parsedelements = chd.parse();            //Parse test request
            foreach (List<string> test in parsedelements)
            {
                Count++;
                repoMsg3.arguments.Clear();
                foreach (string name in test)
                {
                    repoMsg3.arguments.Add(name);
                    Console.WriteLine(name);
                }
                Console.WriteLine("\nSending message to receiver to transfer source files");
                repo.postMessage(repoMsg3);
                Thread.Sleep(500);
            }
        }

        /*Process incoming messages and take suitable action*/
        private static void processMessage(string portN)
        {
            switch (rcv.command)
            {
                case "Parse":
                    {
                        Parse_method(portN);
                        Console.WriteLine("\nRequirement: 6");
                        rcv.show();
                        break;
                    }
                case "BuildCodes":
                    {
                        BuildCodes_method(portN);
                        rcv.show();
                        break;
                    }
                case "XmlConnect":
                    {
                        XmlConnect_method(portN);
                        rcv.show();

                        break;
                    }
                case "ParseXml":
                    {
                        ParseXml_method(portN);
                        rcv.show();
                        break;
                    }
            }
        }

        /*Delete files from builder's storage*/
        private static void deletefiles(string dir)
        {
            try
            {
                if (System.IO.Directory.Exists(dir))
                {
                    string[] tempfiles = Directory.GetFileSystemEntries(dir);
                    foreach (string f in tempfiles)
                    {
                        string file = Path.GetFullPath(f);
                        System.IO.File.Delete(file);
                    }
                    System.IO.Directory.Delete(dir);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("/nUnable to clear Child Builder Storage");
            }         
        }

        /*start receiver thread to receive incoming messages*/
        private static void StartRcvThread(string args)
        {
            while (true)
            {
                rcv = crcvr.getMessage();
                if (rcv.type == CommMessage.MessageType.closeReceiver)
                {                                       // Close all senders is closeReceiver message is received
                    rcv.show();
                    CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
                    csndr.postMessage(closeS);
                    Thread.Sleep(500);
                    repo.postMessage(closeS);
                    Thread.Sleep(500);
                    harness.postMessage(closeS);
                    Thread.Sleep(500);
                    string dir = Path.GetFullPath("../../../ChildBuilder" + (Int32.Parse(args) - 8081) + "Storage");
                    deletefiles(dir);               // delete storage files   
                    break;
                }
                else
                {
                    processMessage(args);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.Write("\n  please enter a port number on command line\n");
                return;
            }
            else
            {
                Console.WriteLine("******New Child Builder created listening on port " + Int32.Parse(args[0]) + "******");
                csndr = new Sender("http://localhost", 8080);           //Start all sender objects
                repo = new Sender("http://localhost", 8081);
                harness = new Sender("http://localhost", 8078);
                try
                {
                    crcvr.start("http://localhost", Int32.Parse(args[0]));  //Start receiver
                }
                catch (Exception)
                {
                    Console.WriteLine("\nUnable to start receiver port of Child Builder...");
                }               
                mMsg = new CommMessage(CommMessage.MessageType.request);
                mMsg.command = "Ready";
                mMsg.author = "Client";
                mMsg.to = "http://localhost:8080/IPluggableComm";
                mMsg.from = args[0];
                csndr.postMessage(mMsg);                //Send ready message to mother builder
                mMsg.show();
                Thread.Sleep(500);
                mMsg.to = "http://localhost:8078/IPluggableComm";
                harness.postMessage(mMsg);
                StartRcvThread(args[0]);              
            }
        }
    }

    class DllBuilder
    {
        /*build dll files and generate logs*/
        public bool CreateDll(ref string logfile,string port,string folder)
        {
            Process p = new Process();                              //Build the source files using Process class
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/Ccsc /target:library /out:TestCase.dll /warn:0 /nologo *.cs";
            p.StartInfo.WorkingDirectory = @"../../../ChildBuilder"+(Int32.Parse(port)-8081)+"Storage/"+folder;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            string output="..";
            try
            {
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to build Dlls or no source files found...");
            }         
           
            logfile = "log_" + folder + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt";
            string path = "../../../ChildBuilder" + (Int32.Parse(port) - 8081) + "Storage/" + folder +"\\"+ logfile;
            TestRequest lg = new TestRequest();
            try
            {
                using (System.IO.StreamWriter log = new System.IO.StreamWriter(path))
                {
                    if (output == "")
                    {                                                   //If build successfull store log and return true
                        Console.WriteLine("\n"+DateTime.Now.ToString() + ":   ***Build Succeeded***\n");
                        log.WriteLine(DateTime.Now.ToString() + ":   ***Build Succeeded***");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("\n"+DateTime.Now.ToString() + ":  ***The build was unsuccessfull with the following errors***\n");
                        Console.WriteLine(output);
                        log.WriteLine(output);                          //Else store log and return false
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to report log");
                return false;
            }          
        }
    }
}
