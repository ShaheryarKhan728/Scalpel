using System.Diagnostics;
class Program
{
    static string requirementId = "Req1";
    static void Main()
    {
        string gitOutput = RunGitCommand($"log --grep={requirementId} --oneline");
        if (string.IsNullOrWhiteSpace(gitOutput))
        {
            Console.WriteLine("No commits found for this requirement.");
            return;
        }
        Console.WriteLine("\nCommits:");
        Console.WriteLine(gitOutput);
        var commitHashes = PrintCommitHash(gitOutput);
        PrintFilesChanged(commitHashes);
    }
    static string RunGitCommand(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    static List<string> PrintCommitHash(string output)
    {
        var commitLines = output
        .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var commitHashes = commitLines
            .Select(line => line.Split(' ')[0])
            .ToList();
        Console.WriteLine("\nCommit Hashes:");
        foreach (var hash in commitHashes)
        {
            Console.WriteLine(hash);
        }
        return commitHashes;
    }

    static void PrintFilesChanged(List<string> commitHashes)
    {
        var filesChanged = new HashSet<string>();

        foreach (var hash in commitHashes)
        {
            string filesOutput = RunGitCommand($"diff-tree --no-commit-id --name-only -r {hash}");

            var files = filesOutput
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var file in files)
            {
                filesChanged.Add(file.Trim());
            }
        }
        Console.WriteLine("\nFiles affected:");
        foreach (var file in filesChanged)
        {
            Console.WriteLine($"- {file}");
        }
    }
}