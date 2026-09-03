using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Task2
{
    /// <summary>
    /// Представляет клиентское приложение для TCP-чата с поддержкой удаленного счетчика.
    /// Обеспечивает подключение к серверу, отправку и прием сообщений,
    /// а также взаимодействие с серверным счетчиком через специальные команды.
    /// Реализует асинхронный обмен данными с использованием отдельных потоков
    /// для приема и отправки сообщений.
    /// </summary>
    public class Client
    {
        private static TcpClient? _client;
        private static NetworkStream? _stream;
        private static StreamReader? _reader;
        private static StreamWriter? _writer;
        private static bool _isConnected;

        /// <summary>
        /// Запускает клиент
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== TCP Client ===");

            try
            {
                Console.Write("Введите IP адрес сервера (по умолчанию 127.0.0.1): ");
                var ipInput = Console.ReadLine();
                var ipAddress = string.IsNullOrEmpty(ipInput) 
                    ? IPAddress.Loopback 
                    : IPAddress.Parse(ipInput);

                Console.Write("Введите порт сервера (по умолчанию 8888): ");
                var portInput = Console.ReadLine();
                var port = string.IsNullOrEmpty(portInput) 
                    ? 8888 
                    : int.Parse(portInput);

                ConnectToServer(ipAddress, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}\n" + 
                                  "Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Устанавливает TCP-соединение с сервером по указанному IP-адресу и порту.
        /// Инициализирует потоки для чтения и записи данных, запускает фоновый поток для приема сообщений
        /// и переходит в режим отправки сообщений от пользователя.
        /// </summary>
        /// <param name="ipAddress">IP-адрес сервера для подключения.</param>
        /// <param name="port">Порт сервера для подключения.</param>
        private static void ConnectToServer(IPAddress ipAddress, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(ipAddress, port);
                
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
                _isConnected = true;

                Console.WriteLine($"""
                                   Подключено к серверу {ipAddress}:{port}
                                   Введите сообщения (для выхода введите /quit):
                                   Доступные команды для работы со счетчиком:
                                     /get - получить значение счетчика
                                     /add <число> - добавить число к счетчику
                                   {new string('-', 50)}
                                   """);
                
                var receiveThread = new Thread(ReceiveMessages)
                {
                    IsBackground = true
                };
                receiveThread.Start();
                
                SendMessages();
                Disconnect();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Не удалось подключиться к серверу: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Основной цикл отправки сообщений серверу. Читает ввод пользователя с консоли
        /// и передает его через сетевой поток. Поддерживает команду /quit для выхода.
        /// </summary>
        private static void SendMessages()
        {
            try
            {
                while (_isConnected)
                {
                    var message = Console.ReadLine();

                    if (string.IsNullOrEmpty(message)) continue;

                    if (message.Trim().ToLower() == "/quit")
                    {
                        break;
                    }

                    _writer?.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                    Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        /// <summary>
        /// Фоновый поток для непрерывного приема сообщений от сервера.
        /// Полученные сообщения выводятся в консоль. При разрыве соединения инициирует отключение.
        /// </summary>
        private static void ReceiveMessages()
        {
            try
            {
                while (_isConnected)
                {
                    var message = _reader?.ReadLine();
                    if (message == null) break;

                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                    Console.WriteLine($"Ошибка приема сообщения: {ex.Message}");
            }
            finally
            {
                if (_isConnected)
                {
                    Console.WriteLine("Соединение с сервером разорвано.");
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Выполняет корректное отключение от сервера: отправляет команду /quit,
        /// закрывает все сетевые потоки и сокет, освобождает ресурсы.
        /// После завершения ожидает нажатия клавиши перед выходом из приложения.
        /// </summary>
        private static void Disconnect()
        {
            _isConnected = false;
            try
            {
                if (_client?.Connected == true)
                {
                    _writer?.WriteLine("/quit");
                }

                _reader?.Close();
                _writer?.Close();
                _stream?.Close();
                _client?.Close();

                Console.WriteLine("Отключено от сервера.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}