using System.Runtime.Serialization;

namespace JwtTechTask.Models
{
    public enum TransactionStatus
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "Completed")]
        Completed = 1,

        [EnumMember(Value = "InProgress")]
        InProgress = 2,

        [EnumMember(Value = "Failed")]
        Failed = 3
    }
}
