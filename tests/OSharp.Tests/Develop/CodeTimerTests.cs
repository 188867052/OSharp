using System;
using NSubstitute.Exceptions;
using Shouldly;

using Xunit;
using Xunit.Abstractions;

namespace OSharp.Develop.Tests
{
    public class CodeTimerTests
    {
        private readonly ITestOutputHelper log;

        public CodeTimerTests(ITestOutputHelper log)
        {
            this.log = log;
        }

        [Fact(Skip = "Linux Error")]
        public void Time_Test()
        {
            CodeTimer.Initialize();
            string output = CodeTimer.Time("name", 10000, () =>
            {
                int sum = 0;
                for (int i = 1; i <= 100; i++)
                {
                    sum++;
                }
                sum.ShouldBe(100);
            });
            log.WriteLine($"output: {output}");
            output.ShouldContain("CPU Cycles");
            output.ShouldContain("ms");
        }
    }
}
