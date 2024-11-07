using System.Diagnostics;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        Process currentProcess = Process.GetCurrentProcess();
        string fullPath = currentProcess.MainModule!.FileName;
        string execFileName = System.IO.Path.GetFileName(fullPath);
        
        if (args.Length == 0 || args.Contains("/?") || args.Contains("/help", StringComparer.InvariantCultureIgnoreCase))
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(2);
            Console.WriteLine("Removes (deletes) subdirectories in the specified directory.");
            Console.WriteLine($"Application version: {version}. Copyright (C) Stan Buran 2024.");
            Console.WriteLine($"{execFileName} /Path:\"target path\" /Dir:\"directory1 mask\" \"directory2 mask\" \"directoryN mask\"");
            Console.WriteLine("The mask parameter can contain a combination of valid literal and wildcard characters, but it doesn't support regular expressions.");
            return;
        }

        string targetPath = Directory.GetCurrentDirectory();
        
        List<string> lstNames = [];

        //parse command line arguments:
        foreach (var arg in args)
        {
            var sRaw = arg.TrimStart(' ', '/').Split(':');

            if(sRaw.Length == 1 && !sRaw.Contains(":") && !sRaw.Contains("\\"))
            {
                lstNames.Add(sRaw[0].Trim(' '));
                continue;
            }

            var key = sRaw[0].ToLowerInvariant();
            var raw = arg[(key.Length+2)..];
                        
            switch(key.ToLowerInvariant())
            {
                case "path":
                    targetPath = raw.Trim(' ', ':', '/');
                    break;
                case "dir":
                    lstNames.Add(raw.Trim(' ', ':', '/'));
                    break;
            }            
        }

        if(string.IsNullOrEmpty(targetPath) || lstNames.Count == 0)
        {
            Console.WriteLine("Invalid command line arguments.");
            return;
        }

        if (!Directory.Exists(targetPath))        
        {
            Console.WriteLine($"Directory {targetPath} does not exist.");
            Console.WriteLine("Press any key to exit.");
            _ = Console.ReadKey();
            return;
        }
        
        Console.Write($"Directory to delete subdirectories: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(targetPath);
        Console.ResetColor();
        Console.Write($"Subdirectories: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(ToSimpleString(lstNames));
        Console.ResetColor();


        bool hasErrors = false;
        foreach(var delMask in lstNames)
        {
            var dirs = Directory.GetDirectories(targetPath, delMask, SearchOption.AllDirectories).ToArray();

            foreach(var dir in dirs)
            {
                try
                {
                    Directory.Delete(dir, true);
                    Console.Write($"Delete directory: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(dir);
                }
                catch (Exception ex)
                {                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error deleting directory {dir}: {ex.Message}");
                    hasErrors = true;
                }
                Console.ResetColor();
            }
        }

        if(hasErrors)
        {
            Console.WriteLine("Errors occurred during deleting directories.");
            Console.WriteLine("Press any key to exit.");
            _ = Console.ReadKey();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All directories deleted successfully.");
            Thread.Sleep(1500);
        }
    }
    public static string ToSimpleString<T>(IEnumerable<T> values, char separator = ',') => values?.Aggregate(string.Empty, (current, val) => current + (val + separator.ToString())).Trim(separator) ?? string.Empty;
}