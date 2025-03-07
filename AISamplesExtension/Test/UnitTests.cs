using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormContents;
using System.Diagnostics;
using AdaptiveCards;
using System.Text.Json;
using AISamplesExtension.Templates;

namespace Test
{
    [TestClass]
    public class ImposterKittensFormContentTests
    {
        [TestMethod]
        public void LeftImageActionClickReturnsValidJson()
        {
            // Arrange
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
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
                string result = ImposterKittensFormContent.ParseJsonData(jsonData);
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
        public void RightImageActionClickReturnsValidJson()
        {
            // Arrange
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
            var rightImageAction = new AdaptiveSubmitAction
            {
                Id = "rightImageAction",
                Title = "Select Right Image",
                Data = "{\"selectedImage\":\"right\"}"
            };

            // Act
            var jsonData = rightImageAction.Data.ToString();
            try
            {
                // validate jsonData is a valid Json string
                var jsonDocument = JsonDocument.Parse(jsonData);
                string result = ImposterKittensFormContent.ParseJsonData(jsonData);
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
        public void ParseJsonDataValidJsonReturnsExpectedValueWithExpectedStringRight()
        {
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
            string jsonData = "{\"selectedImage\":\"right\"}";

            string result = ImposterKittensFormContent.ParseJsonData(jsonData);

            Assert.AreEqual("right", result);
        }

        [TestMethod]
        public void ParseJsonDataValidJsonReturnsExpectedValueWithExpectedStringLeft()
        {
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
            string jsonData = "{\"selectedImage\":\"left\"}";

            string result = ImposterKittensFormContent.ParseJsonData(jsonData);

            Assert.AreEqual("left", result);
        }

        [TestMethod]
        public void ParseJsonDataInvalidJsonReturnsEmptyString()
        {
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
            string invalidJsonData = "{\"selectedImage\":\"left\",\"Id\":\"leftImageAction\"";

            string result = ImposterKittensFormContent.ParseJsonData(invalidJsonData);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ValidateJsonFromTemplateParsesIntoAdaptiveCardCorrectly()
        {
            // Arrange
            var templateLoader = new TemplateLoader();
            var template = templateLoader.LoadTemplate("ImposterKittensTemplate.json", true, null);
            var formContent = new ImposterKittensFormContent(template);
            // Act
            var adaptiveCard = formContent.GetAdapativeCard();
            // Assert
            Assert.IsNotNull(adaptiveCard);
            Assert.IsNotNull((AdaptiveColumnSet)adaptiveCard.Body[2]);
            Assert.AreEqual(2, adaptiveCard.Body.Count);
        }
    }
}
