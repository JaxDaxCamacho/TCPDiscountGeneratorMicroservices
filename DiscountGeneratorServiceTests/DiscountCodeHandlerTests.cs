using DiscountGeneratorService.Handlers;
using System.IO.Abstractions.TestingHelpers;
using TCPLibrary;
using Xunit;

namespace DiscountGeneratorServiceTests
{
    [TestClass]
    public sealed class DiscountCodeHandlerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow((short)300, (short)7)]
        [DataRow((short)2000, (short)8)]
        public async Task DiscountCodeHandler_GenerateCodes_GoldenPath(short numberOfCodes, short lengthOfCodes)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var fakeClientId = 0;
            CancellationTokenSource cts = new CancellationTokenSource();
            var receivedPacket = new Packet((int)RequestPacket.Generate);
            receivedPacket.Write(numberOfCodes);
            receivedPacket.Write(lengthOfCodes);
            receivedPacket.WriteLength();

            await DiscountCodeHandler.GenerateAsync(fakeClientId, receivedPacket, cts.Token);
        }
    }
}
