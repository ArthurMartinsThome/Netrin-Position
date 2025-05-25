using Moq;
using Netrin.Position.Application.Service;
using Netrin.Position.Domain.Interface;
using Netrin.Position.Domain.Model.Auxiliar;
using Netrin.Position.Domain.Model.Enum;
using Netrin.Position.Domain.Model.Filter;
using System.Net;

namespace Netrin.Position.Application.Tests
{
    [TestFixture]
    public class PositionServiceTests
    {
        private Mock<IPositionDataSource> _mockDataSource;
        private PositionService _positionService;

        [SetUp]
        public void Setup()
        {
            _mockDataSource = new Mock<IPositionDataSource>();
            _positionService = new PositionService(_mockDataSource.Object);
        }

        #region Search Tests

        [Test]
        public async Task Search_WithNullFilter_CallsDataSourceSearchWithEmptyFilters()
        {
            PositionFilter filter = null;
            var expectedResult = new DefaultResult<IEnumerable<Domain.Model.Position>>(true, HttpStatusCode.OK, data: new List<Domain.Model.Position>());

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                          .ReturnsAsync(expectedResult);

            var result = await _positionService.Search(filter);

            Assert.IsTrue(result.Succeded);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Data);

            _mockDataSource.Verify(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()), Times.Once());

            _mockDataSource.Verify(ds => ds.Search(It.Is<IEnumerable<Filter>>(f => !f.Any())), Times.Once());
        }

        [Test]
        [TestCase(10)]
        public async Task Search_WithIdFilter_CallsDataSourceSearchWithIdFilter(int filterId)
        {
            var filter = new PositionFilter { Id = filterId };
            var expectedResult = new DefaultResult<IEnumerable<Domain.Model.Position>>(true, HttpStatusCode.OK, data: new List<Domain.Model.Position>());

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                           .ReturnsAsync(expectedResult);

            var result = await _positionService.Search(filter);

            Assert.IsTrue(result.Succeded);
            _mockDataSource.Verify(ds => ds.Search(It.Is<IEnumerable<Filter>>(filters =>
                 filters.Any(f => f._Fields.Contains("Id") &&
                                  f._Operator == EOperator.Equal &&
                                  f._Values.Contains(filterId.GetHashCode()))
             )), Times.Once());
        }

        [Test]
        [TestCase(EStatus.Active)]
        [TestCase(EStatus.Inactive)]
        public async Task Search_WithStatusFilter_CallsDataSourceSearchWithStatusFilter(EStatus status)
        {
            var filter = new PositionFilter { Status = status.GetHashCode() };
            var expectedResult = new DefaultResult<IEnumerable<Domain.Model.Position>>(true, HttpStatusCode.OK, data: new List<Domain.Model.Position>());

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                           .ReturnsAsync(expectedResult);

            var result = await _positionService.Search(filter);

            Assert.IsTrue(result.Succeded);
            _mockDataSource.Verify(ds => ds.Search(It.Is<IEnumerable<Filter>>(filters =>
                 filters.Any(f => f._Fields.Contains("Status") &&
                                  f._Operator == EOperator.Equal &&
                                  f._Values.Contains(status.GetHashCode()))
             )), Times.Once());
        }

        [Test]
        public async Task Search_WithHideInactiveFilter_CallsDataSourceSearchWithNotEqualStatusFilters()
        {
            var filter = new PositionFilter { HideInactive = true };
            var expectedResult = new DefaultResult<IEnumerable<Domain.Model.Position>>(true, HttpStatusCode.OK, data: new List<Domain.Model.Position>());

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                           .ReturnsAsync(expectedResult);

            var result = await _positionService.Search(filter);

            Assert.IsTrue(result.Succeded);
            _mockDataSource.Verify(ds => ds.Search(It.Is<IEnumerable<Filter>>(filters =>
                 filters.Any(f => f._Fields.Contains("Status") &&
                                  f._Operator == EOperator.NotEqual &&
                                  f._Values.Contains(EStatus.Inactive.GetHashCode()) &&
                                  f._Values.Contains(EStatus.Deleted.GetHashCode()))
             )), Times.Once());
        }

        [Test]
        public async Task Search_WithMultipleFilters_CallsDataSourceSearchWithAllFilters()
        {
            var filter = new PositionFilter { Id = 10, Status = EStatus.Active.GetHashCode(), HideInactive = true };
            var expectedResult = new DefaultResult<IEnumerable<Domain.Model.Position>>(true, HttpStatusCode.OK, data: new List<Domain.Model.Position>());

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                           .ReturnsAsync(expectedResult);

            var result = await _positionService.Search(filter);

            Assert.IsTrue(result.Succeded);
            _mockDataSource.Verify(ds => ds.Search(It.Is<IEnumerable<Filter>>(filters =>
                 filters.Count() == 3 &&
                 filters.Any(f => f._Fields.Contains("Id") && f._Operator == EOperator.Equal && f._Values.Contains(filter.Id.Value.GetHashCode())) &&
                 filters.Any(f => f._Fields.Contains("Status") && f._Operator == EOperator.Equal && f._Values.Contains(filter.Status.Value.GetHashCode())) &&
                 filters.Any(f => f._Fields.Contains("Status") && f._Operator == EOperator.NotEqual && f._Values.Contains(EStatus.Inactive.GetHashCode()) && f._Values.Contains(EStatus.Deleted.GetHashCode()))
             )), Times.Once());
        }

        [Test]
        public async Task Search_DataSourceThrowsException_ReturnsInternalServerErrorResult()
        {
            var filter = new PositionFilter { Id = 1 };
            var exceptionMessage = "Database connection failed";

            _mockDataSource.Setup(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()))
                           .ThrowsAsync(new Exception(exceptionMessage));

            var result = await _positionService.Search(filter);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            StringAssert.Contains($"Houve uma falha ao buscar a(s) posição(ões). - {exceptionMessage}", result.Message);
            Assert.IsNull(result.Data);
        }

        #endregion

        #region Insert Tests

        [Test]
        public async Task Insert_NullPosition_ReturnsInternalServerErrorResult()
        {
            Netrin.Position.Domain.Model.Position obj = null;

            var result = await _positionService.Insert(obj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual("O objeto não pode ser nulo.", result.Message);
            Assert.AreEqual(0, result.Data);
            _mockDataSource.Verify(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()), Times.Never());
            _mockDataSource.Verify(ds => ds.Insert(It.IsAny<Domain.Model.Position>()), Times.Never());
        }

        [Test]
        [TestCase("", 10.0, 20.0, "Os campos Nome, Latitude e Longitude são obrigatórios.")]
        [TestCase("Test Position", null, 20.0, "Os campos Nome, Latitude e Longitude são obrigatórios.")]
        [TestCase("Test Position", 10.0, null, "Os campos Nome, Latitude e Longitude são obrigatórios.")]
        public async Task Insert_MissingRequiredFields_ReturnsInternalServerErrorResult(string name, decimal? latitude, decimal? longitude, string expectedMessage)
        {
            var obj = new Domain.Model.Position
            {
                Name = name,
                Latitude = latitude,
                Longitude = longitude
            };

            var result = await _positionService.Insert(obj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual(expectedMessage, result.Message);
            Assert.AreEqual(0, result.Data);
            _mockDataSource.Verify(ds => ds.Search(It.IsAny<IEnumerable<Filter>>()), Times.Never());
            _mockDataSource.Verify(ds => ds.Insert(It.IsAny<Domain.Model.Position>()), Times.Never());
        }

        #endregion

        #region Update Tests

        [Test]
        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-5)]
        public async Task Update_OldObjWithInvalidId_ReturnsInternalServerErrorResult(int? invalidId)
        {
            var oldObj = new Domain.Model.Position { Id = invalidId };
            var newObj = new Domain.Model.Position { Name = "Updated" };

            var result = await _positionService.Update(oldObj, newObj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            StringAssert.Contains("Falta referenciar qual o id para editar a posição", result.Message);
            Assert.IsFalse(result.Data);
            _mockDataSource.Verify(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()), Times.Never());
        }

        [Test]
        public async Task Update_OldObjIsNull_ReturnsInternalServerErrorResult()
        {
            Domain.Model.Position oldObj = null;
            var newObj = new Domain.Model.Position { Name = "Updated" };

            var result = await _positionService.Update(oldObj, newObj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            StringAssert.Contains("Falta referenciar qual o id para editar a posição. - Update: oldPosition.Id == null", result.Message);
            Assert.IsFalse(result.Data);
            _mockDataSource.Verify(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()), Times.Never());
        }

        [Test]
        public async Task Update_DataSourceUpdateReturnsFailure_ReturnsFailedResult()
        {
            var oldObj = new Domain.Model.Position { Id = 10 };
            var newObj = new Domain.Model.Position { Name = "Updated" };
            var failedUpdateResult = new DefaultResult<bool>(false, HttpStatusCode.BadRequest, message: "Update failed in DB");

            _mockDataSource.Setup(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()))
                           .ReturnsAsync(failedUpdateResult);

            var result = await _positionService.Update(oldObj, newObj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual(failedUpdateResult.Message, result.Message);
            Assert.IsFalse(result.Data);
            _mockDataSource.Verify(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()), Times.Once());
        }

        [Test]
        public async Task Update_DataSourceThrowsException_ReturnsInternalServerErrorResult()
        {
            var oldObj = new Domain.Model.Position { Id = 10 };
            var newObj = new Domain.Model.Position { Name = "Updated" };
            var exceptionMessage = "Database update failed";

            _mockDataSource.Setup(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()))
                           .ThrowsAsync(new Exception(exceptionMessage));

            var result = await _positionService.Update(oldObj, newObj);

            Assert.IsFalse(result.Succeded);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            StringAssert.Contains($"Houve uma falha ao editar a posição. - {exceptionMessage}", result.Message);
            Assert.IsFalse(result.Data);
            _mockDataSource.Verify(ds => ds.Update(It.IsAny<IEnumerable<Filter>>(), It.IsAny<Domain.Model.Position>(), It.IsAny<Domain.Model.Position>()), Times.Once());
        }
        #endregion
    }
}