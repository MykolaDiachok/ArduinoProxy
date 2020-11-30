using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ArduinoProxy.Core.Main;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArduinoProxy.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class Digital2Controller : ControllerBase
    {
        private readonly ILogger<Digital2Controller> _logger;
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
        public Digital2Controller(ILogger<Digital2Controller> logger, IMemoryCache cache, ConnectToArduino toArduino)
        {
            _logger = logger;
            _cache = cache;
            _toArduino = toArduino;
        }
        
        // GET api/<DigitalController>/5
        /// <summary>
        /// read reverse status from Arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <returns></returns>
        [HttpGet("{id:int}/r")]
        public async Task<IActionResult> GetR(int id)
        {
            if (_cache.TryGetValue($"revers{id}", out var value)) return Ok(value);
            var answer = await _toArduino.SendQuery($"/digital/{id}/r");
            if (answer.Length == 1)
            {
                var newval = ReturnRevertVal(Convert.ToInt16(answer));
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(Expiration),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration)
                };
                _cache.Set($"revers{id}", newval, cacheExpiryOptions);
                return Ok(newval);
            }

            return Ok("0");

        }

        private static string ReturnRevertVal(int val)
        {
            return val == 0 ? "1" : "0";
        }

        /// <summary>
        /// set revert status to arduino
        /// </summary>
        /// <param name="id">pin</param>
        /// <param name="value">value 0-on, 1-off</param>
        /// <returns></returns>
        [HttpGet("{id:int}/{value:int}")]
        public async Task<IActionResult> Set(int id, int value)
        {
            var newval = ReturnRevertVal(value);
            var answer = await _toArduino.SendQuery($"/digital/{id}/{newval}");
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(Expiration),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromSeconds(SlidingExpiration)
            };
            _cache.Set($"revers{id}", value.ToString(), cacheExpiryOptions);
            return Ok(value.ToString());
        }
    }
}
