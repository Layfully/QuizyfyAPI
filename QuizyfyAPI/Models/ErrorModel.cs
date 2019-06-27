using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// Error with status code and message.
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// Int which tells us error status code.
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
