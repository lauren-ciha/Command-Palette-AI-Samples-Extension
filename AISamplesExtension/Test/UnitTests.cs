using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormContents;
using System.Diagnostics;
using AdaptiveCards;
using System.Text.Json;

namespace Test
{
    [TestClass]
    public class ImposterKittensFormContentTests
    {
        [TestMethod]
        public void LeftImageAction_Click_ReturnsValidJson()
        {
            // Arrange
            var formContent = new ImposterKittensFormContent();
            var leftImageAction = new AdaptiveSubmitAction
            {
                Id = "leftImageAction",
                Title = "Select Left Image",
                Data = "{\"selectedImage\":\"left\"}"
            };

            // Act
            var jsonData = leftImageAction.Data.ToString();
            try
            {
                // validate jsonData is a valid Json string
                var jsonDocument = JsonDocument.Parse(jsonData);
                string result = formContent.ParseJsonData(jsonData);
                // Assert
                Assert.AreEqual("left", result);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Invalid JSON: {ex.Message}");
                Assert.Fail("JSON parsing failed.");
            }
        }

        [TestMethod]
        public void RightImageAction_Click_ReturnsValidJson()
        {
            // Arrange
            var formContent = new ImposterKittensFormContent();
            var rightImageAction = new AdaptiveSubmitAction
            {
                Id = "leftImageAction",
                Title = "Select Left Image",
                Data = "{\"selectedImage\":\"right\"}"
            };

            // Act
            var jsonData = rightImageAction.Data.ToString();
            try
            {
                // validate jsonData is a valid Json string
                var jsonDocument = JsonDocument.Parse(jsonData);
                string result = formContent.ParseJsonData(jsonData);
                // Assert
                Assert.AreEqual("right", result);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Invalid JSON: {ex.Message}");
                Assert.Fail("JSON parsing failed.");
            }
        }

        [TestMethod]
        public void ParseJsonData_ValidJson_ReturnsExpectedValueWithExpectedStringRight()
        {
            var formContent = new ImposterKittensFormContent();
            string jsonData = "{\"selectedImage\":\"right\"}";

            string result = formContent.ParseJsonData(jsonData);

            Assert.AreEqual("right", result);
        }

        [TestMethod]
        public void ParseJsonData_ValidJson_ReturnsExpectedValueWithExpectedStringLeft()
        {
            var formContent = new ImposterKittensFormContent();
            string jsonData = "{\"selectedImage\":\"left\"}";

            string result = formContent.ParseJsonData(jsonData);

            Assert.AreEqual("left", result);
        }

        [TestMethod]
        public void ParseJsonData_InvalidJson_ReturnsEmptyString()
        {
            var formContent = new ImposterKittensFormContent();
            string invalidJsonData = "{\"selectedImage\":\"left\",\"Id\":\"leftImageAction\"";

            string result = formContent.ParseJsonData(invalidJsonData);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
