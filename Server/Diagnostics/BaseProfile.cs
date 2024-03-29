#region References
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
#endregion

namespace Server.Diagnostics
{
	public abstract class BaseProfile
	{
		public static void WriteAll<T>(TextWriter op, IEnumerable<T> profiles) where T : BaseProfile
		{
			List<T> list = new List<T>(profiles);

			list.Sort(delegate (T a, T b) { return -a.TotalTime.CompareTo(b.TotalTime); });

            for (var index = 0; index < list.Count; index++)
            {
                T prof = list[index];

                prof.WriteTo(op);
                op.WriteLine();
            }
        }

		private readonly string _name;

		private long _count;

		private TimeSpan _totalTime;
		private TimeSpan _peakTime;

		private readonly Stopwatch _stopwatch;

		public string Name => _name;

		public long Count => _count;

		public TimeSpan AverageTime => TimeSpan.FromTicks(_totalTime.Ticks / Math.Max(1, _count));

		public TimeSpan PeakTime => _peakTime;

		public TimeSpan TotalTime => _totalTime;

		protected BaseProfile(string name)
		{
			_name = name;

			_stopwatch = new Stopwatch();
		}

		public virtual void Start()
		{
			if (_stopwatch.IsRunning)
			{
				_stopwatch.Reset();
			}

			_stopwatch.Start();
		}

		public virtual void Finish()
		{
			TimeSpan elapsed = _stopwatch.Elapsed;

			_totalTime += elapsed;

			if (elapsed > _peakTime)
			{
				_peakTime = elapsed;
			}

			_count++;

			_stopwatch.Reset();
		}

		public virtual void WriteTo(TextWriter op)
		{
			op.Write(
				"{0,-100} {1,12:N0} {2,12:F5} {3,-12:F5} {4,12:F5}",
				Name,
				Count,
				AverageTime.TotalSeconds,
				PeakTime.TotalSeconds,
				TotalTime.TotalSeconds);
		}
	}
}
