using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;


var supportedLanguages = new Dictionary<string, List<string>>()
{

    { "ALL", new List<string> {} },
    { "TEXT", new List<string> { ".txt" } },
    { "PYTHON", new List<string> { ".py", ".ipynb" } },
    { "C#", new List<string> { ".cs" } },
    { "JAVA", new List<string> { ".java" } },
    { "JAVASCRIPT", new List<string> { ".js", ".jsx" } },
    { "PHP", new List<string> { ".php" } },
    { "RUBY", new List<string> { ".rb" } },
    { "GO", new List<string> { ".go" } },
    { "SWIFT", new List<string> { ".swift" } },
    { "KOTLIN", new List<string> { ".kt" } },
    { "HTML", new List<string> { ".html", ".htm" } },
    { "CSS", new List<string> { ".css" } },
    { "SQL", new List<string> { ".sql" } },
    { "TYPESCRIPT", new List<string> { ".ts", ".tsx" } },
    { "DART", new List<string> { ".dart" } },
    { "PERL", new List<string> { ".pl" } },
    { "HASKELL", new List<string> { ".hs" } },
    { "LUA", new List<string> { ".lua" } },
    { "SCALA", new List<string> { ".scala" } },
    { "CLOJURE", new List<string> { ".clj" } },
    { "COBOL", new List<string> { ".cob", ".cbl" } },
    { "FORTRAN", new List<string> { ".f", ".for" } },
    { "MATLAB", new List<string> { ".m" } },
    { "OBJECTIVEC", new List<string> { ".m" } },
    { "VB.NET", new List<string> { ".vb" } },
    { "ASSEMBLY", new List<string> { ".asm", ".s" } },
    { "C", new List<string> { ".c",".h" } },
    { "C++", new List<string> { ".cpp",".h" } },
    { "R", new List<string> { ".R", ".r" } },
    { "SAS", new List<string> { ".sas" } },
    { "JULIA", new List<string> { ".jl" } },
    { "MATHEMATICA", new List<string> { ".nb" } },
    { "COFFEESCRIPT", new List<string> { ".coffee" } },
    { "REACT", new List<string> { ".js", ".jsx" } },
    { "ELIXIR", new List<string> { ".ex", ".exs" } },
    { "OCAML", new List<string> { ".ml", ".mli" } },
    { "F#", new List<string> { ".fs", ".fsi" } }
};

var rootCommand = new RootCommand("CLI for bundling code files.");


var bundleCommand = new Command("bundle", "Bundles code files into a single file.");

// ----------------------Options------------------------
var languageOption = new Option<string>("--language",
    "Programming languages to include (e.g., PYTHON, CSHARP or 'all')")
{
    IsRequired = true
};
languageOption.AddAlias("-l");
bundleCommand.AddOption(languageOption);

var outputOption = new Option<FileInfo>("--output",
    "Output bundle file name (with path if needed)")
{
    IsRequired = true
};
outputOption.AddAlias("-o");
bundleCommand.AddOption(outputOption);

var noteOption = new Option<bool>("--note",
    "Include source code as a comment in the bundle.")
{
    IsRequired = false
};
noteOption.AddAlias("-n");
bundleCommand.AddOption(noteOption);

var sortOption = new Option<string>("--sort",
    "Sort files by name (default) or type.")
{
    IsRequired = false
};
sortOption.AddAlias("-s");
bundleCommand.AddOption(sortOption);

var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines",
    "Remove empty lines from the code.")
{
    IsRequired = false
};
removeEmptyLinesOption.AddAlias("-r");
bundleCommand.AddOption(removeEmptyLinesOption);

var authorOption = new Option<string>("--author",
    "Author name to include in the bundle.")
{
    IsRequired = false
};
authorOption.AddAlias("-a");
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((FileInfo output, string language, bool note, string sort, bool removeEmptyLines, string author) =>
{
    try
    {
        string outputPath = Path.IsPathRooted(output.FullName) ? output.FullName : Path.Combine(Directory.GetCurrentDirectory(), output.Name);
        if (File.Exists(outputPath))
        {
            throw new IOException("Output file already exists. Please choose a different name.");
        }
        using (var fileStream = new FileStream(outputPath, FileMode.Create))
        {
            List<string> codeFiles = GetFilesToBundle(supportedLanguages, language, Directory.GetCurrentDirectory(), output.FullName);
            if (codeFiles.Count == 0)
            {
                throw new FileNotFoundException("No code files found for the specified language.");
            }
            if (sort == "type")
            {
                codeFiles = codeFiles.OrderBy(file => Path.GetExtension(file)).ToList();
            }
            else
            {
                codeFiles = codeFiles.OrderBy(file => file).ToList();
            }
            using (var bundleStream = new StreamWriter(fileStream))
            {
                if (!string.IsNullOrEmpty(author))
                {
                    bundleStream.WriteLine($"// Author: {author}");
                }

                foreach (var file in codeFiles)
                {
                    try
                    {
                        var fileContent = File.ReadAllText(file);
                        if (removeEmptyLines)
                        {
                            fileContent = string.Join("\n", fileContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                        }
                        if (note)
                        {
                            bundleStream.WriteLine($"// Source: {file}");
                        }
                        bundleStream.WriteLine(fileContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading file {file}: {ex.Message}");
                    }
                }
            }
        }
        Console.WriteLine($"Files bundled successfully into {outputPath}");
    }
    catch (DirectoryNotFoundException dirEx)
    {
        Console.WriteLine($"Directory not found: {dirEx.Message}");
    }
    catch (FileNotFoundException fileEx)
    {
        Console.WriteLine($"File not found: {fileEx.Message}");
    }
    catch (PathTooLongException pathEx)
    {
        Console.WriteLine($"Path too long: {pathEx.Message}");
    }
    catch (IOException ioEx)
    {
        Console.WriteLine($"IO Error: {ioEx.Message}");
    }
    catch (UnauthorizedAccessException authEx)
    {
        Console.WriteLine($"Access Error: {authEx.Message}");
    }
    catch (FormatException formatEx)
    {
        Console.WriteLine($"Format Error: {formatEx.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);



var createRspCommand = new Command("create-rsp", "Creates a response file for the bundling command.");


createRspCommand.SetHandler((InvocationContext context) =>
{
    try
    {
        Console.Write("Enter language: ");
        var language = Console.ReadLine();

        Console.Write("Enter output file name (If you want the file in a different folder than the current one, type a full path including the file name.): ");
        var outputName = Console.ReadLine();

        Console.Write("Include source code as comment? (y/n): ");
        var note = Console.ReadLine()?.ToLower() == "y";

        Console.Write("Sort files by name or type? (name/type): ");
        var sort = Console.ReadLine();

        Console.Write("Remove empty lines? (y/n): ");
        var removeEmptyLines = Console.ReadLine()?.ToLower() == "y";

        Console.Write("Enter author name: ");
        var author = Console.ReadLine();
        using (var responseFile = new StreamWriter("command.rsp"))
        {
            responseFile.WriteLine($"bundle --language {language} --output {outputName} --note {note} --sort {sort} --remove-empty-lines {removeEmptyLines} --author \"{author}\"");
        }
        Console.WriteLine($"Response file created: command.rsp");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
});

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

// הרצת הפקודה
return await rootCommand.InvokeAsync(args);





List<string> GetFilesToBundle(Dictionary<string, List<string>> languages, string userInput, string outputPath, string outputFileName)
{
    var selectedLanguages = userInput.ToString().Split(',').Select(lang => lang.Trim().ToUpper()).ToList();
    // אם המשתמש הכניס "all", נבחר את כל הסיומות
    List<string> fileExtensionsToInclude;

    if (selectedLanguages.Contains("ALL"))
    {
        fileExtensionsToInclude = languages.Values.SelectMany(ext => ext).ToList();
    }
    else
    {
        fileExtensionsToInclude = selectedLanguages
            .Where(lang => languages.ContainsKey(lang))
            .SelectMany(lang => languages[lang])
            .ToList();
    }

    var codeFiles = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories)
        .Where(file => fileExtensionsToInclude.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        .Where(file => file.IndexOf("bin", StringComparison.OrdinalIgnoreCase) < 0 &&
                       file.IndexOf("debug", StringComparison.OrdinalIgnoreCase) < 0 &&
                       file.IndexOf("node_modules", StringComparison.OrdinalIgnoreCase) < 0 &&
                       file.IndexOf(".git", StringComparison.OrdinalIgnoreCase) < 0 &&
                       !file.Equals(Path.GetFullPath(outputFileName), StringComparison.OrdinalIgnoreCase))
        .ToList();

    return codeFiles;
}
