using System;
using System.Linq;
using System.Linq.Expressions;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Model;

namespace AssetTable.Application.TableList.Command.Model
{
    public class GetTableListDto : TagDtos
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? AssetId { get; set; }
        public string AssetName { get; set; }
        public bool Disabled { get; set; }
        public string CurrentUserUpn { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string LockedByUpn { get; set; }
        public string CreatedBy { get; set; }
        public string ResourcePath { get; set; }
        static Func<Domain.Entity.Table, GetTableListDto> Converter = Projection.Compile();
        public static Expression<Func<Domain.Entity.Table, GetTableListDto>> Projection
        {
            get
            {
                return entity => new GetTableListDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    AssetId = entity.AssetId,
                    Disabled = entity.Deleted,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    AssetName = entity.AssetName,
                    CreatedBy = entity.CreatedBy,
                    ResourcePath = entity.ResourcePath,
                    Tags = entity.EntityTags.MappingTagDto()
                };
            }
        }

        public static GetTableListDto Create(Domain.Entity.Table model)
        {
            if (model != null)
            {
                return Converter(model);
            }
            return null;
        }
    }
}
