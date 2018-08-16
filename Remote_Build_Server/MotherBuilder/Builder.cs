/////////////////////////////////////////////////////////////////////
// Builder.cs -Spawns and Controls all the child builders          //
//                                                                 //
// Author: Vikrant Bhopatrao                                       //
// Application: CSE681-Software Modeling and Analysis Project 4    //
// Environment: C# console                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * 1. Spawns child builders to build testrequests
 * 2. Generate blocking queues for test requests and ready messages
 * 3. Close all child builders on request from client
 * 4. Dequeue messages from queue and send them to child builders
 * 
 * Public Interface:
 * -----------------
 * -None
 * 
 * Required Files:
 * ---------------
 * Builder.cs
 * MPCommService.cs
 * IMPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 02 Dec 2017
 * - first release
 * 
 */

using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using MessagePassingComm;
using System.Threading;
using System.Runtime.InteropServices;

namespace MotherBuilder
{
    class Builder
    {       
        private static SWTools.BlockingQueue<string> readyQ = new SWTools.BlockingQueue<string>();  //Two queues to store ready messages and test requests
        private static SWTools.BlockingQueue<string> requestQ = new SWTools.BlockingQueue<string>();
        private static Receiver rcvr = new Receiver();          //Receiver object
        private static CommMessage rcvMsg;
        private static Sender[] bs;                             // An array of sender objects, one for each child
        private static int childCount=0;

        /*To get messages from client and child builder and store them in respective queues*/
        private static void CreateQueues()
        {
            try
            {
                rcvr.start("http://localhost", 8080);                              // Starts the receiver object 
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to start Mother builder receiver port or port already in use...");
            }              
            while (true)
            {
                rcvMsg = rcvr.getMessage();
                if (rcvMsg.from != null)
                {
                    rcvMsg.show();
                    if (rcvMsg.from == "GUI")
                    {
                        if (rcvMsg.type == CommMessage.MessageType.closeReceiver)
                        {                                                        //if closeReceiver message type is received then break from loop
                            break;
                        }
                        else
                        processGUI();                      
                    }
                    else
                    {
                        readyQ.enQ(rcvMsg.from);
                        Console.WriteLine("\nEnqueing message to Ready Queue");
                    }
                }
            }
        }

        /*Process messages received grom gui*/
        private static void processGUI()
        {
            if (rcvMsg.command == "DeleteSenders")
            {
                while (readyQ.size() != 0)
                {
                    readyQ.deQ();
                }
                CloseServers();
            }
            
            /*Spawn pool processes depending on the input received*/ 
            else if (rcvMsg.command == "spawnchilds")
            {
                childCount = Int32.Parse(rcvMsg.arguments[0]);
                Console.WriteLine("\nRequirement: 5");
                Console.WriteLine("\nCreating "+childCount+" child builder processes");
                bs = new Sender[childCount];
                for (int p = 1; p <= childCount; p++)
                {
                    createProcess(8081 + p, "ChildBuilder\\bin\\debug\\ChildBuilder.exe");      // Spawn all the child builders
                    bs[p - 1] = new Sender("http://localhost", 8081 + p);                                                                //bs[p - 1] = new Sender("http://localhost", 8081 + p);
                }
            }
            else
            {
                requestQ.enQ(rcvMsg.arguments[0]);
                Console.WriteLine("\nEnqueing message to TestRequest Queue");
            }
        }

        /*To dequeue a ready port and a test request and send a message to child builder*/
        private static void CreateBuildRequests()
        {
            while (true)
            {
                string portName = readyQ.deQ();                     // Dequeue port number
                string request = requestQ.deQ();                    // Dequeue test request
                CommMessage sndMsg = new CommMessage(CommMessage.MessageType.reply);
                sndMsg.command = "Parse";
                sndMsg.author = "Vikrant";
                sndMsg.to = "http://localhost:" + portName + "/IPluggableComm";
                sndMsg.from = "Mother Builder";
                sndMsg.arguments.Add(request);
                try
                {
                    bs[Int32.Parse(portName) - 8082].postMessage(sndMsg);   // Send message
                    Console.WriteLine("\nSending Message to Child Builder " + (Int32.Parse(portName) - 8081));
                    Console.WriteLine("\nTestRequestName: " + request);
                }
                catch
                {
                    Console.WriteLine("\nChild Builder is Closed");
                }
            }
        }

        /* To Spawn child builders*/
        private static bool createProcess(int i, string pname)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\" + pname;
            string absFileSpec = Path.GetFullPath(fileName);
            Console.Write("\n  Attempting to start {0}", absFileSpec);
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        /* To Close all the sender objects of child builders*/
        private static void CloseServers()
        {
            CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
            for (int p = 1; p <= childCount; p++)
            {
                Console.WriteLine("\nClosing Sender Port for Child Builder Port: " + (8081 + p));
                bs[p - 1].postMessage(closeS);
            }
            bs = null;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("******Mother Builder started listening on Port 8080******");
            if (args.Count() != 0)
            {              
                Thread QThread = new Thread(CreateQueues);          // Initialize a new thread to get messages and generate queues
                QThread.Start();
        
                Thread BThread = new Thread(CreateBuildRequests);   // Initialize thread to dequeue messages
                BThread.Start();

                QThread.Join(); // Wait till all threads close 
                BThread.Abort();
            }
        }
    }
}
