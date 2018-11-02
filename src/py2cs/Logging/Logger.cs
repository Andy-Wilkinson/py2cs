using System;

namespace Py2Cs.Logging
{
    public class Logger
    {
        public void Log(string str, LogLevel level)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColor(level);

            Console.WriteLine(str);

            Console.ForegroundColor = oldColor;
        }

        public void LogHeading(string str, LogLevel level)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColor(level);

            Console.WriteLine();
            Console.WriteLine(str);
            Console.WriteLine(new string('-', str.Length));

            Console.ForegroundColor = oldColor;
        }

        private ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return Console.ForegroundColor;
            }
        }
    }
}