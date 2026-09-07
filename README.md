# Notenokand

ระบบบริหารตึกนกและวิเคราะห์ผลผลิต พัฒนาด้วย ASP.NET Core MVC, Entity Framework Core และ SQL Server

## โครงสร้าง

- `src/Notenokand.Domain` โมเดลธุรกิจและชนิดข้อมูลหลัก
- `src/Notenokand.Infrastructure` Entity Framework Core, Identity และการเชื่อมต่อ SQL Server
- `src/Notenokand.Web` เว็บแอปแบบ Mobile First
- `tests/Notenokand.Tests` การทดสอบอัตโนมัติ

## เริ่มใช้งาน

1. สร้างไฟล์ `src/Notenokand.Web/appsettings.Local.json`
2. เพิ่ม connection string ชื่อ `Notenokand`
3. ตั้ง environment เป็น `Local`
4. รัน migration แล้วเริ่มเว็บแอป

ตัวอย่าง connection string:

```json
{
  "ConnectionStrings": {
    "Notenokand": "Server=.\\SQL2016;Database=notenokand;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

ห้าม commit รหัสผ่านหรือไฟล์ `appsettings.Local.json` ขึ้น GitHub
