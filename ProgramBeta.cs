using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        Console.WriteLine("Syntax Terminal v1.0.0a-alpha Prototype Build");
        Console.WriteLine("Copyright (c) 2026 SpideyBash4116. Syntax is rightfully owned and created by SpideyBash4116. All rights reserved.");
        Console.WriteLine("Type 'help' for a list of commands.");

        while (true)
        {
            Console.Write($"{currentDirectory}> ");
            string input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
                continue;

            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1].Trim() : "";

            switch (command)
            {
                case "exit":
                    return;

                case "help":
                    ShowHelp();
                    break;

                case "clear":
                    Console.Clear();
                    break;

                case "echo":
                    Console.WriteLine(argument);
                    break;

                case "cd":
                    ChangeDirectory(ref currentDirectory, argument);
                    break;

                case "dir":
                    ListDirectory(currentDirectory);
                    break;

                case "beep":
                    Console.Beep();
                    break;

                case "open":
                    OpenPath(currentDirectory, argument);
                    break;

                case "move":
                    MoveFile(currentDirectory, argument);
                    break;

                case "start":
                    StartProgram(currentDirectory, argument);
                    break;

                case "notepad":
                    StartProgram(currentDirectory, "notepad.exe " + argument);
                    break;

                case "explorer":
                    StartProgram(currentDirectory, "explorer.exe " + (string.IsNullOrEmpty(argument) ? "." : argument));
                    break;

                case "restart":
                    Restart(currentDirectory);
                    break;

                default:
                    Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                    break;
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("help                 - Show this help");
        Console.WriteLine("exit                 - Exit terminal");
        Console.WriteLine("clear                - Clear screen");
        Console.WriteLine("echo <text>          - Print text");
        Console.WriteLine("cd <path>            - Change directory");
        Console.WriteLine("dir                  - List files");
        Console.WriteLine("beep                 - Make a beep sound");
        Console.WriteLine("");
        Console.WriteLine("open <path>          - Open file or folder with default application");
        Console.WriteLine("move <src> <dest>    - Move a file from src to dest (use quotes if paths contain spaces)");
        Console.WriteLine("start <program> [args] - Start a program (e.g. start notepad.exe file.txt)");
        Console.WriteLine("notepad <file>       - Open file in Notepad");
        Console.WriteLine("explorer [path]      - Open File Explorer at path (default: current directory)");
        Console.WriteLine("restart              - Restart the terminal (relaunches current executable)");
    }

    static void ChangeDirectory(ref string currentDirectory, string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine(currentDirectory);
                return;
            }

            string newPath = ResolvePath(currentDirectory, path);

            if (Directory.Exists(newPath))
            {
                currentDirectory = Path.GetFullPath(newPath);
            }
            else
            {
                Console.WriteLine("Directory not found.");
            }
        }
        catch
        {
            Console.WriteLine("Invalid path.");
        }
    }

    static void ListDirectory(string currentDirectory)
    {
        try
        {
            string[] dirs = Directory.GetDirectories(currentDirectory);
            string[] files = Directory.GetFiles(currentDirectory);

            Console.WriteLine("Directories:");
            foreach (var dir in dirs)
                Console.WriteLine("  [D] " + Path.GetFileName(dir));

            Console.WriteLine("Files:");
            foreach (var file in files)
                Console.WriteLine("  [F] " + Path.GetFileName(file));
        }
        catch
        {
            Console.WriteLine("Error reading directory.");
        }
    }

    static void OpenPath(string currentDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Usage: open <path>");
            return;
        }

        try
        {
            string target = ResolvePath(currentDirectory, path);

            if (!File.Exists(target) && !Directory.Exists(target))
            {
                Console.WriteLine("File or directory not found.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open: {ex.Message}");
        }
    }

    static void MoveFile(string currentDirectory, string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            Console.WriteLine("Usage: move <src> <dest>   (use quotes if paths contain spaces)");
            return;
        }

        var parts = SplitTwoArguments(argument);
        if (parts == null)
        {
            Console.WriteLine("Invalid arguments. Use quotes for paths with spaces: move \"C:\\a b.txt\" \"D:\\dest folder\\b.txt\"");
            return;
        }

        string src = ResolvePath(currentDirectory, parts.Item1);
        string dest = ResolvePath(currentDirectory, parts.Item2);

        try
        {
            if (!File.Exists(src))
            {
                Console.WriteLine("Source file not found.");
                return;
            }

            string destDir = Path.GetDirectoryName(dest) ?? currentDirectory;
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            if (File.Exists(dest))
            {
                Console.WriteLine("Destination already exists. Move aborted.");
                return;
            }

            File.Move(src, dest);
            Console.WriteLine("File moved.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move failed: {ex.Message}");
        }
    }

    static void StartProgram(string currentDirectory, string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            Console.WriteLine("Usage: start <program> [args]");
            return;
        }

        try
        {
            var parts = SplitCommandAndArgs(argument);
            var psi = new ProcessStartInfo
            {
                FileName = parts.Item1,
                Arguments = parts.Item2,
                UseShellExecute = true,
                WorkingDirectory = currentDirectory
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start program: {ex.Message}");
        }
    }

    static void Restart(string currentDirectory)
    {
        try
        {
            // Determine current executable path. MainModule may be null in some environments, so fallback to argv[0].
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.GetCommandLineArgs()[0];

            // Reconstruct original args (skip argv[0]) and quote args with spaces.
            string[] originalArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
            string args = string.Join(" ", originalArgs.Select(QuoteIfNeeded));

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = true,
                WorkingDirectory = currentDirectory
            };

            Process.Start(psi);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Restart failed: {ex.Message}");
        }
    }

    static string ResolvePath(string currentDirectory, string path)
    {
        path = path.Trim().Trim('"');
        return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(currentDirectory, path));
    }

    // Splits a command like: notepad.exe "file name.txt"  => ("notepad.exe", "\"file name.txt\"")
    static Tuple<string, string> SplitCommandAndArgs(string input)
    {
        input = input.Trim();
        if (input.StartsWith("\""))
        {
            int end = input.IndexOf('"', 1);
            if (end > 0)
            {
                // unlikely scenario where executable is quoted; handle simply
                int firstSpace = input.IndexOf(' ', end + 1);
                if (firstSpace < 0)
                    return Tuple.Create(input.Trim('"'), "");
                var exe = input.Substring(1, end - 1);
                var args = input.Substring(firstSpace + 1);
                return Tuple.Create(exe, args);
            }
        }

        int space = input.IndexOf(' ');
        if (space < 0)
            return Tuple.Create(input, "");
        return Tuple.Create(input.Substring(0, space), input.Substring(space + 1));
    }

    // Parse two args while supporting quoted paths:
    // Examples:
    //   C:\a.txt D:\b.txt
    //   "C:\a b.txt" "D:\dest folder\b.txt"
    static Tuple<string, string>? SplitTwoArguments(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return null;

        string first, second;

        if (input.StartsWith("\""))
        {
            int endQuote = input.IndexOf('"', 1);
            if (endQuote < 1) return null;
            first = input.Substring(1, endQuote - 1);
            int restStart = endQuote + 1;
            while (restStart < input.Length && char.IsWhiteSpace(input[restStart])) restStart++;
            if (restStart >= input.Length) return null;
            if (input[restStart] == '"')
            {
                int endQuote2 = input.IndexOf('"', restStart + 1);
                if (endQuote2 < 0) return null;
                second = input.Substring(restStart + 1, endQuote2 - restStart - 1);
            }
            else
            {
                second = input.Substring(restStart);
            }
        }
        else
        {
            int space = input.IndexOf(' ');
            if (space < 0) return null;
            first = input.Substring(0, space);
            second = input.Substring(space + 1).Trim();
            if (second.StartsWith("\"") && second.EndsWith("\"") && second.Length >= 2)
                second = second.Substring(1, second.Length - 2);
        }

        return Tuple.Create(first, second);
    }

    static string QuoteIfNeeded(string s) => s.Contains(' ') ? $"\"{s}\"" : s;
}
