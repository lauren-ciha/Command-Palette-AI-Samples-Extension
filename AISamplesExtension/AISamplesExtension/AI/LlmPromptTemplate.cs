﻿namespace AISamplesExtension.AI;

internal class LlmPromptTemplate
{
    public string? System { get; init; }
    public string? User { get; init; }
    public string? Assistant { get; init; }
    public string[]? Stop { get; init; }
}