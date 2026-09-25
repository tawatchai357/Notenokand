using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Services;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Quality;

namespace Notenokand.Web.Controllers;

[Authorize, Route("app/quality")]
public sealed class QualityController(NotenokandDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var context = await ContextAsync(); if (context is null) return RedirectToAction("Index", "Onboarding");
        var rounds = await db.HarvestRounds.AsNoTracking()
            .Where(x => x.Building.AccountId == context.Value.AccountId && !x.IsDeleted)
            .Include(x => x.Building).OrderByDescending(x => x.HarvestedOn).ThenByDescending(x => x.CreatedAt).Take(100).ToListAsync();
        var ids = rounds.Select(x => x.Id).ToArray();
        var scores = await db.QualityScores.AsNoTracking().Where(x => ids.Contains(x.HarvestRoundId) && !x.IsDeleted)
            .OrderByDescending(x => x.ConfirmedAt).ToListAsync();
        var latest = scores.GroupBy(x => x.HarvestRoundId).ToDictionary(x => x.Key, x => x.First());
        var chronological = rounds.OrderBy(x => x.HarvestedOn).Where(x => latest.ContainsKey(x.Id)).Select(x => latest[x.Id].TotalScore).ToList();
        return View(new QualityIndexViewModel
        {
            Rounds = rounds.Select(x => new QualityRoundRow { Round=x, Score=latest.GetValueOrDefault(x.Id) }).ToList(),
            Trend = QualityTrendCalculator.Calculate(chronological),
            AverageScore = chronological.Count == 0 ? null : chronological.Average()
        });
    }

    [HttpPost("score"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Score(QualityScoreEditViewModel model)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        var round = await db.HarvestRounds.SingleOrDefaultAsync(x => x.Id == model.HarvestRoundId && x.Building.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (round is null) return NotFound();
        if (!ModelState.IsValid) { TempData["QualityError"]="กรุณากรอกคะแนน 0–100"; return RedirectToAction(nameof(Index)); }
        var ownerUserId = await db.AccountUsers.Where(x => x.AccountId == context.Value.AccountId && x.RoleName == "Owner" && x.IsActive && !x.IsDeleted).Select(x => x.UserId).FirstAsync();
        var standard = await db.QualityStandards.FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.Name == "มาตรฐานคะแนนรวม 100" && x.IsActive && !x.IsDeleted);
        if (standard is null)
        {
            standard = new QualityStandard { OwnerUserId=ownerUserId, Name="มาตรฐานคะแนนรวม 100", Version=1, EffectiveFrom=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)), IsActive=true, CreatedByUserId=context.Value.UserId };
            db.QualityStandards.Add(standard);
        }
        var score = await db.QualityScores.OrderByDescending(x => x.ConfirmedAt).FirstOrDefaultAsync(x => x.HarvestRoundId == round.Id && !x.IsDeleted);
        var label = Label(model.TotalScore);
        if (score is null) db.QualityScores.Add(new QualityScore { HarvestRoundId=round.Id, QualityStandardId=standard.Id, TotalScore=model.TotalScore, ResultLabel=label, Explanation=Clean(model.Explanation), ConfirmedByUserId=context.Value.UserId, ConfirmedAt=DateTimeOffset.UtcNow, CreatedByUserId=context.Value.UserId });
        else { score.TotalScore=model.TotalScore; score.ResultLabel=label; score.Explanation=Clean(model.Explanation); score.ConfirmedByUserId=context.Value.UserId; score.ConfirmedAt=DateTimeOffset.UtcNow; score.UpdatedAt=DateTimeOffset.UtcNow; score.UpdatedByUserId=context.Value.UserId; }
        await db.SaveChangesAsync(); TempData["SuccessMessage"]="บันทึกผลตรวจคุณภาพแล้ว"; return RedirectToAction(nameof(Index));
    }
    private async Task<(Guid AccountId, Guid UserId)?> ContextAsync()
    {
        if(!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var userId))return null;
        var accountId=await db.AccountUsers.Where(x=>x.UserId==userId && x.IsActive && !x.IsDeleted).Select(x=>(Guid?)x.AccountId).FirstOrDefaultAsync();
        return accountId.HasValue ? (accountId.Value,userId) : null;
    }
    private static string Label(decimal score) => score >= 90 ? "ดีเยี่ยม" : score >= 75 ? "ดี" : score >= 60 ? "พอใช้" : "ควรปรับปรุง";
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
