using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Task2
{
    /// <summary>
    /// Обработчик клиентского подключения. Управляет обменом сообщениями с одним клиентом,
    /// поддерживает его состояние и обеспечивает корректное освобождение ресурсов.
    /// </summary>
    public class ClientHandler : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly Action<string, ClientHandler> _broadcastMessage;
        private readonly Action<ClientHandler> _removeClient;
        private bool _isConnected;

        public bool IsConnected => _isConnected && _client.Connected;

        /// <summary>
        /// Инициализирует новый экземпляр обработчика клиента с указанными сетевыми параметрами
        /// и делегатами для взаимодействия с сервером.
        /// </summary>
        /// <param name="client">TCP-клиент для взаимодействия.</param>
        /// <param name="broadcastMessage">Делегат для рассылки сообщений всем клиентам.</param>
        /// <param name="removeClient">Делегат для удаления клиента из списка активных подключений.</param>
        public ClientHandler(TcpClient client, Action<string, ClientHandler> broadcastMessage,
            Action<ClientHandler> removeClient)
        {
            _client = client;
            _stream = client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
            _broadcastMessage = broadcastMessage;
            _removeClient = removeClient;
            _isConnected = true;
        }

        /// <summary>
        /// Основной цикл обработки клиента. Постоянно читает входящие сообщения от клиента,
        /// обрабатывает команду /quit и передает полученные сообщения на рассылку всем клиентам.
        /// При разрыве соединения инициирует удаление клиента из списка.
        /// </summary>
        public void HandleClient()
        {
            try
            {
                while (_isConnected && _client.Connected)
                {
                    var message = _reader.ReadLine();
                    if (message == null) break;

                    if (message.Trim().ToLower() == "/quit")
                    {
                        SendMessage("До свидания!");
                        break;
                    }

                    _broadcastMessage(message, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента {GetClientEndPoint()}: {ex.Message}");
            }
            finally
            {
                _isConnected = false;
                _removeClient(this);
            }
        }

        /// <summary>
        /// Отправляет сообщение клиенту через сетевой поток.
        /// </summary>
        /// <param name="message">Текст сообщения для отправки.</param>
        public void SendMessage(string message)
        {
            try
            {
                if (IsConnected)
                {
                    _writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки сообщения клиенту {GetClientEndPoint()}: {ex.Message}");
                _isConnected = false;
            }
        }

        /// <summary>
        /// Возвращает IP-адрес и порт удаленного клиента.
        /// </summary>
        /// <returns>Объект IPEndPoint с информацией о клиенте, или null, если информация недоступна.</returns>
        public IPEndPoint? GetClientEndPoint() => _client.Client.RemoteEndPoint as IPEndPoint;
        
        /// <summary>
        /// Освобождает все используемые ресурсы: закрывает сетевые потоки и TCP-соединение.
        /// Гарантирует корректное завершение работы с клиентом даже при возникновении ошибок.
        /// </summary>
        public void Dispose()
        {
            _isConnected = false;
            try
            {
                _reader.Close();
                _writer.Close();
                _stream.Close();
                _client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при освобождении ресурсов: {ex.Message}");
            }
        }
    }
}