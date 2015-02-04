using System.Collections.Generic;
using System.Text;

namespace LaTeX
{
    public class Table
    {

        public string[] Header { get; set; }
        public List<string[]> Rows { get; set; }

        public string Caption { get; set; }
        public string Label { get; set; }
        public string Format { get; set; }

        public string Injection { get; set; }

        public string ToTeX()
        {
            var builder = new StringBuilder();

            builder.AppendLine(@"\begin{table}[h!]");
            builder.AppendLine(@"\centering");
            builder.AppendLine(@"\caption{" + Caption + "}");
            builder.AppendLine(@"\label{tab:" + Label + "}");
            builder.AppendLine(Injection);
            builder.AppendLine(@"\begin{tabular}{" + Format + @"} \hline");
            AppendHeader(builder);
            AppendRows(builder);
            builder.AppendLine(@"\end{tabular}");
            builder.AppendLine(@"\end{table}");

            return builder.ToString();
        }

        private void AppendHeader(StringBuilder builder)
        {
            for (int i = 0; i < Header.Length; i++)
            {
                builder.Append(Header[i]);
                builder.Append(i < Header.Length - 1 ? " & " : @"\\ \hline");
            }
            builder.AppendLine();
        }

        private void AppendRows(StringBuilder builder)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                for (int j = 0; j < row.Length; j++)
                {
                    builder.Append(row[j]);
                    builder.Append(j < row.Length - 1 ? " & " : @"\\");
                }
                builder.AppendLine(i < Rows.Count - 1 ? "" : @" \hline");
            }
        }

    }
}
