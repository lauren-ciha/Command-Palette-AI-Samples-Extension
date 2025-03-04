using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UnnamedExtension.Templates
{
    public class TemplateLoader
    {
#pragma warning disable CA1822 // Mark members as static
        public async Task<string> LoadTemplateAsync(string fileName)
#pragma warning restore CA1822 // Mark members as static
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("The specified file does not exist", fileName);
            }

            using (StreamReader reader = new StreamReader(fileName))
            {
                return await reader.ReadToEndAsync();
            }
        }

#pragma warning disable CA1822 // Mark members as static
        public string ReplaceTemplateKeys(string template, Dictionary<string, string> replacements)
#pragma warning restore CA1822 // Mark members as static
        {
            ArgumentNullException.ThrowIfNull(template);

            ArgumentNullException.ThrowIfNull(replacements);

            foreach (var replacement in replacements)
            {
                template = template.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
            }

            return template;
        }
    }
}
