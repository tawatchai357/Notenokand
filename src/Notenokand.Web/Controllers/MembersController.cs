using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Members;
using Notenokand.Web.Services;
namespace Notenokand.Web.Controllers;

[Authorize, Route("app/members")]
public sealed class MembersController(NotenokandDbContext db, UserManager<ApplicationUser> users, AccountEmailService email) : Controller
{
    private async Task<AccountUser?> OwnerAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        return await db.AccountUsers.FirstOrDefaultAsync(x => x.UserId == id && x.IsActive && !x.IsDeleted && x.RoleName == "Owner");
    }
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var owner = await OwnerAsync(); if (owner is null) return Forbid();
        var memberships = await db.AccountUsers.AsNoTracking().Where(x => x.AccountId == owner.AccountId && !x.IsDeleted).ToListAsync();
        var ids = memberships.Select(x => x.UserId).ToArray();
        var people = await users.Users.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        var assignments = await db.BuildingUsers.AsNoTracking().Where(x => ids.Contains(x.UserId) && !x.IsDeleted).ToListAsync();
        return View(new MembersPageModel {
            Members = memberships.Select(x => new MemberRowModel { UserId=x.UserId, Name=people[x.UserId].DisplayName, Email=people[x.UserId].Email ?? "", Role=x.RoleName, Active=x.IsActive, BuildingIds=assignments.Where(a => a.UserId==x.UserId).Select(a=>a.BuildingId).ToList() }).ToList(),
            Buildings = await db.BirdBuildings.AsNoTracking().Where(x=>x.AccountId==owner.AccountId && !x.IsDeleted).OrderBy(x=>x.Name).Select(x=>new SelectListItem(x.Name,x.Id.ToString())).ToListAsync()
        });
    }
    [HttpPost("invite"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteMemberModel model)
    {
        var owner = await OwnerAsync(); if (owner is null) return Forbid();
        if (!ModelState.IsValid) { TempData["MemberError"]="อีเมลหรือสิทธิ์ไม่ถูกต้อง"; return RedirectToAction(nameof(Index)); }
        var normalized = model.Email.Trim().ToLowerInvariant();
        var invitedUserId = await users.Users.Where(u => u.NormalizedEmail == normalized.ToUpperInvariant())
            .Select(u => (Guid?)u.Id).FirstOrDefaultAsync();
        if (invitedUserId.HasValue && await db.AccountUsers.AnyAsync(x => x.AccountId == owner.AccountId && x.UserId == invitedUserId.Value && !x.IsDeleted))
        { TempData["MemberError"]="อีเมลนี้เป็นสมาชิกอยู่แล้ว"; return RedirectToAction(nameof(Index)); }
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = WebEncoders.Base64UrlEncode(bytes);
        db.AccountInvitations.Add(new AccountInvitation { AccountId=owner.AccountId, Email=normalized, RoleName=model.Role, TokenHash=Convert.ToHexString(SHA256.HashData(bytes)), ExpiresAt=DateTimeOffset.UtcNow.AddDays(7), CreatedByUserId=owner.UserId });
        await db.SaveChangesAsync();
        var relative = "app/members/accept?token=" + Uri.EscapeDataString(token);
        var sent = email.IsConfigured && await email.SendLinkAsync(normalized, "คำเชิญเข้าร่วม Notenokand", email.Link(relative));
        if (!sent && HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()) TempData["DevelopmentInviteUrl"] = "/" + relative;
        TempData["SuccessMessage"] = sent ? "ส่งคำเชิญแล้ว" : "สร้างคำเชิญแล้ว แต่ยังส่งอีเมลไม่ได้";
        return RedirectToAction(nameof(Index));
    }
    [HttpGet("accept")]
    public async Task<IActionResult> Accept(string token)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Challenge();
        byte[] bytes; try { bytes=WebEncoders.Base64UrlDecode(token); } catch { return BadRequest(); }
        var hash=Convert.ToHexString(SHA256.HashData(bytes));
        var invitation=await db.AccountInvitations.FirstOrDefaultAsync(x=>x.TokenHash==hash && !x.IsDeleted && x.AcceptedAt==null && x.ExpiresAt>DateTimeOffset.UtcNow);
        var user=await users.FindByIdAsync(userId.ToString());
        if (invitation is null || user?.Email is null || !string.Equals(invitation.Email,user.Email,StringComparison.OrdinalIgnoreCase)) return Forbid();
        if (!await db.AccountUsers.AnyAsync(x=>x.AccountId==invitation.AccountId && x.UserId==userId))
            db.AccountUsers.Add(new AccountUser { AccountId=invitation.AccountId, UserId=userId, RoleName=invitation.RoleName, IsActive=true, CreatedByUserId=invitation.CreatedByUserId });
        invitation.AcceptedAt=DateTimeOffset.UtcNow; invitation.UpdatedAt=DateTimeOffset.UtcNow; invitation.UpdatedByUserId=userId;
        await db.SaveChangesAsync(); TempData["SuccessMessage"]="เข้าร่วมกิจการแล้ว"; return RedirectToAction("Index","Dashboard");
    }
    [HttpPost("{userId:guid}/role"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Role(Guid userId, string role)
    {
        var owner=await OwnerAsync(); if(owner is null)return Forbid();
        if (role is not ("Editor" or "Viewer")) return BadRequest();
        var member=await db.AccountUsers.SingleOrDefaultAsync(x=>x.AccountId==owner.AccountId && x.UserId==userId && x.RoleName!="Owner" && !x.IsDeleted);
        if(member is null)return NotFound(); member.RoleName=role; member.UpdatedAt=DateTimeOffset.UtcNow; member.UpdatedByUserId=owner.UserId; await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpPost("{userId:guid}/building"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid userId, Guid buildingId, bool allowed)
    {
        var owner=await OwnerAsync(); if(owner is null)return Forbid();
        if(!await db.AccountUsers.AnyAsync(x=>x.AccountId==owner.AccountId && x.UserId==userId && x.RoleName!="Owner" && !x.IsDeleted) ||
           !await db.BirdBuildings.AnyAsync(x=>x.AccountId==owner.AccountId && x.Id==buildingId && !x.IsDeleted)) return NotFound();
        var row=await db.BuildingUsers.SingleOrDefaultAsync(x=>x.UserId==userId && x.BuildingId==buildingId);
        if(allowed && row is null) db.BuildingUsers.Add(new BuildingUser {UserId=userId,BuildingId=buildingId,CreatedByUserId=owner.UserId});
        else if(row is not null){row.IsDeleted=!allowed;row.UpdatedAt=DateTimeOffset.UtcNow;row.UpdatedByUserId=owner.UserId;}
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost("{userId:guid}/toggle"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid userId)
    {
        var owner=await OwnerAsync(); if(owner is null)return Forbid();
        var member=await db.AccountUsers.SingleOrDefaultAsync(x=>x.AccountId==owner.AccountId && x.UserId==userId && x.RoleName!="Owner" && !x.IsDeleted);
        if(member is null)return NotFound();member.IsActive=!member.IsActive;member.UpdatedAt=DateTimeOffset.UtcNow;member.UpdatedByUserId=owner.UserId;await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
    }
}
