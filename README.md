# Smart Library — Installation & Setup Guide

คู่มือการติดตั้งและเรียกใช้งานระบบ Smart Library

## 1. ภาพรวมโปรเจกต์

Smart Library เป็นระบบบริหารจัดการห้องสมุด ประกอบด้วยระบบจัดการหนังสือ สมาชิก การยืม-คืนหนังสือ และค่าปรับ โดยพัฒนาด้วยเทคโนโลยีดังนี้

| เทคโนโลยี | รายละเอียด |
|---|---|
| Programming Language | C# |
| Backend | ASP.NET Web Application (.NET Framework) |
| API | WCF REST Service |
| Database | Microsoft SQL Server |
| Database Management | SQL Server Management Studio 22 |
| IDE | Visual Studio Community 2026 |
| Frontend | HTML, CSS, JavaScript, Tailwind CSS |

## 2. สิ่งที่ต้องติดตั้งก่อนใช้งาน

โปรดติดตั้งโปรแกรมต่อไปนี้ก่อนเริ่มต้น

1. **Visual Studio Community 2026**  
   ใช้สำหรับเปิด แก้ไข และรันโปรเจกต์ C#

   ดาวน์โหลด: https://visualstudio.microsoft.com/vs/community/

   ใน Visual Studio Installer ให้เลือก Workload:
   - ASP.NET and web development

   ตรวจสอบให้มี .NET Framework Developer Pack / Targeting Pack ตามเวอร์ชันที่โปรเจกต์กำหนดด้วย

2. **SQL Server Management Studio 22 (SSMS 22)**  
   ใช้สำหรับสร้างฐานข้อมูลและจัดการข้อมูลใน SQL Server

   ดาวน์โหลด: https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

3. **Microsoft SQL Server Database Engine**  
   ต้องติดตั้ง SQL Server Database Engine เพื่อให้มี Database Server สำหรับจัดเก็บข้อมูล

   หมายเหตุ: SSMS เป็นเครื่องมือจัดการฐานข้อมูล ไม่ใช่ตัว Database Engine

## 3. ดาวน์โหลดและแตกไฟล์โปรเจกต์

ผู้พัฒนาได้จัดเตรียมไฟล์สำหรับติดตั้งไว้ดังนี้

- `ProjectV1.slnx` — ไฟล์ Solution สำหรับเปิดโปรเจกต์ด้วย Visual Studio
- `ProjectV1.zip` — ไฟล์โปรเจกต์ทั้งหมดในรูปแบบ ZIP
- `schema.sql` — ไฟล์ SQL สำหรับสร้างโครงสร้างฐานข้อมูล

ขั้นตอนการเตรียมไฟล์:

1. ดาวน์โหลดไฟล์ ZIP ของโปรเจกต์
2. คลิกขวาที่ไฟล์ ZIP
3. เลือก **Extract All...** หรือใช้โปรแกรมแตกไฟล์ที่ต้องการ
4. เลือกตำแหน่งสำหรับจัดเก็บโปรเจกต์
5. ตรวจสอบว่าไฟล์และโฟลเดอร์ของโปรเจกต์ถูกแตกออกมาเรียบร้อยแล้ว

ตัวอย่างโครงสร้างไฟล์:

```text
source/
│
├── ProjectV1.slnx
├── schema.sql
│
└── ProjectV1/
    ├── ProjectV1.csproj
    ├── Web.config
    ├── dbService.svc
    ├── dbService.svc.cs
    ├── ...
    │
    └── ...
```

หมายเหตุ: โครงสร้างข้างต้นเป็นตัวอย่าง ไฟล์จริงอาจมีโฟลเดอร์และชื่อไฟล์แตกต่างกันตามชุดโปรเจกต์ที่ได้รับ

## 4. สร้างฐานข้อมูลด้วย SQL Server Management Studio 22

### 4.1 เชื่อมต่อ SQL Server

1. เปิด SQL Server Management Studio 22
2. ที่หน้าต่าง Connect to Server เลือก:
   - Server type: Database Engine
   - Server name: ชื่อ SQL Server ที่ติดตั้งไว้
   - Authentication: เลือกวิธีการยืนยันตัวตนที่ตั้งค่าไว้
3. คลิก **Connect**

### 4.2 เปิดไฟล์ schema.sql

1. ไปที่เมนู **File > Open > File...**
2. เลือกไฟล์ `schema.sql`
3. รอให้ SQL Script เปิดขึ้นในหน้าต่าง Query
4. ตรวจสอบว่าเชื่อมต่อกับ SQL Server Instance ที่ต้องการแล้ว
5. กดปุ่ม **Execute** หรือกด `F5`

### 4.3 ตรวจสอบฐานข้อมูล

เมื่อ Execute สำเร็จ ให้ไปที่ Object Explorer

1. คลิกขวาที่ Databases
2. เลือก Refresh
3. ตรวจสอบว่ามีฐานข้อมูล `smart_library_v1`
4. ขยายฐานข้อมูลและตรวจสอบว่าตารางถูกสร้างเรียบร้อยแล้ว

ตารางหลักของระบบ ได้แก่

- Category
- Staff
- MemberType
- Book
- Member
- Book_Copies
- Fines
- Borrowing
- Reservation

**ข้อควรระวัง:** ตรวจสอบเนื้อหา `schema.sql` ก่อน Execute ทุกครั้ง หากสคริปต์มีคำสั่ง DROP TABLE หรือ DROP VIEW การรันซ้ำอาจลบตารางและข้อมูลเดิม ควรสำรองฐานข้อมูลก่อนดำเนินการ

## 5. ตั้งค่าการเชื่อมต่อฐานข้อมูล

หลังจากสร้างฐานข้อมูลแล้ว ให้ตั้งค่าการเชื่อมต่อ SQL Server ในโปรเจกต์

1. เปิดโฟลเดอร์โปรเจกต์ด้วย Visual Studio
2. ค้นหาไฟล์ `Web.config`
3. ค้นหาส่วน `<connectionStrings>`
4. แก้ไข Connection String ให้ตรงกับ SQL Server ของเครื่องที่ใช้งาน

ตัวอย่าง:

```xml
<connectionStrings>
    <add name="SmartLibraryConn"
         connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=smart_library_v1;Integrated Security=True;TrustServerCertificate=True;"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

ให้เปลี่ยน `YOUR_SERVER_NAME` เป็นชื่อ SQL Server Instance ของเครื่อง เช่น

```text
.\SQLEXPRESS
```

หรือ

```text
localhost
```

หากใช้ SQL Server Authentication ให้เปลี่ยนรูปแบบการยืนยันตัวตนให้ตรงกับบัญชี SQL Server ที่ตั้งค่าไว้ และไม่ควรเผยแพร่รหัสผ่านจริงในไฟล์ ZIP หรือ Git Repository

หมายเหตุ: หากโปรเจกต์มี Connection String มากกว่าหนึ่งตำแหน่ง ให้ตรวจสอบและแก้ไขทุกตำแหน่งที่ใช้งานจริง

## 6. เปิดโปรเจกต์ด้วย Visual Studio Community 2026

1. เปิด Visual Studio Community 2026
2. เลือก **Open a project or solution**
3. เลือกไฟล์ `ProjectV1.slnx`
4. รอให้ Visual Studio โหลด Solution และโปรเจกต์ทั้งหมด
5. หากมีข้อความให้ Restore NuGet Packages ให้ดำเนินการ Restore
6. ตรวจสอบว่าไม่มี Error จากการโหลดโปรเจกต์

หากมีปัญหาเกี่ยวกับ .NET Framework ให้ตรวจสอบว่าได้ติดตั้ง Developer Pack / Targeting Pack ตามเวอร์ชันที่โปรเจกต์กำหนดแล้ว

## 7. ตั้งค่า Startup Project

1. ใน Solution Explorer คลิกขวาที่โปรเจกต์ ASP.NET Web Application
2. เลือก **Set as Startup Project**
3. ตรวจสอบว่าโปรเจกต์ที่เลือกเป็นโปรเจกต์ Web Application ที่มีไฟล์ `Web.config` และ WCF Service
4. ตรวจสอบการตั้งค่า Web Server เป็น IIS Express ตามการตั้งค่าของโปรเจกต์

## 8. เรียกใช้งานโปรเจกต์

เมื่อกำหนดค่าฐานข้อมูลและ Startup Project เรียบร้อยแล้ว

1. กด `Ctrl + Shift + B` เพื่อ Build Solution
2. ตรวจสอบว่า Build สำเร็จโดยไม่มี Error
3. กด `F5` เพื่อเริ่มต้นโปรเจกต์ หรือ `Ctrl + F5` เพื่อรันโดยไม่เปิด Debugger
4. รอให้ IIS Express เริ่มทำงาน
5. เว็บเบราว์เซอร์จะเปิดหน้าเว็บไซต์ของระบบ

## 9. ตรวจสอบ WCF REST API

ระบบใช้ WCF REST Service สำหรับติดต่อระหว่าง Frontend และ SQL Server

ตัวอย่าง Endpoint:

| HTTP Method | Endpoint | รายละเอียด |
|---|---|---|
| GET | `/dbService.svc/test` | ตรวจสอบการทำงานของ API |
| GET | `/dbService.svc/getData` | ดึงข้อมูลระบบห้องสมุด |
| POST | `/dbService.svc/borrowBook` | ยืมหนังสือ |
| POST | `/dbService.svc/returnBook` | คืนหนังสือ |
| POST | `/dbService.svc/addBook` | เพิ่มหนังสือ (หากติดตั้ง Endpoint นี้แล้ว) |
| POST | `/dbService.svc/addMember` | ลงทะเบียนสมาชิก (หากติดตั้ง Endpoint นี้แล้ว) |
| POST | `/dbService.svc/updateFineStatus` | อัปเดตสถานะการชำระค่าปรับ (Unpaid → Paid) (หากติดตั้ง Endpoint นี้แล้ว) |

ตัวอย่างการทดสอบ:

```text
https://localhost:<PORT>/dbService.svc/test
```

เปลี่ยน `<PORT>` เป็น Port ที่ IIS Express กำหนดให้โปรเจกต์

หาก API ทำงานปกติ จะได้รับ Response กลับจาก WCF Service

## 10. การตั้งค่า API URL ใน Frontend

หาก Frontend เรียกใช้งาน WCF API ผ่าน JavaScript ให้ตรวจสอบตัวแปร `API_BASE`

ตัวอย่าง:

```javascript
const API_BASE = "https://localhost:<PORT>/dbService.svc/";
```

เปลี่ยน `<PORT>` ให้ตรงกับ Port ที่โปรเจกต์กำลังใช้งานจริง

หาก Frontend และ WCF API ทำงานคนละ Origin หรือคนละ Port อาจต้องตั้งค่า CORS ใน WCF เพื่ออนุญาตให้ Frontend เรียกใช้งาน API ได้

## 11. การแก้ไขปัญหาเบื้องต้น

| ปัญหา | แนวทางตรวจสอบ |
|---|---|
| Cannot open database | ตรวจสอบว่ามีฐานข้อมูล `smart_library_v1` แล้ว |
| Login failed for user | ตรวจสอบ Authentication และสิทธิ์การเข้าถึง SQL Server |
| A network-related error occurred | ตรวจสอบ Server Name และสถานะ SQL Server Service |
| Build failed | ตรวจสอบ NuGet Packages และ .NET Framework Targeting Pack |
| HTTP 404 | ตรวจสอบ URL และตำแหน่งไฟล์ `dbService.svc` |
| HTTP 500 | ตรวจสอบ Exception ใน WCF และ Connection String |
| CORS Error | ตรวจสอบการตั้งค่า CORS ของ WCF |
| API ไม่สามารถเชื่อมต่อได้ | ตรวจสอบ Port, IIS Express และสถานะ Service |

## 12. ลำดับการติดตั้งโดยสรุป

```text
ดาวน์โหลดไฟล์โปรเจกต์
        │
        ▼
แตกไฟล์ ZIP
        │
        ▼
ติดตั้ง SQL Server และ SSMS 22
        │
        ▼
เปิด SSMS และ Execute schema.sql
        │
        ▼
ตรวจสอบฐานข้อมูล smart_library_v1
        │
        ▼
แก้ไข Connection String ใน Web.config
        │
        ▼
เปิด ProjectV1.slnx ด้วย Visual Studio 2026
        │
        ▼
Restore NuGet Packages
        │
        ▼
Set as Startup Project
        │
        ▼
Build Solution
        │
        ▼
Run ด้วย IIS Express (F5)
        │
        ▼
เข้าใช้งาน Smart Library
```

---

**หมายเหตุ:** ชื่อ Server, Port, Authentication และ .NET Framework Targeting Pack อาจแตกต่างกันในแต่ละเครื่อง ผู้ติดตั้งต้องกำหนดค่าให้ตรงกับสภาพแวดล้อมของตนเอง