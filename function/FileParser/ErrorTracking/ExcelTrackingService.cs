using System;
using System.Collections.Generic;
using AHI.AssetTable.Function.FileParser.Abstraction;
using AHI.AssetTable.Function.FileParser.ErrorTracking.Model;
using AHI.Infrastructure.SharedKernel.Constant;

namespace AHI.AssetTable.Function.FileParser.ErrorTracking
{
    public class ExcelTrackingService : IImportTrackingService
    {
        protected ICollection<TrackError> _currentErrors { get; set; }
        protected IDictionary<Type, ModelTrackInfo> _trackInfos { get; set; } = new Dictionary<Type, ModelTrackInfo>();
        public IDictionary<string, ICollection<TrackError>> FileErrors { get; } = new Dictionary<string, ICollection<TrackError>>();

        private string _file;
        public string File
        {
            get => _file;
            set
            {
                _file = value;
                _currentErrors = new List<TrackError>();
                _trackInfos.Clear();
                FileErrors[_file] = _currentErrors;
            }
        }

        public bool HasError => (_currentErrors?.Count ?? -1) > 0;

        public virtual void RegisterError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null)
        {
            _currentErrors.Add(new TrackError(message, errorType, validationInfo));
        }
    }
}