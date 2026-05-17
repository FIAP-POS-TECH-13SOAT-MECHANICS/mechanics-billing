using Mechanics.Application.Notification.Services;
using Mechanics.Application.Notification;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mechanics.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notification")]
[TestCategory("Email")]
public class EmailServiceTests
{
    [TestMethod]
    public async Task It_ShouldSendEmail_WhenPaymentIsApproved()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var notification = new PaymentApprovedNotification
        {
            CustomerEmail = "customer@example.com",
            CustomerName = "Customer Test",
            PaymentId = "158916459128",
            Amount = 10000,
        };

        // Act
        await service.SendCustomerPaymentApproved(notification, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(notification.CustomerEmail, emailMessage.Recipient);
        Assert.AreEqual("Pagamento Aprovado - FIAP Mechanics", emailMessage.Subject);
        StringAssert.Contains(emailMessage.Body, notification.CustomerName);
        StringAssert.Contains(emailMessage.Body, "O seu pagamento foi aprovado.");
        StringAssert.Contains(emailMessage.Body, $"R$ {notification.Amount:N2}");
    }

    private static EmailService CreateInstance(IEmailSenderService senderService) =>
        new(NullLogger<EmailService>.Instance, senderService);
}
