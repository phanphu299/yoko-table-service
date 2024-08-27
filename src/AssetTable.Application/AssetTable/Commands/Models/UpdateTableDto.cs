using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Tag.Model;
using AHI.Infrastructure.Service.Tag.Extension;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class UpdateTableDto : TagDtos
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<GetColumnByIdDto> Columns { get; set; }

        static Func<Domain.Entity.Table, UpdateTableDto> Converter = Projection.Compile();
        public static Expression<Func<Domain.Entity.Table, UpdateTableDto>> Projection
        {
            get
            {
                return model => new UpdateTableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Columns = model.Columns.Select(GetColumnByIdDto.Create).OrderBy(x => x.Order),
                    Tags = model.EntityTags.MappingTagDto()
                };
            }
        }

        public static UpdateTableDto Create(Domain.Entity.Table model)
        {
            if (model != null)
            {
                return Converter(model);
            }
            return null;
        }
    }
}
