using Microsoft.JSInterop;
using Netrin.Position.Application.Service;

namespace Netrin.Position.Web.Components.Pages
{
    public partial class Map
    {
        private const int EARTH_RADIUS_KM = 6371;

        private double distance = 2.0;
        private int pointsQtd = 2;
        private string? errorMessage = default;
        private bool isLoading = true;
        private bool hasData = false;
        private bool firstRender = true;

        private List<Domain.Model.Position> positions = new();
        private List<List<Domain.Model.Position>> nearPositions = new();

        private PositionService _positionService = new();

        protected override async Task OnInitializedAsync()
        {
            errorMessage = null;
            try
            {
                isLoading = true;
                hasData = false;
                positions = new List<Domain.Model.Position>();
                nearPositions = new List<List<Domain.Model.Position>>();

                await LoadPositions();

                hasData = positions != null && positions.Any();

                if (hasData)
                {
                    var resultNearPositions = FindNearPositions(positions);
                    if(resultNearPositions != null)
                        nearPositions = resultNearPositions;
                }

                isLoading = false;
            }
            catch (Exception ex)
            {
                errorMessage = $"OnInitializedAsync - Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            errorMessage = null;
            try
            {
                if (this.firstRender && hasData)
                {
                    await JS.InvokeVoidAsync("initMap", positions, nearPositions);
                    this.firstRender = false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"OnAfterRenderAsync - Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private async Task LoadPositions()
        {
            errorMessage = null;
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
                errorMessage = $"LoadPositions - Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private List<List<Domain.Model.Position>> FindNearPositions(List<Domain.Model.Position> allPositions)
        {
            errorMessage = null;
            try
            {
                var pointsCopy = allPositions.Select(p => new Domain.Model.Position
                {
                    Id = p.Id,
                    Name = p.Name,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Verified = false,
                    IsVisibleOnMap = p.IsVisibleOnMap
                }).ToList();

                var clusters = new List<List<Domain.Model.Position>>();
                var visited = new HashSet<int>();

                foreach (var point in pointsCopy)
                {
                    if (!visited.Contains(point.Id.Value))
                    {
                        var currentCluster = new List<Domain.Model.Position>();
                        var queue = new Queue<Domain.Model.Position>();

                        queue.Enqueue(point);
                        visited.Add(point.Id.Value);
                        currentCluster.Add(point);

                        while (queue.Count > 0)
                        {
                            var current = queue.Dequeue();

                            var neighbors = pointsCopy.Where(p =>
                                !visited.Contains(p.Id.Value) &&
                                p.Id != current.Id &&
                                CalculateDistance(current.Latitude.Value, current.Longitude.Value, p.Latitude.Value, p.Longitude.Value) <= distance
                            ).ToList();

                            foreach (var neighbor in neighbors)
                            {
                                visited.Add(neighbor.Id.Value);
                                var originalNeighbor = allPositions.First(p => p.Id == neighbor.Id);
                                currentCluster.Add(originalNeighbor);
                                queue.Enqueue(neighbor);
                            }
                        }

                        if (currentCluster.Count >= pointsQtd)
                            clusters.Add(currentCluster);
                    }
                }

                return clusters;
            }
            catch (Exception ex)
            {
                errorMessage = $"FindNearPositions - Ocorreu um erro inesperado: {ex.Message}";
                return null;
            }
        }

        private double CalculateDistance(decimal currentLat, decimal currentLong, decimal nearLat, decimal nearLong)
        { // Fórmula de Haversine para calcular a distância aproximada em quilômetros de dois pontos na superfície da Terra
            errorMessage = null;
            try
            {
                var currentLatRad = (double)currentLat * (Math.PI / 180);
                var currentLongRad = (double)currentLong * (Math.PI / 180);
                var nearLatRad = (double)nearLat * (Math.PI / 180);
                var nearLongRad = (double)nearLong * (Math.PI / 180);

                var latDiference = nearLatRad - currentLatRad;
                var longDiference = nearLongRad - currentLongRad;

                var resultSenoCosseno = Math.Sin(latDiference / 2) * Math.Sin(latDiference / 2) + Math.Cos(currentLatRad) * Math.Cos(nearLatRad) * Math.Sin(longDiference / 2) * Math.Sin(longDiference / 2);
                var resultAtan2 = 2 * Math.Atan2(Math.Sqrt(resultSenoCosseno), Math.Sqrt(1 - resultSenoCosseno));

                return EARTH_RADIUS_KM * resultAtan2;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task HandleVisibilityChange(int positionId, bool isVisible)
        {
            errorMessage = null;
            try
            {
                var pointToUpdate = positions.FirstOrDefault(p => p.Id == positionId);
                if (pointToUpdate == null)
                {
                    errorMessage = $"Ponto com ID {positionId} não encontrado na lista 'positions'!";
                    return;
                }

                pointToUpdate.IsVisibleOnMap = isVisible;

                if (isVisible)
                    await JS.InvokeVoidAsync("showMarker", positionId);
                else
                    await JS.InvokeVoidAsync("hideMarker", positionId);

                var visiblePoints = positions.Where(p => p.IsVisibleOnMap).ToList();
                nearPositions = FindNearPositions(visiblePoints);

                await JS.InvokeVoidAsync("redrawSquares", nearPositions);
            }
            catch (Exception ex)
            {
                errorMessage = $"HandleVisibilityChange - Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private async Task HandleDistanceQuantityChange()
        {
            errorMessage = null;
            try
            {
                var visiblePoints = positions.Where(p => p.IsVisibleOnMap).ToList();
                nearPositions = FindNearPositions(visiblePoints);

                await JS.InvokeVoidAsync("redrawSquares", nearPositions);
            }
            catch (Exception ex)
            {
                errorMessage = $"HandleVisibilityChange - Ocorreu um erro inesperado: {ex.Message}";
            }
        }
        private void ClearError()
        {
            errorMessage = null;
        }
    }
}