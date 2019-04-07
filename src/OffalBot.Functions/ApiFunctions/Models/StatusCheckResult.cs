using System;

namespace OffalBot.Functions.ApiFunctions.Models
{
    public class StatusCheckResult
    {
        public string Context { get; set; }
        public string Description { get; set; }
        public StatusCheckState State { get; set; }
        public Uri TargetUrl { get; set; }
    }

    public enum StatusCheckState
    {
        Error,
        Failure,
        Pending,
        Success
    }
}