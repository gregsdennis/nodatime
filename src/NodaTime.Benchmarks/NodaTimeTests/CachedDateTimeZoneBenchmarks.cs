// Copyright 2010 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using BenchmarkDotNet.Attributes;
using NodaTime.Benchmarks.BclTests;

namespace NodaTime.Benchmarks.NodaTimeTests
{
    [Config(typeof(BenchmarkConfig))]
    public class CachedDateTimeZoneBenchmarks
    {
#if V1
        private const long TicksPerDay = NodaConstants.TicksPerStandardDay;
#else
        private const long TicksPerDay = NodaConstants.TicksPerDay;
#endif

        private static readonly DateTimeZone Pacific = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        private static readonly Instant SummerUtc = Instant.FromDateTimeUtc(TimeZoneInfoBenchmarks.SummerUtc);
        private static readonly LocalDateTime SummerLocal = SummerUtc.InZone(Pacific).LocalDateTime;

        private readonly Instant[] cacheInstants = new Instant[100];
        private readonly Instant[] noCacheInstants = new Instant[500];
        private readonly DateTimeZone paris = DateTimeZoneProviders.Tzdb["Europe/Paris"];
        private readonly Instant[] twoYearsCacheInstants = new Instant[365];
        private int cacheIndex;
        private int noCacheIndex;
        private int twoYearsCacheIndex;

        public CachedDateTimeZoneBenchmarks()
        {
            var adjustment = Duration.FromTicks(TicksPerDay * 365);
            for (int i = 0; i < noCacheInstants.Length; i++)
            {
                noCacheInstants[i] = NodaConstants.UnixEpoch + (adjustment * i);
            }
            for (int i = 0; i < cacheInstants.Length; i++)
            {
                cacheInstants[i] = NodaConstants.UnixEpoch + (adjustment * i);
            }
            var twoDays = Duration.FromTicks(TicksPerDay * 2);
            for (int i = 0; i < twoYearsCacheInstants.Length; i++)
            {
                twoYearsCacheInstants[i] = NodaConstants.UnixEpoch + (twoDays * i);
            }
        }

        [Benchmark]
        public void GetZoneInterval_UnixEpoch()
        {
            paris.GetZoneInterval(NodaConstants.UnixEpoch);
        }

        // Instants across 500 years, which breaks the caching
        [Benchmark]
        public void GetZoneInterval_CacheBustingInstants()
        {
            paris.GetZoneInterval(noCacheInstants[noCacheIndex]);
            noCacheIndex = (noCacheIndex + 1) % noCacheInstants.Length;
        }

        // Instants across 100 years, so we'll always hit the cache.
        [Benchmark]
        public void GetZoneInterval_CachedInstants()
        {
            paris.GetZoneInterval(cacheInstants[cacheIndex]);
            cacheIndex = (cacheIndex + 1) % cacheInstants.Length;
        }

        [Benchmark]
        public void GetZoneInterval_TwoYears()
        {
            paris.GetZoneInterval(twoYearsCacheInstants[twoYearsCacheIndex]);
            twoYearsCacheIndex = (twoYearsCacheIndex + 1) % twoYearsCacheInstants.Length;
        }

        [Benchmark]
        public void GetUtcOffset()
        {
            paris.GetUtcOffset(NodaConstants.UnixEpoch);
        }

        [Benchmark]
        public void ConvertUtcToLocal()
        {
            SummerUtc.InZone(Pacific);
        }

        [Benchmark]
        public void ConvertLocalToUtc()
        {
            SummerLocal.InZoneStrictly(Pacific);
        }
    }
}