using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

////
//    Here is a sample code that only depends on Razor (for parsing and C# code generation)
//     and Roslyn (for C# code compilation, but you could use the old CodeDom as well).
//    There is no MVC in that piece of code, so, no View, no .cshtml files, no Controller,
// just Razor source parsing and compiled runtime execution. There is still the notion of Model though.
//    You will only need to add following nuget packages: Microsoft.AspNetCore.Razor.Language (v2.1.1),
// Microsoft.AspNetCore.Razor.Runtime (v2.1.1) and Microsoft.CodeAnalysis.CSharp (v2.8.2) nugets.
//    //

namespace starskywebhtmlcli.Services
{
    public class ViewRender
    {
        public static void Render()
        {
            // points to the local path
            var fs = RazorProjectFileSystem.Create(".");

            // customize the default engine a little bit
            var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                InheritsDirective.Register(builder);
                builder.SetNamespace("starskywebhtmlcli.Services"); // define a namespace for the Template class
            });

            // get a razor-templated file. My "hello.txt" template file is defined like this:
            //
            // @inherits RazorTemplate.MyTemplate
            // Hello @Model.Name, welcome to Razor World!
            //

            var item = fs.GetItem("hello.txt");

            // parse and generate C# code, outputs it on the console
            var codeDocument = engine.Process(item);
            var cs = codeDocument.GetCSharpDocument();

            // now, use roslyn, parse the C# code
            var tree = CSharpSyntaxTree.ParseText(cs.GeneratedCode);

            // define the dll
            const string dllName = "hello";
            var compilation = CSharpCompilation.Create(dllName, new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
                    MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly
                        .Location), // include Microsoft.AspNetCore.Razor.Runtime
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly()
                        .Location), // this file (that contains the MyTemplate base class)

                    // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(
                        Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

                    // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux,
                    // we need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(
                        Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll"))
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // we want a dll


            // compile the dll
            string path = Path.Combine(Path.GetFullPath("."), dllName + ".dll");
            var result = compilation.Emit(path);
            if (!result.Success)
            {
                Console.WriteLine(string.Join(Environment.NewLine, result.Diagnostics));
                return;
            }

            // load the built dll
            Console.WriteLine(path);
            var asm = Assembly.LoadFile(path);

            // the generated type is defined in our custom namespace, as we asked.
            // "Template" is the type name that razor uses by default.
            var template = (ToHtml) Activator.CreateInstance(asm.GetType("starskywebhtmlcli.Services.Template"));

            // run the code.
            // should display "Hello Killroy, welcome to Razor World!"
            template.ExecuteAsync().Wait();
        }
    } 
        
    // the model class. this is 100% specific to your context
    public class MyModel
    {
        // this will map to @Model.Name
        public string Name => "Killroy";
    }

    // the sample base template class. It's not mandatory but I think it's much easier.
    public abstract class ToHtml
    {
        // this will map to @Model (property name)
        public MyModel Model => new MyModel();

        public void WriteLiteral(string literal)
        {
            // replace that by a text writer for example
            Console.Write(literal);
        }

        public void Write(object obj)
        {
            // replace that by a text writer for example
            Console.Write(obj);
        }

        public async virtual Task ExecuteAsync()
        {
            await Task.Yield(); // whatever, we just need something that compiles...
        }
    }
}