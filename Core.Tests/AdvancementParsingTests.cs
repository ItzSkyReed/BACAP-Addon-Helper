using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Core.Advancements.Models;

namespace Core.Tests;

public class AdvancementParsingTests
{
    private readonly ITestOutputHelper _output;

    // Inject ITestOutputHelper to print messages to the xUnit test runner output
    public AdvancementParsingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_Successfully_Parse_All_Advancement_Files_With_Metrics()
    {
        const string targetDirectory = @"C:\Users\skyreed\PycharmProjects\Bacap-scripts\datapacks\bacap\data\blazeandcave\advancement";
        Assert.True(Directory.Exists(targetDirectory), $"Directory not found: {targetDirectory}");

        var jsonFiles = Directory.GetFiles(targetDirectory, "*.json", SearchOption.AllDirectories);
        var parsedCount = 0;

        // Pre-load all files into memory so we benchmark CPU/Parser speed, NOT Disk I/O speed
        var sw1 = Stopwatch.StartNew();
        var fileContents = new string[jsonFiles.Length];
        for (var i = 0; i < jsonFiles.Length; i++)
        {
            fileContents[i] = File.ReadAllText(jsonFiles[i]);
        }
        sw1.Stop();
        _output.WriteLine($"Total Time Files         : {sw1.ElapsedMilliseconds} ms");

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < fileContents.Length; i++)
        {
            try
            {
                var advancement = Advancement.Parse(fileContents[i]);
                Assert.NotNull(advancement);
                parsedCount++;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to parse advancement file: {jsonFiles[i]}\nError: {ex.Message}");
            }
        }

        sw.Stop();

        // Output the results to the test explorer
        _output.WriteLine("--- Parsing Metrics ---");
        _output.WriteLine($"Total Files Parsed : {parsedCount}");
        _output.WriteLine($"Total Time         : {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Average Time/File  : {(double)sw.ElapsedMilliseconds / parsedCount:F4} ms");
        _output.WriteLine($"Throughput         : {Math.Round(parsedCount / sw.Elapsed.TotalSeconds)} files/sec");
    }
}