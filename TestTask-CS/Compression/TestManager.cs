namespace Task1
{
    /// <summary>
    /// Класс для управления тестированием алгоритма сжатия
    /// </summary>
    public class TestManager
    {
        /// <summary>
        /// Запускает меню выбора режима тестирования
        /// </summary>
        public static void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"""
                                   {new string('=', 60)}
                                   ТЕСТИРОВАНИЕ АЛГОРИТМА СЖАТИЯ
                                   {new string('=', 60)}

                                   Выберите режим:
                                   1. Демонстрация заранее заготовленных тестов
                                   2. Ручное тестирование
                                   3. Выход

                                   Ваш выбор: 
                                   """);

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RunPredefinedTests();
                        break;
                    case "2":
                        RunManualTests();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Запускает демонстрацию заранее заготовленных тестов
        /// </summary>
        private static void RunPredefinedTests()
        {
            Console.Clear();
            Console.WriteLine($"""
                               {new string('=', 60)}
                               ДЕМОНСТРАЦИЯ ТЕСТОВ
                               {new string('=', 60)}

                               --- СЖАТИЕ ---

                               """);
            
            TestCompress("aaabbcccdde");
            TestCompress("abc");
            TestCompress("a");
            TestCompress("aaaaa");
            TestCompress("aabbcc");
            TestCompress("");

            // Тесты на декомпрессию
            Console.WriteLine("--- ДЕКОМПРЕССИЯ ---\n");
            
            TestDecompress("a3b2c3d2e");
            TestDecompress("abc");
            TestDecompress("a");
            TestDecompress("a5");
            TestDecompress("a2b2c2");
            TestDecompress("");
            TestDecompress("a10b5c");

            Console.WriteLine(new string('=', 60) +
                              "\n\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }

        /// <summary>
        /// Запускает режим ручного тестирования
        /// </summary>
        private static void RunManualTests()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"""
                                   {new string('=', 60)}
                                   РУЧНОЕ ТЕСТИРОВАНИЕ
                                   {new string('=', 60)}

                                   Выберите действие:
                                   1. Сжать строку
                                   2. Распаковать строку
                                   3. Вернуться в главное меню

                                   Ваш выбор: 
                                   """);

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        PerformCompress();
                        break;
                    case "2":
                        PerformDecompress();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод. Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Выполняет сжатие строки, введенной пользователем
        /// </summary>
        private static void PerformCompress()
        {
            Console.Clear();
            Console.WriteLine("--- СЖАТИЕ СТРОКИ ---" +
                              "\n\nВведите строку для сжатия (только строчные латинские буквы):");
            Console.Write("> ");
            
            var input = Console.ReadLine();

            Console.WriteLine();
            
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Исходная: (пустая строка)\n" +
                                  "Сжатая:   (пустая строка)");
            }
            else
            {
                try
                {
                    var result = CompressionService.Compress(input);
                    Console.WriteLine($"Исходная: {input}\n"+
                                      $"Сжатая:   {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        /// <summary>
        /// Выполняет распаковку строки, введенной пользователем
        /// </summary>
        private static void PerformDecompress()
        {
            Console.Clear();
            Console.WriteLine("--- РАСПАКОВКА СТРОКИ ---" + 
                              "\n\nВведите сжатую строку для распаковки:");
            Console.Write("> ");
            
            var input = Console.ReadLine();

            Console.WriteLine();
            
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Сжатая:     (пустая строка)\n" +
                                  "Исходная:   (пустая строка)");
            }
            else
            {
                try
                {
                    var result = CompressionService.Decompress(input);
                    Console.WriteLine($"Сжатая:     {input}\n" + 
                                      $"Исходная:   {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        /// <summary>
        /// Тестирует сжатие с заданной строкой
        /// </summary>
        private static void TestCompress(string input)
        {
            try
            {
                var result = CompressionService.Compress(input);

                Console.WriteLine(string.IsNullOrEmpty(input) 
                    ? "Исходная: (пустая строка)" 
                    : $"Исходная: {input}");

                Console.WriteLine(string.IsNullOrEmpty(result) 
                    ? "Сжатая:   (пустая строка)" 
                    : $"Сжатая:   {result}");

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исходная: {input}\n" + 
                                  $"Ошибка:   {ex.Message}\n");
            }
        }

        /// <summary>
        /// Тестирует распаковку с заданной строкой
        /// </summary>
        private static void TestDecompress(string input)
        {
            try
            {
                var result = CompressionService.Decompress(input);

                Console.WriteLine(string.IsNullOrEmpty(input)
                    ? "Сжатая:     (пустая строка)"
                    : $"Сжатая:     {input}");

                Console.WriteLine(string.IsNullOrEmpty(result)
                    ? "Исходная:   (пустая строка)"
                    : $"Исходная:   {result}");

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сжатая: {input}\n" + 
                                  $"Ошибка: {ex.Message}\n");
            }
        }
    }
}