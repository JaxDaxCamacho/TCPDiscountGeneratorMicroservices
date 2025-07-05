using DiscountGeneratorService;
using DiscountGeneratorService.Handlers;
using FluentAssertions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace DiscountGeneratorServiceTests
{
    [TestClass]
    public sealed class DiscountCodeHandlerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow((short)300, (short)7, 1)]
        [DataRow((short)2000, (short)8, 2)]
        [DataRow((short)3000, (short)7, 3)]
        public async Task DiscountCodeHandler_GenerateCodes_CreatesPendingCodes_GoldenPath(short numberOfCodes, short lengthOfCodes, int testCase)
        {

            // Arrange
            CancellationTokenSource cts = new CancellationTokenSource();
            var testFileGenerator = new FileStorageHandler();
            testFileGenerator.ForcePath($"{testCase}");

            var testDiscountGenerator = new EPSDiscountGenerator(testFileGenerator);
            var testClient = new Client(1, testDiscountGenerator);
            testDiscountGenerator.Clients.Add(1, testClient);

            //Act

            await testDiscountGenerator.RequestHandler.HandleGenerateAsync(1, numberOfCodes, lengthOfCodes, cts.Token);
            //Assert
            testDiscountGenerator.PendingCodes.Count().Should().Be(numberOfCodes);
            testDiscountGenerator.PendingCodes.First().Should().HaveLength(lengthOfCodes);
            testFileGenerator.ClearAllData();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("testcod")]
        public async Task DiscountCodeHandler_UseCode_CreatesPendingActivation_GoldenPath(string codeToActivate)
        {
            // Arrange
            CancellationTokenSource cts = new CancellationTokenSource();
            var testFileGenerator = new FileStorageHandler();

            var testDiscountGenerator = new EPSDiscountGenerator(testFileGenerator);
            var testClient = new Client(1, testDiscountGenerator);
            testDiscountGenerator.Clients.Add(1, testClient);

            testFileGenerator._cachedCodes[codeToActivate] = true;

            //Act

            await testDiscountGenerator.RequestHandler.HandleActivateCodeAsync(1, codeToActivate, cts.Token);

            //Assert
            testDiscountGenerator.PendingActivations.Should().Contain(codeToActivate);
            testFileGenerator.ClearAllData();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("testcod")]
        public async Task DiscountCodeHandler_UseCode_AlreadyActiveCode(string codeToActivate)
        {
            // Arrange
            CancellationTokenSource cts = new CancellationTokenSource();
            var testFileGenerator = new FileStorageHandler();

            var testDiscountGenerator = new EPSDiscountGenerator(testFileGenerator);
            var testClient = new Client(1, testDiscountGenerator);
            testDiscountGenerator.Clients.Add(1, testClient);

            testFileGenerator._cachedCodes[codeToActivate] = false;

            //Act
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await testDiscountGenerator.RequestHandler.HandleActivateCodeAsync(1, codeToActivate, cts.Token));

            //Assert
            exception.Message.Should().Be("This Code was already used");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("testcod")]
        public async Task DiscountCodeHandler_UseCode_DoesntExist(string codeToActivate)
        {
            // Arrange
            CancellationTokenSource cts = new CancellationTokenSource();
            var testFileGenerator = new FileStorageHandler();

            var testDiscountGenerator = new EPSDiscountGenerator(testFileGenerator);
            var testClient = new Client(1, testDiscountGenerator);
            testDiscountGenerator.Clients.Add(1, testClient);

            //Act
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            await testDiscountGenerator.RequestHandler.HandleActivateCodeAsync(1, codeToActivate, cts.Token));

            //Assert
            exception.Message.Should().Be("This Code doesn't exist or is pending activation");
        }
    }
}
