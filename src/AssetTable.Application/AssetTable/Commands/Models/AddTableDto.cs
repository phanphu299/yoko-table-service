using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Tag.Model;
using AHI.Infrastructure.Service.Tag.Extension;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class AddTableDto : TagDtos
    {
        static Func<Domain.Entity.Table, AddTableDto> Converter = Projection.Compile();
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? AssetId { get; set; }
        public IEnumerable<GetColumnByIdDto> Columns { get; set; }

        public static Expression<Func<Domain.Entity.Table, AddTableDto>> Projection
        {
            get
            {
                return model => new AddTableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    AssetId = model.AssetId,
                    Columns = model.Columns.Select(GetColumnByIdDto.Create).OrderBy(x => x.Order),
                    Tags = model.EntityTags.MappingTagDto()
                };
            }
        }

        public static AddTableDto Create(Domain.Entity.Table model)
        {
            return Converter(model);
        }
    }
}
