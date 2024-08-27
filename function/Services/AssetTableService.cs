using System.IO;
using System.Threading.Tasks;
using Function.Model.ImportModel;
using Function.Service.Abstraction;
using AHI.Infrastructure.Import;
using AHI.Infrastructure.Import.Abstraction;
using System;
using System.Linq;

namespace Function.Service
{
    public class AssetTableService : BaseEntityFileImport<AssetTableModel>, IAssetTableService
    {
        private readonly IFileHandler<AssetTableModel> _fileHandler;
        private readonly IEntityImportRepository<AssetTableModel> _repository;
        public AssetTableService(IFileHandler<AssetTableModel> fileHandler, IEntityImportRepository<AssetTableModel> repository)
        {
            _fileHandler = fileHandler;
            _repository = repository;
        }

        protected override IFileHandler<AssetTableModel> GetFileHandler()
        {
            return _fileHandler;
        }

        protected override IEntityImportRepository<AssetTableModel> GetRepository()
        {
            return _repository;
        }
        public async Task RunImportAsync(Guid tableId, Stream stream)
        {
            var fileHandler = GetFileHandler();
            var dataTable = fileHandler.Handle(stream);
            if (dataTable != null && dataTable.Any())
            {
                var repository = GetRepository();
                await repository.CommitAsync(tableId, dataTable);
            }
        }
    }
}
