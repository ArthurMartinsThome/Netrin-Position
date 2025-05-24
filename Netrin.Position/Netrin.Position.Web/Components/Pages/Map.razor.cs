using Microsoft.JSInterop;
using Netrin.Position.Application.Service;

namespace Netrin.Position.Web.Components.Pages
{
    public partial class Map
    {
        private const double _distance = 2.0;
        private const int _pointsQtd = 2;
        private const int _earthRadiusKm = 6371;

        private List<Domain.Model.Position> positions = new();
        private List<List<Domain.Model.Position>> nearPositions = new();

        private bool isLoading = true;
        private bool hasData = false;


        private PositionService _positionService = new();

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            hasData = false;
            positions = new List<Domain.Model.Position>();
            nearPositions = new List<List<Domain.Model.Position>>();

            await LoadPositions();

            hasData = positions != null && positions.Any();

            if (hasData)
                nearPositions = await FindNearPositions(positions);

            isLoading = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (hasData)
            {
                await Task.Delay(1000);
                await JS.InvokeVoidAsync("initMap", positions, nearPositions);
            }
        }

        private async Task LoadPositions()
        {
            try
            {
                var resultSearch = await _positionService.Search(new Domain.Model.Filter.PositionFilter() { });
                if (!resultSearch.Succeded)
                {
                    return;
                }

                positions = resultSearch.Data.ToList();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<List<List<Domain.Model.Position>>> FindNearPositions(List<Domain.Model.Position> allPositions)
        {
            var nearPositions = new List<List<Domain.Model.Position>>();

            foreach (var position in allPositions)
            {
                if (!position.Verified)
                {
                    var currentGroup = new List<Domain.Model.Position>();
                    var verifiedQueue = new Queue<Domain.Model.Position>();

                    verifiedQueue.Enqueue(position);
                    position.Verified = true;
                    currentGroup.Add(position);

                    while (verifiedQueue.Count > 0)
                    {
                        var current = verifiedQueue.Dequeue();

                        var neighbors = allPositions.Where(p =>
                            !p.Verified &&
                            CalculateDistance(current.Latitude.Value, current.Longitude.Value, p.Latitude.Value, p.Longitude.Value) <= _distance
                        ).ToList();

                        foreach (var neighbor in neighbors)
                        {
                            neighbor.Verified = true;
                            currentGroup.Add(neighbor);
                            verifiedQueue.Enqueue(neighbor);
                        }
                    }

                    if (currentGroup.Count >= _pointsQtd)
                        nearPositions.Add(currentGroup);
                }
            }

            return nearPositions;
        }

        private double CalculateDistance(decimal currentLat, decimal currentLong, decimal nearLat, decimal nearLong)
        { // Fórmula de Haversine para calcular a distância aproximada em quilômetros de dois pontos na superfície da Terra
            var currentLatRad = (double)currentLat * (Math.PI / 180);
            var currentLongRad = (double)currentLong * (Math.PI / 180);
            var nearLatRad = (double)nearLat * (Math.PI / 180);
            var nearLongRad = (double)nearLong * (Math.PI / 180);

            var latDiference = nearLatRad - currentLatRad;
            var longDiference = nearLongRad - currentLongRad;

            var a = Math.Sin(latDiference / 2) * Math.Sin(latDiference / 2) + Math.Cos(currentLatRad) * Math.Cos(nearLatRad) * Math.Sin(longDiference / 2) * Math.Sin(longDiference / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return _earthRadiusKm * c;
        }
    }
}