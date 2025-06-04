namespace ConfirmMe.Services
{
    public interface IAuditTrailService
    {
        Task LogActionAsync(
            string userId,
            string action,
            string tableName,
            int recordId,
            string oldValue = null,
            string newValue = null,
            string actionDetails = null,
            string approverId = null, 
            string role = null, 
            ActionType? actionType = null,  
            string remark = null,      
            string ipAddress = null,
            string userAgent = null
        );
    }
}
