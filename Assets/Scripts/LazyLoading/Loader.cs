using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using Microsoft.CSharp;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using ProceduralNoiseProject;

public class Loader
{
    public static Action<Assembly> listener = file => { };
    private static FileSystemWatcher watcher;
    public static Assembly currentAssembly;
    private static string srcDir = Path.Combine(Directory.GetCurrentDirectory(), "src");

    static Loader()
    {
#if (UNITY_EDITOR)
        watcher = new FileSystemWatcher();
        // only LazyLoad when in editor
        watch();
        loadFile("");
#endif
    }

    public static void watch()
    {
        Directory.CreateDirectory(srcDir);
        watcher.Path = srcDir;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "*.cs";
        watcher.Changed += new FileSystemEventHandler(FileChanged);
        watcher.EnableRaisingEvents = true;
    }

    private static bool x = true;

    public static void FileChanged(object source, FileSystemEventArgs args)
    {
        if (x)
        {
            Debug.Log("File Changed: " + args.FullPath);
            loadFile(args.FullPath);
        }
        x = true;
    }

    static void loadFile(string fileName)
    {
        AppDomain domain = AppDomain.CurrentDomain;
        Assembly[] assemblies = domain.GetAssemblies();
        string[] fileNames = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories);

        CSharpCodeProvider codeProvider = new CSharpCodeProvider();

        System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateInMemory = true;
        /*
        IEnumerable<Assembly> filteredAssemblies = assemblies
          .Where(a => !a.IsDynamic && !String.IsNullOrEmpty(a.Location))
          .Distinct();
        foreach(Assembly assembly in filteredAssemblies) {
          Debug.Log("adding assembly: " + assembly.FullName);
          parameters.ReferencedAssemblies.Add(assembly.Location);
        }
        */
        parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(Noise)).Location);
        parameters.ReferencedAssemblies.Add(Assembly.GetAssembly(typeof(Mathf)).Location);
        Debug.Log("compiling files: " + string.Join(";", fileNames) + " " + fileNames.Length);
        CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, fileNames);
        Debug.Log("compiling finsihed");

        if (results.Errors.Count > 0)
        {
            string error = "";
            foreach (CompilerError CompErr in results.Errors)
            {
                error = "Line number " + CompErr.Line +
                  ", Error Number: " + CompErr.ErrorNumber +
                  ", '" + CompErr.ErrorText + ";" +
                  Environment.NewLine + Environment.NewLine;
            }
            Debug.LogError(error);
        }

        currentAssembly = results.CompiledAssembly;
        listener(results.CompiledAssembly);
    }

    static void execute(Assembly ass)
    {
        Type[] types = ass.GetTypes();

        foreach (Type t in types) { Console.Write("" + t.ToString()); }
        System.Object o = ass.CreateInstance("LazyClass", true);

        if (o == null) return;
        MethodInfo m = types[0].GetMethod("Start");
        if (m == null) return;
        System.Object ret = m.Invoke(o, new System.Object[0]);
        //Console.WriteLine("SampleMethod returned {0}.", ret);

        Console.WriteLine("\nAssembly entry point:");
        Console.WriteLine(ass.EntryPoint);

    }
}
