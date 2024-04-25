using System;
using System.Linq;
using ConsoleApp.Constants;

namespace ConsoleApp.Helpers
{
  internal class ConsoleTable
  {
    internal static void PrintLine()
    {
      Console.WriteLine(new string('-', Const.TableWidth));
    }

    internal static void PrintRow(params string[] columns)
    {
      var width = (Const.TableWidth - columns.Length) / columns.Length;
      var row = columns.Aggregate("|", (current, column) => current + (AlignCentre(column, width) + "|"));

      Console.WriteLine(row);
    }

    private static string AlignCentre(string text, int width)
    {
      text = text.Length > width ? string.Concat(text.AsSpan(0, width - 3), "...") : text;

      return string.IsNullOrEmpty(text) ? new string(' ', width) : text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
    }
  }
}
