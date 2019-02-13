using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System;
using System.Reflection;

public class Loader
{

public static void Main() {
loadFile("lazyLoaded.cs");
}

  static void loadFile(string fileName) {
    string SourceString = System.IO.File.ReadAllText(fileName);
    CSharpCodeProvider codeProvider = new CSharpCodeProvider();
    ICodeCompiler icc = codeProvider.CreateCompiler();

    System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
    parameters.GenerateInMemory = true;
    CompilerResults results = icc.CompileAssemblyFromSource(parameters,SourceString);

    if (results.Errors.Count > 0)
    {
string error = "";
      foreach(CompilerError CompErr in results.Errors)
      {
	error = "Line number " + CompErr.Line + 
	  ", Error Number: " + CompErr.ErrorNumber + 
	  ", '" + CompErr.ErrorText + ";" + 
	  Environment.NewLine + Environment.NewLine;
      }
Console.Write(error);
    }

    execute(results.CompiledAssembly);
  }

  static void execute(Assembly ass) {
    Type[] types = ass.GetTypes();

    foreach(Type t in types) { Console.Write("" + t.ToString()); }
    Object o = ass.CreateInstance("LazyClass", true);

if(o == null) return;
    MethodInfo m = types[0].GetMethod("Start");
if(m==null) return;
    Object ret = m.Invoke(o, new Object[0]);
    //Console.WriteLine("SampleMethod returned {0}.", ret);

    Console.WriteLine("\nAssembly entry point:");
    Console.WriteLine(ass.EntryPoint);

  }
}
