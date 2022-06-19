using System;
using System.Security.Cryptography;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[ RDF - REMOVE DUPLICATE FILES ]");

            if (args.Length != 1)
            {
                Console.WriteLine("Arguments: <directory>");
                return;
            }

            var root = args[0];
            var allFiles = Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(f => f).ToList();
            var filesToDelete = new List<string>();

            //using (var hasher = SHA1.Create())
            //using (var hasher = SHA256.Create())
            using (var hasher = MD5.Create()) // MD5 == ~20x faster than SHA256
            {
                var groups = allFiles.Select(f => new { File = f, Hash = BitConverter.ToString(hasher.ComputeHash(File.ReadAllBytes(f))) })
                    .GroupBy(p => new { RelativeFile = Path.GetFileName(p.File), p.Hash }, p => p.File)
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in groups)
                {
                    // Skip the "earliest" file - fortunately versions
                    // always come before "developer" or "unstable".
                    filesToDelete.AddRange(group.Skip(1));
                }
            }

            if (filesToDelete.Count == 0)
            {
                Console.WriteLine("No duplicates found");
                return;
            }

            Console.WriteLine("Files to delete:");
            foreach (var file in filesToDelete)
            {
                Console.WriteLine(file.StartsWith(
                    root, StringComparison.OrdinalIgnoreCase)
                        ? file.Substring(root.Length)
                        : file);
            }

            Console.WriteLine();
            Console.Write("Delete files? [y / n] :");
            string response = Console.ReadLine();
            if (response.ToLower() == "y")
            {
                filesToDelete.ForEach(action: f =>
                {
                    Console.WriteLine("Deleting: '{0}'", f);
                    File.Delete(f);
                });
            }

        }
    }
}