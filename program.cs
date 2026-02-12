// using System.Diagnostics;
// using System.Text.RegularExpressions;
// using System;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// namespace Scalpel
// {
//     class Program
//     {
//         static string requirementId = "Req2";
//         static Regex reqRegex = new Regex(@"Req\d+");
//         static string[] filesChanged;
//         static void Main()
//         {
//             string gitOutput = RunGitCommand($"log --grep={requirementId} --oneline");
//             if (string.IsNullOrWhiteSpace(gitOutput))
//             {
//                 Console.WriteLine("No commits found for this requirement.");
//                 return;
//             }
//             Console.WriteLine("\nCommits:");
//             Console.WriteLine(gitOutput);
//             var commitHashes = PrintCommitHash(gitOutput);
//             PrintFilesChanged(commitHashes);

//             Console.WriteLine("\nCompiled Result:");
//             // DetectIsolation.CompileRequirements();

//             string logLines = RunGitCommand($"log --oneline");
//             if (string.IsNullOrWhiteSpace(logLines))
//             {
//                 Console.WriteLine("No commits found for this requirement.");
//                 return;
//             }
//             string[] lines = logLines.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
//             Dictionary<string, List<string>> commitToRequirements = new();
//             foreach (var line in lines)
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

//             Dictionary<string, HashSet<string>> fileToRequirements = new();
//             foreach (var entry in commitToRequirements)
//             {
//                 string commitHash = entry.Key;
//                 var requirements = entry.Value;

//                 string filesOutput = RunGitCommand($"diff-tree --no-commit-id --name-only --root -r {commitHash}");
//                 filesChanged = filesOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//                 foreach (var file in filesChanged)
//                 {
//                     if (!fileToRequirements.ContainsKey(file))
//                         fileToRequirements[file] = new HashSet<string>();

//                     foreach (var req in requirements)
//                         fileToRequirements[file].Add(req);
//                 }
//             }
//             Console.WriteLine("\n--- Files to Requirement Mapping ---");

//             foreach (var entry in fileToRequirements)
//             {
//                 // entry.Key is the File Path
//                 // entry.Value is the List<string> of REQ IDs
//                 string requirements = string.Join(", ", entry.Value);

//                 Console.WriteLine($"File: {entry.Key} | Requirements: {requirements}");
//             }

//             var entangledFiles = fileToRequirements
//             .Where(f => f.Value.Count > 1)
//             .ToList();
//             Console.WriteLine("\n⚠️ Shared (High-Risk) Files:");

//             foreach (var file in entangledFiles)
//             {
//                 Console.WriteLine($"- {file.Key}");
//                 Console.WriteLine($"  Requirements: {string.Join(", ", file.Value)}");
//             }

//             ShowFileChanges(commitHashes);

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

//         static void ShowFileChanges(List<string> commitHashes)
//         {
//             Console.WriteLine("\n--- Detailed File Changes ---");
//             foreach (var hash in commitHashes)
//             {
//                 string diffOutput = RunGitCommand($"show {hash}");
//                 // string diffOutput = RunGitCommand($"show --name-status {hash}");
//                 var changedLineRanges = new List<(int start, int end)>();
//                 foreach (var line in diffOutput.Split('\n'))
//                 {
//                     if (line.StartsWith("@@"))
//                     {
//                         var parts = line.Split(' ');
//                         var rangePart = parts[2]; // +45,12

//                         var nums = rangePart
//                             .TrimStart('+')
//                             .Split(',');

//                         int start = int.Parse(nums[0]);
//                         int length = nums.Length > 1 ? int.Parse(nums[1]) : 1;
//                         //we need to bind line changes to file names along with requirements as well
//                         changedLineRanges.Add((start, start + length));
//                     }
//                 }
//                 if (changedLineRanges.Count == 0)
//                 {
//                     Console.WriteLine("  No line-level changes detected.");
//                 }
//                 else
//                 {
//                     Console.WriteLine("  Changed Line Ranges:");

//                     foreach (var range in changedLineRanges)
//                     {
//                         Console.WriteLine($"    Lines {range.start} - {range.end}");
//                     }
//                 }
//                 foreach (var file in filesChanged)
//                 {
//                     // finding file changes exactly in each file per commit hash
//                     var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), file);
//                     if (!File.Exists(absolutePath) && !file.EndsWith(".cs")) continue;
//                     string code = File.ReadAllText(absolutePath);
//                     var tree = CSharpSyntaxTree.ParseText(code);
//                     var root = tree.GetRoot();
//                     var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
//                     foreach (var method in methods)
//                     {
//                         var span = method.SyntaxTree.GetLineSpan(method.Span);

//                         int methodStart = span.StartLinePosition.Line + 1;
//                         int methodEnd = span.EndLinePosition.Line + 1;

//                         foreach (var change in changedLineRanges)
//                         {
//                             if (Overlaps((methodStart, methodEnd), change))
//                             {
//                                 Console.WriteLine($"Method impacted: {method.Identifier.Text}");
//                             }
//                         }
//                     }
//                 }
//             }
//         }

//         static bool Overlaps((int start, int end) a, (int start, int end) b)
//         {
//             return a.start <= b.end && b.start <= a.end;
//         }
//     }
// }

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Scalpel
{
    class Program
    {
        private static readonly string RequirementId = "Req2";
        private static readonly Regex ReqRegex = new(@"Req\d+");

        static void Main()
        {
            // Phase 1: Search for specific requirement commits
            var specificCommits = GetCommitsForRequirement(RequirementId);
            if (specificCommits.Count == 0)
            {
                Console.WriteLine("No commits found for this requirement.");
                return;
            }

            Console.WriteLine("\nCommits:");
            foreach (var commit in specificCommits)
            {
                Console.WriteLine(commit);
            }

            var commitHashes = ExtractCommitHashes(specificCommits);
            PrintCommitHashes(commitHashes);
            PrintFilesChanged(commitHashes);

            Console.WriteLine("\nCompiled Result:");
            // DetectIsolation.CompileRequirements();

            // Phase 2: Build comprehensive requirement mappings
            var commitToRequirements = BuildCommitToRequirementsMap();
            PrintCommitToRequirementsMap(commitToRequirements);

            var fileToRequirements = BuildFileToRequirementsMap(commitToRequirements);
            PrintFileToRequirementsMap(fileToRequirements);

            PrintEntangledFiles(fileToRequirements);

            AnalyzeFileChanges(commitHashes, commitToRequirements);
        }

        private static List<string> GetCommitsForRequirement(string requirementId)
        {
            string output = RunGitCommand($"log --grep={requirementId} --oneline");
            return string.IsNullOrWhiteSpace(output) 
                ? new List<string>() 
                : output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static List<string> ExtractCommitHashes(List<string> commitLines)
        {
            return commitLines.Select(line => line.Split(' ')[0]).ToList();
        }

        private static void PrintCommitHashes(List<string> commitHashes)
        {
            Console.WriteLine("\nCommit Hashes:");
            foreach (var hash in commitHashes)
            {
                Console.WriteLine(hash);
            }
        }

        private static Dictionary<string, List<string>> BuildCommitToRequirementsMap()
        {
            string logLines = RunGitCommand("log --oneline");
            if (string.IsNullOrWhiteSpace(logLines))
            {
                return new Dictionary<string, List<string>>();
            }

            var lines = logLines.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var commitToRequirements = new Dictionary<string, List<string>>();

            foreach (var line in lines)
            {
                var matches = ReqRegex.Matches(line);
                if (matches.Count == 0) continue;

                var commitHash = line.Split(' ')[0];
                var requirements = matches.Select(m => m.Value).ToList();
                commitToRequirements[commitHash] = requirements;
            }

            return commitToRequirements;
        }

        private static void PrintCommitToRequirementsMap(Dictionary<string, List<string>> commitToRequirements)
        {
            Console.WriteLine("\n--- Commit to Requirement Mapping ---");
            foreach (var entry in commitToRequirements)
            {
                Console.WriteLine($"Commit: {entry.Key} | Requirements: {string.Join(", ", entry.Value)}");
            }
        }

        private static Dictionary<string, HashSet<string>> BuildFileToRequirementsMap(
            Dictionary<string, List<string>> commitToRequirements)
        {
            var fileToRequirements = new Dictionary<string, HashSet<string>>();

            foreach (var (commitHash, requirements) in commitToRequirements)
            {
                var filesChanged = GetFilesChangedInCommit(commitHash);

                foreach (var file in filesChanged)
                {
                    if (!fileToRequirements.ContainsKey(file))
                    {
                        fileToRequirements[file] = new HashSet<string>();
                    }

                    foreach (var req in requirements)
                    {
                        fileToRequirements[file].Add(req);
                    }
                }
            }

            return fileToRequirements;
        }

        private static void PrintFileToRequirementsMap(Dictionary<string, HashSet<string>> fileToRequirements)
        {
            Console.WriteLine("\n--- Files to Requirement Mapping ---");
            foreach (var entry in fileToRequirements)
            {
                Console.WriteLine($"File: {entry.Key} | Requirements: {string.Join(", ", entry.Value)}");
            }
        }

        private static void PrintEntangledFiles(Dictionary<string, HashSet<string>> fileToRequirements)
        {
            var entangledFiles = fileToRequirements.Where(f => f.Value.Count > 1).ToList();
            
            Console.WriteLine("\n⚠️ Shared (High-Risk) Files:");
            foreach (var file in entangledFiles)
            {
                Console.WriteLine($"- {file.Key}");
                Console.WriteLine($"  Requirements: {string.Join(", ", file.Value)}");
            }
        }

        private static void PrintFilesChanged(List<string> commitHashes)
        {
            var filesChanged = new HashSet<string>();

            foreach (var hash in commitHashes)
            {
                var files = GetFilesChangedInCommit(hash);
                foreach (var file in files)
                {
                    filesChanged.Add(file);
                }
            }

            Console.WriteLine("\nFiles affected:");
            foreach (var file in filesChanged)
            {
                Console.WriteLine($"- {file}");
            }
        }

        private static string[] GetFilesChangedInCommit(string commitHash)
        {
            string filesOutput = RunGitCommand($"diff-tree --no-commit-id --name-only --root -r {commitHash}");
            return filesOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(f => f.Trim())
                              .ToArray();
        }

        private static void AnalyzeFileChanges(List<string> commitHashes, Dictionary<string, List<string>> commitToRequirements)
        {
            Console.WriteLine("\n--- Detailed File Changes ---");

            foreach (var hash in commitHashes)
            {
                var changedLineRanges = GetChangedLineRanges(hash);
                var filesChanged = GetFilesChangedInCommit(hash);

                if (changedLineRanges.Count == 0)
                {
                    Console.WriteLine("  No line-level changes detected.");
                }
                else
                {
                    Console.WriteLine("  Changed Line Ranges:");
                    foreach (var range in changedLineRanges)
                    {
                        Console.WriteLine($"    Lines {range.start} - {range.end}");
                    }
                }

                AnalyzeImpactedMethods(filesChanged, changedLineRanges);
            }
        }

        private static List<(int start, int end)> GetChangedLineRanges(string commitHash)
        {
            string diffOutput = RunGitCommand($"show {commitHash}");
            var changedLineRanges = new List<(int start, int end)>();

            foreach (var line in diffOutput.Split('\n'))
            {
                if (line.StartsWith("@@"))
                {
                    var parts = line.Split(' ');
                    var rangePart = parts[2]; // +45,12

                    var nums = rangePart.TrimStart('+').Split(',');
                    int start = int.Parse(nums[0]);
                    int length = nums.Length > 1 ? int.Parse(nums[1]) : 1;

                    changedLineRanges.Add((start, start + length));
                }
            }

            return changedLineRanges;
        }

        private static void AnalyzeImpactedMethods(string[] filesChanged, List<(int start, int end)> changedLineRanges)
        {
            foreach (var file in filesChanged)
            {
                if (!file.EndsWith(".cs")) continue;

                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), file);
                if (!File.Exists(absolutePath)) continue;

                var methods = GetMethodsFromFile(absolutePath);
                Console.WriteLine($"Analyzing methods in file: {file}");

                foreach (var method in methods)
                {
                    Console.WriteLine($"Checking method: {method.Identifier.Text}");
                    var (methodStart, methodEnd) = GetMethodLineRange(method);

                    foreach (var change in changedLineRanges)
                    {
                        Console.WriteLine($"Comparing with changed range: Lines {change.start} - {change.end}");
                        if (Overlaps((methodStart, methodEnd), change))
                        {
                            Console.WriteLine($"Method impacted: {method.Identifier.Text}");
                        }
                    }
                }
            }
        }

        private static IEnumerable<MethodDeclarationSyntax> GetMethodsFromFile(string filePath)
        {
            string code = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        private static (int start, int end) GetMethodLineRange(MethodDeclarationSyntax method)
        {
            var span = method.SyntaxTree.GetLineSpan(method.Span);
            int methodStart = span.StartLinePosition.Line + 1;
            int methodEnd = span.EndLinePosition.Line + 1;
            return (methodStart, methodEnd);
        }

        private static bool Overlaps((int start, int end) a, (int start, int end) b)
        {
            var overlaps = a.start <= b.end && b.start <= a.end;
            Console.WriteLine($"Checking overlap between ({a.start}, {a.end}) and ({b.start}, {b.end}): {overlaps}");
            return overlaps;
        }

        private static string RunGitCommand(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
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
    }
}