using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArduinoProxy.Core.Main
{
    /// <summary>
    /// ConnectToArduino - the main class for exchange to Arduino over http
    /// </summary>
    public class ConnectToArduino
    {
        private readonly ILogger<ConnectToArduino> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public ConnectToArduino(ILogger<ConnectToArduino> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
                var baseUrl = _configuration.GetValue<string>("ArduinoServer:server"); //["ArduinoServer"];

                using var client = new HttpClient {Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("ArduinoServer:connectionTimeout")) };
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
