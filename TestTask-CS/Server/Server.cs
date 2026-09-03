using System.Net;
using System.Net.Sockets;

namespace Task2
{
    /// <summary>
    /// Основной класс серверного приложения.
    /// </summary>
    public class Server
    {
        private static readonly List<ClientHandler> _clients = new();
        private static readonly Lock _clientsLock = new();
        private static TcpListener? _server;

        /// <summary>
        /// Запускает сервер
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== TCP Server ===");

            try
            {
                Console.Write($"""
                               Выберите режим запуска:
                               1 - Локальный (127.0.0.1)
                               2 - Сетевой (автоматическое определение IP и порта)
                               Ваш выбор (1 или 2): 
                               """);
                var choice = Console.ReadLine();

                IPAddress? ipAddress;
                int port;

                if (choice == "1")
                {
                    ipAddress = IPAddress.Loopback;
                    Console.Write("Введите порт (по умолчанию 8888): ");
                    var portInput = Console.ReadLine();
                    
                    port = string.IsNullOrEmpty(portInput) 
                        ? 8888 
                        : int.Parse(portInput);
                    
                    Console.WriteLine($"Запуск в локальном режиме на {ipAddress}:{port}");
                }
                else
                {
                    ipAddress = GetLocalIPAddress();
                    if (ipAddress == null)
                    {
                        Console.WriteLine("Не удалось определить IP адрес. Использую localhost.");
                        ipAddress = IPAddress.Loopback;
                    }
                    
                    var random = new Random();
                    port = random.Next(1024, 65535);

                    Console.WriteLine($"""
                                      Запуск в сетевом режиме.
                                      IP адрес: {ipAddress}
                                      Порт: {port}
                                      """);
                }
                
                var counterDisplayThread = new Thread(DisplayCounterStatus);
                counterDisplayThread.IsBackground = true;
                counterDisplayThread.Start();

                StartServer(ipAddress, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Определяет локальный IPv4-адрес компьютера в сети.
        /// Перебирает все сетевые интерфейсы и возвращает первый найденный
        /// IPv4-адрес, исключая loopback (127.0.0.1).
        /// </summary>
        /// <returns>Объект IPAddress с локальным IP-адресом, или null при ошибке.</returns>
        private static IPAddress? GetLocalIPAddress()
        {
            try
            {
                var hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);

                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ip))
                    {
                        return ip;
                    }
                }
                
                return IPAddress.Loopback;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Фоновый поток для мониторинга и отображения изменений счетчика.
        /// Каждую секунду проверяет текущее значение и выводит информацию
        /// об изменениях с указанием времени и разницы.
        /// </summary>
        private static void DisplayCounterStatus()
        {
            Console.WriteLine($"""
                              
                              === Мониторинг счетчика ===
                              Сервер отслеживает изменения счетчика
                              {new string('-', 50)}
                              """);

            var lastValue = 0;
            while (true)
            {
                Thread.Sleep(1000);
                var currentValue = CounterServer.GetCount();

                if (currentValue != lastValue)
                {
                    var difference = currentValue - lastValue;
                    Console.WriteLine(
                        $"[{DateTime.Now:HH:mm:ss}] Счетчик изменен на {difference}. Текущее значение: {currentValue}");
                    lastValue = currentValue;
                }
            }
        }

        /// <summary>
        /// Запускает TCP-сервер на указанном IP-адресе и порту.
        /// Инициализирует прослушивание входящих подключений, создает обработчики
        /// для каждого нового клиента, выводит информацию о запуске и доступных адресах.
        /// Обрабатывает системное прерывание (Ctrl+C) для корректного завершения.
        /// </summary>
        /// <param name="ipAddress">IP-адрес для прослушивания.</param>
        /// <param name="port">Порт для прослушивания.</param>
        private static void StartServer(IPAddress ipAddress, int port)
        {
            _server = new TcpListener(ipAddress, port);
            _server.Start();

            Console.WriteLine($"""
                               {new string('=', 50)}
                               СЕРВЕР ЗАПУЩЕН!
                               IP адрес: {ipAddress}
                               Порт: {port}
                               Полный адрес для подключения: {ipAddress}:{port}
                               """);

            if (!ipAddress.Equals(IPAddress.Loopback))
            {
                Console.WriteLine("\nДоступные IP адреса для подключения:");

                var hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);
                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"  {ip}:{port}");
                    }
                }
            }

            Console.WriteLine(new string('=', 50));
            Console.WriteLine("Ожидание подключений...");

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                StopServer();
            };

            try
            {
                while (true)
                {
                    var client = _server.AcceptTcpClient();
                    var clientHandler = new ClientHandler(client, BroadcastMessage, RemoveClient);
                    lock (_clientsLock)
                    {
                        _clients.Add(clientHandler);
                    }

                    var clientThread = new Thread(clientHandler.HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start();

                    var clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine($"Новое подключение: {clientEndPoint?.Address}:{clientEndPoint?.Port}\n" + 
                                      $"Активных клиентов: {_clients.Count}");
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                Console.WriteLine("Сервер остановлен.");
            }
        }

        /// <summary>
        /// Обрабатывает входящие сообщения от клиентов и выполняет маршрутизацию.
        /// Распознает специальные команды для работы со счетчиком (/get, /add),
        /// обрабатывает их и отправляет ответ отправителю. Обычные сообщения
        /// рассылаются всем подключенным клиентам, кроме отправителя.
        /// </summary>
        /// <param name="message">Текст полученного сообщения.</param>
        /// <param name="sender">Обработчик клиента-отправителя.</param>
        private static void BroadcastMessage(string message, ClientHandler sender)
        {
            if (message == "/quit") return;

            if (message.StartsWith("/get"))
            {
                var value = CounterServer.GetCount();
                sender.SendMessage($"[Счетчик] Текущее значение: {value}");
                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss}] Клиент {GetClientInfo(sender)} запросил значение: {value}");
                return;
            }

            if (message.StartsWith("/add "))
            {
                var parts = message.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out var addValue))
                {
                    CounterServer.AddToCount(addValue);
                    var newValue = CounterServer.GetCount();
                    var response = $"[Счетчик] Добавлено {addValue}. Новое значение: {newValue}";
                    sender.SendMessage(response);

                    var clientInfo = GetClientInfo(sender);
                    Console.WriteLine(
                        $"[{DateTime.Now:HH:mm:ss}] Клиент {clientInfo} добавил {addValue}. Текущее значение: {newValue}");
                    return;
                }

                sender.SendMessage("[Ошибка] Используйте: /add <число>");
                return;
            }

            var senderInfo = GetClientInfo(sender);
            var formattedMessage = $"[{senderInfo}]> {message}";

            Console.WriteLine($"Рассылка: {formattedMessage}");

            lock (_clientsLock)
            {
                foreach (var client in _clients.ToArray())
                {
                    if (client != sender && client.IsConnected)
                    {
                        try
                        {
                            client.SendMessage(formattedMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка отправки клиенту {GetClientInfo(client)}: {ex.Message}");
                            RemoveClient(client);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет клиента из списка активных подключений.
        /// Выполняет потокобезопасное удаление с использованием блокировки,
        /// выводит информацию об отключении и освобождает ресурсы клиента.
        /// </summary>
        /// <param name="client">Обработчик клиента для удаления.</param>
        private static void RemoveClient(ClientHandler client)
        {
            lock (_clientsLock)
            {
                if (_clients.Remove(client))
                {
                    var clientEndPoint = client.GetClientEndPoint();
                    Console.WriteLine($"Клиент отключен: {clientEndPoint?.Address}:{clientEndPoint?.Port}");
                    Console.WriteLine($"Активных клиентов: {_clients.Count}");
                }
            }

            client.Dispose();
        }

        /// <summary>
        /// Формирует строковое представление информации о клиенте
        /// в формате "IP-адрес:порт".
        /// </summary>
        /// <param name="client">Обработчик клиента.</param>
        /// <returns>Строка с IP-адресом и портом клиента.</returns>
        private static string GetClientInfo(ClientHandler client)
        {
            var endPoint = client.GetClientEndPoint();
            return $"{endPoint?.Address}:{endPoint?.Port}";
        }

        /// <summary>
        /// Выполняет корректное завершение работы сервера.
        /// Отправляет всем клиентам уведомление о завершении, освобождает
        /// ресурсы всех клиентских подключений, останавливает TCP-листенер
        /// и завершает процесс с кодом 0.
        /// </summary>
        private static void StopServer()
        {
            Console.WriteLine("\nЗавершение работы сервера...");

            lock (_clientsLock)
            {
                foreach (var client in _clients.ToArray())
                {
                    try
                    {
                        client.SendMessage("Сервер завершает работу...");
                        client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отключении клиента: {ex.Message}");
                    }
                }

                _clients.Clear();
            }

            _server?.Stop();
            Console.WriteLine("Сервер остановлен.");
            Environment.Exit(0);
        }
    }
}