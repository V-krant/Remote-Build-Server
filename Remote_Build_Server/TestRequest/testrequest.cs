/////////////////////////////////////////////////////////////////////
// testrequest.cs - build and parse test requests                  //
//                                                                 //
// Author: Vikrant Bhopatrao                                       //
// Application: CSE681-Software Modeling and Analysis Project 3    //
// Environment: C# console                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Creates and parses TestRequest XML messages using XDocument
 * 
 * Public Interface:
 * -----------------
 *  public void createPath(string server,string requestName); -Create storage path
 *  public void makeRequest(); -Build xml file
 *  public void editRequest(); -edit existing xml file
 *  public bool loadXml(string server, string requestName); -Load xml file from storage
 *  public bool saveXml(string server,string requestName); -save xml file to storage
 *  public List<List<string>> parse(); -parse an xml file
 *  public void parseDll(); -parse just the testdriver name
 * 
 * Required Files:
 * ---------------
 * TestRequest.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 01 Dec 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xml
{
    ///////////////////////////////////////////////////////////////////
    // TestRequest class

    public class TestRequest
    {
        public string fileSpec { get; set; } = "";
        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";
        public string testDriver { get; set; } = "";
        public List<string> testedFiles { get; set; } = new List<string>();
        public XDocument doc { get; set; } = new XDocument();

        /*----< build XML document that represents a test request >----*/
        public void createPath(string server, string requestName)
        {

            string savePath = server;
            string fileName = requestName;
            if (!System.IO.Directory.Exists(savePath))
                System.IO.Directory.CreateDirectory(savePath);
            fileSpec = System.IO.Path.Combine(savePath, fileName);
            fileSpec = System.IO.Path.GetFullPath(fileSpec);
        }
        public void makeRequest()
        {

            XElement testRequestElem = new XElement("testRequest");
            doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            XElement testElem = new XElement("test");
            testRequestElem.Add(testElem);

            XElement driverElem = new XElement("testDriver");
            driverElem.Add(testDriver);
            testElem.Add(driverElem);

            foreach (string file in testedFiles)
            {
                XElement testedElem = new XElement("tested");
                testedElem.Add(file);
                testElem.Add(testedElem);
            }
        }

        /*edit an existing test request*/
        public void editRequest()
        {
            XElement testElem = new XElement("test");

            XElement driverElem = new XElement("testDriver");
            driverElem.Add(testDriver);
            testElem.Add(driverElem);

            foreach (string file in testedFiles)
            {
                XElement testedElem = new XElement("tested");
                testedElem.Add(file);
                testElem.Add(testedElem);
            }
            doc.Root.Add(testElem);
        }

        /*----< load TestRequest from XML file >-----------------------*/

        public bool loadXml(string server, string requestName)
        {
            createPath(server, requestName);
            try
            {
                doc = XDocument.Load(fileSpec);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--{0}--\n", ex.Message);
                return false;
            }
        }
        /*----< save TestRequest to XML file >-------------------------*/

        public bool saveXml(string server, string requestName)
        {
            createPath(server, requestName);
            Console.WriteLine("\nsaving to \"{0}\"\n", fileSpec);
            try
            {
                if (System.IO.File.Exists(fileSpec))
                    System.IO.File.Delete(fileSpec);
                doc.Save(fileSpec);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--{0}--\n", ex.Message);
                return false;
            }
        }
        /*----< parse document for property value >--------------------*/

        public List<List<string>> parse()
        {
            List<List<string>> parsedvalues = new List<List<string>>();
            IEnumerable<XElement> parseElems = doc.Descendants("test");
            foreach (XElement elem in parseElems)
            {
                List<string> values = new List<string>();
                string parseStr = elem.Descendants("testDriver").First().Value;
                values.Add(parseStr);
                IEnumerable<XElement> testedfiles = elem.Descendants("tested");
                if (testedfiles.Count() > 0)
                {
                    foreach (XElement test in testedfiles)
                    {
                        values.Add(test.Value);
                    }
                }
                parsedvalues.Add(values);
            }
            return parsedvalues;
        }

        public void parseDll()
        {
            string parseStr = doc.Descendants("testDriver").First().Value;
            if (parseStr.Length > 0)
                testDriver = parseStr;
        }
    }

#if (TEST_TESTREQUEST)
        class Test_TestRequest
    {

        static void Main(string[] args)
        {
            Console.Write("\n  Testing TestRequest");
            Console.Write("\n =====================");

            TestRequest tr = new TestRequest();
            tr.author = "Vikrant Bhopatrao";
            tr.testDriver = "td1.cs";
            tr.testedFiles.Add("tf1.cs");
            tr.testedFiles.Add("tf2.cs");
            tr.testedFiles.Add("tf3.cs");
            tr.makeRequest();
            Console.Write("\n{0}", tr.doc.ToString());

            tr.saveXml("../../../RepoStorage", "TestRequest.xml");

            TestRequest tr2 = new TestRequest();
            tr2.loadXml("../../../RepoStorage", "TestRequest.xml");

            Console.Write("\n{0}", tr2.doc.ToString());
            Console.Write("\n");
            tr2.parse();
            Console.WriteLine("\n{0}\n", tr2.doc.ToString());
            Console.Write("\n\n");
        }
  }
#endif
}

