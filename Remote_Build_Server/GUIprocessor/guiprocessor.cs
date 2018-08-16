//////////////////////////////////////////////////////////////////////////
// guiprocessor.cs - Helper class to Client GUI Events                  //
//                                                                      //
// Author: Vikrant Bhopatrao (Email: vsbhopat@syr.edu)                  //
// Application: CSE681-Software Modeling and Analysis Project 4         //
// Environment: Class Library                                           //
//////////////////////////////////////////////////////////////////////////
/*  
*   Purpose:
*   Gui helper to send messages and files. 
*   
*   Public Interface:
*   -----------------
*   public void toggleRunning(bool b);- To toggle childer builder running state
*   public void createChildSenders(int n);- to initialize sender objects
*   public void createBuilderRepoHarnessSender();- to initialize sender objects
*   public async void closeAll(object n);- to close all processes
*   public async void closePoolProcess(int n);- close child builder processes
*   public void getSourceFiles(); -receive source file names frmo repo
*   public void buildRequest(string request); -send test request to mother builder
*   public async void spawnChilds(int no); -send spawn child request to mother builder
*   public void getXmlFiles();- to get xml files from repo
*   public void sendXml(string name); -send build request file to repo
*  
*   Required Files:
*   ---------------
*   MPCommService.cs
*   IMPCommService.cs
*  
*   Maintenance History:
*   Ver 1.1 : 04 Dec 2017 
*    - first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using System.Diagnostics;
using System.Threading;


namespace GUIprocessor
{
    public class guiprocessor
    {
        private static Sender grepo;
        private static Sender gbuilder;
        private static Sender gharness;
        private static Sender[] gchild;
        private static bool running = false;

        /*To toggle childer builder running state*/
        public void toggleRunning(bool b)
        {
            running = b;
        }

        /*to initialize sender objects*/
        public void createChildSenders(int n)
        {
            gchild = new Sender[n];
            for (int i = 1; i <= n; i++)
            {
                gchild[i - 1] = new Sender("http://localhost", 8081 + i);
            }
        }

        /*to initialize sender objects*/
        public void createBuilderRepoHarnessSender()
        {
            gbuilder = new Sender("http://localhost", 8080);
            grepo = new Sender("http://localhost", 8081);
            gharness= new Sender("http://localhost", 8078);
        }

        /* Delay method used to introduce a delay */
        async Task PutTaskDelay()
        {
            await Task.Delay(800);
        }

        /*close all processes and their sender objects*/
        public async void closeAll(object n)
        {
            CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
            CommMessage closeR = new CommMessage(CommMessage.MessageType.closeReceiver);
            closeR.command = "CloseServer";
            closeR.author = "Client";
            closeR.from = "GUI";
            int n2 = Convert.ToInt32(n);
            if(running)
            closePoolProcess(n2);           
            closeR.to = "http://localhost:8080/IPluggableComm";
            await PutTaskDelay();
            try
            {
                gbuilder.postMessage(closeR);
                gbuilder.postMessage(closeS);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mother Builder or process already closed...");
            }
            await PutTaskDelay();
            closeR.to = "http://localhost:8081/IPluggableComm";
            try
            {
                grepo.postMessage(closeR);
                grepo.postMessage(closeS);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock Repo or process already closed...");
            }           
            await PutTaskDelay();        
            closeR.to = "http://localhost:8078/IPluggableComm";
            try
            {
                gharness.postMessage(closeR);
                gharness.postMessage(closeS);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock TestHarness or process already closed...");
            }           
        }

        /*close child builder processes*/
        public async void closePoolProcess(int n)
        {
            CommMessage closeS = new CommMessage(CommMessage.MessageType.closeSender);
            CommMessage closeChilds = new CommMessage(CommMessage.MessageType.closeReceiver);
            CommMessage ClearSenders = new CommMessage(CommMessage.MessageType.request);
            ClearSenders.command = "DeleteSenders";
            ClearSenders.author = "Client";
            ClearSenders.to = "http://localhost:8080/IPluggableComm";
            ClearSenders.from = "GUI";
            try
            {
                gbuilder.postMessage(ClearSenders);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mother Builder or process already closed...");
            }           
            await PutTaskDelay();
            ClearSenders.to = "http://localhost:8081/IPluggableComm";
            try
            {
                grepo.postMessage(ClearSenders);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock Repo or process already closed...");
            }         
            await PutTaskDelay();
            for (int p = 1; p <= n; p++)
            {
                closeChilds.to = "http://localhost:" + (8081 + p) + "/IPluggableComm";
                try
                {
                    gchild[p - 1].postMessage(closeChilds);
                    gchild[p - 1].postMessage(closeS);
                }
                catch
                {
                    Console.WriteLine("\nChild Builder is already closed...");
                }              
                await PutTaskDelay();
            }
        }

        /*-receive source file names frmo repo*/
        public void getSourceFiles()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = "GUI";
            msg1.to = "http://localhost:8081/IPluggableComm";
            msg1.author = "Client";
            msg1.command = "GetSourceFiles";
            msg1.arguments.Add("");
            try
            {
                grepo.postMessage(msg1);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock Repo or process already closed...");
            }         
        }

        /*send build request file to repo*/
        public void sendXml(string name)
        {
            try
            {
                grepo.postFile(name, "../../../RepoStorage", "../../../ClientStorage");
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to store  test request in RepoStorage...");
            }         
        }

        /*to get xml files from repo*/
        public void getXmlFiles()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = "GUI";
            msg1.to = "http://localhost:8081/IPluggableComm";
            msg1.author = "Client";
            msg1.command = "GetXmlFiles";
            msg1.arguments.Add("");
            try
            {
                grepo.postMessage(msg1);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock Repo or process already closed...");
            }
        }

        /*send spawn child request to mother builder*/
        public async void spawnChilds(int no)
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "spawnchilds";
            sndMsg.author = "Client";
            sndMsg.to = "http://localhost:8080/IPluggableComm";
            sndMsg.from = "GUI";
            sndMsg.arguments.Clear();
            sndMsg.arguments.Add(no.ToString());              // Add created test request name to arguments
            try
            {
                gbuilder.postMessage(sndMsg);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mother Builder or process closed...");
            }        
            await PutTaskDelay();
            sndMsg.to = "http://localhost:8081/IPluggableComm";
            try
            {
                grepo.postMessage(sndMsg);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mock Repo or process closed...");
            }       
        }

        /*end test request to mother builder*/
        public void buildRequest(string request)
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "show";
            sndMsg.author = "Client";
            sndMsg.to = "http://localhost:8080/IPluggableComm";
            sndMsg.from = "GUI";
            sndMsg.arguments.Clear();
            sndMsg.arguments.Add(request);              // Add created test request name to arguments
            try
            {
                Console.WriteLine("\nRequirement: 13");
                Console.WriteLine("\nSending " + request + " to Build Server");
                gbuilder.postMessage(sndMsg);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to connect to Mother Builder or process closed...");
            }        
        }

#if (TEST_GUIHELPER)
        static void Main(string[] args)
        {
            guiprocessor gp = new guiprocessor();
            gp.createChildSenders(3);
            Console.WriteLine("\nStarting child builder sender objects");
            gp.createBuilderRepoHarnessSender();
            Console.WriteLine("\nStarting all other sender objects");
            gp.sendXml("testrequest.xml");
            Console.WriteLine("\nSending Testrequest.xml to RepoStorage");
            gp.spawnChilds(3);
            Console.WriteLine("Sending message to mother builder to spawn 3 child builders");
            gp.toggleRunning(true);
            Console.WriteLine("\nClosing all Receivers");
            gp.closeAll(3);
            Console.WriteLine("\nAll Servers Closed...");
        }
#endif
    }
}
