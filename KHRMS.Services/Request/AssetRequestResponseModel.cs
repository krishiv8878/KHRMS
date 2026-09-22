namespace KHRMS.Services.Request
{
    public class AssetRequestResponseModel
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? SerialNumber { get; set; }
        public string? AssetType { get; set; }
        public string? AssetStatus { get; set; }

        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmail { get; set; }

        public string RequestType { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public string? ImageUrls { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? CourierPartner { get; set; }
        public string? TrackingNumber { get; set; }

        public DateTime? DispatchedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        public string? AdminRemarks { get; set; }
        public string? InspectionRemarks { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<AssetRequestLogDto> Logs { get; set; } = new List<AssetRequestLogDto>();
    }

    public class AssetRequestLogDto
    {
        public long Id { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public long? ActionByEmployeeId { get; set; }
        public string? ActionByName { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
