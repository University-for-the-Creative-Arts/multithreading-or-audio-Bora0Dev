using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ImageJobs
{
    public struct TelemetryRow
    {
        public string Image;
        public string Mode;
        public int BatchSize;
        public int WorkerThreads;
        public double Milliseconds;
        public long Result;
    }

    public class TelemetryTable
    {
        private readonly List<TelemetryRow> _rows = new List<TelemetryRow>();

        public void Add(TelemetryRow row) => _rows.Add(row);

        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("image,mode,batch_size,worker_threads,milliseconds,result");
            foreach (var r in _rows)
            {
                sb.Append(Escape(r.Image)).Append(',');
                sb.Append(Escape(r.Mode)).Append(',');
                sb.Append(r.BatchSize.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(r.WorkerThreads.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(r.Milliseconds.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(r.Result.ToString(CultureInfo.InvariantCulture)).AppendLine();
            }
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Contains(",") || s.Contains("\""))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
