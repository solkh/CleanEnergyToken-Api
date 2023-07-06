using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CleanEnergyToken_Api.Models
{
    public class ApiErr
    {
        public static ApiErr Create(params string[] msgs) =>
            new ApiErr { Errors = msgs };

        public static ApiErr Create(IEnumerable<string> msgs) =>
            Create(msgs.ToArray());

        public static ApiErr Create(IdentityResult result) =>
            Create(result.Errors.Select(x => x.Code + ":" + x.Description));

        public static ApiErr Create(ModelStateDictionary modelState) =>
            Create(modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));

        public static ApiErr Create(Exception ex) =>
            Create(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine +
                (ex.InnerException != null ?
                "InnerException: " + ex.InnerException?.Message + Environment.NewLine + ex.InnerException?.StackTrace :
                ""));

        /// <summary>
        /// List of errors.
        /// </summary>
        public string[] Errors { get; set; } = Array.Empty<string>();

        internal static object Create(object ex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request Id
        /// </summary>
        public string RequestId = Guid.NewGuid().ToString("D");

        /// <summary>
        /// Response TimeStamp
        /// </summary>
        public DateTime TimeStamp = DateTime.UtcNow;
    }
}
