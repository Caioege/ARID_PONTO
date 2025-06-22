namespace AriD.GerenciamentoDePonto.Helpers
{
    public static class Logger
    {
        public static string CreateAndGetLogDirectory()
        {
            var root = Directory.GetCurrentDirectory();
            var logPath = Path.Combine(root, "logs");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            return logPath;
        }

        /// <summary>
        /// Salva o Log da API
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="tenantOrigem"></param>
        public static void Write(Exception ex, KeyValuePair<int, string> congregacao)
        {
            if (ex as ApplicationException == null)
            {
                var message = ex.Message;
                if (congregacao.Key > 0)
                    message = $"Congregação: [{congregacao.Key}] {congregacao.Value}\n{ex.Message}";
                if (ex.InnerException != null)
                    message += $"\nInnerException: {ex.InnerException.ToString()}";
                if (ex.Source != null)
                    message += $"\nSource: {ex.Source}";
                if (ex.StackTrace != null)
                    message += $"\nStack Trace: {ex.StackTrace}";

                Write(message);
            }
        }

        /// <summary>
        /// Salva o Log da API
        /// </summary>
        /// <param name="ex"></param>
        public static void Write(Exception ex)
        {
            if (ex as ApplicationException == null)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                    message += $": {ex.InnerException.ToString()}";
                if (ex.Source != null)
                    message += $"\nSource: {ex.Source}";
                if (ex.StackTrace != null)
                    message += $"\nStack Trace: {ex.StackTrace}";

                Write(message);
            }
        }

        public static async Task Write(string message)
        {
            var now = DateTime.Now;
            var pathFile = Path.Combine(CreateAndGetLogDirectory(), $"{now.ToString("yyyy-MM-dd")}_log.txt");
            if (!File.Exists(pathFile))
                using (File.Create(pathFile)) { }

            var messageToWrite = string.Format("\r\n{0}:\n{1}\r\n", now.ToString("dd/MM/yyyy HH:mm:ss"), message);
            await File.AppendAllTextAsync(pathFile, messageToWrite);
        }
    }
}
