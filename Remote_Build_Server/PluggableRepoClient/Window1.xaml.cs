////////////////////////////////////////////////////////////////////////
// Window1.xaml.cs - Client GUI for Creating and Sending Test Requests//
// Ver 1.1                                                            //
// Author: Vikrant Bhopatrao                                          //
// Application: CSE681-Software Modeling and Analysis Project 4       //
// Environment: Windows Application                                   //
////////////////////////////////////////////////////////////////////////
/*  
*    Purpose:
*    Receive input from the client for the no. of child servers to be spawned.
*    Select source files to build test request.
*    elect a test request to be built.
*    
*    Public Interface:
*    Window1(); - Constructor for window1.
*    
*   Required Files:
*     Window1.xaml, Window1.xaml.cs, guiprocessor.cs,
*     IMPCommService.cs, MPCommService.cs, testrequest.cs
*  
*   Maintenance History:
*     Ver 1.1 : 04 Dec 2017 
*    - first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using MessagePassingComm;
using Xml;
using GUIprocessor;
using System.Threading;

namespace PluggableRepoClient
{
    public partial class Window1 : Window
    {
        private static int port;
        private static double leftOffset = 500.0;
        private static double topOffset = -20.0;
        private int rcount = 0;
        private static Receiver client = new Receiver();
        private static guiprocessor gp = new guiprocessor();
        private static Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        private static Thread rcvThread = null;
        private static List<string> temp=new List<string>();

        public Window1()
        {
            InitializeComponent();
            gp.createBuilderRepoHarnessSender();
            try
            {
                client.start("http://localhost", 8079);
            }
            catch (Exception)
            {
                Console.WriteLine("\nUnable to start Client Receiver Port or Client aleady running...");
            }
            testexec();
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            try
            {
                gp.getSourceFiles();
            }
            catch
            {
                Console.WriteLine("\nDirectory not found...");
            }
            temp.Add(" ");
            double Left = Application.Current.MainWindow.Left;
            double Top = Application.Current.MainWindow.Top;
            this.Left = Left + leftOffset;
            this.Top = Top + topOffset;
            this.Width = 800.0;
            this.Height = 600.0;
            leftOffset += 20.0;
            topOffset += 20.0;
            if (leftOffset > 700.0)
                leftOffset = 500.0;
            if (topOffset > 180.0)
                topOffset = -20.0;
        }

        void initializeMessageDispatcher()
        {
            // load testdriver and tested files listboxes with files from repository

            messageDispatcher["displaysourcefiles"] = (CommMessage msg) =>
            {
                Console.WriteLine("\nRequirement: 11");
                testdriver_list.Items.Clear();
                tested_list.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    Console.WriteLine("\n"+file);
                    testdriver_list.Items.Add(file);
                    tested_list.Items.Add(file);                    // Add file names to the list boxes
                }
            };
            messageDispatcher["displayxmlfiles"] = (CommMessage msg) =>
            {
                testrequest_list.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    testrequest_list.Items.Add(file);               // Add file names to the list boxes
                }
            };
        }

        //----< define processing for GUI's receive thread >-------------
        void rcvThreadProc()
        {
            while (true)
            {
                CommMessage msg = client.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        /* Delay method */
        async Task PutTaskDelay1()
        {
            await Task.Delay(700);
        }
        async Task PutTaskDelay2()
        {
            await Task.Delay(9000);
        }
        async Task PutTaskDelay3()
        {
            await Task.Delay(3000);
        }

        /* Spawn a process passing an argument provided by the client*/
        bool createProcess(string pname)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\" + pname;
            string absFileSpec = Path.GetFullPath(fileName);
            string commandline = "null";
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

        /* Spawn the mother builder and pass no. of childs as argument when "Start Builder" button is pressed*/
        private void entered_childs_Button_Click(object sender, RoutedEventArgs e)
        {           
             entered_childs_Button_Click_method(temp,false);
        }

        /* Send message to close all child builders*/
        private void exit_childs_Button_Click(object sender, RoutedEventArgs e)
        {
            exit_childs_Button_Click_method(temp,false);
        }

        /* send a test request name to the mother builder*/
        private void buildButton_Click(object sender, RoutedEventArgs e)
        {
            buildButton_Click_method(temp,false);
        }

        /* To enable and disable different buttons depending on the input */
        private void testdriver_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (testdriver_list.SelectedItem != null)
                buildXml.IsEnabled = true;
            else
                buildXml.IsEnabled = false;
            if (testrequest_list.SelectedItem != null && testdriver_list.SelectedItem != null)
                editXml.IsEnabled = true;
            else
                editXml.IsEnabled = false;
        }

        /* Close all server windows if GUI window is closed */
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rcvThread.Abort();
            Thread T1 = new Thread(gp.closeAll);
            T1.Start(port);
            Notifier popup = new Notifier();
            popup.displayText.Text = "Please wait for a few seconds for all the processes to close";
            popup.Show();
            await PutTaskDelay2();
            popup.Close();
            T1.Join();
        }

        private void entered_childs_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tested_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            createProcess("MockRepo\\bin\\Debug\\MockRepo.exe");
            createProcess("MotherBuilder\\bin\\Debug\\MotherBuilder.exe");
            createProcess("MockTestHarness\\bin\\Debug\\MockTestHarness.exe");           
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        /*Build xml file from the selected source files*/
        private void buildXml_Click(object sender, RoutedEventArgs e)
        {
            buildXml_Click_method(temp,false);           
        }

        /*edit xml file from the selected source file and xml files*/
        private void editXml_Click(object sender, RoutedEventArgs e)
        {
            editXml_Click_method(temp,false);            
        }

        /* To enable and disable different buttons depending on the input */
        private void testrequest_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (testrequest_list.SelectedItem != null && testdriver_list.SelectedItem != null)
                editXml.IsEnabled = true;
            else
                editXml.IsEnabled = false;
            if (testrequest_list.SelectedItem != null && entered_childs_Button.IsEnabled == false)
                buildButton.IsEnabled = true;
            else
                buildButton.IsEnabled = false;
        }

        /*Helper method for "entered_child_button_click" event*/
        private async void entered_childs_Button_Click_method(List<string>s,bool test)
        {
            bool t;
            if (test)
            {
                port = Int32.Parse(s[0]);
                t = true;
            }
            else {
                t = Int32.TryParse(entered_childs.Text, out port);
            }
            if (t)
            {
                gp.toggleRunning(true);
                gp.createChildSenders(port);
                exit_childs_Button.IsEnabled = true;
                entered_childs_Button.IsEnabled = false;
                if (testdriver_list.SelectedItem != null)
                    buildButton.IsEnabled = true;
                gp.spawnChilds(port);
                
                Notifier popup = new Notifier();
                popup.displayText.Text = "Please wait for a few seconds for all the child builders to spawn";
                popup.Show();
                await PutTaskDelay3();
                popup.Close();
            }
        }

        /*Helper method for "exit_childs_Button_Click" event*/
        private async void exit_childs_Button_Click_method(List<string> s, bool test)
        {
            gp.toggleRunning(false);
            exit_childs_Button.IsEnabled = false;
            entered_childs_Button.IsEnabled = true;
            buildButton.IsEnabled = false;
            gp.closePoolProcess(port);                       // call closeall method to send the messages
            Notifier popup = new Notifier();
            popup.displayText.Text = "Please wait for a few seconds for all the child builders to close";
            popup.Show();
            await PutTaskDelay2();
            popup.Close();
        }

        /*Helper method for "buildButton_Click" event*/
        private async void buildButton_Click_method(List<string> s, bool test)
        {
            string testname;
            if (test)
                testname = s[0];              
            else
            testname = testrequest_list.SelectedItem.ToString();
            gp.buildRequest(testname);
            buildButton.IsEnabled = false;
            await PutTaskDelay1();
            if(testrequest_list.SelectedItem!=null)
            buildButton.IsEnabled = true;

        }

        /*Helper method for "buildXml_Click" event*/
        private void buildXml_Click_method(List<string> s, bool test)
        {
            rcount++;
            TestRequest tr = new TestRequest();
            tr.author = "Client";
            if (test)
            {
                tr.testDriver = s[0];
                tr.testedFiles.Add(s[1]);
                tr.testedFiles.Add(s[2]);
                tr.testedFiles.Add(s[3]);
            }
            else
            {
                tr.testDriver = testdriver_list.SelectedItem.ToString();
                foreach (string si in tested_list.SelectedItems)
                {
                    tr.testedFiles.Add(si);
                }
            }
            tr.makeRequest();                                                   //Create Test Request
            try
            {
                Console.WriteLine("\nRequirement: 11");
                Console.WriteLine("\nBuilding XML file");
                Console.WriteLine("\n{0}\n", tr.doc.ToString());
                tr.saveXml("../../../ClientStorage", "TestRequest" + rcount + ".xml");     //Save test request in RepoStorage
                Console.WriteLine("\nRequirement: 12");
                Console.WriteLine("\nSending TestRequest" + rcount + ".xml file to RepoStorage");
                gp.sendXml("TestRequest" + rcount + ".xml");
                gp.getXmlFiles();
            }
            catch (Exception)
            {
                Console.WriteLine("\nCould not create test request...");
            }
        }

        /*Helper method for "editXml_Click" event*/
        private void editXml_Click_method(List<string> s, bool test)
        {
            TestRequest tr = new TestRequest();
            try
            {
                if (test)
                {
                    tr.loadXml("../../../RepoStorage", s[0]);
                    tr.testDriver = s[1];
                    tr.testedFiles.Add(s[2]);
                    tr.testedFiles.Add(s[3]);
                    tr.testedFiles.Add(s[4]);
                }
                else
                {
                    tr.loadXml("../../../RepoStorage", testrequest_list.SelectedItem.ToString());
                    tr.testDriver = testdriver_list.SelectedItem.ToString();
                    foreach (string si in tested_list.SelectedItems)
                    {
                        tr.testedFiles.Add(si);
                    }
                }
                tr.editRequest();
                if (test)
                {
                    tr.saveXml("../../../ClientStorage", s[0]);     //Save test request in RepoStorage
                    gp.sendXml(s[0]);
                }
                else
                {
                    tr.saveXml("../../../ClientStorage", testrequest_list.SelectedItem.ToString());     //Save test request in RepoStorage
                    gp.sendXml(testrequest_list.SelectedItem.ToString());
                }
                Console.WriteLine("\nRequirement: 11");
                Console.WriteLine("\nAdding element to an existing test request");
                Console.WriteLine("\n{0}\n", tr.doc.ToString());
                gp.getXmlFiles();
            }
            catch (Exception)
            {
                Console.WriteLine("\nCould not load the selected TestRequest");
            }
        }

        /*Delay method*/
        async Task PutTaskDelay4()
        {
            await Task.Delay(1500);
        }

        /*Test function used to initially automate the build process*/
        private async void testexec()
        {
            await PutTaskDelay3();
            List<string> names = new List<string>();
            names.Add("3");
            entered_childs_Button_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestDriver1.cs");
            names.Add("TestedLib1.cs");
            names.Add("TestedLib2.cs");
            names.Add("TestedLib3.cs");
            buildXml_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestDriver2.cs");
            names.Add("TestedLib4.cs");
            names.Add("TestedLib5.cs");
            names.Add("TestedLib6.cs");
            buildXml_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestDriver1.cs");
            names.Add("TestedLib1.cs");
            names.Add("TestedLib2.cs");
            names.Add("TestedLib3.cs");
            buildXml_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestRequest3.xml");
            names.Add("TestDriver2.cs");
            names.Add("TestedLib4.cs");
            names.Add("TestedLib5.cs");
            names.Add("TestedLib6.cs");
            editXml_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestRequest1.xml");
            buildButton_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestRequest2.xml");
            buildButton_Click_method(names,true);
            await PutTaskDelay4();
            names.Clear();
            names.Add("TestRequest3.xml");
            buildButton_Click_method(names,true);
        }
    }  
}
