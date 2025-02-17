/*
 * Thanks to Andreas Warberg for the idea/implementation for this class.
 * https://stackoverflow.com/a/74078463
 */

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace MarkovText;

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(Job.MediumRun.WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}