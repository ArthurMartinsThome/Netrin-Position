using Netrin.Position.Application.Service;

namespace Netrin.Position.Web.Components.Pages
{
    public partial class Position
    {
        private IEnumerable<Domain.Model.Position>? positions;
        private Domain.Model.Position currentPosition = new();
        private Domain.Model.Position oldPosition = new();
        private bool onlyActive = true;
        private string? errorMessage;

        private PositionService _positionService = new();

        protected override async Task OnInitializedAsync()
        {
            currentPosition = new Domain.Model.Position();
            _positionService = new PositionService();
            await LoadPositions();
        }

        private async Task LoadPositions()
        {
            errorMessage = null;
            try
            {
                var resultSearch = await _positionService.Search(new Domain.Model.Filter.PositionFilter() { HideInactive = onlyActive });
                if (!resultSearch.Succeded)
                {
                    errorMessage = resultSearch.Message;
                    return;
                }

                positions = resultSearch.Data;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private void EditPosition(Domain.Model.Position obj)
        {
            oldPosition = (Domain.Model.Position)obj.Clone();
            currentPosition = (Domain.Model.Position)obj.Clone();
            StateHasChanged();
        }

        private async Task Save()
        {
            errorMessage = null;
            try
            {
                if (currentPosition.Id == null || currentPosition.Id == 0)
                {
                    var createdPosition = await _positionService.Insert(currentPosition);
                    if (!createdPosition.Succeded)
                    {
                        errorMessage = createdPosition.Message;
                        return;
                    }
                }
                else
                {
                    var resultUpdate = await _positionService.Update(oldPosition, currentPosition);
                    if (!resultUpdate.Succeded)
                    {
                        errorMessage = resultUpdate.Message;
                        return;
                    }
                }

                await LoadPositions();
                currentPosition = new Domain.Model.Position();
                oldPosition = new Domain.Model.Position();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private async Task DeletePosition(Domain.Model.Position obj)
        {
            errorMessage = null;
            try
            {
                bool confirmado = await JS.InvokeAsync<bool>("confirm", [$"Tem certeza que deseja inativar o registro '{obj.Name}'?"]);

                if (confirmado)
                {
                    oldPosition = (Domain.Model.Position)obj.Clone();
                    currentPosition = (Domain.Model.Position)obj.Clone();
                    currentPosition.Status = Domain.Model.Enum.EStatus.Inactive;

                    await Save();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Ocorreu um erro inesperado: {ex.Message}";
            }
        }

        private void ClearError()
        {
            errorMessage = null;
        }
    }
}