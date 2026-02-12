// using System.Diagnostics;
// namespace Scalpel
// {
//     public class DetectIsolation
//     {
//         static string requirementId = "Req2";
//         static var reqRegex = new Regex(@"Req\d+");
//         static void CompileRequirements()
//         {
//             string logLines = RunGitCommand($"log --oneline");
//             if (string.IsNullOrWhiteSpace(logLines))
//             {
//                 Console.WriteLine("No commits found for this requirement.");
//                 return;
//             }
//             Dictionary<string, List<string>> commitToRequirements = new();
//             foreach (var line in logLines)
//             {
//                 var match = reqRegex.Matches(line);
//                 if (match.Count == 0)
//                     continue;

//                 var commitHash = line.Split(' ')[0];
//                 var reqs = match.Select(m => m.Value).ToList();

//                 commitToRequirements[commitHash] = reqs;
//             }

//             Console.WriteLine("\n--- Commit to Requirement Mapping ---");

//             foreach (var entry in commitToRequirements)
//             {
//                 // entry.Key is the Commit Hash
//                 // entry.Value is the List<string> of REQ IDs
//                 string requirements = string.Join(", ", entry.Value);
                
//                 Console.WriteLine($"Commit: {entry.Key} | Requirements: {requirements}");
//             }
//         }
//         static string RunGitCommand(string arguments)
//         {
//             var process = new Process
//             {
//                 StartInfo = new ProcessStartInfo
//                 {
//                     FileName = "git",
//                     RedirectStandardOutput = true,
//                     Arguments = arguments,
//                     RedirectStandardError = true,
//                     UseShellExecute = false,
//                     CreateNoWindow = true
//                 }
//             };
//             process.Start();
//             string output = process.StandardOutput.ReadToEnd();
//             process.WaitForExit();
//             return output;
//         }

//         static List<string> PrintCommitHash(string output)
//         {
//             var commitLines = output
//             .Split('\n', StringSplitOptions.RemoveEmptyEntries);

//             var commitHashes = commitLines
//                 .Select(line => line.Split(' ')[0])
//                 .ToList();
//             Console.WriteLine("\nCommit Hashes:");
//             foreach (var hash in commitHashes)
//             {
//                 Console.WriteLine(hash);
//             }
//             return commitHashes;
//         }

//         static void PrintFilesChanged(List<string> commitHashes)
//         {
//             var filesChanged = new HashSet<string>();

//             foreach (var hash in commitHashes)
//             {
//                 string filesOutput = RunGitCommand($"diff-tree --no-commit-id --name-only -r {hash}");

//                 var files = filesOutput
//                     .Split('\n', StringSplitOptions.RemoveEmptyEntries);

//                 foreach (var file in files)
//                 {
//                     filesChanged.Add(file.Trim());
//                 }
//             }
//             Console.WriteLine("\nFiles affected:");
//             foreach (var file in filesChanged)
//             {
//                 Console.WriteLine($"- {file}");
//             }
//         }
//     }
// }