using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model
{
    public class ResponseModel<T>
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public string Desc { get; set; }
        public T Data { get; set; }
    }
}
