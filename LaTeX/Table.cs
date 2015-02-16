using System.Collections.Generic;
using System.Text;

namespace LaTeX
{
    public class Table
    {

        public string[] Header { get; set; }
        public string[] Units { get; set; }
        public List<string[]> Rows { get; set; }

        public string Caption { get; set; }
        public string Label { get; set; }
        public string Format { get; set; }

        public string Injection { get; set; }

        public string ToTeX()
        {
            var builder = new StringBuilder();

            builder.AppendLine(@"\begin{table}[h!]");
            builder.AppendLine(@"\caption{" + Caption + "}");
            builder.AppendLine(@"\label{tab:" + Label + "}");
            if(!string.IsNullOrEmpty(Injection)) builder.AppendLine(Injection);
            builder.AppendLine(@"\begin{adjustbox}{center}");
            builder.AppendLine(@"\begin{tabular}{" + Format + @"}  \hline");
            AppendHeader(builder);
            builder.AppendLine(@" \hline");
            AppendRows(builder);
            builder.AppendLine(@" \hline");
            builder.AppendLine(@"\end{tabular}");
            builder.AppendLine(@"\end{adjustbox}");
            builder.AppendLine(@"\end{table}");

            return builder.ToString();
        }

        private void AppendHeader(StringBuilder builder)
        {
            for (int i = 0; i < Header.Length; i++)
            {
                builder.Append(Header[i]);
                builder.Append(i < Header.Length - 1 ? " & " : @"\\");
            }
            if (Units == null) return;

            builder.AppendLine();
            for (int i = 0; i < Units.Length; i++)
            {
                builder.Append(Units[i]);
                builder.Append(i < Units.Length - 1 ? " & " : @"\\");
            }
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
                if(i < Rows.Count - 1) builder.AppendLine();
            }
        }

    }
}
