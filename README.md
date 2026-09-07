# Notenokand

ระบบบริหารกิจการบ้านนกแอ่น พัฒนาด้วย ASP.NET Core MVC, Entity Framework Core และ SQL Server ออกแบบแบบ Mobile First และแสดงข้อมูลหลักด้วย Card View

## ความสามารถที่พร้อมใช้งาน

- หน้า Landing, สมัครสมาชิก, ยืนยันอีเมล, เข้าสู่ระบบ และออกจากระบบ
- Onboarding สร้างบัญชีกิจการและตึกนกแห่งแรก
- Dropdown ที่อยู่ไทยแบบ จังหวัด → อำเภอ/เขต → ตำบล/แขวง → รหัสไปรษณีย์
- Dashboard เริ่มต้นแบบ responsive สำหรับมือถือ แท็บเล็ต และ PC
- ASP.NET Core Identity พร้อมยืนยันอีเมล, lockout, anti-forgery และ rate limit แยกตาม IP
- SQL Server schema และ idempotent migration script

## โครงสร้าง

- `src/Notenokand.Domain` โมเดลธุรกิจและชนิดข้อมูลหลัก
- `src/Notenokand.Infrastructure` Entity Framework Core, Identity และการเชื่อมต่อ SQL Server
- `src/Notenokand.Web` เว็บแอป ASP.NET Core MVC
- `tests/Notenokand.Tests` การทดสอบอัตโนมัติ
- `tools/Notenokand.DataImporter` ตัวนำเข้าข้อมูลที่อยู่ประเทศไทย
- `database/InitialCreate.sql` SQL script รวมแบบ idempotent

## เปิดและรันโครงการ

เปิดไฟล์ `D:\Notenokand\Notenokand.slnx` ด้วย Visual Studio 2022 ที่รองรับ .NET 10 หรือเปิด folder `D:\Notenokand` ด้วย Visual Studio Code

จาก PowerShell:

```powershell
cd D:\Notenokand
dotnet restore
dotnet ef database update --project src/Notenokand.Infrastructure --startup-project src/Notenokand.Web
dotnet run --project src/Notenokand.Web
```

จากนั้นเปิด URL ที่แสดงใน Terminal และเลือก “เริ่มใช้งาน”

## การเชื่อมต่อฐานข้อมูล

สร้างไฟล์ `src/Notenokand.Web/appsettings.Local.json` และเพิ่ม connection string ชื่อ `Notenokand`:

```json
{
  "ConnectionStrings": {
    "Notenokand": "Server=.\\SQL2016;Database=notenokand;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

ห้าม commit รหัสผ่านหรือไฟล์ `appsettings.Local.json` ขึ้น GitHub

## การยืนยันอีเมล

ใน `Development` ระบบแสดงลิงก์ยืนยันบนหน้าจอเพื่อให้ทดสอบ flow ได้ทันที ส่วน Production จะไม่แสดง token ต้องเชื่อมผู้ให้บริการอีเมลก่อนเปิดใช้งานจริง

## ข้อมูลที่อยู่ประเทศไทย

ข้อมูลต้นทางและ license อยู่ใน `data/thai-addresses` นำเข้าหรืออัปเดตแบบ idempotent ด้วย:

```powershell
dotnet run --project tools/Notenokand.DataImporter/Notenokand.DataImporter.csproj
```

สามารถส่ง connection string อื่นด้วยตัวเลือก `--connection` โดยไม่บันทึกรหัสผ่านลง source control