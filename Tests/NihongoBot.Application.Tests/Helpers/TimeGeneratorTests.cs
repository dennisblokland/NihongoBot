using NihongoBot.Application.Helpers;

namespace NihongoBot.Application.Tests.Helpers;

public class TimeGeneratorTests
{
        [Fact]
        public void GetRandomTimes_ShouldBeAbleToSelectFinalInterval()
        {
                TimeOnly start = new(0, 0);
                TimeOnly end = new(1, 0);
                const int count = 3;

                bool found = false;

                for (int i = 0; i < 1000 && !found; i++)
                {
                        List<TimeOnly> times = TimeGenerator.GetRandomTimes(start, end, count);
                        if (times.Contains(end))
                        {
                                found = true;
                        }
                }

                Assert.True(found, "Final interval was never returned.");
        }
}
