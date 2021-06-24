namespace PoissonBlending.Lib
{
    public interface ILogService
    {
        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        void LogStarted(string colorModel, bool isAsync);

        /// <summary>
        /// Логирование результатов шага решения.
        /// </summary>
        /// <param name="colorComponentName">Название цветовой компоненты.</param>
        /// <param name="iteration">Номер шага.</param>
        /// <param name="error">Оценка погрешности.</param>
        void LogSolveProgress(string colorComponentName, int iteration, double error, long? elapsedMs);

        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        void LogProcessResult(long elapsedMs);

        void Log(string message);
    }
}
