using System.Text;

namespace Task1
{
    /// <summary>
    /// Предоставляет методы для сжатия и декомпрессии строк по алгоритму RLE (Run-Length Encoding)
    /// с оптимизацией для одиночных символов.
    /// </summary>
    public static class CompressionService
    {
        /// <summary>
        /// Сжимает строку, заменяя группы повторяющихся символов на "[символ][количество]".
        /// Для одиночных символов количество не указывается.
        /// </summary>
        /// <param name="input">Исходная строка (только строчные латинские буквы).</param>
        /// <returns>Сжатая строка.</returns>
        /// <exception cref="ArgumentNullException">Если входная строка null.</exception>
        /// <exception cref="ArgumentException">Если строка содержит недопустимые символы.</exception>
        public static string Compress(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "Входная строка не может быть null.");

            if (input.Length == 0)
                return string.Empty;

            ValidateInput(input);

            var result = new StringBuilder();
            var i = 0;

            while (i < input.Length)
            {
                var currentChar = input[i];
                var count = 1;
                
                while (i + count < input.Length && input[i + count] == currentChar) 
                    count++;
                
                result.Append(currentChar);
                
                if (count > 1) 
                    result.Append(count);
                
                i += count;
            }

            return result.ToString();
        }

        /// <summary>
        /// Восстанавливает исходную строку из сжатой.
        /// </summary>
        /// <param name="compressed">Сжатая строка.</param>
        /// <returns>Исходная строка.</returns>
        /// <exception cref="ArgumentNullException">Если входная строка null.</exception>
        /// <exception cref="ArgumentException">Если строка имеет неверный формат.</exception>
        public static string Decompress(string compressed)
        {
            if (compressed == null)
                throw new ArgumentNullException(nameof(compressed), "Входная строка не может быть null.");

            if (compressed.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            var i = 0;

            while (i < compressed.Length)
            {
                var currentChar = compressed[i];
                
                if (!char.IsLower(currentChar))
                    throw new ArgumentException($"Некорректный символ '{currentChar}' в сжатой строке.");

                i++;
                
                var count = 0;
                while (i < compressed.Length && char.IsDigit(compressed[i]))
                {
                    count = count * 10 + (compressed[i] - '0');
                    i++;
                }
                
                if (count == 0) 
                    result.Append(currentChar);
                else 
                    result.Append(currentChar, count);
            }

            return result.ToString();
        }

        /// <summary>
        /// Проверяет, что строка содержит только строчные латинские буквы.
        /// </summary>
        private static void ValidateInput(string input)
        {
            foreach (var c in input.Where(c => !char.IsLower(c) || !char.IsLetter(c)))
            {
                throw new ArgumentException(
                    $"Строка содержит недопустимый символ '{c}'. " +
                    "Разрешены только строчные латинские буквы (a-z).");
            }
        }
    }
}