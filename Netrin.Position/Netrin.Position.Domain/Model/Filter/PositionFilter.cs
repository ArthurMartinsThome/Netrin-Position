namespace Netrin.Position.Domain.Model.Filter
{
    public class PositionFilter
    {
        public int? Id { get; set; }
        public int? Status { get; set; }

        public bool HideInactive { get; set; } = true;
    }
}