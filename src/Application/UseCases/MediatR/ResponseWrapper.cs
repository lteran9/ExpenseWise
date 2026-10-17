using System;

namespace Application.UseCases.MediatR
{
    public class ResponseWrapper<T>
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public T? Result { get; set; }
        public List<string>? ValidationMessages { get; set; }

        public ResponseWrapper() { }
    }
}
