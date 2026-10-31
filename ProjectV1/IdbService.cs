
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SmartLibraryApp
{
    [ServiceContract]
    public interface IdbService
    {
        [WebGet(
            UriTemplate = "test",
            ResponseFormat = WebMessageFormat.Json)]
        string test();

        [WebGet(
            UriTemplate = "getData",
            ResponseFormat = WebMessageFormat.Json)]
        LibraryDataDTO getLibraryData();

        [WebInvoke(
            Method = "POST",
            UriTemplate = "borrowBook",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string borrowBook(BorrowReqObj input);

        [WebInvoke(
            Method = "POST",
            UriTemplate = "returnBook",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string returnBook(ReturnReqObj input);

        [WebInvoke(
            Method = "POST",
            UriTemplate = "addBook",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string addBook(AddBookReqObj input);

        [WebInvoke(
            Method = "POST",
            UriTemplate = "addMember",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string addMember(AddMemberReqObj input);

        [WebInvoke(
            Method = "POST",
            UriTemplate = "updateFineStatus",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string updateFineStatus(UpdateFineStatusReqObj input);
    }
}
