using System;
using System.Threading;

namespace RodSoft.Core.Communications
{
    /// <summary>
    /// Базовый класс организации измерений и других фоновых операций в отдельном потоке
    /// </summary>
    public abstract class BackgroundCircleBase
    {
        /// <summary>
        /// Дополнительный поток
        /// </summary>
        public Thread _RegistrationThead;

        /// <summary>
        /// Интервал выполнения
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Сообщение об ошибках, возникающих при работе дополнительного потока
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Флаг требования прекращения работы дополнительного потока
        /// </summary>
        private bool _MustAbort = false;

        /// <summary>
        /// Конструктор объекта запуска дополнительного потока
        /// </summary>
        /// <param name="interval">Интервал опроса</param>
        public BackgroundCircleBase(int interval)
        {
            this.Interval = interval;
        }

        /// <summary>
        /// Метод реализации действий, выполняемых в дополнительном потоке
        /// </summary>
        protected abstract void ExecuteImplementation();

        /// <summary>
        /// Метод цикла в допонительном потоке
        /// </summary>
        protected void Execute()
        {
            while (!_MustAbort)
            {
                DateTime t = DateTime.Now;
                try
                {
                    ExecuteImplementation();
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                }
                TimeSpan s1 = DateTime.Now.Subtract(t);
                if (this.Interval - s1.TotalMilliseconds > 0)
                    Thread.Sleep(Convert.ToInt32(this.Interval - s1.TotalMilliseconds));
            }
        }

        /// <summary>
        /// Запуск дополнительного потока
        /// </summary>
        public void Start()
        {
            if (_RegistrationThead == null || (_RegistrationThead != null && _RegistrationThead.ThreadState == ThreadState.Stopped))
            {
                _RegistrationThead = new Thread(new ThreadStart(Execute));
                _RegistrationThead.IsBackground = true;
                _RegistrationThead.Start();
            }
        }

        /// <summary>
        /// Завершение дополнительного потока
        /// </summary>
        public void Stop()
        {
            _MustAbort = true;
        }

    }
}
