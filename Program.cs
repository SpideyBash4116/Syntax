using System;
using System.IO;

class Program
{
    static void Main()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        while (true)
        {
            Console.Write($"{currentDirectory}> ");
            string input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
                continue;

            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1] : "";

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

                default:
                    Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                    break;
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("help        - Show this help");
        Console.WriteLine("exit        - Exit terminal");
        Console.WriteLine("clear       - Clear screen");
        Console.WriteLine("echo <text> - Print text");
        Console.WriteLine("cd <path>   - Change directory");
        Console.WriteLine("dir         - List files");
    }

    static void ChangeDirectory(ref string currentDirectory, string path)
    {
        try
        {
            string newPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(currentDirectory, path);

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
}
