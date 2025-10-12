namespace Talabat.APIs.Errors
{
    ///ApiValidationErrorResponse
    public class ApiValidationErrorResponse:ApiResponse
    {
        public IEnumerable<string> Errors { get; set; }
        public ApiValidationErrorResponse() : base(400)
        {
            Errors = new List<string>();
        }

    }
}
