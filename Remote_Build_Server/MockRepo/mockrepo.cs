//////////////////////////////////////////////////////////////////////////
// mockrepo.cs - Mock repository for copying test files                 //
//                                                                      //
// Author: Vikrant Bhopatrao                                            //
// Application: CSE681-Software Modeling and Analysis Project 4         //
// Environment: Console Application                                     //
//////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * 1. Receive test requests names from the Child Builder
 * 2. Send testRequest file to the ChildBuilder
 * 3. Send files required for the test
 * 4. Send files names to client GUI
 * 
 * Public Interface:
 * -----------------
 * -None
 * 
 * Required Files:
 * ---------------
 * mockrepo.cs
 * MPCommService.cs
 * IMPCommService.cs
 *
 * 
 * Maintenance History:
 * --------------------
 * Ver 1.1 : 03 Dec 2017 
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace MockRepo
{
    class mockrepo
    {
        private static string file = "";
        private static CommMessage rmsg;
        private static Receiver rrcvr = new Receiver();
        private static Sender[] cs;
        private static Sender harness;
        private static int scount = 0;
        private static List<string> files { get; set; } = new List<string>();
        private static Sender gui;

        /*Send connection message to child builder*/
        private static void Xml_method()
        {
            file = rmsg.arguments[0];
            CommMessage cmsg = new CommMessage(CommMessage.MessageType.request);
            cmsg.command = "XmlConnect";
            cmsg.author = "Client";
            cmsg.to = "http://localhost:" + rmsg.from + "/IPluggableComm";
            cmsg.from = "Repo";
            cmsg.arguments.Add(rmsg.arguments[0]);
            cs[Int32.Parse(rmsg.from) - 8082].postMessage(cmsg);
        }

        /*Send xml file to child builder*/
        private static void sendxml_method()
        {
            Console.WriteLine("\nCopying Test Request File to Child Builder" + (Int32.Parse(rmsg.from) - 8081) + "Storage");
            cs[Int32.Parse(rmsg.from) - 8082].postFile(rmsg.arguments[0], "../../../ChildBuilder" + (Int32.Parse(rmsg.from) - 8081) + "Storage", "../../../RepoStorage");
            CommMessage cmsg = new CommMessage(CommMessage.MessageType.request);
            cmsg.command = "ParseXml";
            cmsg.to = "http://localhost:" + rmsg.from + "/IPluggableComm";
            cmsg.from = "Repo";
            cmsg.arguments.Add(rmsg.arguments[0]);
            cs[Int32.Parse(rmsg.from) - 8082].postMessage(cmsg);
        }

        /*Send source files to child builder*/
        private static void sendFiles_method()
        {
            string dir = rmsg.arguments[0];
            foreach (string filename in rmsg.arguments)
            {
                Console.WriteLine("\nCopying File " + filename + " to ChildBuilder" + (Int32.Parse(rmsg.from) - 8081) + "Storage");
                cs[Int32.Parse(rmsg.from) - 8082].postFile(filename, "../../../ChildBuilder" + (Int32.Parse(rmsg.from) - 8081) + "Storage/" + dir, "../../../RepoStorage");
            }
            CommMessage cmsg = new CommMessage(CommMessage.MessageType.reply);
            cmsg.command = "BuildCodes";
            cmsg.to = "http://localhost:" + rmsg.from + "/IPluggableComm";
            cmsg.from = "Repo";
            cmsg.arguments.Add(dir);
            cs[Int32.Parse(rmsg.from) - 8082].postMessage(cmsg);
        }

        /*send source file names to client GUI*/
        private static void GetSourceFiles_method()
        {
            getFilesHelper("../../../RepoStorage", "*.cs");
            CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
            reply.to = "http://localhost:8079/IPluggableComm";
            reply.from = "8081";
            reply.command = "displaysourcefiles";
            reply.arguments = files;
            gui.postMessage(reply);
        }

        /*send xml file names to client GUI*/
        private static void GetXmlFiles_method()
        {
            getFilesHelper("../../../RepoStorage", "*.xml");
            CommMessage reply2 = new CommMessage(CommMessage.MessageType.reply);
            reply2.to = "http://localhost:8079/IPluggableComm";
            reply2.from = "8081";
            reply2.command = "displayxmlfiles";
            reply2.arguments = files;
            gui.postMessage(reply2);
        }

        /*send connection message to harness*/
        private static void ConnectHarness()
        {
            CommMessage tmsg = new CommMessage(CommMessage.MessageType.reply);
            tmsg.command = "Connected";
            tmsg.author = "Client";
            tmsg.to = "http://localhost:8078/IPluggableComm";
            tmsg.from = "8081";
            harness.postMessage(tmsg);
        }

        /*Start sender objects of child builders*/
        private static void spawnchilds_method()
        {
            scount = Int32.Parse(rmsg.arguments[0]);
            cs = new Sender[scount];
            for (int i = 0; i < scount; i++)                           // Initaialize sender for all child servers
            {
                cs[i] = new Sender("http://localhost", 8082 + i);
            }
        }

        /*Delete sender objects of child builders*/
        private static void DeleteSenders_method()
        {
            CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
            for (int i = 0; i < scount; i++)
            {
                cs[i].postMessage(closeS);                          // Close all senders

            }
        }

        /*process incoming message and take suitable action*/
        private static void processmessage()
        {                                                        //Process the received messages
            switch (rmsg.command)
            {
                case "Xml":
                    {                                                  
                        Xml_method();                           //If message command is xml start a connection
                        break;
                    }
                case "sendXml":                                 //If message command is sendXml, send the xml file
                    {
                        rmsg.show();
                        sendxml_method();
                        break;
                    }
                case "sendFiles":
                    {                                           //If message command is sendFiles, send the source files 
                        rmsg.show();
                        sendFiles_method();
                        break;
                    }
                case "GetSourceFiles":
                    {
                        GetSourceFiles_method();
                        break;
                    }
                case "GetXmlFiles":
                    {
                        GetXmlFiles_method();
                        break;
                    }
                case "ConnectHarness":
                    {
                        ConnectHarness();
                        break;
                    }
                case "spawnchilds":
                    {
                        spawnchilds_method();
                        break;
                    }
            }
        }

        /*Helper method to get file names from repository*/
        private static void getFilesHelper(string path,string type)
        {
            files.Clear();
            try
            {
                string[] tempFiles = Directory.GetFiles(path, type);
                foreach (string f in tempFiles)
                {
                    string name = System.IO.Path.GetFileName(f);
                    files.Add(name);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to retrieve filenames");
            }             
        }

        /*Close all repo's sender objects*/
        private static void closeRepo_method()
        {
            CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
            gui.postMessage(closeS);
            harness.postMessage(closeS);
            string dir = Path.GetFullPath("../../../RepoStorage");
            try
            {
                if (System.IO.Directory.Exists(dir))
                {                                                  // Delete al newly created testrequests
                    string[] tempfiles = Directory.GetFiles(dir, "*.xml");
                    foreach (string f in tempfiles)
                    {
                        string fi = Path.GetFullPath(f);
                        System.IO.File.Delete(fi);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to delete RepoStorage...");
            }           
        }

        static void Main(string[] args)
        {
            if (args.Count() != 0)
            {
                Console.Write("\n *******Repository started listening on port 8081******* \n");
                try
                {
                    rrcvr.start("http://localhost", 8081);
                }
                catch (Exception)
                {
                    Console.WriteLine("\nUnable to start Mock Repo receiver port or port already in use...");
                }
                
                gui = new Sender("http://localhost", 8079);         //Start sender objects
                harness = new Sender("http://localhost", 8078);
                while (true)
                {
                    rmsg = rrcvr.getMessage();                                  // Continuously get messages received at the receiver port
                    if (rmsg.type == CommMessage.MessageType.closeReceiver)     
                    {
                        rmsg.show();
                        closeRepo_method();                       
                        break;
                    }
                    else if (rmsg.command == "DeleteSenders")
                    {
                        rmsg.show();
                        DeleteSenders_method();
                    }
                    else
                    {
                        processmessage();
                    }
                    
                }
            }
        }
    }
}
