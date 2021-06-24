namespace PoissonBlending.Lib
{
    public class LogService : ILogService
    {
        private readonly bool _showIntermediateProgress;
        private readonly LogProgressDelegate _logProgress;

        public LogService(LogProgressDelegate logProgress, bool showIntermediateProgress)
        {
            _showIntermediateProgress = showIntermediateProgress;
            _logProgress = logProgress;
        }

        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        public void LogStarted(string colorModel, bool isAsync)
        {
            if (_logProgress == null)
            {
                return;
            }

            _logProgress($"{(isAsync ? "Async b" : "B")}lending started for {colorModel} color model.");
        }

        /// <summary>
        /// Логирование результатов шага решения.
        /// </summary>
        /// <param name="colorComponentName">Название цветовой компоненты.</param>
        /// <param name="iteration">Номер шага.</param>
        /// <param name="error">Оценка погрешности.</param>
        public void LogSolveProgress(string colorComponentName, int iteration, double error, long? elapsedMs)
        {
            if (_logProgress == null)
            {
                return;
            }

            if (elapsedMs.HasValue)
            {
                _logProgress($"Blending finished in {elapsedMs}ms; Color component: {colorComponentName}; Iterations: {iteration}.");
            }
            else if (_showIntermediateProgress)
            {
                _logProgress($"Color component: {colorComponentName}; Iteration: {iteration}; Error: {error}.");
            }
        }

        /// <summary>
        /// Логирование результата наложения.
        /// </summary>
        /// <param name="elapsedMs">Время выполнения в миллисекундах.</param>
        public void LogProcessResult(long elapsedMs)
        {
            if (_logProgress == null)
            {
                return;
            }

            _logProgress($"Blending finished in {elapsedMs}ms.");
            _logProgress("");
        }

        public void Log(string message)
        {
            _logProgress(message);
        }
    }
}
