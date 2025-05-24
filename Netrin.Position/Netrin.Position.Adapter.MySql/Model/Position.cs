using Netrin.Position.Infra.MySql;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netrin.Position.Adapter.MySql.Model
{
    [Table("position")]
    public class Position
    {
        [FilterIdentifier("Id")]
        public int? id { get; set; }
        [FilterIdentifier("Status")]
        public int? status { get; set; }
        public string? name { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}