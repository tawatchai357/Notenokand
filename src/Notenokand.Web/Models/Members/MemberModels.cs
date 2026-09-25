using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Notenokand.Web.Models.Members;
public sealed class MembersPageModel
{
    public IReadOnlyList<MemberRowModel> Members { get; init; } = [];
    public IReadOnlyList<SelectListItem> Buildings { get; init; } = [];
}
public sealed class MemberRowModel
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Role { get; init; } = "";
    public bool Active { get; init; }
    public IReadOnlyList<Guid> BuildingIds { get; init; } = [];
}
public sealed class InviteMemberModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, RegularExpression("Editor|Viewer")] public string Role { get; set; } = "Editor";
}
