using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
        private const int Expiration = 360;
        private const int SlidingExpiration = Expiration/2;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="toArduino"></param>
        public DigitalController(ILogger<DigitalController> logger, IMemoryCache cache, ConnectToArduino toArduino)
        {
            _logger = logger;
            _cache = cache;
            _toArduino = toArduino;
        }
        
        // GET api/<DigitalController>/5
        /// <summary>
        /// read status from  Arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <returns></returns>
        [HttpGet("{id}/r")]
        public async Task<IActionResult> GetR(int id)
        {
            if (_cache.TryGetValue($"normal{id}", out var value)) return Ok(value);
            var answer = await _toArduino.SendQuery($"/digital/{id}/r");
            if (answer.Length == 1)
            {
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(Expiration),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration)
                };
                _cache.Set($"normal{id}", answer, cacheExpiryOptions);
                return Ok(answer);
            }

            return Ok("0");
        }



        /// <summary>
        /// set status to Arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <param name="value">value 0-off, 1-on</param>
        /// <returns></returns>
        [HttpGet("{id}/{value}")]
        public async Task<IActionResult> Set(int id, int value)
        {
            var answer = await _toArduino.SendQuery($"/digital/{id}/{value}");
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(Expiration),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration)
            };
            _cache.Set($"normal{id}", value, cacheExpiryOptions);
            return Ok(value);
        }
    }
}
