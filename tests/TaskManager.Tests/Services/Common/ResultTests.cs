using TaskManager.Core.Common;

namespace TaskManager.Tests.Services.Common
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void Success_ShouldReturnResultWithIsSuccessTrue_AndValueSet_AndNoErrors()
        {
            // Arrange
            var value = "TestValue";

            // Act
            var result = Result<string>.Success(value);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public void Failure_ShouldReturnResultWithIsSuccessFalse_AndErrorSet_AndValueNull()
        {
            // Arrange
            var error = "TestError";

            // Act
            var result = Result<string>.Failure(error);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Is.EquivalentTo([error]));
            }
        }

        [Test]
        public void Failure_ShouldReturnResultWithIsSuccessFalse_AndMultipleErrors_AndValueNull()
        {
            // Arrange
            var errors = new string[] { "Error1", "Error2", "Error3" };

            // Act
            var result = Result<string>.Failure(errors);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Is.EquivalentTo(errors));
            }
        }

        [Test]
        public void Failure_ShouldReturnResultWithIsSuccessFalse_AndValidationErrors_AndValueNull()
        {
            // Arrange
            var errors = new List<string> { "Error1", "Error2", "Error3" };

            // Act
            var result = Result<string>.Failure(errors);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Is.EquivalentTo(errors));
            }
        }
    }
}
