using System;

namespace Api.Contracts
{
    public record ValidationErrorResponse
    {
        public List<string> ValidationMessages { get; set; }

        public ValidationErrorResponse()
        {
            ValidationMessages = new List<string>();
        }
    }
}
