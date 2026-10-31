
using System.Runtime.Serialization;

namespace SmartLibraryApp
{
    [DataContract]
    public class LibraryDataDTO
    {
        [DataMember(Name = "categories")]
        public CategoryDTO[] Categories { get; set; }

        [DataMember(Name = "books")]
        public BookDTO[] Books { get; set; }

        [DataMember(Name = "bookCopies")]
        public BookCopyDTO[] BookCopies { get; set; }

        [DataMember(Name = "memberTypes")]
        public MemberTypeDTO[] MemberTypes { get; set; }

        [DataMember(Name = "members")]
        public MemberDTO[] Members { get; set; }

        [DataMember(Name = "borrowings")]
        public BorrowingDTO[] Borrowings { get; set; }

        [DataMember(Name = "fines")]
        public FineDTO[] Fines { get; set; }

        [DataMember(Name = "reservations")]
        public ReservationDTO[] Reservations { get; set; }
    }

    [DataContract]
    public class CategoryDTO
    {
        [DataMember(Name = "id")] public string Id { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
        [DataMember(Name = "description")] public string Description { get; set; }
    }

    [DataContract]
    public class BookDTO
    {
        [DataMember(Name = "id")] public string Id { get; set; }
        [DataMember(Name = "categoryId")] public string CategoryId { get; set; }
        [DataMember(Name = "quantity")] public int Quantity { get; set; }
        [DataMember(Name = "description")] public string Description { get; set; }
        [DataMember(Name = "title")] public string Title { get; set; }
        [DataMember(Name = "author")] public string Author { get; set; }
        [DataMember(Name = "publisher")] public string Publisher { get; set; }
        [DataMember(Name = "publishYear")] public int? PublishYear { get; set; }
        [DataMember(Name = "totalBorrows")] public int TotalBorrows { get; set; }
    }

    [DataContract]
    public class BookCopyDTO
    {
        [DataMember(Name = "copyId")] public string CopyId { get; set; }
        [DataMember(Name = "bookId")] public string BookId { get; set; }
        [DataMember(Name = "shelf")] public string Shelf { get; set; }
        [DataMember(Name = "price")] public decimal? Price { get; set; }
        [DataMember(Name = "status")] public string Status { get; set; }
    }

    [DataContract]
    public class MemberTypeDTO
    {
        [DataMember(Name = "typeId")] public string TypeId { get; set; }
        [DataMember(Name = "maxQuota")] public int MaxQuota { get; set; }
        [DataMember(Name = "loanDays")] public int LoanDays { get; set; }
        [DataMember(Name = "description")] public string Description { get; set; }
    }

    [DataContract]
    public class MemberDTO
    {
        [DataMember(Name = "id")] public string Id { get; set; }
        [DataMember(Name = "memberTypeId")] public string MemberTypeId { get; set; }
        [DataMember(Name = "status")] public string Status { get; set; }
        [DataMember(Name = "dues")] public decimal Dues { get; set; }
        [DataMember(Name = "firstName")] public string FirstName { get; set; }
        [DataMember(Name = "lastName")] public string LastName { get; set; }
        [DataMember(Name = "phoneNumber")] public string PhoneNumber { get; set; }
        [DataMember(Name = "registerDate")] public string RegisterDate { get; set; }
        [DataMember(Name = "expireDate")] public string ExpireDate { get; set; }
        [DataMember(Name = "duesDate")] public string DuesDate { get; set; }
    }

    [DataContract]
    public class BorrowingDTO
    {
        [DataMember(Name = "borrowId")] public string BorrowId { get; set; }
        [DataMember(Name = "memberId")] public string MemberId { get; set; }
        [DataMember(Name = "copyId")] public string CopyId { get; set; }
        [DataMember(Name = "borrowDate")] public string BorrowDate { get; set; }
        [DataMember(Name = "dueDate")] public string DueDate { get; set; }
        [DataMember(Name = "returnDate")] public string ReturnDate { get; set; }
        [DataMember(Name = "status")] public string Status { get; set; }
        [DataMember(Name = "fineId")] public string FineId { get; set; }
    }

    [DataContract]
    public class FineDTO
    {
        [DataMember(Name = "fineId")] public string FineId { get; set; }
        [DataMember(Name = "amount")] public decimal Amount { get; set; }
        [DataMember(Name = "status")] public string Status { get; set; }
        [DataMember(Name = "note")] public string Note { get; set; }
    }

    [DataContract]
    public class ReservationDTO
    {
        [DataMember(Name = "reservationId")] public string ReservationId { get; set; }
        [DataMember(Name = "copyId")] public string CopyId { get; set; }
        [DataMember(Name = "memberId")] public string MemberId { get; set; }
        [DataMember(Name = "pickupDate")] public string PickupDate { get; set; }
        [DataMember(Name = "dueDate")] public string DueDate { get; set; }
        [DataMember(Name = "status")] public string Status { get; set; }
    }

    [DataContract]
    public class BorrowReqObj
    {
        [DataMember] public string MemberId { get; set; }
        [DataMember] public string CopyId { get; set; }
        [DataMember] public int Days { get; set; }
    }

    [DataContract]
    public class ReturnReqObj
    {
        [DataMember] public string BorrowId { get; set; }
    }

    [DataContract]
    public class AddBookReqObj
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Cat_Id { get; set; }

        [DataMember]
        public string Shelf_location { get; set; }

        [DataMember]
        public int Copies { get; set; }
    }

    [DataContract]
    public class AddMemberReqObj
    {
        [DataMember]
        public string Fname { get; set; }

        [DataMember]
        public string Lname { get; set; }

        [DataMember]
        public string Mt_Id { get; set; }
    }

    [DataContract]
    public class UpdateFineStatusReqObj
    {
        [DataMember]
        public string FineId { get; set; }

        [DataMember]
        public string Status { get; set; }
    }
}
