namespace Task2
{
    /// <summary>
    /// Статический класс-сервер для потокобезопасного управления счетчиком.
    /// </summary>
    public static class CounterServer
    {
        private static int _count = 0;
        private static readonly ReaderWriterLockSlim _lock = new ();

        /// <summary>
        /// Возвращает текущее значение счетчика. Использует блокировку чтения, позволяя 
        /// множеству читателей одновременно получать значение без взаимных блокировок.
        /// </summary>
        /// <returns>Текущее целочисленное значение счетчика.</returns>
        public static int GetCount()
        {
            _lock.EnterReadLock();
            try
            {
                return _count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Добавляет указанное значение к текущему счетчику.
        /// Использует эксклюзивную блокировку записи, обеспечивая атомарность операции.
        /// Во время выполнения этого метода все читатели ожидают завершения записи.
        /// </summary>
        /// <param name="value">Целочисленное значение для добавления к счетчику.</param>
        public static void AddToCount(int value)
        {
            _lock.EnterWriteLock();
            try
            {
                _count += value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}