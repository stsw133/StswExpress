using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace StswExpress.Globals
{
    public static class Log
    {
        /// <summary>
        /// Plik tekstowy do wstawiania komunikatów.
        /// </summary>
        private static FileInfo logFile;
        /// <summary>
        /// Data utworzenia pliku Log
        /// </summary>
        private static DateTime LogFileCreateTime = DateTime.Today;
        /// <summary>
        /// Nazwa pliku tekstowego logu.
        /// </summary>
        private static string LogFileName = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), $"log {LogFileCreateTime:yyyy-MM-dd)}.log");
        /// <summary>
        /// Długość pliku przeznaczonego do archiwizacji.
        /// </summary>
        private const long ArchivizationFileLenght = 40 * 1024 * 1024; /// 40 MB
        /// <summary>
        /// Kodowanie pliku tektowego logu.
        /// </summary>
        private static Encoding ansi = Encoding.GetEncoding("windows-1250");
        /// <summary>
        /// Piska tekstowy do pliku.
        /// </summary>
        private static StreamWriter strWr;
        /// <summary>
        /// Wymuszanie pokazywania nazw funkcji wywołujących w komunikatach logów
        /// </summary>
        private const bool EnforceShowCallerFunction = true;
        /// <summary>
        /// Dopisywanie do logu obecnego użycia pamięci
        /// </summary>
        private const bool ShowMemoryUsage = false;
        /// <summary>
        /// Obecny proces - do wyciągania informacji o pamięci
        /// </summary>
        private static Process process;

        /// <summary>
        /// Inicjalizuje log w postaci pliku tekstowego.
        /// </summary>
        /// <returns>Kod błędu lub 0 gdy poprawny.</returns>
        private static void InitializeTextFileLog()
        {
            logFile = new FileInfo(LogFileName);
            if (logFile != null)
            {
                /// Tworzymy wpis informacji startowej              
                WriteTextFileEntry($"{DateTime.Now:yyyy-MM-dd | HH:mm:ss} |   v{Global.AppVersion()}{Environment.NewLine}");
                WriteLogEntry($"ConnStr: {SQL.connDef}");
            }
        }

        /// <summary>
        /// Archiwizuje plik logu jeżeli przekracza on zadaną wielkość.
        /// </summary>
        /// <returns>Kod błędu lub 0 gdy poprawny.</returns>
        public static void FileLogArchivization()
        {
            if (File.Exists(LogFileName))
            {
                if (logFile.Length > ArchivizationFileLenght)
                {
                    File.Copy(LogFileName, LogFileName.Replace(".", $"_archive_{DateTime.Now:yyyyMMddhhmmss}."));
                    File.WriteAllText(LogFileName, string.Empty, ansi);
                }
            }
        }

        /// <summary>
        /// Wpisuje zadany tekst do plik tekstowego.
        /// </summary>
        /// <param name="text">Zadany tekst.</param>
        /// <returns>Kod błędu lub 0 gdy poprawny.</returns>
        private static void WriteTextFileEntry(string text)
        {
            try
            {
                FileLogArchivization();
                FileLogDateCheck();

                /// Tworzymy nowy pisak do pliku, zapisujemy informację i zamykamy pisak
                using (strWr = new StreamWriter(LogFileName, true, ansi))
                {
                    strWr.Write(text);
                }
            }
            catch { }
        }

        public static void FileLogDateCheck()
        {
            if (File.Exists(LogFileName))
            {
                DateTime today = DateTime.Today;
                if (today > LogFileCreateTime)
                {
                    LogFileCreateTime = today;
                    LogFileName = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), $"log {LogFileCreateTime:yyyy-MM-dd}.log");
                    WriteTextFileEntry($"{DateTime.Now:yyyy-MM-dd | HH:mm:ss} |   v{Global.AppVersion()}{Environment.NewLine}");
                    WriteTextFileEntry($"{DateTime.Now:yyyy-MM-dd | HH:mm:ss} | Kontynuacja logu z dnia poprzedniego{Environment.NewLine}");
                }
            }
        }

        /// <summary>
        /// Inicjalizuje logi programu.
        /// </summary>
        /// <param name="tb">Tekstowe okno logu.</param>
        /// <returns>Kod błędu lub 0 gdy poprawny.</returns>
        public static void InitializeLogs()
        {
            process = Process.GetCurrentProcess();
            InitializeTextFileLog();
        }

        /// <summary>
        /// Wstawia informację końcową i zamyka logowanie.
        /// </summary>
        /// <returns>Kod błędu lub 0 gdy poprawny.</returns>
        public static void CloseLogs()
        {
            WriteTextFileEntry($"{DateTime.Now:yyyy-MM-dd | HH:mm:ss} | Koniec pracy.{Environment.NewLine}");
            process.Dispose();
        }

        /// <summary>
        /// Wpisuje zadany text do logów.
        /// </summary>
        /// <param name="text">Zadany tekst.</param>
        /// <returns>Kod błędu lub 0 gdu poprawny.</returns>
        public static void WriteLogEntry(string text)
        {
            WriteLogEntry(text, 0, false);
        }

        /// <summary>
        /// Wpisuje zadany tekst wraz z informcją o funkcji do wszystkich logów.
        /// </summary>
        /// <param name="text">Zadany tekst.</param>
        /// <param name="addfunccallername">Czy dodawać informację o funkcji.</param>
        /// <returns>Kod błędu lub 0 jeżeli poprawny.</returns>
        public static void WriteLogEntry(string text, bool addfunccallername)
        {
            WriteLogEntry(text, 0, addfunccallername);
        }

        /// <summary>
        /// Wpisuje zadany tekst do wybranych logów.
        /// </summary>
        /// <param name="text">Zadany tekst.</param>
        /// <param name="log">Wybrany log. 1 - pole tekstowe logu, 2- plik logu, inna liczba - wszystkie logi.</param>
        /// <returns>Kod błedu lub 0 gdy poprawny.</returns>
        public static void WriteLogEntry(string text, int log)
        {
            WriteLogEntry(text, log, false);
        }

        /// <summary>
        /// Wpisuje zadany tekst wraz z informcją o funkcji do wybranego logu.
        /// </summary>
        /// <param name="text">Zadany tekst.</param>
        /// <param name="log">Wybrny log. 1 - pole tekstowe logu, 2- plik logu, inna liczba - wszystkie logi.</param>
        /// <param name="addfunccallername">Czy dodawać informację o funkcji.</param>
        /// <returns>od błędu lub 0 gdy poprawny.</returns>
        public static void WriteLogEntry(string text, int log, bool addfunccallername)
        {
            /// Dodaje informację o funkcji wywołuącej wstawiany komunikat
            if (addfunccallername == true || EnforceShowCallerFunction == true)
            {
                StackTrace stackTrace = new StackTrace();
                /// Pobieramy nazwę przedostatniej funkcji bo ostatnia jest zawsze przeciążoną funkcją WriteLogEntry
                text += $" ({stackTrace.GetFrame(2).GetMethod().Name})";
            }

            /// Tworzy tekst wstawiany do logu
            string entrytext = $"{DateTime.Now:yyyy-MM-dd | HH:mm:ss.fff} | {text}{Environment.NewLine}";
            /*
            if (ShowMemoryUsage)
            {
                process.Refresh();
                entrytext += $"{DateTime.Now:yyyy-MM-dd | HH:mm:ss.fff} | Zestaw roboczy: {string.Format("{0:n0}", process.WorkingSet64 / 1024)} K   Pamięć prywatna: {string.Format("{0:n0}", process.PrivateMemorySize64 / 1024)} K{Environment.NewLine}";
            }
            */
            WriteTextFileEntry(entrytext);
        }
    }
}
