﻿namespace Aviator.Acars.Entities;

public class BasicAcars
{
    public string Type { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Freq { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Registration { get; set; } = string.Empty;
    public string Flight { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}