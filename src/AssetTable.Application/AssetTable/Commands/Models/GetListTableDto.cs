using System;
using System.Linq.Expressions;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class GetListTableDto
    {
        static Func<Domain.Entity.Table, GetListTableDto> Converter = Projection.Compile();
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string LockedByUpn { get; set; }

        public static Expression<Func<Domain.Entity.Table, GetListTableDto>> Projection
        {
            get
            {
                return model => new GetListTableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    CreatedUtc = model.CreatedUtc,
                    UpdatedUtc = model.UpdatedUtc,
                };
            }
        }

        public static GetListTableDto Create(Domain.Entity.Table model)
        {
            return Converter(model);
        }
    }
}
