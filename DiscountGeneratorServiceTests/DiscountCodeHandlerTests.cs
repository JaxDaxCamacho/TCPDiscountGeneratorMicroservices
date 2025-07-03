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
        public async Task DiscountCodeHandler_GenerateCodes_GoldenPath(short numberOfCodes, short lengthOfCodes, int testCase)
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
            Thread.Sleep(TimeSpan.FromSeconds(2));
            //Assert
            testDiscountGenerator.Codes.Should().HaveCount(numberOfCodes);
            testDiscountGenerator.Codes.First().Key.Should().HaveLength(lengthOfCodes);
            testFileGenerator.ClearAllData();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("testcod")]
        public async Task DiscountCodeHandler_UseCode_GoldenPath(string codeToActivate)
        {
            // Arrange
            CancellationTokenSource cts = new CancellationTokenSource();
            var testFileGenerator = new FileStorageHandler();

            var testDiscountGenerator = new EPSDiscountGenerator(testFileGenerator);
            var testClient = new Client(1, testDiscountGenerator);
            testDiscountGenerator.Clients.Add(1, testClient);

            testDiscountGenerator.Codes[codeToActivate] = true;

            //Act

            await testDiscountGenerator.RequestHandler.HandleUseCodeAsync(1, codeToActivate, cts.Token);
            
            //Assert
            testDiscountGenerator.Codes.Should().ContainKey(codeToActivate);
            testDiscountGenerator.Codes[codeToActivate].Should().Be(false);
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

            testDiscountGenerator.Codes[codeToActivate] = false;

            //Act
            var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
            await testDiscountGenerator.RequestHandler.HandleUseCodeAsync(1, codeToActivate, cts.Token));

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
            var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
            await testDiscountGenerator.RequestHandler.HandleUseCodeAsync(1, codeToActivate, cts.Token));

            //Assert
            exception.Message.Should().Be("This Code doesn't exist or is pending activation");
        }
    }
}
