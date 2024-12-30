using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMSYS.Models
{
    public class AspNetUserRoles : IdentityUserRole<string>
    {
    }
}