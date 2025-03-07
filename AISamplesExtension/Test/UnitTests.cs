using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormContents;
using System.Diagnostics;

namespace Test
{
    [TestClass]
    public class ImposterKittensFormContentTests
    {
        [TestMethod]
        public void ParseJsonData_ValidJson_ReturnsExpectedValuesLeft()
        {
            var formContent = new ImposterKittensFormContent();
            string jsonData = "{\"selectedImage\":\"left\",\"Id\":\"leftImageAction\"}";

            string result = formContent.ParseJsonData(jsonData);

            Assert.AreEqual("left", result);
        }

        [TestMethod]
        public void ParseJsonData_ValidJson_ReturnsExpectedValuesRight()
        {
            var formContent = new ImposterKittensFormContent();
            string jsonData = "{\"selectedImage\":\"right\",\"Id\":\"rightImageAction\"}";

            string result = formContent.ParseJsonData(jsonData);

            Assert.AreEqual("right", result);
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
