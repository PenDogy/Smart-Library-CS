
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SmartLibraryApp
{
    public class dbService : IdbService
    {
        private string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["SmartLibraryConn"];

                if (setting == null)
                    throw new Exception("Connection String SmartLibraryConn not found.");

                return setting.ConnectionString;
            }
        }

        // =========================================
        // TEST API
        // GET /dbService.svc/test
        // =========================================
        public string test()
        {
            return "hello world";
        }

        // =========================================
        // GET LIBRARY DATA
        // GET /dbService.svc/getLibraryData
        // =========================================
        public LibraryDataDTO getLibraryData()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                var result = new LibraryDataDTO();

                // 1. Category
                result.Categories = ReadRows(conn, @"
                    SELECT Cat_Id, CatName, [Desc]
                    FROM Category
                    ORDER BY CatName",
                    r => new CategoryDTO
                    {
                        Id = GetString(r, "Cat_Id"),
                        Name = GetString(r, "CatName"),
                        Description = GetString(r, "Desc")
                    });

                // 2. Book
                result.Books = ReadRows(conn, @"
                    SELECT Book_Id, Cat_id, Quantity, [Desc],
                           Title, Author, Publisher, PublishY
                    FROM Book
                    ORDER BY Title",
                    r => new BookDTO
                    {
                        Id = GetString(r, "Book_Id"),
                        CategoryId = GetString(r, "Cat_id"),
                        Quantity = GetInt(r, "Quantity"),
                        Description = GetString(r, "Desc"),
                        Title = GetString(r, "Title"),
                        Author = GetString(r, "Author"),
                        Publisher = GetString(r, "Publisher"),
                        PublishYear = GetNullableInt(r, "PublishY"),
                        TotalBorrows = 0
                    });

                // 3. Book Copies
                result.BookCopies = ReadRows(conn, @"
                    SELECT Copy_Id, Book_Id, Shelf_location,
                           Price, [Status]
                    FROM Book_Copies",
                    r => new BookCopyDTO
                    {
                        CopyId = GetString(r, "Copy_Id"),
                        BookId = GetString(r, "Book_Id"),
                        Shelf = GetString(r, "Shelf_location"),
                        Price = GetNullableDecimal(r, "Price"),
                        Status = GetString(r, "Status")
                    });

                // 4. Member Type
                // ใช้ชื่อ Column จริง: Max_Quota และ Loan_Days
                result.MemberTypes = ReadRows(conn, @"
                    SELECT Mt_id, Max_Quota, Loan_Days, [Desc]
                    FROM MemberType",
                    r => new MemberTypeDTO
                    {
                        TypeId = GetString(r, "Mt_id"),
                        MaxQuota = GetInt(r, "Max_Quota"),
                        LoanDays = GetInt(r, "Loan_Days"),
                        Description = GetString(r, "Desc")
                    });

                // 5. Member
                result.Members = ReadRows(conn, @"
                    SELECT Member_Id, Mt_Id, [Status], Dues,
                           Fname, Lname, Phonenum,
                           R_Date, E_Date, Dues_Date
                    FROM Member",
                    r => new MemberDTO
                    {
                        Id = GetString(r, "Member_Id"),
                        MemberTypeId = GetString(r, "Mt_Id"),
                        Status = GetString(r, "Status"),
                        Dues = GetDecimal(r, "Dues"),
                        FirstName = GetString(r, "Fname"),
                        LastName = GetString(r, "Lname"),
                        PhoneNumber = GetString(r, "Phonenum"),
                        RegisterDate = GetDateString(r, "R_Date"),
                        ExpireDate = GetDateString(r, "E_Date"),
                        DuesDate = GetDateString(r, "Dues_Date")
                    });

                // 6. Borrowing
                result.Borrowings = ReadRows(conn, @"
                    SELECT Borrow_Id, Member_Id, Copy_Id,
                           Date_Taken, Date_Due, Return_Date,
                           [Status], Fine_Id
                    FROM Borrowing",
                    r => new BorrowingDTO
                    {
                        BorrowId = GetString(r, "Borrow_Id"),
                        MemberId = GetString(r, "Member_Id"),
                        CopyId = GetString(r, "Copy_Id"),
                        BorrowDate = GetDateString(r, "Date_Taken"),
                        DueDate = GetDateString(r, "Date_Due"),
                        ReturnDate = GetDateString(r, "Return_Date"),
                        Status = GetString(r, "Status"),
                        FineId = GetString(r, "Fine_Id")
                    });

                // 7. Fines
                result.Fines = ReadRows(conn, @"
                    SELECT Fine_Id, Amount, [Status], Note
                    FROM Fines",
                    r => new FineDTO
                    {
                        FineId = GetString(r, "Fine_Id"),
                        Amount = GetDecimal(r, "Amount"),
                        Status = GetString(r, "Status"),
                        Note = GetString(r, "Note")
                    });

                // 8. Reservation
                result.Reservations = ReadRows(conn, @"
                    SELECT Reservation_Id, Copy_Id, Member_Id,
                           Pickup_date, Date_due, [Status]
                    FROM Reservation",
                    r => new ReservationDTO
                    {
                        ReservationId = GetString(r, "Reservation_Id"),
                        CopyId = GetString(r, "Copy_Id"),
                        MemberId = GetString(r, "Member_Id"),
                        PickupDate = GetDateString(r, "Pickup_date"),
                        DueDate = GetDateString(r, "Date_due"),
                        Status = GetString(r, "Status")
                    });

                return result;
            }
        }

        // =========================================
        // BORROW BOOK
        // POST /dbService.svc/borrowBook
        // =========================================
        public string borrowBook(BorrowReqObj input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.MemberId) ||
                string.IsNullOrWhiteSpace(input.CopyId))
            {
                return "Invalid request";
            }

            int days = input.Days > 0 ? input.Days : 7;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // ตรวจสอบสมาชิก
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Member WHERE Member_Id = @MemberId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@MemberId", SqlDbType.VarChar, 50)
                                .Value = input.MemberId;

                            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            {
                                transaction.Rollback();
                                return "Member not found";
                            }
                        }

                        // ตรวจสอบและล็อกสำเนาหนังสือ
                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT [Status]
                            FROM Book_Copies WITH (UPDLOCK, ROWLOCK)
                            WHERE Copy_Id = @CopyId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@CopyId", SqlDbType.VarChar, 50)
                                .Value = input.CopyId;

                            object status = cmd.ExecuteScalar();

                            if (status == null || status == DBNull.Value)
                            {
                                transaction.Rollback();
                                return "Book copy not found";
                            }

                            if (Convert.ToString(status) != "Available")
                            {
                                transaction.Rollback();
                                return "Book copy is not available";
                            }
                        }

                        // เปลี่ยนสถานะสำเนาหนังสือ
                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE Book_Copies
                            SET [Status] = 'Borrowed'
                            WHERE Copy_Id = @CopyId
                              AND [Status] = 'Available'",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@CopyId", SqlDbType.VarChar, 50)
                                .Value = input.CopyId;

                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                transaction.Rollback();
                                return "Book copy is not available";
                            }
                        }

                        // เพิ่มประวัติการยืม
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Borrowing
                            (
                                Borrow_Id, Member_Id, Copy_Id,
                                Date_Taken, Date_Due, Return_Date, [Status]
                            )
                            VALUES
                            (
                                @BorrowId, @MemberId, @CopyId,
                                @DateTaken, @DateDue, NULL, 'Borrowed'
                            )",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@BorrowId", SqlDbType.VarChar, 50)
                                .Value = Guid.NewGuid().ToString();

                            cmd.Parameters.Add("@MemberId", SqlDbType.VarChar, 50)
                                .Value = input.MemberId;

                            cmd.Parameters.Add("@CopyId", SqlDbType.VarChar, 50)
                                .Value = input.CopyId;

                            cmd.Parameters.Add("@DateTaken", SqlDbType.Date)
                                .Value = DateTime.Today;

                            cmd.Parameters.Add("@DateDue", SqlDbType.Date)
                                .Value = DateTime.Today.AddDays(days);

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Borrow successful";
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================================
        // RETURN BOOK
        // POST /dbService.svc/returnBook
        // =========================================
        public string returnBook(ReturnReqObj input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.BorrowId))
            {
                return "Invalid request";
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string copyId;

                        // ค้นหารายการยืม
                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT Copy_Id
                            FROM Borrowing WITH (UPDLOCK, ROWLOCK)
                            WHERE Borrow_Id = @BorrowId
                              AND [Status] <> 'Returned'",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@BorrowId", SqlDbType.VarChar, 50)
                                .Value = input.BorrowId;

                            object value = cmd.ExecuteScalar();

                            if (value == null || value == DBNull.Value)
                            {
                                transaction.Rollback();
                                return "Borrowing record not found or already returned";
                            }

                            copyId = Convert.ToString(value);
                        }

                        // อัปเดตประวัติการคืน
                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE Borrowing
                            SET Return_Date = @ReturnDate,
                                [Status] = 'Returned'
                            WHERE Borrow_Id = @BorrowId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@ReturnDate", SqlDbType.Date)
                                .Value = DateTime.Today;

                            cmd.Parameters.Add("@BorrowId", SqlDbType.VarChar, 50)
                                .Value = input.BorrowId;

                            cmd.ExecuteNonQuery();
                        }

                        // เปลี่ยนสถานะสำเนาหนังสือ
                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE Book_Copies
                            SET [Status] = 'Available'
                            WHERE Copy_Id = @CopyId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@CopyId", SqlDbType.VarChar, 50)
                                .Value = copyId;

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Return successful";
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================================
        // COMMON SQL READER
        // =========================================
        private static T[] ReadRows<T>(
            SqlConnection conn,
            string sql,
            Func<SqlDataReader, T> mapper)
        {
            List<T> items = new List<T>();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    items.Add(mapper(reader));
                }
            }

            return items.ToArray();
        }

        private static string GetString(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? null
                : Convert.ToString(r.GetValue(index));
        }

        private static int GetInt(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? 0
                : Convert.ToInt32(r.GetValue(index));
        }

        private static int? GetNullableInt(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? (int?)null
                : Convert.ToInt32(r.GetValue(index));
        }

        private static decimal GetDecimal(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? 0m
                : Convert.ToDecimal(r.GetValue(index));
        }

        private static decimal? GetNullableDecimal(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? (decimal?)null
                : Convert.ToDecimal(r.GetValue(index));
        }

        private static string GetDateString(SqlDataReader r, string column)
        {
            int index = r.GetOrdinal(column);

            return r.IsDBNull(index)
                ? null
                : Convert.ToDateTime(r.GetValue(index)).ToString("yyyy-MM-dd");
        }

        // =========================================
        // ADD BOOK
        // POST /dbService.svc/addBook
        // =========================================
        public string addBook(AddBookReqObj input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.Title) ||
                string.IsNullOrWhiteSpace(input.Cat_Id) ||
                string.IsNullOrWhiteSpace(input.Shelf_location) ||
                input.Copies < 1)
            {
                return "Invalid request";
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlTransaction transaction =
                    conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        // ตรวจสอบ Category
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Category WHERE Cat_Id = @CatId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@CatId", SqlDbType.VarChar, 50)
                                .Value = input.Cat_Id;

                            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            {
                                transaction.Rollback();
                                return "Category not found";
                            }
                        }

                        // สร้าง Book_Id ถัดไป
                        int nextBookNumber;

                        using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ISNULL(
                        MAX(TRY_CONVERT(INT, SUBSTRING(Book_Id, 4, 47))), 0
                    ) + 1
                    FROM Book WITH (UPDLOCK, HOLDLOCK)
                    WHERE Book_Id LIKE 'BK-%'",
                            conn, transaction))
                        {
                            nextBookNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string bookId = "BK-" + nextBookNumber.ToString("D4");

                        // สร้าง Book_Id
                        using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Book
                    (
                        Book_Id, Cat_id, Quantity,
                        Title, Author
                    )
                    VALUES
                    (
                        @BookId, @CatId, @Quantity,
                        @Title, @Author
                    )",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@BookId", SqlDbType.VarChar, 50)
                                .Value = bookId;

                            cmd.Parameters.Add("@CatId", SqlDbType.VarChar, 50)
                                .Value = input.Cat_Id;

                            cmd.Parameters.Add("@Quantity", SqlDbType.Int)
                                .Value = input.Copies;

                            cmd.Parameters.Add("@Title", SqlDbType.VarChar, 255)
                                .Value = input.Title.Trim();

                            cmd.Parameters.Add("@Author", SqlDbType.VarChar, 255)
                                .Value = string.IsNullOrWhiteSpace(input.Author)
                                    ? (object)DBNull.Value
                                    : input.Author.Trim();

                            cmd.ExecuteNonQuery();
                        }

                        // หาหมายเลข Copy_Id ถัดไป
                        int nextCopyNumber;

                        using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ISNULL(
                        MAX(TRY_CONVERT(INT, SUBSTRING(Copy_Id, 4, 47))), 0
                    ) + 1
                    FROM Book_Copies WITH (UPDLOCK, HOLDLOCK)
                    WHERE Copy_Id LIKE 'CP-%'",
                            conn, transaction))
                        {
                            nextCopyNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // สร้างสำเนาหนังสือตามจำนวน
                        for (int i = 0; i < input.Copies; i++)
                        {
                            string copyId =
                                "CP-" + (nextCopyNumber + i).ToString("D5");

                            using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Book_Copies
                        (
                            Copy_Id, Book_Id,
                            Shelf_location, Price, [Status]
                        )
                        VALUES
                        (
                            @CopyId, @BookId,
                            @Shelf, NULL, 'Available'
                        )",
                                conn, transaction))
                            {
                                cmd.Parameters.Add("@CopyId", SqlDbType.VarChar, 50)
                                    .Value = copyId;

                                cmd.Parameters.Add("@BookId", SqlDbType.VarChar, 50)
                                    .Value = bookId;

                                cmd.Parameters.Add("@Shelf", SqlDbType.VarChar, 50)
                                    .Value = input.Shelf_location.Trim();

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return "Add book successful: " + bookId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================================
        // ADD MEMBER
        // POST /dbService.svc/addMember
        // =========================================
        public string addMember(AddMemberReqObj input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.Fname) ||
                string.IsNullOrWhiteSpace(input.Lname) ||
                string.IsNullOrWhiteSpace(input.Mt_Id))
            {
                return "Invalid request";
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // ตรวจสอบประเภทสมาชิก
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM MemberType WHERE Mt_id = @MtId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@MtId", SqlDbType.VarChar, 50)
                                .Value = input.Mt_Id;

                            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            {
                                transaction.Rollback();
                                return "Member type not found";
                            }
                        }

                        string memberId =
                            "MB-" + Guid.NewGuid().ToString("N").Substring(0, 12)
                            .ToUpperInvariant();

                        using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Member
                    (
                        Member_Id, Mt_Id,
                        Fname, Lname, R_Date
                    )
                    VALUES
                    (
                        @MemberId, @MtId,
                        @Fname, @Lname, @RegisterDate
                    )",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@MemberId", SqlDbType.VarChar, 50)
                                .Value = memberId;

                            cmd.Parameters.Add("@MtId", SqlDbType.VarChar, 50)
                                .Value = input.Mt_Id;

                            cmd.Parameters.Add("@Fname", SqlDbType.VarChar, 50)
                                .Value = input.Fname.Trim();

                            cmd.Parameters.Add("@Lname", SqlDbType.VarChar, 50)
                                .Value = input.Lname.Trim();

                            cmd.Parameters.Add("@RegisterDate", SqlDbType.Date)
                                .Value = DateTime.Today;

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        return "Add member successful: " + memberId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================================
        // UPDATE FINE STATUS
        // POST /dbService.svc/updateFineStatus
        // =========================================
        public string updateFineStatus(UpdateFineStatusReqObj input)
        {
            if (input == null ||
                string.IsNullOrWhiteSpace(input.FineId))
            {
                return "Invalid request";
            }

            // อนุญาตให้เปลี่ยนเป็น Paid เท่านั้น
            if (!string.Equals(
                input.Status?.Trim(),
                "Paid",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Invalid fine status";
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string currentStatus;

                        // ตรวจสอบค่าปรับและล็อกแถว ป้องกันการชำระซ้ำ
                        using (SqlCommand cmd = new SqlCommand(@"
                    SELECT [Status]
                    FROM Fines WITH (UPDLOCK, ROWLOCK)
                    WHERE Fine_Id = @FineId",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@FineId", SqlDbType.VarChar, 50)
                                .Value = input.FineId.Trim();

                            object result = cmd.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                transaction.Rollback();
                                return "Fine not found";
                            }

                            currentStatus = Convert.ToString(result);
                        }

                        if (string.Equals(
                            currentStatus,
                            "Paid",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Rollback();
                            return "Fine already paid";
                        }

                        if (!string.Equals(
                            currentStatus,
                            "Unpaid",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Rollback();
                            return "Invalid current fine status";
                        }

                        // บันทึกสถานะลง SQL Server
                        using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Fines
                    SET [Status] = 'Paid'
                    WHERE Fine_Id = @FineId
                      AND [Status] = 'Unpaid'",
                            conn, transaction))
                        {
                            cmd.Parameters.Add("@FineId", SqlDbType.VarChar, 50)
                                .Value = input.FineId.Trim();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return "Fine update failed";
                            }
                        }

                        transaction.Commit();

                        return "Fine payment successful";
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
