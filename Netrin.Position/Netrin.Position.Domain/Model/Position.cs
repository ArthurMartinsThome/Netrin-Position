using Netrin.Position.Domain.Model.Enum;

namespace Netrin.Position.Domain.Model
{
    public class Position : ICloneable
    {
        public int? Id { get; set; }
        public EStatus? Status { get; set; }
        public string? Name { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public object Clone() => this.MemberwiseClone();
    }
}