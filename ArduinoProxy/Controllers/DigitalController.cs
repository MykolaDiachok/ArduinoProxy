using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ArduinoProxy.Core.Main;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArduinoProxy.Controllers
{
    /// <summary>
    /// read and set to Arduino
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DigitalController : ControllerBase
    {
        private readonly ILogger<DigitalController> _logger;
        private readonly IMemoryCache _cache;
        private readonly ConnectToArduino _toArduino;
        private readonly MemoryCacheEntryOptions CacheEntryOptions;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="toArduino"></param>
        /// <param name="configuration"></param>
        public DigitalController(ILogger<DigitalController> logger, IMemoryCache cache, ConnectToArduino toArduino, IConfiguration configuration)
        {
            _logger = logger;
            _cache = cache;
            _toArduino = toArduino;
            _configuration = configuration;
            var expiration = _configuration.GetValue<int>("Cache:AbsoluteExpirationInSec");
            CacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(expiration),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromSeconds(expiration)
            };
        }
        
        // GET api/<DigitalController>/5
        /// <summary>
        /// read status from  Arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <returns></returns>
        [HttpGet("{id:int}/r")]
        public async Task<IActionResult> GetR(int id)
        {
            if (_cache.TryGetValue($"normal{id}", out var value))
            {
                _logger.LogDebug($"Take from cache for pin:{id} value:{value}");
                return Ok(value);
            }
            var answer = await _toArduino.SendQuery($"/digital/{id}/r");
            if (answer.Length == 1)
            {
                _cache.Set($"normal{id}", answer, CacheEntryOptions);
                return Ok(answer);
            }
            _logger.LogDebug($"Exception to get value for pin:{id} set:0");
            return Ok("0");
        }



        /// <summary>
        /// set status to Arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <param name="value">value 0-off, 1-on</param>
        /// <returns></returns>
        [HttpGet("{id:int}/{value:int}")]
        public async Task<IActionResult> Set(int id, int value)
        {
            await _toArduino.SendQuery($"/digital/{id}/{value}");
            _cache.Set($"normal{id}", value, CacheEntryOptions);
            return Ok(value);
        }
    }
}
