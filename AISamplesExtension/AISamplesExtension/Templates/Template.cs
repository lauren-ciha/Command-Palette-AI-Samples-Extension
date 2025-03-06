using System.Collections.Generic;

namespace AISamplesExtension.Templates
{
    public class Template 
    {
        public string TemplateJson { get; set; }

        public Template(TemplateLoader templateLoader, string fileName, Dictionary<string, string> variableReplacements)
        {
            var templateJson = templateLoader.LoadTemplateAsync(fileName).Result;
            TemplateJson = templateLoader.ReplaceTemplateKeys(templateJson, variableReplacements);
        }
    }
}
