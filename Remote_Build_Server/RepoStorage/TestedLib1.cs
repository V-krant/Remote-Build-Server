///////////////////////////////////////////////////////////////////////////
// TestedLIb.cs - Simulates operation of a tested package                //
//                                                                       //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017       //
///////////////////////////////////////////////////////////////////////////

using System;

namespace DllLoaderDemo
{
  public class Tested : ITested
  {
    public Tested()
    {
      Console.Write("\n    constructing instance of Tested");
    }
    public void say()
    {
      Console.Write("\n    Production code - TestedLib");
      TestedLibDependency tld = new TestedLibDependency();
      tld.sayHi();
    }
  }
}
