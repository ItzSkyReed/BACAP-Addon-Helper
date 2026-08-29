using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.McFunctions;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class McFunctionParsingTests(ITestOutputHelper output)
{
    [Fact]
    public void Should_Successfully_Parse_All_McFunction_Files_With_Metrics()
    {
        // Adjust the path to target the 'function' directory instead of 'advancement'
        const string targetDirectory = @"C:\Users\skyreed\PycharmProjects\Bacap-scripts\datapacks\bacaped\data\bacaped\function";
        Assert.True(Directory.Exists(targetDirectory), $"Directory not found: {targetDirectory}");

        // Retrieve only .mcfunction files
        var mcFunctionFiles = Directory.GetFiles(targetDirectory, "*.mcfunction", SearchOption.AllDirectories);
        var parsedCount = 0;

        // Pre-load all files into memory so we benchmark CPU/Parser speed, NOT Disk I/O speed
        var sw1 = Stopwatch.StartNew();
        var fileContents = new string[mcFunctionFiles.Length];
        for (var i = 0; i < mcFunctionFiles.Length; i++)
        {
            fileContents[i] = File.ReadAllText(mcFunctionFiles[i]);
        }
        sw1.Stop();
        output.WriteLine($"Total Time (File I/O)    : {sw1.ElapsedMilliseconds} ms");

        // Metrics accumulators
        long totalLines = 0;
        long totalCharacters = 0;

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < fileContents.Length; i++)
        {
            try
            {
                // Hooking into our Pidgin-based preprocessor and parser
                var mcFunction = McFunctionParser.Parse(fileContents[i]);

                Assert.NotNull(mcFunction);

                parsedCount++;
                totalLines += mcFunction.Lines.Count;
                totalCharacters += fileContents[i].Length;
            }
            catch (Exception ex)
            {
                // Pinpoint the exact file that caused the parser to fail
                Assert.Fail($"Failed to parse function file: {mcFunctionFiles[i]}\nError: {ex.Message}");
            }
        }

        sw.Stop();

        // Calculate averages safely
        var avgLinesPerFile = parsedCount > 0 ? (double)totalLines / parsedCount : 0;
        var avgLineLength = totalLines > 0 ? (double)totalCharacters / totalLines : 0;
        var fileThroughput = sw.Elapsed.TotalSeconds > 0 ? parsedCount / sw.Elapsed.TotalSeconds : 0;
        var charThroughput = sw.Elapsed.TotalSeconds > 0 ? totalCharacters / sw.Elapsed.TotalSeconds : 0;

        // Output the results to the test explorer
        output.WriteLine("--- Parsing Metrics ---");
        output.WriteLine($"Total Files Parsed : {parsedCount:N0}");
        output.WriteLine($"Total Lines Parsed : {totalLines:N0}");
        output.WriteLine($"Total Characters   : {totalCharacters:N0}");
        output.WriteLine($"Total Time         : {sw.ElapsedMilliseconds} ms");
        output.WriteLine("-----------------------");
        output.WriteLine($"Avg Time/File      : {(double)sw.ElapsedMilliseconds / parsedCount:F4} ms");
        output.WriteLine($"Avg Lines / File   : {avgLinesPerFile:F1}");
        output.WriteLine($"Avg Line Length    : {avgLineLength:F1} chars");
        output.WriteLine("-----------------------");
        output.WriteLine($"File Throughput    : {Math.Round(fileThroughput):N0} files/sec");
        output.WriteLine($"Char Throughput    : {Math.Round(charThroughput):N0} chars/sec");
    }
}