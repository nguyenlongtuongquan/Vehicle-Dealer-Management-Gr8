namespace Vehicle_Dealer_Management.DAL.Constants
{
    public static class SalesContractStatus
    {
        public const string PendingCustomerSignature = "PENDING_SIGNATURE";
        public const string CustomerSigned = "SIGNED";
        public const string OrderCreated = "ORDER_CREATED";
        public const string Cancelled = "CANCELLED";

        public static bool IsSigned(string status)
        {
            return status == CustomerSigned || status == OrderCreated;
        }
    }
}


