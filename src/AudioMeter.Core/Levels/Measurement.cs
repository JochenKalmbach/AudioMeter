namespace AudioMeter.Core.Levels;

/// <summary>One 0.5 s measurement. Dba is null when there is no signal (gap).</summary>
public sealed record Measurement(DateTimeOffset Timestamp, double LevelDbfs, double? Dba);
