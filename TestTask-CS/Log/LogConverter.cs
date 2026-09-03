using System.Text.RegularExpressions;

namespace Task3
{
    /// <summary>
    /// Конвертер лог-файлов для стандартизации различных форматов логирования.
    /// Нераспознанные записи сохраняются в отдельный файл(problems.txt) проблемных записей.
    /// </summary>
    public class LogConverter
    {
        /// <summary>
        /// Точка входа в приложение конвертера логов. Запрашивает у пользователя путь к входному лог-файлу,
        /// проверяет его существование, инициализирует пути для выходных файлов
        /// и запускает процесс стандартизации логов.
        /// </summary>
        public static void Run()
        {
            Console.Write("=== Стандартизация лог-файлов ===\n" + 
                          "Введите путь к входному лог-файлу: ");
            var inputPath = Console.ReadLine();

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Файл не найден!");
                return;
            }

            var outputPath = Path.Combine(Path.GetDirectoryName(inputPath) ?? string.Empty, "standardized_log.txt");
            var problemsPath = Path.Combine(Path.GetDirectoryName(inputPath) ?? string.Empty, "problems.txt");

            ProcessLogFile(inputPath, outputPath, problemsPath);

            Console.WriteLine("\nОбработка завершена!" + 
                              $"\nСтандартизированные логи: {outputPath}" + 
                              $"\nПроблемные записи: {problemsPath}");
            Console.ReadKey();
        }

        /// <summary>
        /// Выполняет основную обработку лог-файла: читает все строки, пытается распарсить каждую
        /// с использованием двух поддерживаемых форматов, стандартизирует успешные записи
        /// и сохраняет нераспознанные строки в файл проблем.
        /// </summary>
        /// <param name="inputPath">Путь к входному лог-файлу.</param>
        /// <param name="outputPath">Путь для сохранения стандартизированных логов.</param>
        /// <param name="problemsPath">Путь для сохранения проблемных записей.</param>
        private static void ProcessLogFile(string inputPath, string outputPath, string problemsPath)
        {
            List<string> standardizedLogs = new();
            List<string> problems = new();

            var lines = File.ReadAllLines(inputPath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parsedFormat1 = ParseFormat1(line);
                if (parsedFormat1 != null)
                {
                    standardizedLogs.Add(parsedFormat1);
                    continue;
                }

                var parsedFormat2 = ParseFormat2(line);
                if (parsedFormat2 != null)
                {
                    standardizedLogs.Add(parsedFormat2);
                    continue;
                }

                problems.Add(line);
            }

            File.WriteAllLines(outputPath, standardizedLogs);
            File.WriteAllLines(problemsPath, problems);
        }

        /// <summary>
        /// Парсит строку лога в формате 1: "DD.MM.YYYY HH:MM:SS.fff LEVEL message".
        /// Извлекает дату, время, уровень логирования и сообщение.
        /// В случае успешного парсинга возвращает стандартизированную строку
        /// с полем METHOD = "DEFAULT".
        /// </summary>
        /// <param name="line">Строка лога для парсинга.</param>
        /// <returns>Стандартизированная строка или null, если формат не соответствует.</returns>
        private static string? ParseFormat1(string line)
        {
            var pattern = @"^(\d{2})\.(\d{2})\.(\d{4})\s+(\d{2}:\d{2}:\d{2}\.\d+)\s+(\w+)\s+(.+)$";
            var match = Regex.Match(line, pattern);

            if (!match.Success)
                return null;

            var day = match.Groups[1].Value;
            var month = match.Groups[2].Value;
            var year = match.Groups[3].Value;
            var time = match.Groups[4].Value;
            var logLevel = match.Groups[5].Value;
            var message = match.Groups[6].Value;

            var date = $"{day}-{month}-{year}";
            var standardizedLevel = GetStandardLevel(logLevel);
            var method = "DEFAULT";

            return $"{date}\t{time}\t{standardizedLevel}\t{method}\t{message}";
        }

        /// <summary>
        /// Парсит строку лога в формате 2: "YYYY-MM-DD HH:MM:SS.fff|LEVEL|ID|METHOD|message".
        /// Извлекает дату, время, уровень логирования, метод и сообщение.
        /// В случае успешного парсинга возвращает стандартизированную строку.
        /// </summary>
        /// <param name="line">Строка лога для парсинга.</param>
        /// <returns>Стандартизированная строка или null, если формат не соответствует.</returns>
        private static string? ParseFormat2(string line)
        {
            var pattern = @"^(\d{4})-(\d{2})-(\d{2})\s+([\d:.]+)\|\s*(\w+)\|\d+\|([^|]+)\|\s*(.+)$";
            var match = Regex.Match(line, pattern);

            if (!match.Success)
                return null;

            var year = match.Groups[1].Value;
            var month = match.Groups[2].Value;
            var day = match.Groups[3].Value;
            var time = match.Groups[4].Value;
            var logLevel = match.Groups[5].Value;
            var method = match.Groups[6].Value.Trim();
            var message = match.Groups[7].Value;

            var date = $"{day}-{month}-{year}";
            var standardizedLevel = GetStandardLevel(logLevel);

            return $"{date}\t{time}\t{standardizedLevel}\t{method}\t{message}";
        }

        /// <summary>
        /// Приводит уровень логирования к стандартизированному виду.
        /// Выполняет нормализацию: INFORMATION/INFO → INFO, WARNING/WARN → WARN,
        /// ERROR → ERROR, DEBUG → DEBUG. Неизвестные уровни возвращаются без изменений.
        /// </summary>
        /// <param name="inputLevel">Исходный уровень логирования.</param>
        /// <returns>Стандартизированный уровень логирования.</returns>
        private static string GetStandardLevel(string inputLevel)
        {
            var upperLevel = inputLevel.ToUpper();

            return upperLevel switch
            {
                "INFORMATION" or "INFO" => "INFO",
                "WARNING" or "WARN" => "WARN",
                "ERROR" => "ERROR",
                "DEBUG" => "DEBUG",
                _ => inputLevel
            };
        }
    }
}