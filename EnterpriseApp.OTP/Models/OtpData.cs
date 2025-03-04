namespace EnterpriseApp.OTP.Models
{
    public class OtpData
    {
        public string Code { get; set; }
        public int RemainingAttempts { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}