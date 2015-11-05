using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Configuration;
using UCS.PacketProcessing;
using UCS.Logic;

namespace UCS.Core
{
    static class Debugger
    {
        private static readonly object m_vSyncObject = new object();
        private static readonly TextWriter m_vTextWriter;
        private static int m_vLogLevel;

        static Debugger()
        {
            m_vTextWriter = TextWriter.Synchronized(File.AppendText("logs/debug_" + DateTime.Now.ToString("yyyyMMdd") + ".log"));
            m_vLogLevel = 1;
        }

        public static void SetLogLevel(int level)
        {
            m_vLogLevel = level;
        }

        public static void WriteLine(string text, Exception ex = null, int logLevel = 4, ConsoleColor color = ConsoleColor.White)
        {
            string content = text;
            if (ex != null)
                content += ex.ToString();
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["consoleDebug"]))
            {
                if (color != ConsoleColor.White)
                {
                    Console.ForegroundColor = color;
                }
                else
                {
                    if (logLevel == 5)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }
                }
                Console.WriteLine(content);

                Console.ResetColor();
            }
            else
            {
            }
            if(logLevel <= m_vLogLevel)
            {
                lock (m_vSyncObject)
                {
                    m_vTextWriter.Write(DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss"));
                    m_vTextWriter.Write("\t");
                    m_vTextWriter.WriteLine(content);
                    if (ex != null)
                        m_vTextWriter.WriteLine(ex.ToString());
                    m_vTextWriter.Flush();
                }
            } 
        }
    }
}
