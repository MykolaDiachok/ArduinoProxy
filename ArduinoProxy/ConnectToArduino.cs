using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ArduinoProxy
{
    /// <summary>
    /// ConnectToArduino - the main class for exchange to Arduino over http
    /// </summary>
    public class ConnectToArduino
    {
        private readonly ILogger<ConnectToArduino> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        public ConnectToArduino(ILogger<ConnectToArduino> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// send simple query to arduino
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<string> SendQuery(string parameter)
        {
            try
            {
                const string baseUrl = "http://192.168.1.61";

                using var client = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};
                var stringT = baseUrl + parameter;
                _logger.LogInformation(stringT);
                using var res = await client.GetAsync(stringT);
                using var content = res.Content;
                var data = await content.ReadAsStringAsync();
                _logger.LogInformation($"{stringT}{Environment.NewLine} return data:{data}");
                return data.Trim() ?? "";
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}", e);
                return "";
            }
            
        }
    }
}
