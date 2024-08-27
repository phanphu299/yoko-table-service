using System.Collections.Generic;
using AHI.AssetTable.Function.FileParser.ErrorTracking.Model;
using AHI.Infrastructure.SharedKernel.Constant;

namespace AHI.AssetTable.Function.FileParser.Abstraction
{
    public interface IErrorService
    {
        bool HasError { get; }

        void RegisterError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null);
    }

    public interface IImportTrackingService : IErrorService
    {
        IDictionary<string, ICollection<TrackError>> FileErrors { get; }
        string File { get; set; }
    }
}
