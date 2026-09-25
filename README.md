# Notenokand

ระบบบริหารกิจการบ้านนกแอ่น พัฒนาด้วย ASP.NET Core MVC, Entity Framework Core และ SQL Server ออกแบบแบบ Mobile First และแสดงข้อมูลหลักด้วย Card View

## ความสามารถที่พร้อมใช้งาน

- หน้า Landing, สมัครสมาชิก, ยืนยันอีเมล, เข้าสู่ระบบ และออกจากระบบ
- Onboarding สร้างบัญชีกิจการและตึกนกแห่งแรก
- อัปโหลดรูปตึกนกได้ 1 รูปต่อหลัง พร้อมพรีวิวและเก็บไฟล์แบบ private
- บันทึกรายรับ–ค่าใช้จ่าย หมวดหมู่ หลักฐาน และสรุปยอดรายเดือนแบบ Card View
- Dropdown ที่อยู่ไทยแบบ จังหวัด → อำเภอ/เขต → ตำบล/แขวง → รหัสไปรษณีย์
- Dashboard แสดงตึกนกแบบ Card View พร้อมกรองสถานะ แก้ไข และเปิด/ปิดการใช้งาน
- เพิ่มและแก้ไขตึกพร้อมที่อยู่ไทย พิกัด และแผนที่ OpenStreetMap
- Dashboard แบบ responsive สำหรับมือถือ แท็บเล็ต และ PC
- Footer Navigation สำหรับมือถือ/แท็บเล็ต พร้อม Quick Add และ safe area
- งานซ่อมบำรุงแบบครบสถานะ พร้อมค่าอะไหล่ ค่าแรง เวลาหยุดทำงาน ผลตรวจรับ และรอบตรวจถัดไป
- ตรวจคุณภาพรังนกแต่ละรอบแบบ 0–100 พร้อมระดับผลและแนวโน้มคุณภาพ
- ศูนย์แจ้งเตือนรวมทั้งนัดหมายใกล้ถึง นัดหมายเลยกำหนด และงานซ่อมที่ยังไม่ปิด
- การขายจากล็อตรองรับยังไม่ชำระ ชำระบางส่วน วันครบกำหนด และรับชำระเพิ่ม โดยลงรายรับตามเงินจริงที่ได้รับ
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

ระบบรองรับอีเมลยืนยันบัญชี ลืมรหัสผ่าน และคำเชิญสมาชิกผ่าน SMTP ให้คัดลอก
`src/Notenokand.Web/appsettings.Local.example.json` เป็น `appsettings.Local.json` แล้วตั้งค่า
`PublicBaseUrl` เป็น HTTPS พร้อมค่า `Email` ของผู้ให้บริการจริง อย่า commit รหัสผ่าน SMTP ขึ้น GitHub

ใน `Development` ระบบยังแสดงลิงก์ทดสอบบนหน้าจอเมื่อส่งอีเมลไม่ได้ ส่วน Production
จะไม่เปิดเผย token หรือลิงก์สำรอง

สามารถใช้ environment variables แทนไฟล์ เช่น `Email__Password` และ `PublicBaseUrl`
เพื่อให้ secret อยู่ในระบบ deploy เท่านั้น

## สิทธิ์ผู้ใช้งานและประวัติการแก้ไข

- Owner จัดการสมาชิก สิทธิ์ ตึก รายงาน และประวัติการแก้ไขได้ทั้งหมด
- Editor แก้ไขข้อมูลได้เฉพาะตึกที่ Owner มอบหมาย
- Viewer เปิดดูข้อมูลได้ แต่ไม่สามารถเปลี่ยนแปลงข้อมูลธุรกิจ
- เมนู “ประวัติ” เก็บผู้ใช้ เวลา IP และค่าก่อน/หลังของรายการธุรกิจ โดยไม่บันทึก path หรือ hash ของไฟล์ส่วนตัว

## รายงานและผลตอบแทน

หน้า “รายงาน” กรองช่วงวันที่และดาวน์โหลด CSV สำหรับรายรับ–รายจ่าย ผลผลิต/สต็อก และการขาย
พร้อมมุมมองสำหรับพิมพ์หรือบันทึกเป็น PDF จากเบราว์เซอร์ หน้ารายละเอียดตึกแยกค่าใช้จ่ายดำเนินงาน
ค่าใช้จ่ายลงทุน กำไรดำเนินงาน และผลตอบแทนต่อเงินลงทุน

## ข้อมูลที่อยู่ประเทศไทย

ข้อมูลต้นทางและ license อยู่ใน `data/thai-addresses` นำเข้าหรืออัปเดตแบบ idempotent ด้วย:

```powershell
dotnet run --project tools/Notenokand.DataImporter/Notenokand.DataImporter.csproj
```

สามารถส่ง connection string อื่นด้วยตัวเลือก `--connection` โดยไม่บันทึกรหัสผ่านลง source control
## Thai Date/DateTime Picker

ปฏิทินกลางโหลดจาก Layout ทุกหน้า แสดงชื่อเดือนภาษาไทยและปี พ.ศ. แต่ส่งค่า ISO ให้ฝั่งเซิร์ฟเวอร์

- วันที่: ช่องแสดงผลใช้ `data-thai-datepicker` และระบุ `data-target` ไปยัง hidden input
- วันที่และเวลา: ใช้ `data-thai-datetimepicker` ด้วยโครงสร้างเดียวกัน
- DateOnly และ DateTime ถูกอ่านผ่าน `IsoTemporalModelBinder` เพื่อไม่ให้ Thai culture แปลงปีซ้ำ

ตัวอย่าง:

```html
<input id="EventDate" name="EventDate" type="hidden" data-thai-date-value />
<input type="text" data-thai-datepicker data-target="EventDate" readonly />
```
