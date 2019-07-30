using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Domain
{
    public class ObjectResult<T> : DetailedResult
    {
        public T Object { get; set; }
    }
}
