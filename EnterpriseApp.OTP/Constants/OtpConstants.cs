namespace EnterpriseApp.OTP.Constants
{
    public static class OtpConstants
    {
        public const string STATUS_EMAIL_OK = "Email containing OTP has been sent successfully.";
        public const string STATUS_EMAIL_FAIL = "Email address does not exist or sending to the email has failed.";
        public const string STATUS_EMAIL_INVALID = "Email address is invalid.";
        public const string STATUS_OTP_OK = "OTP is valid and checked.";
        public const string STATUS_OTP_FAIL = "OTP is wrong after maximum attempts.";
        public const string STATUS_OTP_TIMEOUT = "Timeout after 1 min.";

        public const int OTP_VALID_MINUTES = 1;
        public const int MAX_OTP_ATTEMPTS = 10;
        public const int OTP_LENGTH = 6;
    }
}